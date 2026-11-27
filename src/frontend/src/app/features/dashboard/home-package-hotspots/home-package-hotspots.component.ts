import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  Injector,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ScrollerModule } from 'primeng/scroller';
import { TagModule } from 'primeng/tag';
import { forkJoin, map, of, switchMap } from 'rxjs';

import { HomePackageHotspotDto } from '@api/models/home-package-hotspot-dto';
import { HomeWhereIsPackageItemDto } from '@api/models/home-where-is-package-item-dto';
import { HomeWhereIsPackageVulnerabilityDto } from '@api/models/home-where-is-package-vulnerability-dto';
import { HomeService } from '@api/services/home.service';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { VulnerabilityTagSource } from '@shared/ui/trivy-table/trivy-table.types';
import { VulnerabilityTagsPipe } from '@shared/ui/trivy-table/vulnerability-tags.pipe';
import { VulnerabilityScoreTagComponent } from '@shared/ui/vulnerability-score-tag/vulnerability-score-tag.component';
import { ScrollerResizeDirective } from '@shared/ui/scroller-resize/scroller-resize.directive';
import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { ScrollerComponent } from '@shared/ui/scroller/scroller.component';
import { MeterGroupComponent } from '@shared/ui/meter-group/meter-group.component';
import { ChartTipDirective } from '@shared/ui/popover/chart-tip.directive';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { MeterGroupSegment } from '@shared/ui/meter-group/meter-group.types';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { HomeListSort, HomeListSortKey, nextSort, sortIconOf, sortRows, sortTitleOf } from '../home-list-sort/home-list-sort';
import { HomeExploitTagComponent } from '../home-pivot/home-exploit-tag.component';
import { matchesFilter } from '../home-search-box/home-list-filter';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// the versions of a package whose vulnerability ids are read (the most widespread first)
const MAX_VERSIONS_WITH_IDS = 8;

type PackageSortKey = 'name' | 'ids' | 'images' | 'namespaces';

const SORT_TITLES: Record<PackageSortKey, { label: string; natural: string; reversed: string }> = {
  name: { label: 'Sort by name', natural: 'A to Z', reversed: 'Z to A' },
  ids: { label: 'Sort by vulnerability ids', natural: 'Most ids first', reversed: 'Fewest ids first' },
  images: { label: 'Sort by images', natural: 'Most images first', reversed: 'Fewest images first' },
  namespaces: { label: 'Sort by namespaces', natural: 'Most namespaces first', reversed: 'Fewest namespaces first' },
};

interface HotspotRow {
  item: HomePackageHotspotDto;
  segments: MeterGroupSegment[];
}

interface VersionRow {
  version: string;
  images: number;
  segments: MeterGroupSegment[];
}

interface IdRow {
  vulnerabilityId: string;
  severityId: number;
  isKnownExploited: boolean;
  title: string | null;
  // the fixed versions (empty: no fix yet)
  fixedVersions: string[];
  images: number;
}

interface PackageDetail {
  versions: VersionRow[];
  // the ids of the (up to MAX_VERSIONS_WITH_IDS) most widespread versions
  ids: IdRow[];
  // the versions that were not read for ids
  versionsWithoutIds: number;
}

