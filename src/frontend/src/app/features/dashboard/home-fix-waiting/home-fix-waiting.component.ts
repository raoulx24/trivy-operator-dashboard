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
import { ScrollerModule } from 'primeng/scroller';
import { TagModule } from 'primeng/tag';

import { HomeActiveVulnerabilityDto } from '@api/models/home-active-vulnerability-dto';
import { HomeFixWaitingDetailDto } from '@api/models/home-fix-waiting-detail-dto';
import { HomeService } from '@api/services/home.service';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { ScrollerResizeDirective } from '@shared/ui/scroller-resize/scroller-resize.directive';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { SelectedDtosEvent, TrivyTableColumn, VulnerabilityTagSource } from '@shared/ui/trivy-table/trivy-table.types';
import { VulnerabilityTagsPipe } from '@shared/ui/trivy-table/vulnerability-tags.pipe';
import { VulnerabilityScoreTagComponent } from '@shared/ui/vulnerability-score-tag/vulnerability-score-tag.component';

import { HomeExploitTagComponent } from '../home-pivot/home-exploit-tag.component';
import { matchesFilter } from '../home-search-box/home-list-filter';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

interface PackageRow {
  // the place of the package in the packages of the detail
  index: number;
  packageName: string;
  installed: string;
  fixedVersion: string;
  severityId: number;
  firstSeen: string;
  fixSince: string;
  images: number;
}

// the rows (id, package) of one vulnerability id, shown as one entry
interface IdGroup {
  vulnerabilityId: string;
  title?: string | null;
  severityId: number;
  isKnownExploited: boolean;
  rows: HomeActiveVulnerabilityDto[];
}

