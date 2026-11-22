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
  viewChild,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Scroller, ScrollerModule } from 'primeng/scroller';
import { TagModule } from 'primeng/tag';

import { HomeActiveVulnerabilityDto } from '@api/models/home-active-vulnerability-dto';
import { HomeNewFixDetailDto } from '@api/models/home-new-fix-detail-dto';
import { HomeService } from '@api/services/home.service';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { StackedBarChartComponent } from '@shared/ui/stacked-bar-chart/stacked-bar-chart.component';
import { StackedBarClick, StackedBarColumn, StackedBarSeries } from '@shared/ui/stacked-bar-chart/stacked-bar-chart.types';
import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { LayeredGraphComponent } from '@shared/ui/layered-graph/layered-graph.component';
import {
  LayeredGraphColumn,
  LayeredGraphEdge,
  LayeredGraphNode,
} from '@shared/ui/layered-graph/layered-graph.types';
import { ScrollerResizeDirective } from '@shared/ui/scroller-resize/scroller-resize.directive';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { VulnerabilityTagSource } from '@shared/ui/trivy-table/trivy-table.types';
import { TrivyTableDialogComponent } from '@shared/ui/trivy-table-dialog/trivy-table-dialog.component';
import { VulnerabilityTagsPipe } from '@shared/ui/trivy-table/vulnerability-tags.pipe';
import { VulnerabilityScoreTagComponent } from '@shared/ui/vulnerability-score-tag/vulnerability-score-tag.component';

import { HomeExploitTagComponent } from '../home-pivot/home-exploit-tag.component';
import { HomeNodeFiltersComponent } from '../home-pivot/home-node-filters.component';
import {
  HomeSelectedNodesComponent,
  SelectedImage,
  SelectedNamespace,
  SelectedPackage,
} from '../home-pivot/home-selected-nodes.component';
import { matchesFilter } from '../home-search-box/home-list-filter';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { dayRange, firstDayIndex, windowDaysLabel } from '../home-collapsed/home-collapsed';
import {
  HomeListSort,
  HomeListSortKey,
  nextSort,
  sortIconOf,
  sortLabelOf,
  sortRows,
  sortTitleOf,
} from '../home-list-sort/home-list-sort';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';
import {
  HomeVulnerabilityTableRow,
  homeVulnerabilityColumns,
  toHomeVulnerabilityTableRows,
} from '../home-vulnerability-rows/home-vulnerability-rows';


