import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { HomeHistoryDriftDto } from '@api/models/home-history-drift-dto';
import { HomeNewImagesDto } from '@api/models/home-new-images-dto';
import { HomeService } from '@api/services/home.service';
import { LaneStripComponent } from '@shared/ui/lane-strip/lane-strip.component';
import { LaneStripCellClick, LaneStripLane, LaneStripSelection } from '@shared/ui/lane-strip/lane-strip.types';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeDaysResult, HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';
import { ChartTipDirective } from '@shared/ui/popover/chart-tip.directive';

// the lanes, in order; the widget the lane opens on its day is the one of the same key
export type SinceLastVisitLane = 'vulnerabilities' | 'fixes' | 'exploited' | 'drift' | 'images';

export interface SinceLastVisitClick {
  lane: SinceLastVisitLane;
  // "yyyy-MM-dd" (UTC)
  day: string;
}

const LANES: { key: SinceLastVisitLane; caption: string }[] = [
  { key: 'vulnerabilities', caption: 'New vulns' },
  { key: 'fixes', caption: 'New fixes' },
  { key: 'exploited', caption: 'Newly exploited' },
  { key: 'drift', caption: 'Drift' },
  { key: 'images', caption: 'New images' },
];

// Since your last visit: one strip for the "new" kinds of the home page (always collapsed): a lane per kind, a square
// per UTC day, the whole history range (as the days endpoints give it, the retention), not a window
// - the days of History drift and New images are not asked again: the dashboard hands over the answers of the two widgets
//   (one call of each endpoint per page); newly exploited is made of them, see the help
// - new vulns / new fixes / newly exploited: the summary endpoints (all per day, not limited)
// - the teal corner is a day the home page was opened (as in the heatmaps), the orange one a day with known exploited
// a click on a square opens the widget of its lane on that day (the dashboard does it, see laneClicked)
@Component({
  selector: 'app-home-since-last-visit',
  standalone: true,
  imports: [HomeWidgetFrameComponent, LaneStripComponent, TrivyBoxComponent, ChartTipDirective],
  templateUrl: './home-since-last-visit.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block min-w-0' },
})
export class HomeSinceLastVisitComponent extends HomeWidgetBase {
  protected readonly help: HomeWidgetHelp = {
    question: `What <strong>changed in my cluster</strong>, day by day? Spot the busy days and what came with them.`,
    goodToKnow: [
      `One lane per kind, one square per <strong>UTC day</strong>: new vulnerabilities (by first seen), new fixes (by the day the fix appeared), drift, new images, newly exploited.`,
      `The <strong>teal corner</strong> is a day you opened the home page, the <strong>orange corner</strong> a day with a known exploited (KEV / EUVD) vulnerability.`,
    ],
    trickyBits: [
      `The lanes <strong>do not add up</strong>. A new image that brings vulnerabilities other images already have is in <strong>new images</strong>, not in new vulnerabilities.`,
      `A vulnerability added to a running image is in <strong>drift</strong>, even when another image already had it; it is not in new vulnerabilities.`,
      `One new vulnerability on three images is three images in drift or new images, but <strong>one</strong> in new vulnerabilities.`,
      `Cluster reports have no image digest: never in drift or new images, but they can be in <strong>new vulnerabilities</strong>.`,
    ],
    tips: [
      `<strong>Click a square</strong> to open the widget of that lane on that day.`,
    ],
  };

  private readonly homeService = inject(HomeService);

  hint = input<string>('');
  namespaces = input.required<string[]>();
  severities = input.required<number[]>();
  // "yyyy-MM-dd"
  today = input.required<string>();
  // the days the home page was opened
  visitDays = input<string[]>([]);
  // the previous visit day ("yyyy-MM-dd"); none = no earlier visit
  lastVisitDay = input<string | undefined>(undefined);
  // the answers of the History drift and the New images widgets (undefined: the widget is not there yet)
  drift = input<HomeDaysResult<HomeHistoryDriftDto> | undefined>(undefined);
  images = input<HomeDaysResult<HomeNewImagesDto> | undefined>(undefined);
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  laneClicked = output<SinceLastVisitClick>();
  // the refresh button: the strip reloads its own data, and asks for the widgets whose days it shows to reload theirs
  sourcesRefreshRequested = output<void>();

  protected readonly selected = signal<LaneStripSelection | null>(null);

  // every day of the history range: from the first day any of the endpoints starts at, to today
  protected readonly columns = computed(() => {
    const first = [
      this.drift()?.dto?.from,
      this.images()?.dto?.from,
      this.newVulnerabilities.value()?.from,
      this.newFixes.value()?.from,
      this.knownExploited.value()?.from,
    ]
      .filter((x): x is string => !!x)
      .sort()[0];
    if (!first) {
      return [];
    }

    const [year, month, day] = first.split('-').map(Number);
    const start = Date.UTC(year, month - 1, day);
    const count = Math.round((Date.parse(this.today()) - start) / 86_400_000) + 1;

    return Array.from({ length: Math.max(0, count) }, (_, i) =>
      new Date(start + i * 86_400_000).toISOString().substring(0, 10),
    );
  });