// Fix waiting time: the fixable active (vulnerability id, package) rows, the longest known fix first
// - the filters of the page (severities, namespaces) come as inputs; the window does not apply (it is the state now); a change of them, or of refresh, reloads it
// - collapsed: the first rows (the server sorts them by reach, and is asked for just these); a click on a row selects its
//   id and expands the widget (where it can expand)
// - expanded: on the left, in a box, all the rows (a list that scrolls); on the right one id at a time, all of its data
//   (asked from the detailed endpoint): its packages with a fix, and the namespaces and images they run in
// - the expanded widget is at least expandedHeight tall and grows with its row of the page, what does not fit scrolls inside
@Component({
  selector: 'app-home-fix-waiting',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    HomeSearchBoxComponent,
    ButtonModule,
    ScrollerResizeDirective,
    NgTemplateOutlet,
    ScrollerModule,
    TagModule,
    TrivyBoxComponent,
    TrivyTableComponent,
    HomeExploitTagComponent,
    VulnerabilityScoreTagComponent,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
  ],
  templateUrl: './home-fix-waiting.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeFixWaitingComponent extends HomeWidgetBase {
  private static readonly collapsedLimit = 5;
  // the server allows 500
  private static readonly expandedLimit = 500;

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help = `
    <p class="mb-2">The <strong>active vulnerabilities that have a fix</strong> and run now, in the <strong>severities</strong> and <strong>namespaces</strong> of the filter bar. A row is a vulnerability id with one package.</p>
    <p class="mb-2"><strong>Collapsed</strong>: the 5 rows whose fix is known the longest. A click on a row expands the widget on its id.</p>
    <p class="mb-2"><strong>Expanded</strong>: on the left every row (up to 500; a click selects its id), on the right the namespaces and the images where the selected id runs, and its packages with a fix, with the installed and the fixed version, since when the fix is known and when the row was first seen.</p>
    <p class="mb-2"><strong>CVSS</strong> opens the CVE details, <strong>EPSS</strong> the exploit prediction score and <strong>KEV / EUVD</strong> the known exploited data (when the CVE is in one of them).</p>
    <p class="mb-0 tod-text-muted-color"><strong>Fix known since</strong> is a lower bound of the waiting time: a row without history has no date and goes last.</p>
  `;

  // the height of a row of the list (a scroller needs the same for all): the id, three lines of title, the package, the tags
  protected readonly rowHeight = 150;
  // the height of the expanded widget body (it needs a fixed one: the lists and the table scroll in it)
  protected readonly expandedHeight = '40rem';

  private readonly homeService = inject(HomeService);
  private readonly mainAppInitService = inject(MainAppInitService);
  private readonly cveEnrichmentState = inject(CveEnrichmentStateService);
  // the tags of the vulnerability id cell of the tables: here the labels of its EPSS and exploited ones
  private readonly vulnerabilityTags = new VulnerabilityTagsPipe();

  protected readonly packageColumns: TrivyTableColumn[] = [
    { field: 'packageName', header: 'Package', isFilterable: false, isSortable: true, style: 'width: 20%', renderType: 'standard' },
    { field: 'installed', header: 'Installed', isFilterable: false, isSortable: true, style: 'width: 18%', renderType: 'standard' },
    { field: 'fixedVersion', header: 'Fixed', isFilterable: false, isSortable: true, style: 'width: 18%', renderType: 'standard' },
    { field: 'severityId', header: 'Severity', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'severityBadge' },
    { field: 'firstSeen', header: 'First seen', isFilterable: false, isSortable: true, style: 'width: 9rem', renderType: 'standard' },
    { field: 'fixSince', header: 'Fix known since', isFilterable: false, isSortable: true, style: 'width: 10rem', renderType: 'standard' },
    { field: 'images', header: 'Images', isFilterable: false, isSortable: true, style: 'width: 6rem; text-align: right', renderType: 'standard' },
  ];

  hint = input<string>('');
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  protected readonly selectedId = signal<string | undefined>(undefined);

  // the width of the widget as it is collapsed, which is the width of the box of the list (measured while it is collapsed)
  protected readonly collapsedWidth = signal<number | undefined>(undefined);

  // a new object on any change, so the resources reload; empty namespaces are not sent (the backend default applies)
  private readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  // the text of the filter box (collapsed and expanded); a filter looks at all the rows, so while there is one they are asked for
  protected readonly filterText = signal('');
  protected readonly isFiltering = computed(() => this.filterText().trim().length > 0);

  private readonly collapsedData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeFixWaiting({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeFixWaitingComponent.collapsedLimit,
      }),
  });

  // idle (and empty) while collapsed
  private readonly expandedData = rxResource({
    params: () => (this.isExpanded() || this.isFiltering() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeFixWaiting({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeFixWaitingComponent.expandedLimit,
      }),
  });

  // what is shown: the many rows when they are there, else the few
  private readonly shown = computed(() =>
    (this.isExpanded() || this.isFiltering()) && this.expandedData.hasValue() ? this.expandedData : this.collapsedData,
  );

  protected readonly allItems = computed(() => this.shown().value()?.items ?? []);
  // a started filter looks at all the rows: while they load there is nothing to show
  protected readonly isSearching = computed(
    () => this.isFiltering() && !this.expandedData.hasValue() && this.expandedData.isLoading(),
  );
  // the rows that match the filter (all of them without one)
  protected readonly items = computed(() =>
    this.isSearching() ? [] : this.allItems().filter((x) => matchesFilter(this.filterText(), x.vulnerabilityId, x.packageName)),
  );
  protected readonly noMatch = computed(
    () => this.isFiltering() && !this.isSearching() && this.items().length === 0 && this.allItems().length > 0,
  );
  // the collapsed widget: the first rows
  // the server sends at most expandedLimit rows: a filter over them says so when there are more
  protected readonly partialLabel = computed(() => {
    const total = this.totals()?.activeVulnerabilities ?? 0;
    return this.isFiltering() && this.expandedData.hasValue() && total > this.allItems().length
      ? `The filter looks at the first ${this.allItems().length} of ${total}`
      : '';
  });
  // the rows of one vulnerability id are one entry (the server sorts by waiting time: the order of first appearance is kept)
  protected readonly groups = computed<IdGroup[]>(() => {
    const byId = new Map<string, IdGroup>();
    for (const row of this.items()) {
      const key = row.vulnerabilityId.toLowerCase();
      const group = byId.get(key);
      if (group) {
        group.rows.push(row);
        group.severityId = Math.min(group.severityId, row.severityId);
        group.isKnownExploited ||= row.isKnownExploited;
      } else {
        byId.set(key, {
          vulnerabilityId: row.vulnerabilityId,
          title: row.title,
          severityId: row.severityId,
          isKnownExploited: row.isKnownExploited,
          rows: [row],
        });
      }
    }
    return [...byId.values()];
  });
  // the collapsed widget: the first ids
  protected readonly collapsedGroups = computed(() => this.groups().slice(0, HomeFixWaitingComponent.collapsedLimit));
  protected readonly totals = computed(() => this.shown().value()?.totals);
  protected readonly more = computed(() =>
    this.isFiltering()
      ? Math.max(0, this.groups().length - HomeFixWaitingComponent.collapsedLimit)
      : Math.max(0, (this.totals()?.activeVulnerabilities ?? 0) - this.items().length),
  );

  // the selected id, all of its data (only while expanded); the filters are those of the list
  private readonly detailData = rxResource({
    params: () => {
      const scope = this.scope();
      const id = this.selectedId();
      return this.isExpanded() && id ? { ...scope, id } : undefined;
    },
    stream: ({ params }) =>
      this.homeService.getHomeFixWaitingDetailed({
        vulnerabilityId: params.id,
        severities: params.severities,
        namespaces: params.namespaces,
      }),
  });

  // what is on screen: the last data that arrived for the selected id; it stays while the next one loads, so a change of id
  // does not tear the layout down
  private readonly loaded = signal<HomeFixWaitingDetailDto | undefined>(undefined);
  protected readonly detail = computed(() => this.loaded()?.item ?? undefined);
  protected readonly detailNote = computed(() => this.loaded()?.note);
  // the id of the header: the one on screen, not the one that is loading
  protected readonly displayedId = computed(() => this.detail()?.vulnerabilityId ?? this.selectedId());
  protected readonly isDetailLoading = computed(() => this.detailData.isLoading());
  protected readonly detailError = computed(() => (this.detailData.error() as Error | undefined)?.message);

  // the packages of the table
  protected readonly packageRows = computed<PackageRow[]>(() =>
    (this.detail()?.packages ?? []).map((p, index) => ({
      index,
      packageName: p.packageName,
      installed: p.installedVersions.join(', '),
      fixedVersion: p.fixedVersion,
      severityId: p.severityId,
      firstSeen: this.toDay(p.firstSeenAt),
      fixSince: this.toDay(p.fixSince),
      images: p.images,
    })),
  );

  // ---- the filter: the lists of the detail filter each other
  // one selection per list (an index in the list; the package is the selected row of the table). Each list is filtered only
  // by the selections of the other two, never by its own: so what is faded is what cannot go with them, and it cannot be
  // selected (that would be an empty combination). A selection that was allowed stays so when another is made.
  protected readonly selectedNamespace = signal<number | null>(null);
  protected readonly selectedImage = signal<number | null>(null);
  protected readonly packageSelection = signal<SelectedDtosEvent<PackageRow>>({ selectedDtos: [], source: 'programmatic' });
  private readonly selectedPackage = computed(() => this.packageSelection().selectedDtos[0]?.index ?? null);

  protected readonly isFiltered = computed(
    () => this.selectedNamespace() !== null || this.selectedImage() !== null || this.selectedPackage() !== null,
  );

  // the relations of the detail as sets: the namespaces of each image, the images of each package
  private readonly relations = computed(() => {
    const d = this.detail();
    return d
      ? {
          namespacesOfImage: d.imageNamespaceIndexes.map((x) => new Set(x)),
          imagesOfPackage: d.packages.map((x) => new Set(x.imageIndexes)),
        }
      : undefined;
  });

  protected readonly allowedNamespaces = computed<ReadonlySet<number>>(() => {
    const d = this.detail();
    const r = this.relations();
    const image = this.selectedImage();
    const pack = this.selectedPackage();
    const allowed = new Set<number>();

    for (let n = 0; d && r && n < d.namespaceNames.length; n++) {
      const inImage = image === null || !!r.namespacesOfImage[image]?.has(n);
      const inPackage =
        pack === null || [...(r.imagesOfPackage[pack] ?? [])].some((i) => r.namespacesOfImage[i]?.has(n));
      if (inImage && inPackage) {
        allowed.add(n);
      }
    }
    return allowed;
  });

  protected readonly allowedImages = computed<ReadonlySet<number>>(() => {
    const d = this.detail();
    const r = this.relations();
    const namespace = this.selectedNamespace();
    const pack = this.selectedPackage();
    const allowed = new Set<number>();

    for (let i = 0; d && r && i < d.imageNames.length; i++) {
      const inNamespace = namespace === null || !!r.namespacesOfImage[i]?.has(namespace);
      const inPackage = pack === null || !!r.imagesOfPackage[pack]?.has(i);
      if (inNamespace && inPackage) {
        allowed.add(i);
      }
    }
    return allowed;
  });

  protected readonly allowedPackages = computed<ReadonlySet<number>>(() => {
    const d = this.detail();
    const r = this.relations();
    const namespace = this.selectedNamespace();
    const image = this.selectedImage();
    const allowed = new Set<number>();

    for (let p = 0; d && r && p < d.packages.length; p++) {
      const images = r.imagesOfPackage[p] ?? new Set<number>();
      const inImage = image === null || images.has(image);
      const inNamespace = namespace === null || [...images].some((i) => r.namespacesOfImage[i]?.has(namespace));
      if (inImage && inNamespace) {
        allowed.add(p);
      }
    }
    return allowed;
  });

  // the rows of the table that do not go with the selections are faded, and cannot be selected (dimmedRowsSelectable)
  protected readonly packageRowDimmer = (row: PackageRow): boolean => !this.allowedPackages().has(row.index);

  protected readonly state = computed<HomeWidgetState>(() => {
    const resource = this.shown();
    if (resource.error()) {
      return { error: (resource.error() as Error)?.message ?? 'request failed' };
    }

    const dto = resource.value();
    if (!dto) {
      return {};
    }

    const totals = dto.totals;
    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary:
        `${totals.vulnerabilityIds} ids, ${totals.activeVulnerabilities} rows (${this.items().length} shown), ` +
        `${totals.images} images, ${totals.namespaces} ns` +
        (this.isExpanded() && this.expandedData.isLoading() ? ' (loading more...)' : ''),
    };
  });

  constructor() {
    super();

    this.cveEnrichmentState.ensureLoaded();

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

    // expanded without a choice (the expand button), or the selected id is not in the rows any more (the filters changed):
    // the first id of the list (only judged on the many rows, the few are a part of them)
    effect(() => {
      const first = this.items()[0];
      const id = this.selectedId();
      if (!this.isExpanded() || !first) {
        return;
      }
      if (!id || (this.expandedData.hasValue() && !this.items().some((x) => x.vulnerabilityId === id))) {
        this.selectedId.set(first.vulnerabilityId);
      }
    });

    // other data (another id, or the same one loaded again): the indexes of the selections are of the old lists
    effect(() => {
      this.detail();
      untracked(() => this.clearFilters());
    });

    // the data of the selected id arrived (or failed): it goes on screen
    effect(() => {
      const dto = this.detailData.value();
      const id = this.selectedId();
      if (this.detailData.isLoading() || !id) {
        return;
      }

      if (this.detailData.error()) {
        this.loaded.set(undefined);
      } else if (dto?.vulnerabilityId.toLowerCase() === id.toLowerCase()) {
        this.loaded.set(dto);
      }
      // else the resource did not take the new id yet
    });
  }

  // a click on a row (of the collapsed widget, or of the list of the expanded one)
  protected select(item: { vulnerabilityId: string }): void {
    this.selectedId.set(item.vulnerabilityId);
    if (this.canExpand() && !this.isExpanded()) {
      this.toggleExpand();
    }
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

  // a click on a row of the packages table (the table does not take it on a faded row)
  protected onPackageSelection(event: SelectedDtosEvent<PackageRow>): void {
    if (event.source === 'user') {
      this.packageSelection.set(event);
    }
  }

  protected clearFilters(): void {
    this.selectedNamespace.set(null);
    this.selectedImage.set(null);
    this.packageSelection.set({ selectedDtos: [], source: 'programmatic' });
  }

  // the details of a CVE (the dialog behind the CVSS tag of a vulnerability id): only for CVE ids, and only when the
  // CVE details are enabled
  protected hasCveDetails(vulnerabilityId: string): boolean {
    return (
      this.mainAppInitService.backendSettingsDto().isCveDetailsEnabled &&
      vulnerabilityId.trim().toUpperCase().startsWith('CVE-')
    );
  }

  // the label of the tag of the tables for the id (the same pipe), or '': 'EPSS 46%'; 'KEV', 'EUVD' or 'KEV/EUVD' for the
  // known exploited catalogs listing the CVE
  protected tagLabel(vulnerabilityId: string, source: VulnerabilityTagSource): string {
    return (
      this.vulnerabilityTags
        .transform(vulnerabilityId, this.cveEnrichmentState.byId(), false)
        .find((x) => x.source === source)?.label ?? ''
    );
  }

  protected packageNames(rows: HomeActiveVulnerabilityDto[]): string {
    return [...new Set(rows.map((x) => x.packageName))].join(', ');
  }

  // the longest wait of the rows of a group (the oldest fix date), or undefined without a date
  protected longestFixSince(rows: HomeActiveVulnerabilityDto[]): string | undefined {
    return rows
      .map((x) => x.fixSince)
      .filter((x): x is string => !!x)
      .sort()[0];
  }

  // "fix known 17 days (since 2026-11-01)": the fix is known since a date, a lower bound of the waiting time
  protected waitingLabel(fixSince: string | null | undefined): string {
    if (!fixSince) {
      return 'fix known: no date';
    }
    const days = Math.max(0, Math.floor((Date.now() - new Date(fixSince).getTime()) / 86_400_000));
    return `fix known ${days} ${days === 1 ? 'day' : 'days'} (since ${this.toDay(fixSince)})`;
  }

  // the UTC day of a date, or '-'
  protected toDay(value: string | null | undefined): string {
    return value ? value.substring(0, 10) : '-';
  }
}