// Fixes now available: the (vulnerability id, package) rows whose fix appeared in the window of the page
// - the filters of the page (window, severities, namespaces) come as inputs; a change of them, or of refresh, reloads it
// - collapsed: the first rows (the server sorts them by reach, and is asked for just these); a click on a row selects its
//   id and expands the widget (where it can expand)
// - expanded: on the left, in a box, all the rows (a list that scrolls); on the right one id at a time, all of its data
//   (asked from the detailed endpoint): its packages with a new fix, and the namespaces and images they run in
// - the expanded widget is at least expandedHeight tall and grows with its row of the page, what does not fit scrolls inside
@Component({
  selector: 'app-home-new-fixes',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    HomeSearchBoxComponent,
    ScrollerResizeDirective,
    NgTemplateOutlet,
    ScrollerModule,
    TagModule,
    TrivyBoxComponent,
    LayeredGraphComponent,
    PillButtonComponent,
    StackedBarChartComponent,
    TrivyTableDialogComponent,
    HomeExploitTagComponent,
    HomeSelectedNodesComponent,
    HomeNodeFiltersComponent,
    VulnerabilityScoreTagComponent,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
  ],
  templateUrl: './home-new-fixes.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeNewFixesComponent extends HomeWidgetBase {
  // the collapsed widget reads the rows too, as many as the server gives (it shows the mix of what they say, not the rows)
  private static readonly collapsedFetchLimit = 500;
  // the server allows 500
  private static readonly expandedLimit = 500;
  // more images than this: the images of the graph are in two columns
  private static readonly graphImagesTwoColumnsOver = 30;

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help: HomeWidgetHelp = {
    question: `Which active vulnerabilities <strong>just got a fix</strong>? So you know what is worth updating now.`,
    oddities: [
      `Only a fix that appeared <strong>after</strong> the row was first seen counts: a row that came with its fix is not here.`,
      `A row is a vulnerability id with one package. The window is since the previous visit, or the history window.`,
      `Needs the vulnerability reports history.`,
    ],
    expanded: [
      `Every row (up to 500) on the left; on the right where the selected id runs as a graph (packages, images, namespaces); click a node for its data.`,
    ],
    tips: [
      `Collapsed: the 5 rows that reach the most images; click one to expand on it.`,
      `The CVSS, EPSS and KEV / EUVD tags open their details.`,
    ],
  };

  // the height of a row of the list (a scroller needs the same for all): the id, three lines of title, the tags
  protected readonly rowHeight = 130;
  // the height of the expanded widget body (it needs a fixed one: the list and the graph scroll in it)
  protected readonly expandedHeight = '56rem';

  private readonly homeService = inject(HomeService);
  private readonly mainAppInitService = inject(MainAppInitService);
  private readonly cveEnrichmentState = inject(CveEnrichmentStateService);
  // the tags of the vulnerability id cell of the tables: here the labels of its EPSS and exploited ones
  private readonly vulnerabilityTags = new VulnerabilityTagsPipe();

  hint = input<string>('');
  // the start of the window (ISO, UTC)
  since = input.required<string>();
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  protected readonly selectedId = signal<string | undefined>(undefined);

  // the width of the widget as it is collapsed, which is the width of the box of the list (measured while it is collapsed)
  protected readonly collapsedWidth = signal<number | undefined>(undefined);

  // a new object on any change, so the resources reload; empty namespaces are not sent (the backend default applies)
  private readonly scope = computed(() => ({
    since: this.since(),
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
      this.homeService.getHomeNewFixes({
        since: params.since,
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeNewFixesComponent.collapsedFetchLimit,
      }),
  });

  // idle (and empty) while collapsed
  private readonly expandedData = rxResource({
    params: () => (this.isExpanded() || this.isFiltering() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeNewFixes({
        since: params.since,
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeNewFixesComponent.expandedLimit,
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
  // the list without the sort: one entry per vulnerability id (its first row, the server sorts by reach) among the rows that
  // match the filter (all of them without one); the packages of an id are in its detail
  private readonly baseItems = computed(() => {
    if (this.isSearching()) {
      return [];
    }

    const seen = new Set<string>();
    return this.allItems().filter((x) => {
      const id = x.vulnerabilityId.toLowerCase();
      if (seen.has(id) || !matchesFilter(this.filterText(), x.vulnerabilityId, x.packageName)) {
        return false;
      }
      seen.add(id);
      return true;
    });
  });

  // what an id is, over all of its rows: its highest severity, and its latest moment (a row of it first seen, or whose fix
  // appeared, the last)
  private readonly sortFacts = computed(() => {
    const worst = new Map<string, number>();
    const latest = new Map<string, string>();
    for (const x of this.allItems()) {
      const id = x.vulnerabilityId.toLowerCase();
      worst.set(id, Math.min(worst.get(id) ?? x.severityId, x.severityId));
      const moment = x.fixAppearedAt;
      if (moment && moment > (latest.get(id) ?? '')) {
        latest.set(id, moment);
      }
    }

    return { worst, latest };
  });

  // the sort of the list (null: by reach, the order of the server)
  protected readonly sort = signal<HomeListSort | null>(null);

  protected readonly items = computed(() => {
    const facts = this.sortFacts();

    return sortRows(this.baseItems(), this.sort(), {
      moment: (x) => facts.latest.get(x.vulnerabilityId.toLowerCase()),
      severity: (x) => facts.worst.get(x.vulnerabilityId.toLowerCase()) ?? x.severityId,
      name: (x) => x.vulnerabilityId,
    });
  });  protected readonly noMatch = computed(
    () => this.isFiltering() && !this.isSearching() && this.items().length === 0 && this.allItems().length > 0,
  );
  // the server sends at most expandedLimit rows: a filter over them says so when there are more
  protected readonly partialLabel = computed(() => {
    const total = this.totals()?.activeVulnerabilities ?? 0;
    return this.isFiltering() && this.expandedData.hasValue() && total > this.allItems().length
      ? `The filter looks at the first ${this.allItems().length} of ${total}`
      : '';
  });
  protected readonly totals = computed(() => this.shown().value()?.totals);

  // the selected id, all of its data (only while expanded); the filters are those of the list
  private readonly detailData = rxResource({
    params: () => {
      const scope = this.scope();
      const id = this.selectedId();
      return this.isExpanded() && id ? { ...scope, id } : undefined;
    },
    stream: ({ params }) =>
      this.homeService.getHomeNewFixDetailed({
        vulnerabilityId: params.id,
        since: params.since,
        severities: params.severities,
        namespaces: params.namespaces,
      }),
  });

  // what is on screen: the last data that arrived for the selected id; it stays while the next one loads, so a change of id
  // does not tear the layout down
  private readonly loaded = signal<HomeNewFixDetailDto | undefined>(undefined);
  protected readonly detail = computed(() => this.loaded()?.item ?? undefined);
  protected readonly detailNote = computed(() => this.loaded()?.note);
  // the id of the header: the one on screen, not the one that is loading
  protected readonly displayedId = computed(() => this.detail()?.vulnerabilityId ?? this.selectedId());
  protected readonly isDetailLoading = computed(() => this.detailData.isLoading());
  protected readonly detailError = computed(() => (this.detailData.error() as Error | undefined)?.message);

  // "Show all": the dialog with every row (vulnerability id, package, image), flat; the rows are asked only while it is
  // open, and released when it closes (the resource is idle, and empty, without params)
  protected readonly showAllDialog = signal(false);

  private readonly allRowsData = rxResource({
    params: () => (this.showAllDialog() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeNewFixesDenormalized({
        since: params.since,
        severities: params.severities,
        namespaces: params.namespaces,
      }),
  });

  protected readonly allRows = computed<HomeVulnerabilityTableRow[]>(() =>
    toHomeVulnerabilityTableRows(this.allRowsData.value()?.items),
  );
  protected readonly allRowsColumns = homeVulnerabilityColumns('fix');

  // ---- the collapsed widget: the headline, the new fix ids per day over the whole history range as bars (the days of the
  // window are highlighted), and the packages with the most ids to bump as chips
  private readonly todayUtc = new Date().toISOString().substring(0, 10);
  protected readonly windowDays = computed(() => windowDaysLabel(this.since()));

  private readonly summaryData = rxResource({
    params: () => ({
      severities: this.severities(),
      namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
      refresh: this.refresh(),
      localRefresh: this.localRefresh(),
    }),
    stream: ({ params }) =>
      this.homeService.getHomeNewFixesSummary({ severities: params.severities, namespaces: params.namespaces }),
  });

  // the new fix ids of the days before the window (an id on several days counts on each of them)
  protected readonly earlierIds = computed(() => {
    const sinceDay = this.since().substring(0, 10);

    return (this.summaryData.value()?.days ?? [])
      .filter((x) => x.day < sinceDay)
      .reduce((sum, x) => sum + x.vulnerabilityIds, 0);
  });

  protected readonly stripSeries: StackedBarSeries[] = [{ name: 'New fixes', color: 'var(--p-green-500)' }];

  protected readonly strip = computed(() => {
    const dto = this.summaryData.value();
    const days = dayRange(dto?.from, this.todayUtc);
    const byDay = new Map((dto?.days ?? []).map((d) => [d.day, d.vulnerabilityIds]));
    const columns: StackedBarColumn[] = days.map((day) => ({ label: day, values: [byDay.get(day) ?? 0] }));

    return { columns, highlightFrom: firstDayIndex(days, this.since()) };
  });

  // the packages that fix the most ids (of the rows the server gave), the first three
  protected readonly chips = computed(() => {
    const idsOf = new Map<string, Set<string>>();
    for (const x of this.collapsedData.value()?.items ?? []) {
      const ids = idsOf.get(x.packageName) ?? new Set<string>();
      ids.add(x.vulnerabilityId.toLowerCase());
      idsOf.set(x.packageName, ids);
    }

    return [...idsOf.entries()]
      .map(([name, ids]) => ({ name, count: ids.size }))
      .sort((a, b) => b.count - a.count || a.name.localeCompare(b.name))
      .slice(0, 3);
  });

  // ---- from the bars of the collapsed widget, and from "On a glance": expand and select the first id that matches (in the
  // order of the list), once the rows of the expanded widget are there
  private readonly pendingPick = signal<{ match: (x: HomeActiveVulnerabilityDto) => boolean } | undefined>(undefined);

  // a UTC day ("yyyy-MM-dd"): the first id whose fix appeared that day
  openDay(date: string): void {
    this.pick({ match: (x) => (x.fixAppearedAt?.substring(0, 10)) === date });
  }

  // a chip of the collapsed widget: the same, for the first id of the package
  protected openPackage(packageName: string): void {
    this.pick({ match: (x) => x.packageName === packageName });
  }
  protected onBarClicked(event: StackedBarClick): void {
    this.openDay(event.column.label);
  }
  private pick(pick: { match: (x: HomeActiveVulnerabilityDto) => boolean }): void {
    if (!this.canExpand()) {
      return;
    }

    this.pendingPick.set(pick);
    this.expand();
  }

  // a click on a sort pill: its natural direction, then the reverse, then off
  protected toggleSort(key: HomeListSortKey): void {
    this.sort.update((current) => nextSort(current, key));
  }

  protected sortIcon(key: HomeListSortKey): string | undefined {
    return sortIconOf(this.sort(), key);
  }

  protected sortTitle(key: HomeListSortKey): string {
    return sortTitleOf(this.sort(), key);
  }

  protected readonly sortLabel = computed(() => sortLabelOf(this.sort()));

  // ---- the selection: the nodes of the graph (multi select: at most one of each kind, each a peer of the next; none = empty)
  protected readonly selectedNodes = signal<string[]>([]);
  protected readonly isFiltered = computed(() => this.selectedNodes().length > 0);

  // the index in the detail of the selected node of a kind ('p:', 'i:', 'n:'), or null
  private selectedIndex(prefix: string): number | null {
    const id = this.selectedNodes().find((x) => x.startsWith(prefix));
    return id === undefined ? null : Number(id.substring(2));
  }

  // the data of the selected nodes, brief: the package, the image (its packages and namespaces), the namespace (its images)
  protected readonly selectedPackage = computed<SelectedPackage | null>(() => {
    const index = this.selectedIndex('p:');
    const p = index === null ? undefined : this.detail()?.packages[index];

    return p
      ? {
          name: p.packageName,
          severityId: p.severityId,
          installed: p.installedVersions.join(', ') || '-',
          fixedVersion: p.fixedVersion,
          extra: this.toDay(p.fixAppearedAt),
        }
      : null;
  });

  protected readonly selectedImage = computed<SelectedImage | null>(() => {
    const d = this.detail();
    const index = this.selectedIndex('i:');
    const name = index === null ? undefined : d?.imageNames[index];

    return d && index !== null && name !== undefined
      ? {
          name,
          packages: d.packages.filter((p) => p.imageIndexes.includes(index)).length,
          namespaces: d.imageNamespaceIndexes[index]?.length ?? 0,
        }
      : null;
  });

  protected readonly selectedNamespace = computed<SelectedNamespace | null>(() => {
    const d = this.detail();
    const index = this.selectedIndex('n:');
    const name = index === null ? undefined : d?.namespaceNames[index];

    return d && index !== null && name !== undefined
      ? { name, images: d.imageNamespaceIndexes.filter((x) => x.includes(index)).length }
      : null;
  });

  protected readonly selectedLabel = 'Fix appeared';

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

  // the first id is selected on its own once per expand: a deselect (a click on the selected row) leaves the empty state
  private autoSelect = true;

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

    // the room of the graph: the box it is in (it appears with the detail)
    const graphObserver = new ResizeObserver((entries) => {
      const height = Math.floor(entries[0]?.contentRect.height ?? 0);
      this.graphHeight.set(height > 0 ? height : null);
    });
    effect(() => {
      const element = this.graphHost()?.nativeElement;
      graphObserver.disconnect();
      if (element) {
        graphObserver.observe(element);
      } else {
        this.graphHeight.set(null);
      }
    });
    destroyRef.onDestroy(() => graphObserver.disconnect());

    // expanded without a choice (the expand button), or the selected id is not in the rows any more (the filters changed):
    // the first id of the list (only judged on the many rows, the few are a part of them)
    effect(() => {
      const first = this.items()[0];
      const id = this.selectedId();
      if (!this.isExpanded()) {
        this.autoSelect = true;
        return;
      }
      if (id) {
        this.autoSelect = false;
      }
      if (!first || this.pendingPick()) {
        return;
      }
      if ((!id && this.autoSelect) || (id && this.expandedData.hasValue() && !this.items().some((x) => x.vulnerabilityId === id))) {
        this.autoSelect = false;
        this.selectedId.set(first.vulnerabilityId);
      }
    });

    // a pick is made when the rows are there: no match leaves the default choice (the first id)
    effect(() => {
      const pick = this.pendingPick();
      if (!pick || !this.isExpanded() || !this.expandedData.hasValue()) {
        return;
      }

      const row = this.allItems().find((x) => pick.match(x));
      untracked(() => {
        this.pendingPick.set(undefined);
        if (row) {
          this.selectedId.set(row.vulnerabilityId);
        }
      });
    });

    // expanded with an id selected: the list scrolls to it, once (when the rows of the expanded widget are there and the
    // list is drawn); a collapse re-arms it
    effect(() => {
      const scroller = this.listScroller();
      const id = this.selectedId();
      const items = this.items();
      if (!this.isExpanded()) {
        this.scrolledToSelected = false;
        return;
      }

      // a new order (a sort, a filter) moves the selected id: it is scrolled to again
      const order = JSON.stringify(this.sort()) + this.filterText();
      if (order !== this.listOrder) {
        this.listOrder = order;
        this.scrolledToSelected = false;
      }

      const index = id ? items.findIndex((x) => x.vulnerabilityId === id) : -1;
      if (!scroller || index < 0 || !this.expandedData.hasValue() || this.scrolledToSelected) {
        return;
      }

      this.scrolledToSelected = true;
      // after the scroller measured its box
      setTimeout(() => scroller.scrollToIndex(index, 'auto'), 50);
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

  // a click on a row (of the collapsed widget, or of the list of the expanded one); on the selected row of the expanded list it deselects (the empty state)
  protected select(item: HomeActiveVulnerabilityDto): void {
    if (this.isExpanded() && this.selectedId() === item.vulnerabilityId) {
      this.selectedId.set(undefined);
      this.loaded.set(undefined);
      return;
    }

    this.selectedId.set(item.vulnerabilityId);
    if (this.canExpand() && !this.isExpanded()) {
      this.toggleExpand();
    }
  }

  // ---- the graph: packages -> images -> namespaces, in warm colors (the theme variables); several nodes can be selected
  // (the id of a node is its kind and its index in the detail: 'p:2', 'i:0', 'n:1'), and its data is in the selected box.
  // The images are the many and the long names: their column is wider, and from graphImagesTwoColumnsOver images on it has
  // two sub-columns
  protected readonly graphColumns = computed<LayeredGraphColumn[]>(() => {
    const twoColumns = (this.detail()?.imageNames.length ?? 0) > HomeNewFixesComponent.graphImagesTwoColumnsOver;

    return [
      // 20% longer than the default node (160px)
      { label: 'Packages', color: 'var(--tod-layered-graph-package)', textColor: 'var(--tod-layered-graph-package-text)', width: 192 },
      {
        label: 'Images',
        color: 'var(--tod-layered-graph-image)',
        textColor: 'var(--tod-layered-graph-image-text)',
        width: twoColumns ? 210 : 280,
        subColumns: twoColumns ? 2 : 1,
      },
      { label: 'Namespaces', color: 'var(--tod-layered-graph-namespace)', textColor: 'var(--tod-layered-graph-namespace-text)' },
    ];
  });

  protected readonly graphData = computed<{ nodes: LayeredGraphNode[]; edges: LayeredGraphEdge[] }>(() => {
    const d = this.detail();
    if (!d) {
      return { nodes: [], edges: [] };
    }

    const nodes: LayeredGraphNode[] = [];
    const edges: LayeredGraphEdge[] = [];
    // the value of an image is its namespaces, that of a namespace its images
    const imagesOfNamespace = new Array<number>(d.namespaceNames.length).fill(0);
    d.imageNamespaceIndexes.forEach((list) => list.forEach((n) => (imagesOfNamespace[n] = (imagesOfNamespace[n] ?? 0) + 1)));

    d.packages.forEach((p, i) => {
      nodes.push({ id: `p:${i}`, column: 0, label: p.packageName, value: p.images, weight: p.images });
      p.imageIndexes.forEach((image) => edges.push({ from: `p:${i}`, to: `i:${image}` }));
    });
    d.imageNames.forEach((name, i) => {
      const namespaces = d.imageNamespaceIndexes[i] ?? [];
      nodes.push({ id: `i:${i}`, column: 1, label: name, value: namespaces.length, weight: namespaces.length });
      namespaces.forEach((n) => edges.push({ from: `i:${i}`, to: `n:${n}` }));
    });
    d.namespaceNames.forEach((name, n) => {
      nodes.push({ id: `n:${n}`, column: 2, label: name, value: imagesOfNamespace[n], weight: imagesOfNamespace[n] });
    });

    return { nodes, edges };
  });

  // the list of the ids: on expand, and when its order changes (sort, filter), it is scrolled to the selected one
  private readonly listScroller = viewChild(Scroller);
  private scrolledToSelected = false;
  private listOrder = '';

  // the height of the box of the graph, which scrolls inside it
  private readonly graphHost = viewChild<ElementRef<HTMLElement>>('graphHost');
  protected readonly graphHeight = signal<number | null>(null);

  // the graph says the selected nodes (multi select; none = empty)
  protected onGraphSelect(ids: string[]): void {
    this.selectedNodes.set(ids);
  }

  protected clearFilters(): void {
    this.selectedNodes.set([]);
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

  // the UTC day of a date, or '-'
  protected toDay(value: string | null | undefined): string {
    return value ? value.substring(0, 10) : '-';
  }
}
