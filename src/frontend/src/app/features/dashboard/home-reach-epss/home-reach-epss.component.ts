import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal, signal, untracked } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { StepSelectComponent } from '@shared/ui/step-select/step-select.component';
import { ScrollerModule } from 'primeng/scroller';
import { TagModule } from 'primeng/tag';

import { HomeReachEpssItemDto } from '@api/models/home-reach-epss-item-dto';
import { HomeService } from '@api/services/home.service';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { ScrollerResizeDirective } from '@shared/ui/scroller-resize/scroller-resize.directive';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { VulnerabilityTagSource } from '@shared/ui/trivy-table/trivy-table.types';
import { VulnerabilityTagsPipe } from '@shared/ui/trivy-table/vulnerability-tags.pipe';
import { VulnerabilityScoreTagComponent } from '@shared/ui/vulnerability-score-tag/vulnerability-score-tag.component';
import { ScatterSelectComponent } from '@shared/ui/scatter-select/scatter-select.component';
import {
  ScatterAxis,
  ScatterLegendItem,
  ScatterPoint,
  ScatterSelection,
} from '@shared/ui/scatter-select/scatter-select.types';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { HomeListSort, HomeListSortKey, nextSort, sortIconOf, sortRows, sortTitleOf } from '../home-list-sort/home-list-sort';
import { HomeExploitTagComponent } from '../home-pivot/home-exploit-tag.component';
import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// the ids listed when nothing is selected on the plot: the top by risk
const TOP_COUNT = 30;

// the widest sideways move of a point, as a factor (log axis): 0.84 .. 1.2
const JITTER = 0.18;

// a factor between exp(-JITTER) and exp(JITTER) from the id (a string hash), so a reload does not move the points
function jitter(id: string): number {
  let hash = 2166136261;
  for (let i = 0; i < id.length; i++) {
    hash = Math.imul(hash ^ id.charCodeAt(i), 16777619);
  }
  return Math.exp(((((hash >>> 0) % 10000) / 10000) * 2 - 1) * JITTER);
}