// Package hotspots: packages (purl without version) by the number of vulnerability ids, with the fixable ones and the images
// it loads by itself (a change of a filter, or of refresh, reloads it): a few rows normally, and while it is expanded
// many more, asked from the server with a bigger limit (the few rows are shown until they arrive)
// - a click on a row expands the widget with that package selected; expanded, the selected package shows its versions
//   (version sprawl: images per installed version) and its vulnerability ids, from the where-is endpoints
@Component({
  selector: 'app-home-package-hotspots',
  standalone: true,
  imports: [
    HomeExploitTagComponent,
    HomeSearchBoxComponent,
    HomeWidgetFrameComponent,
    MeterGroupComponent,
    ChartTipDirective,
    NgTemplateOutlet,
    PillButtonComponent,
    ScrollerComponent,
    ScrollerModule,
    ScrollerResizeDirective,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
    TagModule,
    TrivyBoxComponent,
    VulnerabilityScoreTagComponent,
  ],
  templateUrl: './home-package-hotspots.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomePackageHotspotsComponent extends HomeWidgetBase {
  // the server allows 500
  private static readonly collapsedLimit = 5;
  private static readonly expandedLimit = 100;

  // the height of a row of the list of the vulnerabilities (the virtual scroller needs all the rows to have the same one)
  protected readonly idRowHeight = 120;

  private readonly homeService = inject(HomeService);
  private readonly mainAppInitService = inject(MainAppInitService);
  private readonly cveEnrichmentState = inject(CveEnrichmentStateService);
  // the tags of the vulnerability id cell of the tables: here the labels of its EPSS and exploited ones
  private readonly vulnerabilityTags = new VulnerabilityTagsPipe();

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help: HomeWidgetHelp = {
    question: `Which <strong>packages</strong> bring the most vulnerabilities? Patching them pays off the most.`,
    goodToKnow: [
      `Packages are counted without their version. The bar is the number of vulnerability ids, on one scale for all: <strong>teal</strong> have a fix, <strong>grey</strong> none yet.`,
      `At the right: the images (expanded, also the namespaces) that have the package.`,
      `The ids of a package are read from its ${MAX_VERSIONS_WITH_IDS} most widespread versions.`,
    ],
    expanded: [
      `Up to 100 packages on the left; on the right the selected package: its versions with the images that run each, and its vulnerability ids with the fix.`,
    ],
    tips: [
      `Version sprawl hurts: one version fewer to run is one fewer to patch.`,
      `Collapsed: click one of the 5 worst packages to expand on it.`,
    ],
  };

  hint = input<string>('');
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  // a new object on any change, so the resources reload; empty namespaces are not sent (the backend default applies)
  private readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  private readonly collapsedData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomePackageHotspots({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomePackageHotspotsComponent.collapsedLimit,
      }),
  });

  // idle (and empty) while collapsed
  private readonly expandedData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomePackageHotspots({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomePackageHotspotsComponent.expandedLimit,
      }),
  });

  // set when the many rows of this expand were revealed; reset on collapse (so a reload by a filter does not scroll)
  private revealed = false;

  // the selected package (its key); set by a click on a row (of the collapsed widget too), by default the first
  protected readonly selectedKey = signal<string | null>(null);

  constructor() {
    super();

    this.cveEnrichmentState.ensureLoaded();

    // new data (a filter of the page, a refresh) plays the meters again
    effect(() => {
      this.expandedData.value();
      untracked(() => this.animateMeters.set(true));
    });

    // the many rows arrived and are in the signal: after they are rendered the page may scroll to show the whole widget
    effect(() => {
      if (!this.isExpanded()) {
        this.revealed = false;
        untracked(() => this.deselected.set(false));
      } else if (this.expandedData.hasValue() && !this.revealed) {
        this.revealed = true;
        this.revealAfterRender();
      }
    });
  }

  // what is shown: the many rows when they are there, else the few
  private readonly shown = computed(() =>
    this.isExpanded() && this.expandedData.hasValue() ? this.expandedData : this.collapsedData,
  );

  protected readonly items = computed(() => this.shown().value()?.items ?? []);

  // the most ids of a package: the meters share this scale, so a longer one is a worse package
  protected readonly maxIds = computed(() => Math.max(1, ...this.items().map((x) => x.vulnerabilityIds)));

  // the package with the most vulnerability ids (of the rows shown): the headline of the collapsed widget
  protected readonly top = computed<HomePackageHotspotDto | undefined>(() =>
    this.items().reduce<HomePackageHotspotDto | undefined>(
      (best, x) => (!best || x.vulnerabilityIds > best.vulnerabilityIds ? x : best),
      undefined,
    ),
  );

  protected readonly rows = computed<HotspotRow[]>(() =>
    this.items().map((item) => ({
      item,
      segments: [
        { label: 'fixable ids', value: item.fixableVulnerabilityIds, color: 'var(--tod-package-hotspots-fixable)' },
        {
          label: 'no fix yet',
          value: Math.max(0, item.vulnerabilityIds - item.fixableVulnerabilityIds),
          color: 'var(--tod-package-hotspots-no-fix)',
        },
      ],
    })),
  );

  // the filter box of the expanded list: the text is looked for in the package name and key
  protected readonly filterText = signal('');
  // the sort of the list: a pill per key, going natural direction (A to Z, the most first) -> reversed -> off (the server
  // order: the most ids first)
  protected readonly packageSort = signal<{ key: PackageSortKey; reversed: boolean } | null>(null);
  protected readonly filteredRows = computed(() => {
    const rows = this.rows().filter((r) => matchesFilter(this.filterText(), r.item.packageName, r.item.packageKey));
    const sort = this.packageSort();
    if (!sort) {
      return rows;
    }

    const direction = sort.reversed ? -1 : 1;
    const collator = new Intl.Collator(undefined, { numeric: true, sensitivity: 'base' });
    const compare = (a: HotspotRow, b: HotspotRow): number => {
      switch (sort.key) {
        case 'name':
          return collator.compare(a.item.packageName, b.item.packageName);
        case 'ids':
          return b.item.vulnerabilityIds - a.item.vulnerabilityIds;
        case 'images':
          return b.item.images - a.item.images;
        case 'namespaces':
          return b.item.namespaces - a.item.namespaces;
      }
    };
    // the rows that tie keep the order they had
    return rows
      .map((row, index) => ({ row, index }))
      .sort((a, b) => direction * compare(a.row, b.row) || a.index - b.index)
      .map((x) => x.row);
  });

  // the meters of the list grow when data arrives, not when a sort moves the rows
  protected readonly animateMeters = signal(true);

  protected togglePackageSort(key: PackageSortKey): void {
    this.packageSort.update((current) =>
      current?.key !== key ? { key, reversed: false } : current.reversed ? null : { key, reversed: true },
    );
    // the rows move (the DOM nodes are re-inserted, which would play their meters again): no animation until new data comes
    this.animateMeters.set(false);
    this.centerSelectedAfterRender();
  }

  private readonly packageList = viewChild(ScrollerComponent);
  private readonly sortInjector = inject(Injector);

  // the order changed: the list scrolls to the selected row (centered), not to the start; only the list scrolls, not the page
  private centerSelectedAfterRender(): void {
    afterNextRender(
      () => {
        const list = this.packageList()?.scrollContainer?.nativeElement;
        const row = list?.querySelector<HTMLElement>(".tod-home-pick-row.selected");
        if (list && row) {
          const offset = row.getBoundingClientRect().top - list.getBoundingClientRect().top;
          list.scrollTop += offset - (list.clientHeight - row.clientHeight) / 2;
        }
      },
      { injector: this.sortInjector },
    );
  }

  // the arrow shows the order on screen: down the natural one, up the reverse; none while it is off
  protected packageSortIcon(key: PackageSortKey): string | undefined {
    const sort = this.packageSort();
    return sort?.key !== key ? undefined : sort.reversed ? 'pi-arrow-up' : 'pi-arrow-down';
  }

  protected packageSortTitle(key: PackageSortKey): string {
    const sort = this.packageSort();
    const titles = SORT_TITLES[key];
    return sort?.key !== key ? titles.label : sort.reversed ? titles.reversed : titles.natural;
  }

  protected readonly noMatch = computed(
    () => this.filterText().trim().length > 0 && this.filteredRows().length === 0 && this.rows().length > 0,
  );

  // a click on the selected row of the expanded list deselects it (the empty state) until the next click, or the next expand
  private readonly deselected = signal(false);

  // the selected package: the one clicked if it is in the list, else the first; none after a deselect
  protected readonly selected = computed<HomePackageHotspotDto | undefined>(() =>
    this.deselected()
      ? undefined
      : (this.items().find((x) => x.packageKey === this.selectedKey()) ?? this.items()[0]),
  );

  // a click on a row: the widget is expanded (when it is not) with that package selected; on the selected row of the
  // expanded list it deselects
  protected select(item: HomePackageHotspotDto): void {
    if (this.isExpanded() && this.selected()?.packageKey === item.packageKey) {
      this.deselected.set(true);
      return;
    }

    this.deselected.set(false);
    this.selectedKey.set(item.packageKey);
    this.expand();
  }

  // the versions and ids of the selected package; idle while collapsed
  private readonly detailData = rxResource({
    params: () => {
      const selected = this.selected();
      return this.isExpanded() && selected
        ? { packageKey: selected.packageKey, namespaces: this.scope().namespaces, refresh: this.scope().refresh }
        : undefined;
    },
    stream: ({ params }) =>
      this.homeService
        .getHomeWhereIsPackage({ query: params.packageKey, namespaces: params.namespaces, limit: 100 })
        .pipe(
          switchMap((list) => {
            const versions = list.items
              .filter((x) => x.packageKey === params.packageKey)
              .sort((a, b) => b.images - a.images);
            const read = versions.slice(0, MAX_VERSIONS_WITH_IDS);
            if (read.length === 0) {
              return of(this.toDetail(versions, [], 0));
            }

            return forkJoin(
              read.map((v) =>
                this.homeService.getHomeWhereIsPackageDetailed({
                  packageKey: v.packageKey,
                  version: v.version,
                  namespaces: params.namespaces,
                }),
              ),
            ).pipe(
              map((details) =>
                this.toDetail(
                  versions,
                  details.flatMap((d) => d.item?.vulnerabilities ?? []),
                  versions.length - read.length,
                ),
              ),
            );
          }),
        ),
  });

  protected readonly detail = computed<PackageDetail | undefined>(() =>
    this.detailData.hasValue() ? this.detailData.value() : undefined,
  );
  protected readonly detailLoading = computed(() => this.detailData.isLoading());
  protected readonly detailError = computed(() => (this.detailData.error() as Error | undefined)?.message);

  // the sort of the list of the vulnerabilities (null: the worst first, then the most images); the pills go natural
  // direction -> reversed -> off; the rows have no moment
  protected readonly sort = signal<HomeListSort | null>(null);
  protected readonly sortedIds = computed(() =>
    sortRows(this.detail()?.ids ?? [], this.sort(), {
      moment: () => undefined,
      severity: (x) => x.severityId,
      name: (x) => x.vulnerabilityId,
    }),
  );

  protected toggleSort(key: HomeListSortKey): void {
    this.sort.update((current) => nextSort(current, key));
  }

  protected sortIcon(key: HomeListSortKey): string | undefined {
    return sortIconOf(this.sort(), key);
  }

  protected sortTitle(key: HomeListSortKey): string {
    return sortTitleOf(this.sort(), key);
  }

  protected readonly maxVersionImages = computed(() =>
    Math.max(1, ...(this.detail()?.versions ?? []).map((v) => v.images)),
  );

  private toDetail(
    versions: HomeWhereIsPackageItemDto[],
    vulnerabilities: HomeWhereIsPackageVulnerabilityDto[],
    versionsWithoutIds: number,
  ): PackageDetail {
    const ids = new Map<string, IdRow>();
    for (const v of vulnerabilities) {
      const row = ids.get(v.vulnerabilityId) ?? {
        vulnerabilityId: v.vulnerabilityId,
        severityId: v.severityId,
        isKnownExploited: v.isKnownExploited,
        title: v.title ?? null,
        fixedVersions: [],
        images: 0,
      };
      row.title ??= v.title ?? null;
      row.images += v.imageIndexes.length;
      if (v.fixedVersion && !row.fixedVersions.includes(v.fixedVersion)) {
        row.fixedVersions.push(v.fixedVersion);
      }
      ids.set(v.vulnerabilityId, row);
    }

    return {
      versions: versions.map((v) => {
        // the color is the highest severity of the version
        const color =
          v.maxSeverityId == null ? 'var(--tod-package-hotspots-no-fix)' : SeverityUtils.getCssColor(v.maxSeverityId);
        return { version: v.version, images: v.images, segments: [{ label: v.version, value: v.images, color }] };
      }),
      ids: [...ids.values()].sort((a, b) => a.severityId - b.severityId || b.images - a.images),
      versionsWithoutIds,
    };
  }

  // the details of a CVE (the dialog behind the CVSS tag): only for CVE ids, and only when the CVE details are enabled
  protected hasCveDetails(vulnerabilityId: string): boolean {
    return (
      this.mainAppInitService.backendSettingsDto().isCveDetailsEnabled &&
      vulnerabilityId.trim().toUpperCase().startsWith('CVE-')
    );
  }

  // the label of the tag of the tables for the id (the same pipe), or '': 'EPSS 46%'; 'KEV', 'EUVD' or 'KEV/EUVD'
  protected tagLabel(vulnerabilityId: string, source: VulnerabilityTagSource): string {
    return (
      this.vulnerabilityTags
        .transform(vulnerabilityId, this.cveEnrichmentState.byId(), false)
        .find((x) => x.source === source)?.label ?? ''
    );
  }

  protected readonly state = computed<HomeWidgetState>(() => {
    const resource = this.shown();
    if (resource.error()) {
      return { error: (resource.error() as Error)?.message ?? 'request failed' };
    }

    const dto = resource.value();
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary:
        `${dto.items.length} packages` + (this.isExpanded() && this.expandedData.isLoading() ? ' (loading more...)' : ''),
    };
  });
}