  private readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  private readonly newVulnerabilities = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeNewVulnerabilitiesSummary({
        severities: params.severities,
        namespaces: params.namespaces,
      }),
  });

  // the known exploited ids by the day they became known exploited (the moment of the "new" of the known exploited widget)
  private readonly knownExploited = rxResource({
    params: this.scope,
    stream: ({ params }) => this.homeService.getHomeKnownExploitedSummary({ namespaces: params.namespaces }),
  });

  private readonly newFixes = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeNewFixesSummary({
        severities: params.severities,
        namespaces: params.namespaces,
      }),
  });

  protected readonly lanes = computed<LaneStripLane[]>(() => {
    const columns = this.columns();
    const perDay = (source: { day: string; value: number }[]): number[] => {
      const byDay = new Map<string, number>();
      source.forEach((x) => byDay.set(x.day, (byDay.get(x.day) ?? 0) + x.value));

      return columns.map((day) => byDay.get(day) ?? 0);
    };
    // the indexes of the columns that are one of the days
    const indexesOf = (days: string[]) => columns.flatMap((day, i) => (days.includes(day) ? [i] : []));

    const driftDays = this.drift()?.dto?.days ?? [];
    const imageDays = this.images()?.dto?.days ?? [];
    const visits = indexesOf(this.visitDays());

    const values: Record<SinceLastVisitLane, number[]> = {
      vulnerabilities: perDay(
        (this.newVulnerabilities.value()?.days ?? []).map((d) => ({ day: d.day, value: d.activeVulnerabilities })),
      ),
      fixes: perDay((this.newFixes.value()?.days ?? []).map((d) => ({ day: d.day, value: d.activeVulnerabilities }))),
      exploited: perDay((this.knownExploited.value()?.days ?? []).map((d) => ({ day: d.day, value: d.vulnerabilityIds }))),
      drift: perDay(driftDays.map((d) => ({ day: d.day, value: d.added }))),
      images: perDay(imageDays.map((d) => ({ day: d.day, value: d.images }))),
    };

    const flagged: Partial<Record<SinceLastVisitLane, number[]>> = {
      drift: indexesOf(driftDays.filter((d) => d.addedKnownExploited > 0).map((d) => d.day)),
      images: indexesOf(imageDays.filter((d) => d.knownExploited > 0).map((d) => d.day)),
    };

    return LANES.map((lane) => ({
      caption: lane.caption,
      values: values[lane.key],
      emphasis1: visits,
      emphasis2: flagged[lane.key] ?? [],
    }));
  });

  protected readonly state = computed<HomeWidgetState>(() => {
    const ownError = this.newVulnerabilities.error() ?? this.newFixes.error() ?? this.knownExploited.error();
    const error =
      this.drift()?.error ?? this.images()?.error ?? (ownError ? ((ownError as Error)?.message ?? 'request failed') : undefined);
    if (error) {
      return { error };
    }

    const drift = this.drift()?.dto;
    const images = this.images()?.dto;
    const vulnerabilities = this.newVulnerabilities.value();
    const fixes = this.newFixes.value();
    const exploited = this.knownExploited.value();
    if (!drift || !images || !vulnerabilities || !fixes || !exploited) {
      return {};
    }

    // each lane is as available as its endpoint: an unavailable one shows no squares
    const unavailable = [drift, images, vulnerabilities, fixes, exploited]
      .map((x) => x.unavailableReason)
      .filter((x): x is string => !!x);

    return {
      isAvailable: true,
      summary:
        this.lanes()
          .map((lane) => `${lane.caption}: ${lane.values.reduce((sum, v) => sum + v, 0)}`)
          .join(', ') + (unavailable.length > 0 ? ` (unavailable: ${[...new Set(unavailable)].join('; ')})` : ''),
    };
  });

  protected readonly lastVisitLabel = computed(() => {
    const lastVisit = this.lastVisitDay();
    if (!lastVisit) {
      return 'no earlier visit';
    }
    const days = Math.round((Date.parse(this.today()) - Date.parse(lastVisit)) / 86_400_000);

    return `previous visit ${days === 1 ? 'yesterday' : `${days} days ago`}`;
  });

  protected onRefresh(): void {
    this.refreshWidget();
    this.sourcesRefreshRequested.emit();
  }

  protected onCell(event: LaneStripCellClick): void {
    // the strip only opens another widget: no square stays selected
    this.selected.set(null);
    this.laneClicked.emit({ lane: LANES[event.laneIndex].key, day: event.column });
  }
}