// Reach vs EPSS: one point per vulnerability id, the images that have it (x, log) against its EPSS percentile (y);
// the color is the severity, a cross (with a lighter dot in the middle) is a known exploited id. Drag on the plot to select a region: the top right is
// likely to be exploited and widespread.
// - collapsed: the plot and what is selected
// - expanded: the plot, and on the right the ids (the selected ones, else the top by EPSS x images)
//   a click on an id (or on a point) shows its detail (title, KEV, EPSS, the packages with their fix) under the list
@Component({
  selector: 'app-home-reach-epss',
  standalone: true,
  imports: [
    DecimalPipe,
    HomeExploitTagComponent,
    HomeWidgetFrameComponent,
    PillButtonComponent,
    ScatterSelectComponent,
    ScrollerModule,
    ScrollerResizeDirective,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
    StepSelectComponent,
    TagModule,
    TrivyBoxComponent,
    VulnerabilityScoreTagComponent,
  ],
  templateUrl: './home-reach-epss.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeReachEpssComponent extends HomeWidgetBase {
  // the most the server allows (without a limit it sends 20); it keeps the highest EPSS first
  private static readonly limit = 500;

  private readonly homeService = inject(HomeService);
  private readonly mainAppInitService = inject(MainAppInitService);
  private readonly cveEnrichmentState = inject(CveEnrichmentStateService);
  // the tags of the vulnerability id cell of the tables: here the labels of its EPSS and exploited ones
  private readonly vulnerabilityTags = new VulnerabilityTagsPipe();

  // the height of a row of the list (the virtual scroller needs all the rows to have the same one)
  protected readonly rowHeight = 64;

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help: HomeWidgetHelp = {
    question: `Which vulnerabilities are both <strong>likely to be exploited</strong> and <strong>widespread</strong>? That is where to start.`,
    oddities: [
      `One point per vulnerability id: to the right, more images (log scale); up, a higher <strong>EPSS percentile</strong> (more likely to be exploited).`,
      `The color is the severity; a <strong>cross</strong> is a known exploited id (KEV / EUVD).`,
      `Only the ids with an EPSS score are on the plot.`,
    ],
    expanded: [
      `On the right the selected ids (with no selection, the top ${TOP_COUNT} by EPSS times images). Click an id for its title, KEV addition, EPSS and the packages with the fix.`,
      `<strong>Include top</strong> keeps only the ids in that top share of the EPSS percentiles (default: the top 20%).`,
    ],
    tips: [
      `The <strong>top right</strong> is where to look first.`,
      `<strong>Drag</strong> on the plot to select a region (shift adds one, a click clears); click a point to expand on it.`,
    ],
  };

  constructor() {
    super();

    this.cveEnrichmentState.ensureLoaded();

    // a collapse ends a deselect: the next expand has the first id selected again
    effect(() => {
      if (!this.isExpanded()) {
        untracked(() => this.deselected.set(false));
      }
    });
  }

  hint = input<string>('');
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  // the share of the top EPSS percentiles to include (20..100, the line); the server gets it as a floor on the
  // percentile (top 20% = 0.8 and up; top 100% = no floor)
  protected readonly topShare = signal(20);
  protected readonly percent = (value: number): string => `${value}%`;
  private readonly minEpss = computed(() => 1 - this.topShare() / 100);

  private readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    minEpss: this.minEpss(),
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  private readonly data = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeReachEpss({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeReachEpssComponent.limit,
        minEpss: params.minEpss || undefined,
      }),
  });

  // below 1 so the points of the ids with one image, spread sideways, stay on the plot
  protected readonly xAxis: ScatterAxis = { label: 'images (log)', scale: 'log', min: 0.7 };
  // follows the share: the floor of the percentile (what the server returns) with 10% under it, clamped at 0; the top has
  // 10% of room above 100% (its tick is not labelled: there is no percentile over 100%)
  protected readonly yAxis = computed<ScatterAxis>(() => ({
    label: 'EPSS',
    min: Math.max(0, 1 - this.topShare() / 100 - 0.1),
    max: 1.1,
    format: (value) => (value > 1.001 ? '' : `${Math.round(value * 100)}%`),
  }));
  protected readonly legend: ScatterLegendItem[] = [
    {
      label: 'known exploited',
      shape: 'cross',
      color: 'var(--p-text-muted-color)',
      hint: 'In a known exploited list (KEV or EUVD)',
    },
    { label: 'not known exploited', shape: 'circle', color: 'var(--p-text-muted-color)' },
  ];

  // the answer on screen: the last one that arrived, kept while the next one is asked (the points are not taken away and
  // brought back on a reload)
  private readonly answer = linkedSignal<ReturnType<typeof this.data.value>, ReturnType<typeof this.data.value>>({
    source: this.data.value,
    computation: (value, previous) => value ?? previous?.value,
  });

  private readonly items = computed(() => this.answer()?.items ?? []);

  protected readonly points = computed<ScatterPoint<HomeReachEpssItemDto>[]>(() =>
    // the known exploited ids last: they are drawn on top of the others
    [...this.items()]
      .sort((a, b) => Number(a.isKnownExploited) - Number(b.isKnownExploited))
      .map((item) => ({
        // the images are whole numbers, so many ids have the same x: each is moved a little sideways (always the same
        // way for the same id); the popover shows the real number
        x: item.images * jitter(item.vulnerabilityId),
        y: item.epssPercentile,
        color: SeverityUtils.getCssColor(item.severityId),
        shape: item.isKnownExploited ? 'cross' : 'circle',
        // a known exploited id: a lighter dot of its color in the middle of the cross
        centerColor: item.isKnownExploited ? SeverityUtils.getCssColorLight(item.severityId) : undefined,
        // the identity across answers: a point that is still there stays (and moves), a new one fades in, a gone one fades out
        id: item.vulnerabilityId,
        label: item.vulnerabilityId,
        data: item,
      })),
  );

  // the ids inside the drawn region(s)
  protected readonly selected = signal<HomeReachEpssItemDto[]>([]);

  protected onSelection(selection: ScatterSelection<HomeReachEpssItemDto>): void {
    this.selected.set(selection.points.map((p) => p.data!));
  }

  // the ids of the list: the selected ones, else the top by risk; the most likely to be exploited and widespread first
  protected readonly listIsSelection = computed(() => this.selected().length > 0);
  protected readonly list = computed<HomeReachEpssItemDto[]>(() => {
    const source = this.listIsSelection() ? this.selected() : this.items();
    const sorted = [...source].sort(
      (a, b) => b.epssPercentile * b.images - a.epssPercentile * a.images || b.epssPercentile - a.epssPercentile,
    );
    const shown = this.listIsSelection() ? sorted : sorted.slice(0, TOP_COUNT);

    // the id picked on the plot is always in the list (at the top when it would not be there)
    const picked = this.items().find((x) => x.vulnerabilityId === this.pickedId());
    return picked && !shown.includes(picked) ? [picked, ...shown] : shown;
  });

  // the sort of the list (null: the most likely to be exploited and widespread first); the pills go natural direction ->
  // reversed -> off; the rows have no moment
  protected readonly sort = signal<HomeListSort | null>(null);
  protected readonly sortedList = computed(() =>
    sortRows(this.list(), this.sort(), {
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

  protected readonly knownExploitedCount = computed(() => this.list().filter((x) => x.isKnownExploited).length);

  // the id of the detail: the one clicked if it is in the list, else the first; none after a deselect (a click on the
  // selected row, or on the picked point again) until the next click, or the next expand
  protected readonly pickedId = signal<string | null>(null);
  private readonly deselected = signal(false);
  protected readonly active = computed<HomeReachEpssItemDto | undefined>(() =>
    this.deselected()
      ? undefined
      : (this.list().find((x) => x.vulnerabilityId === this.pickedId()) ?? this.list()[0]),
  );

  protected pick(item: HomeReachEpssItemDto): void {
    if (this.active()?.vulnerabilityId === item.vulnerabilityId) {
      this.deselected.set(true);
      return;
    }

    this.deselected.set(false);
    this.pickedId.set(item.vulnerabilityId);
  }

  // the point picked on the plot (null: cleared): the widget is expanded (when it is not) with that id selected in the list
  protected readonly pickedIndex = computed(() => {
    const index = this.points().findIndex((p) => p.data?.vulnerabilityId === this.pickedId());
    return this.pickedId() === null || this.deselected() || index < 0 ? null : index;
  });

  protected onPointPicked(index: number | null): void {
    const item = index === null ? undefined : this.points()[index]?.data;
    this.pickedId.set(item?.vulnerabilityId ?? null);
    if (item) {
      this.deselected.set(false);
      this.expand();
    } else if (this.isExpanded()) {
      this.deselected.set(true);
    }
  }

  // the detail of the active id; idle while collapsed
  private readonly detailData = rxResource({
    params: () => {
      const active = this.active();
      return this.isExpanded() && active
        ? { vulnerabilityId: active.vulnerabilityId, namespaces: this.scope().namespaces }
        : undefined;
    },
    stream: ({ params }) =>
      this.homeService.getHomeWhereIsVulnerabilityDetailed({
        vulnerabilityId: params.vulnerabilityId,
        namespaces: params.namespaces,
      }),
  });

  protected readonly detail = computed(() => {
    const dto = this.detailData.hasValue() ? this.detailData.value() : undefined;
    return dto && dto.vulnerabilityId === this.active()?.vulnerabilityId ? dto.item : undefined;
  });
  protected readonly detailLoading = computed(() => this.detailData.isLoading());
  protected readonly detailError = computed(() => (this.detailData.error() as Error | undefined)?.message);

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

  protected severityColor(severityId: number): string {
    return SeverityUtils.getCssColor(severityId);
  }

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
      summary: `${dto.items.length} vulnerability ids`,
    };
  });
}
