import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { StepSelectComponent } from '@shared/ui/step-select/step-select.component';
import { TagModule } from 'primeng/tag';

import { HomeReachEpssItemDto } from '@api/models/home-reach-epss-item-dto';
import { HomeService } from '@api/services/home.service';
import { ScatterSelectComponent } from '@shared/ui/scatter-select/scatter-select.component';
import {
  ScatterAxis,
  ScatterLegendItem,
  ScatterPoint,
  ScatterSelection,
} from '@shared/ui/scatter-select/scatter-select.types';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
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
  imports: [HomeWidgetFrameComponent, ScatterSelectComponent, TagModule, DecimalPipe, StepSelectComponent],
  templateUrl: './home-reach-epss.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeReachEpssComponent extends HomeWidgetBase {
  // the most the server allows (without a limit it sends 20); it keeps the highest EPSS first
  private static readonly limit = 500;

  private readonly homeService = inject(HomeService);

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help = `
    <p class="mb-2">Every <strong>vulnerability id</strong> of the active vulnerabilities of the running images (in the <strong>severities</strong> and <strong>namespaces</strong> of the filter bar) as a point: the <strong>images</strong> that have it (to the right: more widespread, <strong>log scale</strong>) against its <strong>EPSS percentile</strong> (up: more likely to be exploited). The color is the <strong>severity</strong>; a <strong>cross</strong> is a <strong>known exploited</strong> id (KEV / EUVD).</p>
    <p class="mb-2">The <strong>top right</strong> is where to look first: likely to be exploited and widespread. <strong>Drag</strong> on the plot to select a region (shift adds one, a click clears).</p>
    <p class="mb-2"><strong>Expanded</strong>: on the right the selected ids (without a selection, the top ${TOP_COUNT} by EPSS times images). A <strong>click on a point</strong> (collapsed too) picks it: the widget expands with that id selected in the list, and the point gets a border. A click on an id of the list shows its title, when it was added to the KEV, its EPSS and the packages that have it, with the fix.</p>
    <p class="mb-2">The <strong>Include top</strong> line (steps of 10%, from 20%) keeps only the ids in that top share of the EPSS percentiles: top 20% is the ids at the 80th percentile and above, top 100% is all of them (the server filters; the default is the top 20%).</p>
    <p class="mb-0 tod-text-muted-color">Only the ids with an EPSS score are on the plot. The percentile is the share of all scored ids with an equal or lower score.</p>
  `;

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
  protected readonly yAxis: ScatterAxis = {
    label: 'EPSS',
    min: 0,
    max: 1,
    format: (value) => `${Math.round(value * 100)}%`,
  };
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

  protected readonly knownExploitedCount = computed(() => this.list().filter((x) => x.isKnownExploited).length);

  // the id of the detail: the one clicked if it is in the list, else the first
  protected readonly pickedId = signal<string | null>(null);
  protected readonly active = computed<HomeReachEpssItemDto | undefined>(
    () => this.list().find((x) => x.vulnerabilityId === this.pickedId()) ?? this.list()[0],
  );

  protected pick(item: HomeReachEpssItemDto): void {
    this.pickedId.set(item.vulnerabilityId);
  }

  // the point picked on the plot (null: cleared): the widget is expanded (when it is not) with that id selected in the list
  protected readonly pickedIndex = computed(() => {
    const index = this.points().findIndex((p) => p.data?.vulnerabilityId === this.pickedId());
    return this.pickedId() === null || index < 0 ? null : index;
  });

  protected onPointPicked(index: number | null): void {
    const item = index === null ? undefined : this.points()[index]?.data;
    this.pickedId.set(item?.vulnerabilityId ?? null);
    if (item) {
      this.expand();
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
