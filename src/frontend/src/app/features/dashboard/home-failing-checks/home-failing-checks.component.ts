import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  linkedSignal,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { TagModule } from 'primeng/tag';

import { HomeFailingCheckDetailDto } from '@api/models/home-failing-check-detail-dto';
import { HomeFailingCheckDto } from '@api/models/home-failing-check-dto';
import { HomeService } from '@api/services/home.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { MatrixHeatmapComponent } from '@shared/ui/matrix-heatmap/matrix-heatmap.component';
import { MatrixHeatmapCellRef, MatrixHeatmapClick, MatrixHeatmapRowDto } from '@shared/ui/matrix-heatmap/matrix-heatmap.types';
import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { SpanTimelineComponent } from '@shared/ui/span-timeline/span-timeline.component';
import { SpanTimelineRow, SpanTimelineTick } from '@shared/ui/span-timeline/span-timeline.types';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { severityColor, severityTextColor } from '../home-collapsed/home-collapsed';
import { matchesFilter } from '../home-search-box/home-list-filter';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { ChartTipDirective } from '@shared/ui/popover/chart-tip.directive';

type SortBy = 'severity' | 'share';

// a family of checks: its name in the backend and its short name (the rows of the grid)
interface Family {
  key: string;
  label: string;
}

const FAMILIES: Family[] = [
  { key: 'ConfigAudit', label: 'config' },
  { key: 'RbacAssessment', label: 'rbac' },
  { key: 'InfraAssessment', label: 'infra' },
];

// fewer evaluated resources than this: the share is shaky (a faint bar)
const FEW_EVALUATED = 5;

// the short kinds of the failing resources of the detail ("deploy/api")
const KIND_SHORT: Record<string, string> = {
  Deployment: 'deploy',
  ReplicaSet: 'rs',
  ReplicationController: 'rc',
  StatefulSet: 'sts',
  DaemonSet: 'ds',
  CronJob: 'cj',
  Job: 'job',
  Pod: 'pod',
  Service: 'svc',
  ConfigMap: 'cm',
  Ingress: 'ing',
  NetworkPolicy: 'netpol',
  ClusterRole: 'clusterrole',
  ClusterRoleBinding: 'clusterrolebinding',
  Role: 'role',
  RoleBinding: 'rolebinding',
};

// the resources of a namespace shown before "+n more"
const NAMESPACE_RESOURCES = 3;

// the room of a character of a row label of the timeline (px, a rough size), to cut a label that does not fit
const LABEL_CHAR_WIDTH = 7;
// the room around a label: its start (8px) and the gap before the bars
const LABEL_PADDING = 28;
// the widest label column of the collapsed bars (px); a longer id is cut (its popover has it whole)
const WIDESPREAD_MAX_LABEL_WIDTH = 170;

// a namespace of the selected check: its failing resources out of the evaluated ones, and the resources
interface NamespaceGroup {
  // '' for the cluster scoped ones
  key: string;
  name: string;
  resources: number;
  evaluated: number;
  names: string[];
}

