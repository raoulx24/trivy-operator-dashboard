import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ButtonModule } from 'primeng/button';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { ScrollerResizeDirective } from '../../../shared/ui/scroller-resize/scroller-resize.directive';
import { ScrollerModule } from 'primeng/scroller';
import { TagModule } from 'primeng/tag';

import { HomeWhereIsPackageDetailDto } from '@api/models/home-where-is-package-detail-dto';
import { HomeWhereIsPackageItemDto } from '@api/models/home-where-is-package-item-dto';
import { HomeService } from '@api/services/home.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { PurlPartsPipe } from '@shared/pipes/purl-parts.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { SelectedDtosEvent, TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

interface VulnerabilityRow {
  // the place of the vulnerability in the vulnerabilities of the detail
  index: number;
  vulnerabilityId: string;
  severityId: number;
  fixedVersion: string;
  images: number;
}

// Where is... package: a package (a name, or a purl prefix) and where each of its versions runs
// - searches only on Enter (and on the refresh button, once something was searched); the namespaces are read when the
//   search is made, so a change of the filter does not re-run it
// - one row per package version, i.e. per purl with its version
// - collapsed: the first rows (the server sorts them by reach, and is asked for just these); a click on a purl selects
//   it and expands the widget (where it can expand)
// - expanded: on the left, in a box, the search and all the purls it found (a list that scrolls, the rows of the
//   collapsed widget); on the right one purl at a time, all of its data (asked from the detailed endpoint): its
//   namespaces, images and licenses, and its vulnerabilities
// - the expanded widget is at least expandedHeight tall and grows with its row of the page, what does not fit scrolls inside
@Component({
  selector: 'app-home-where-is-package',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    ButtonModule,
    HomeSearchBoxComponent,
    ScrollerResizeDirective,
    NgTemplateOutlet,
    ScrollerModule,
    TagModule,
    TrivyBoxComponent,
    TrivyTableComponent,
    PurlPartsPipe,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
  ],
  templateUrl: './home-where-is-package.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeWhereIsPackageComponent extends HomeWidgetBase {
  private static readonly collapsedLimit = 5;
  // the server allows 500
  private static readonly expandedLimit = 500;

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help = `
    <p class="mb-2">Where a <strong>package</strong> runs now, one row per <strong>version</strong> (a purl with its version). Type a <strong>name</strong> (e.g. <em>openssl</em>, <em>libc</em>) or the <strong>beginning of a purl</strong> (<em>pkg:npm/axios</em>, with an <strong>@</strong> and the beginning of a version: <em>pkg:npm/axios@1.6</em>) and press <strong>Enter</strong> (at least 3 characters; case does not matter).</p>
    <p class="mb-2"><strong>Collapsed</strong>: the 5 purls that reach the most images. A click on a purl expands the widget on it.</p>
    <p class="mb-2"><strong>Expanded</strong>: on the left every purl found (up to 500; a click selects one), on the right the namespaces and the images where the selected purl runs, its licenses and its vulnerabilities.</p>
    <p class="mb-1"><strong>A row</strong></p>
    <p class="mb-2">The <strong>severity</strong> is the highest of its vulnerabilities (none when it has no active ones). Then the counts: <strong>images</strong>, <strong>namespaces</strong>, <strong>workloads</strong> and <strong>vulnerabilities</strong> (distinct ids).</p>
    <p class="mb-0 tod-text-muted-color">The packages come from the <strong>SBOM reports</strong>; without them only the vulnerable packages are found and there are no licenses. Only the <strong>namespaces filter</strong> applies; a change of it does not run the search again, the refresh button does.</p>
  `;

  // the height of a row of the list (a scroller needs the same for all): the name and version (two lines at most), the tags, the counts
  protected readonly rowHeight = 104;
  // the height of the expanded widget body (it needs a fixed one: the lists and the table scroll in it)
  protected readonly expandedHeight = '40rem';

  private readonly homeService = inject(HomeService);

  protected readonly vulnerabilityColumns: TrivyTableColumn[] = [
    { field: 'vulnerabilityId', header: 'Vulnerability', isFilterable: false, isSortable: true, style: 'width: 38%', renderType: 'vulnerabilityId' },
    { field: 'severityId', header: 'Severity', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'severityBadge' },
    { field: 'fixedVersion', header: 'Fixed', isFilterable: false, isSortable: true, style: 'width: 30%', renderType: 'standard' },
    { field: 'images', header: 'Images', isFilterable: false, isSortable: true, style: 'width: 6rem; text-align: right', renderType: 'standard' },
  ];

  hint = input<string>('');
  namespaces = input.required<string[]>();

  protected readonly query = signal('');
  // what was asked: a new object at every search (so the same query runs again); undefined before the first search
  private readonly request = signal<{ query: string; namespaces: string[] | undefined } | undefined>(undefined);
  // the selected purl: its package key and version are what the detailed endpoint takes
  protected readonly selected = signal<{ packageKey: string; version: string; purl: string } | undefined>(undefined);

  // the width of the widget as it is collapsed, which is the width of the box of the list (measured while it is collapsed)
  protected readonly collapsedWidth = signal<number | undefined>(undefined);

  private readonly scope = computed(() => {
    const request = this.request();
    return request ? { ...request, localRefresh: this.localRefresh() } : undefined;
  });

  private readonly collapsedData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeWhereIsPackage({
        query: params.query,
        namespaces: params.namespaces,
        limit: HomeWhereIsPackageComponent.collapsedLimit,
      }),
  });

  // idle (and empty) while collapsed
  private readonly expandedData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeWhereIsPackage({
        query: params.query,
        namespaces: params.namespaces,
        limit: HomeWhereIsPackageComponent.expandedLimit,
      }),
  });

  // what is shown: the many rows when they are there, else the few
  private readonly shown = computed(() =>
    this.isExpanded() && this.expandedData.hasValue() ? this.expandedData : this.collapsedData,
  );

  protected readonly items = computed(() => this.shown().value()?.items ?? []);
  protected readonly totalMatches = computed(() => this.shown().value()?.totalMatches ?? 0);
  protected readonly more = computed(() => Math.max(0, this.totalMatches() - this.items().length));
  protected readonly note = computed(() => this.shown().value()?.note);
  protected readonly searched = computed(() => this.request() !== undefined);

  // the selected purl, all of its data (only while expanded); the namespaces are those of the search
  private readonly detailData = rxResource({
    params: () => {
      const scope = this.scope();
      const selected = this.selected();
      return this.isExpanded() && scope && selected
        ? { ...selected, namespaces: scope.namespaces, localRefresh: scope.localRefresh }
        : undefined;
    },
    stream: ({ params }) =>
      this.homeService.getHomeWhereIsPackageDetailed({
        packageKey: params.packageKey,
        version: params.version,
        namespaces: params.namespaces,
      }),
  });

  // what is on screen: the last data that arrived for the selected purl; it stays while the next one loads, so a change
  // of purl does not tear the layout down
  private readonly loaded = signal<HomeWhereIsPackageDetailDto | undefined>(undefined);
  protected readonly detail = computed(() => this.loaded()?.item ?? undefined);
  protected readonly detailNote = computed(() => this.loaded()?.note);
  protected readonly isDetailLoading = computed(() => this.detailData.isLoading());
  protected readonly detailError = computed(() => (this.detailData.error() as Error | undefined)?.message);

  // the vulnerabilities of the table
  protected readonly vulnerabilityRows = computed<VulnerabilityRow[]>(() =>
    (this.detail()?.vulnerabilities ?? []).map((v, index) => ({
      index,
      vulnerabilityId: v.vulnerabilityId,
      severityId: v.severityId,
      fixedVersion: v.fixedVersion ?? 'no fix',
      images: v.imageIndexes.length,
    })),
  );

  // ---- the filter: the lists of the detail filter each other
  // one selection per list (an index in the list; the vulnerability is the selected row of the table). Each list is
  // filtered only by the selections of the other two, never by its own: so what is faded is what cannot go with them, and
  // it cannot be selected (that would be an empty combination). A selection that was allowed stays so when another is made.
  protected readonly selectedNamespace = signal<number | null>(null);
  protected readonly selectedImage = signal<number | null>(null);
  protected readonly vulnerabilitySelection = signal<SelectedDtosEvent<VulnerabilityRow>>({
    selectedDtos: [],
    source: 'programmatic',
  });
  private readonly selectedVulnerability = computed(() => this.vulnerabilitySelection().selectedDtos[0]?.index ?? null);

  protected readonly isFiltered = computed(
    () => this.selectedNamespace() !== null || this.selectedImage() !== null || this.selectedVulnerability() !== null,
  );

  // the relations of the detail as sets: the namespaces of each image, the images of each vulnerability
  private readonly relations = computed(() => {
    const d = this.detail();
    return d
      ? {
          namespacesOfImage: d.imageNamespaceIndexes.map((x) => new Set(x)),
          imagesOfVulnerability: d.vulnerabilities.map((x) => new Set(x.imageIndexes)),
        }
      : undefined;
  });

  protected readonly allowedNamespaces = computed<ReadonlySet<number>>(() => {
    const d = this.detail();
    const r = this.relations();
    const image = this.selectedImage();
    const vulnerability = this.selectedVulnerability();
    const allowed = new Set<number>();

    for (let n = 0; d && r && n < d.namespaceNames.length; n++) {
      const inImage = image === null || !!r.namespacesOfImage[image]?.has(n);
      const inVulnerability =
        vulnerability === null ||
        [...(r.imagesOfVulnerability[vulnerability] ?? [])].some((i) => r.namespacesOfImage[i]?.has(n));
      if (inImage && inVulnerability) {
        allowed.add(n);
      }
    }
    return allowed;
  });

  protected readonly allowedImages = computed<ReadonlySet<number>>(() => {
    const d = this.detail();
    const r = this.relations();
    const namespace = this.selectedNamespace();
    const vulnerability = this.selectedVulnerability();
    const allowed = new Set<number>();

    for (let i = 0; d && r && i < d.imageNames.length; i++) {
      const inNamespace = namespace === null || !!r.namespacesOfImage[i]?.has(namespace);
      const inVulnerability = vulnerability === null || !!r.imagesOfVulnerability[vulnerability]?.has(i);
      if (inNamespace && inVulnerability) {
        allowed.add(i);
      }
    }
    return allowed;
  });

  protected readonly allowedVulnerabilities = computed<ReadonlySet<number>>(() => {
    const d = this.detail();
    const r = this.relations();
    const namespace = this.selectedNamespace();
    const image = this.selectedImage();
    const allowed = new Set<number>();

    for (let v = 0; d && r && v < d.vulnerabilities.length; v++) {
      const images = r.imagesOfVulnerability[v] ?? new Set<number>();
      const inImage = image === null || images.has(image);
      const inNamespace = namespace === null || [...images].some((i) => r.namespacesOfImage[i]?.has(namespace));
      if (inImage && inNamespace) {
        allowed.add(v);
      }
    }
    return allowed;
  });

  // the rows of the table that do not go with the selections are faded, and cannot be selected (dimmedRowsSelectable)
  protected readonly vulnerabilityRowDimmer = (row: VulnerabilityRow): boolean =>
    !this.allowedVulnerabilities().has(row.index);

  protected readonly state = computed<HomeWidgetState>(() => {
    const resource = this.shown();
    if (resource.error()) {
      return { error: (resource.error() as Error)?.message ?? 'request failed' };
    }

    const dto = resource.value();
    if (!this.searched()) {
      return { idleText: 'Type and press Enter to search' };
    }
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary:
        `${dto.totalMatches} purls (${dto.items.length} shown)` +
        (this.isExpanded() && this.expandedData.isLoading() ? ' (loading more...)' : ''),
    };
  });

  constructor() {
    super();

    // the width of the collapsed widget (what is measured while it is expanded is not kept)
    const host = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
    const destroyRef = inject(DestroyRef);
    afterNextRender(() => {
      const observer = new ResizeObserver(() => {
        if (!this.isExpanded() && host.offsetWidth > 0) {
          this.collapsedWidth.set(host.offsetWidth);
        }
      });
      observer.observe(host);
      destroyRef.onDestroy(() => observer.disconnect());
    });

    // expanded without a choice (the expand button, or a new search): the first purl of the list
    effect(() => {
      const first = this.items()[0];
      if (this.isExpanded() && !this.selected() && first) {
        this.selected.set(this.toSelected(first));
      }
    });

    // other data (another purl, or the same one loaded again): the indexes of the selections are of the old lists
    effect(() => {
      this.detail();
      untracked(() => this.clearFilters());
    });

    // the data of the selected purl arrived (or failed): it goes on screen
    effect(() => {
      const dto = this.detailData.value();
      const selected = this.selected();
      if (this.detailData.isLoading() || !selected) {
        return;
      }

      if (this.detailData.error()) {
        this.loaded.set(undefined);
      } else if (dto?.packageKey === selected.packageKey && dto.version === selected.version) {
        this.loaded.set(dto);
      }
      // else the resource did not take the new purl yet
    });
  }

  protected onSearch(): void {
    const query = this.query().trim();
    if (!query) {
      // an empty box and Enter: the widget goes back to before the first search
      this.request.set(undefined);
      this.selected.set(undefined);
      this.loaded.set(undefined);
      return;
    }

    const namespaces = this.namespaces();
    this.selected.set(undefined);
    this.loaded.set(undefined);
    this.request.set({ query, namespaces: namespaces.length > 0 ? namespaces : undefined });
  }

  protected onRefresh(): void {
    if (this.searched()) {
      this.refreshWidget();
    }
  }

  // the purl that was just copied (its button shows a check for a moment)
  protected readonly copiedPurl = signal<string | undefined>(undefined);
  private copiedTimer: ReturnType<typeof setTimeout> | undefined;

  // the copy button of a row: not a click on the row (it would select it)
  protected async copyPurl(purl: string, event: Event): Promise<void> {
    event.stopPropagation();
    try {
      await navigator.clipboard.writeText(purl);
    } catch {
      // no clipboard (insecure page, or refused): nothing was copied, so no check
      return;
    }
    this.copiedPurl.set(purl);
    clearTimeout(this.copiedTimer);
    this.copiedTimer = setTimeout(() => this.copiedPurl.set(undefined), 1500);
  }

  // a click on a purl (of the collapsed widget, or of the list of the expanded one)
  protected select(item: HomeWhereIsPackageItemDto): void {
    this.selected.set(this.toSelected(item));
    if (this.canExpand() && !this.isExpanded()) {
      this.toggleExpand();
    }
  }

  protected isSelected(item: HomeWhereIsPackageItemDto): boolean {
    return this.selected()?.purl === item.purl;
  }

  private toSelected(item: HomeWhereIsPackageItemDto) {
    return { packageKey: item.packageKey, version: item.version, purl: item.purl };
  }

  // "2 of 5" while a selection filters, else "5"
  protected countLabel(allowed: number, total: number): string {
    return this.isFiltered() ? `${allowed} of ${total}` : `${total}`;
  }

  // a click on a namespace or an image: selects it, or clears it when it is the selected one; a faded one is disabled
  protected toggleNamespace(index: number): void {
    if (this.allowedNamespaces().has(index)) {
      this.selectedNamespace.update((x) => (x === index ? null : index));
    }
  }

  protected toggleImage(index: number): void {
    if (this.allowedImages().has(index)) {
      this.selectedImage.update((x) => (x === index ? null : index));
    }
  }

  // a click on a row of the vulnerabilities table (the table does not take it on a faded row)
  protected onVulnerabilitySelection(event: SelectedDtosEvent<VulnerabilityRow>): void {
    if (event.source === 'user') {
      this.vulnerabilitySelection.set(event);
    }
  }

  protected clearFilters(): void {
    this.selectedNamespace.set(null);
    this.selectedImage.set(null);
    this.vulnerabilitySelection.set({ selectedDtos: [], source: 'programmatic' });
  }
}