// Failing checks: the failing config audit / RBAC / infra checks, grouped by check id
// - it loads by itself (a change of a filter, or of refresh, reloads it); only the severities and the namespaces apply
// - one request, all the checks (up to the server limit); the grid, the share and the sort are computed here
// - a check fails in some resources out of the evaluated ones; trivy keeps only the failing checks, so "evaluated" is
//   the resources of the same family whose kind is one a failing resource has (the server says so)
// - collapsed: the distinct failing checks, a grid of them by family and severity, and the most widespread ones; a click
//   on a cell expands the widget filtered by it, a click on a check expands it with that check selected
// - expanded: all the checks as bars (failing / evaluated) on the left, the selected check on the right: its remediation
//   and its failing resources by namespace
@Component({
  selector: 'app-home-failing-checks',
  standalone: true,
  imports: [
    ChartTipDirective,
    IconComponent,
    HomeWidgetFrameComponent,
    HomeSearchBoxComponent,
    NgTemplateOutlet,
    MatrixHeatmapComponent,
    PillButtonComponent,
    SpanTimelineComponent,
    TagModule,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
  ],
  templateUrl: './home-failing-checks.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeFailingChecksComponent extends HomeWidgetBase {
  // the server allows 500; the collapsed widget needs all the rows too (the grid is of all of them)
  private static readonly fetchLimit = 500;
  private static readonly widespreadLimit = 3;

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help: HomeWidgetHelp = {
    question: `Which <strong>security checks</strong> (config audit, RBAC, infra) fail in the widest share of my cluster? So you fix the ones that hit the most resources.`,
    goodToKnow: [
      `<strong>Failing / evaluated</strong> is an estimate: Trivy keeps only the failures, so a kind in which every resource passes is not counted.`,
      `All workload kinds (Pod, Deployment, StatefulSet...) count as one kind, as do Role and ClusterRole.`,
      `A faint bar with a dashed outline has fewer than ${FEW_EVALUATED} evaluated resources: its share is shaky.`,
      `With a namespaces filter the cluster scoped resources are out.`,
    ],
    expanded: [
      `Every check (up to 500) as a bar, sorted by severity then share, or by share.`,
      `Click one for its remediation and its failing resources by namespace.`,
    ],
    tips: [
      `Collapsed: click a cell of the family by severity grid to expand the widget already filtered.`,
    ],
  };

  // the expanded widget body is at least this tall, and grows with its row of the page; the timeline takes the rest of
  // the height (timelineHeight, measured) and its rows scroll in it
  protected readonly expandedMinHeight = 'min(30rem, 70vh)';
  protected readonly expandedMaxHeight = '70vh';
  private readonly timelineHost = viewChild<ElementRef<HTMLElement>>('timelineHost');
  protected readonly timelineHeight = signal<number | null>(null);
  protected readonly shareTicks: SpanTimelineTick[] = [25, 50, 75, 100].map((value) => ({ value }));
  protected readonly shareFormat = (value: number) => `${value}%`;
  protected readonly fewEvaluated = FEW_EVALUATED;
  protected readonly families = FAMILIES;

  private readonly homeService = inject(HomeService);

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

  private readonly data = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeFailingChecks({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeFailingChecksComponent.fetchLimit,
      }),
  });

  // the answer on screen: the last one that arrived, kept while the next one is asked (nothing goes blank on a reload);
  // undefined until the first one, when the grid shows zeros
  private readonly checks = linkedSignal<ReturnType<typeof this.data.value>, ReturnType<typeof this.data.value>>({
    source: this.data.value,
    computation: (value, previous) => value ?? previous?.value,
  });
  // counts the answers received: the animations play again for each one, also when it is the same data (a refresh)
  protected readonly answers = linkedSignal<ReturnType<typeof this.data.value>, number>({
    source: this.data.value,
    computation: (value, previous) => (previous?.value ?? 0) + (value ? 1 : 0),
  });
  protected readonly isLoaded = computed(() => this.checks() !== undefined);

  protected readonly allItems = computed(() => this.checks()?.items ?? []);
  protected readonly totalChecks = computed(() => this.checks()?.totalChecks ?? 0);
  // the server sends at most fetchLimit rows: what is computed here is of those
  protected readonly partialLabel = computed(() => {
    const shown = this.allItems().length;
    return this.totalChecks() > shown ? `Computed on the first ${shown} of ${this.totalChecks()} checks` : '';
  });

  // ---- the grid: distinct checks by family (rows) and severity (columns, those of the filter)
  protected readonly gridSeverities = computed(() => [...this.severities()].sort((a, b) => a - b));
  protected readonly gridColumns = computed(() =>
    this.gridSeverities().map((id) => SeverityUtils.getName(id).toLowerCase()),
  );
  // the cells in the colors of the severities, zeros until the checks are there
  protected readonly gridRows = computed<MatrixHeatmapRowDto[]>(() =>
    FAMILIES.map((f) => ({
      label: f.label,
      cells: this.gridSeverities().map((id) => ({
        value: this.allItems().filter((x) => x.family === f.key && x.severityId === id).length,
        color: severityColor(id),
        textColor: severityTextColor(id),
      })),
    })),
  );

  // the cell of the grid the list is filtered by (family and severity); null = all
  protected readonly gridFilter = signal<{ family: string; severityId: number } | null>(null);
  protected readonly gridCell = computed<MatrixHeatmapCellRef | null>(() => {
    const filter = this.gridFilter();
    if (!filter) {
      return null;
    }
    const rowIndex = FAMILIES.findIndex((f) => f.key === filter.family);
    const columnIndex = this.gridSeverities().indexOf(filter.severityId);
    return rowIndex < 0 || columnIndex < 0 ? null : { rowIndex, columnIndex };
  });
  protected readonly gridFilterLabel = computed(() => {
    const filter = this.gridFilter();
    return filter
      ? `${this.familyLabel(filter.family)} · ${SeverityUtils.getName(filter.severityId).toLowerCase()}`
      : '';
  });

  // ---- the most widespread checks (collapsed): the largest share, among the ones with enough evaluated resources
  private readonly widespreadChecks = computed(() =>
    [...this.allItems()]
      .filter((x) => x.evaluated >= FEW_EVALUATED)
      .sort((a, b) => this.share(b) - this.share(a) || b.resources - a.resources)
      .slice(0, HomeFailingChecksComponent.widespreadLimit),
  );

  // the label column fits the longest id (up to a limit, then the id is cut), so the ids are read whole
  protected readonly widespreadLabelWidth = computed(() => {
    const longest = Math.max(0, ...this.widespreadChecks().map((x) => x.checkId.length));
    return Math.min(WIDESPREAD_MAX_LABEL_WIDTH, longest * LABEL_CHAR_WIDTH + LABEL_PADDING);
  });

  protected readonly widespreadRows = computed<SpanTimelineRow[]>(() => {
    const maxChars = Math.floor((this.widespreadLabelWidth() - LABEL_PADDING) / LABEL_CHAR_WIDTH);
    return this.widespreadChecks().map((x) =>
      this.toRow(x, this.cut(x.checkId, maxChars), `${x.resources}/${x.evaluated}`),
    );
  });

  // ---- the expanded list: the filter box, the grid filter and the sort
  protected readonly filterText = signal('');
  protected readonly sortBy = signal<SortBy>('severity');
  // the key of the selected check (family|checkId)
  protected readonly selectedKey = signal<string | null>(null);

  protected readonly items = computed(() => {
    const filter = this.gridFilter();
    const rows = this.allItems().filter(
      (x) =>
        (!filter || (x.family === filter.family && x.severityId === filter.severityId)) &&
        matchesFilter(this.filterText(), x.checkId, x.family, x.title),
    );
    const severity = (a: HomeFailingCheckDto, b: HomeFailingCheckDto) => a.severityId - b.severityId;
    const share = (a: HomeFailingCheckDto, b: HomeFailingCheckDto) =>
      this.share(b) - this.share(a) || b.resources - a.resources;
    return rows.sort((a, b) =>
      this.sortBy() === 'severity' ? severity(a, b) || share(a, b) : share(a, b) || severity(a, b),
    );
  });
  protected readonly noMatch = computed(
    () => this.filterText().trim().length > 0 && this.items().length === 0 && this.allItems().length > 0,
  );

  // the label: the id and the title, cut to the room of the label
  protected readonly labelWidth = 450;
  protected readonly timelineRows = computed<SpanTimelineRow[]>(() => {
    const maxChars = Math.floor((this.labelWidth - LABEL_PADDING) / LABEL_CHAR_WIDTH);
    return this.items().map((x) =>
      this.toRow(
        x,
        this.cut(`${x.checkId}  ${x.title}`, maxChars),
        `${x.resources}/${x.evaluated} · ${x.namespaces} ns`,
      ),
    );
  });

  // ---- the selected check: its row of the list, and all its data from the detailed endpoint
  protected readonly selectedItem = computed(() => this.allItems().find((x) => this.keyOf(x) === this.selectedKey()));

  private readonly detailData = rxResource({
    params: () => {
      const item = this.selectedItem();
      return this.isExpanded() && item ? { family: item.family, checkId: item.checkId, ...this.scope() } : undefined;
    },
    stream: ({ params }) =>
      this.homeService.getHomeFailingCheckDetailed({
        family: params.family,
        checkId: params.checkId,
        namespaces: params.namespaces,
      }),
  });

  // what is on screen: the last data that arrived for the selected check; it stays while the next one loads, so a change
  // of check does not tear the layout down
  private readonly loaded = signal<HomeFailingCheckDetailDto | undefined>(undefined);
  protected readonly detail = computed(() => this.loaded()?.item ?? undefined);
  protected readonly detailNote = computed(() => this.loaded()?.note);
  protected readonly isDetailLoading = computed(() => this.detailData.isLoading());
  protected readonly detailError = computed(() => (this.detailData.error() as Error | undefined)?.message);

  // the namespaces whose resources are all shown (the others show the first NAMESPACE_RESOURCES and "+n more")
  protected readonly openNamespaces = signal<ReadonlySet<string>>(new Set());

  protected readonly namespaceGroups = computed<NamespaceGroup[]>(() => {
    const detail = this.detail();
    if (!detail) {
      return [];
    }
    const names = new Map<string, string[]>();
    for (const row of detail.rows) {
      const key = row.namespaceName ?? '';
      const list = names.get(key) ?? [];
      list.push(`${KIND_SHORT[row.resourceKind] ?? row.resourceKind.toLowerCase()}/${row.resourceName}`);
      names.set(key, list);
    }
    return detail.byNamespace.map((ns) => {
      const key = ns.namespaceName ?? '';
      return {
        key,
        name: ns.namespaceName ?? '(cluster scoped)',
        resources: ns.resources,
        evaluated: ns.evaluated,
        names: names.get(key) ?? [],
      };
    });
  });

  protected readonly state = computed<HomeWidgetState>(() => {
    if (this.data.error()) {
      return { error: (this.data.error() as Error)?.message ?? 'request failed' };
    }

    const dto = this.data.value();
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary: `${dto.totalChecks} distinct checks failing`,
    };
  });

  constructor() {
    super();

    // the room of the timeline: its box without the padding (the box appears with the expanded widget)
    const destroyRef = inject(DestroyRef);
    const observer = new ResizeObserver((entries) => {
      const height = Math.floor(entries[0]?.contentRect.height ?? 0);
      this.timelineHeight.set(height > 0 ? height : null);
    });
    effect(() => {
      const element = this.timelineHost()?.nativeElement;
      observer.disconnect();
      if (element) {
        observer.observe(element);
      } else {
        this.timelineHeight.set(null);
      }
    });
    destroyRef.onDestroy(() => observer.disconnect());

    // back to collapsed: the filters of the expanded list (a grid cell, the text) go, so no cell stays selected; it acts
    // on the change from expanded to collapsed only, as a click on a cell sets its filter just before the widget expands
    let wasExpanded = false;
    effect(() => {
      const expanded = this.isExpanded();
      untracked(() => {
        if (wasExpanded && !expanded) {
          this.gridFilter.set(null);
          this.filterText.set('');
        }
        wasExpanded = expanded;
      });
    });

    // expanded with nothing selected (the expand button, a grid cell): the first check of the list
    effect(() => {
      const first = this.items()[0];
      if (this.isExpanded() && first) {
        untracked(() => {
          const selected = this.selectedKey();
          if (!selected || !this.items().some((x) => this.keyOf(x) === selected)) {
            this.selectedKey.set(this.keyOf(first));
          }
        });
      }
    });

    // the data of the selected check arrived (or failed): it goes on screen
    effect(() => {
      const dto = this.detailData.value();
      const item = this.selectedItem();
      if (this.detailData.isLoading() || !item) {
        return;
      }

      if (this.detailData.error()) {
        this.loaded.set(undefined);
      } else if (dto?.checkId === item.checkId && dto.family === item.family) {
        this.loaded.set(dto);
        this.openNamespaces.set(new Set());
      }
      // else the resource did not take the new check yet
    });
  }

  // a click on a cell of the grid (collapsed): the list filtered by it; again on the same cell: all
  protected onGridCell(click: MatrixHeatmapClick): void {
    const family = FAMILIES[click.rowIndex]?.key;
    const severityId = this.gridSeverities()[click.columnIndex];
    const current = this.gridFilter();
    if (current && current.family === family && current.severityId === severityId) {
      this.gridFilter.set(null);
      return;
    }
    this.gridFilter.set({ family, severityId });
    this.expandIfPossible();
  }

  // a click on a widespread check (collapsed): the whole list, with that check selected
  protected onWidespread(key: string | null): void {
    if (!key) {
      return;
    }
    this.gridFilter.set(null);
    this.filterText.set('');
    this.selectedKey.set(key);
    this.expandIfPossible();
  }

  protected showAll(): void {
    this.gridFilter.set(null);
    this.filterText.set('');
  }

  protected openNamespace(key: string): void {
    this.openNamespaces.update((set) => new Set([...set, key]));
  }

  protected shownNames(group: NamespaceGroup): string[] {
    return this.openNamespaces().has(group.key) ? group.names : group.names.slice(0, NAMESPACE_RESOURCES);
  }

  // the failing resources in percent of the evaluated ones (rounded)
  protected percent(resources: number, evaluated: number): number {
    return evaluated > 0 ? Math.round((resources / evaluated) * 100) : 0;
  }

  protected familyLabel(family: string): string {
    return FAMILIES.find((f) => f.key === family)?.label ?? family;
  }

  // expands the widget where it can expand (the group is wide enough)
  private expandIfPossible(): void {
    if (this.canExpand()) {
      this.expand();
    }
  }

  private toRow(x: HomeFailingCheckDto, label: string, valueLabel: string): SpanTimelineRow {
    const share = this.share(x);
    return {
      id: this.keyOf(x),
      label,
      start: share * 100,
      color: SeverityUtils.getCssColor(x.severityId),
      valueLabel,
      faint: x.evaluated < FEW_EVALUATED,
      details: [
        x.title,
        `${this.familyLabel(x.family)} · ${SeverityUtils.getName(x.severityId).toLowerCase()}`,
        `${x.resources} of ${x.evaluated} evaluated resources fail (${this.percent(x.resources, x.evaluated)}%)`,
        `${x.namespaces} ns${x.hasClusterScopedResources ? ' · cluster scoped' : ''}`,
        ...(x.evaluated < FEW_EVALUATED ? [`fewer than ${FEW_EVALUATED} evaluated: the share is shaky`] : []),
      ],
    };
  }

  private share(x: HomeFailingCheckDto): number {
    return x.evaluated > 0 ? Math.min(1, x.resources / x.evaluated) : 0;
  }

  private keyOf(x: HomeFailingCheckDto): string {
    return `${x.family}|${x.checkId}`;
  }

  private cut(text: string, maxChars: number): string {
    return text.length <= maxChars ? text : `${text.slice(0, maxChars - 1)}…`;
  }
}
