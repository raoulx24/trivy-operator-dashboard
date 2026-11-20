import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

import { ActivityHeatmapComponent } from '@shared/ui/activity-heatmap/activity-heatmap.component';
import { ActivityHeatmapDayDto } from '@shared/ui/activity-heatmap/activity-heatmap.types';
import { EolRunwayComponent } from '@shared/ui/eol-runway/eol-runway.component';
import { EolRunwayItem } from '@shared/ui/eol-runway/eol-runway.types';
import { LaneStripComponent } from '@shared/ui/lane-strip/lane-strip.component';
import { LaneStripLane, LaneStripSelection } from '@shared/ui/lane-strip/lane-strip.types';
import { MatrixHeatmapComponent } from '@shared/ui/matrix-heatmap/matrix-heatmap.component';
import { MeterGroupComponent } from '@shared/ui/meter-group/meter-group.component';
import { MeterGroupSegment } from '@shared/ui/meter-group/meter-group.types';
import { RingGaugeComponent } from '@shared/ui/ring-gauge/ring-gauge.component';
import { RingGaugeData } from '@shared/ui/ring-gauge/ring-gauge.types';
import { ScatterSelectComponent } from '@shared/ui/scatter-select/scatter-select.component';
import { ScatterAxis, ScatterLegendItem, ScatterPoint } from '@shared/ui/scatter-select/scatter-select.types';
import { SpanTimelineComponent } from '@shared/ui/span-timeline/span-timeline.component';
import { SpanTimelineLegendItem, SpanTimelineRow } from '@shared/ui/span-timeline/span-timeline.types';
import { StackedBarChartComponent } from '@shared/ui/stacked-bar-chart/stacked-bar-chart.component';
import { StackedBarColumn, StackedBarSeries } from '@shared/ui/stacked-bar-chart/stacked-bar-chart.types';

const DAY_MS = 24 * 60 * 60 * 1000;

const SEVERITY_COLORS = ['#c0392b', '#e67e22', '#f1c40f', '#3b82c4'];

// a small deterministic generator, so the fake data is the same on every visit
function seeded(seed: number): () => number {
  let state = seed;

  return () => {
    state = (state * 1664525 + 1013904223) % 4294967296;

    return state / 4294967296;
  };
}

function isoDay(offsetDays: number): string {
  return new Date(Date.now() + offsetDays * DAY_MS).toISOString().substring(0, 10);
}

function shortDay(offsetDays: number): string {
  return isoDay(offsetDays).substring(5);
}

// the showcase of the shared ui components (fake data), in alphabetical order; a component that has two looks shows both
// in its box
@Component({
  selector: 'app-tests',
  imports: [
    ActivityHeatmapComponent,
    EolRunwayComponent,
    LaneStripComponent,
    MatrixHeatmapComponent,
    MeterGroupComponent,
    RingGaugeComponent,
    ScatterSelectComponent,
    SpanTimelineComponent,
    StackedBarChartComponent,
  ],
  templateUrl: './tests.component.html',
  styleUrl: './tests.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TestsComponent {
  // activity-heatmap
  protected readonly activityData: ActivityHeatmapDayDto[] = (() => {
    const random = seeded(7);

    return Array.from({ length: 150 }, (_, i) => ({
      day: isoDay(i - 149),
      value: random() < 0.3 ? 0 : Math.ceil(random() * 12),
    }));
  })();
  protected readonly activityEmphasis1 = [isoDay(-3), isoDay(-20), isoDay(-45)];
  protected readonly activityEmphasis2 = [isoDay(-1), isoDay(-60)];
  protected readonly activitySelectedDay = signal<string | null>(isoDay(-20));

  // eol-runway
  protected readonly today = isoDay(0);
  protected readonly eolItems: EolRunwayItem[] = [
    { label: 'Debian 10', eolFrom: isoDay(-400), eoasFrom: isoDay(-700), images: 3 },
    { label: 'Ubuntu 20.04', eolFrom: isoDay(-60), eoasFrom: isoDay(-300), images: 12 },
    { label: 'Alpine 3.17', eolFrom: isoDay(25), images: 5 },
    { label: 'Debian 11', eolFrom: isoDay(300), eoasFrom: isoDay(120), images: 21 },
    { label: 'Ubuntu 22.04', eolFrom: isoDay(500), eoasFrom: isoDay(200), images: 34 },
    // beyond the window of the runway (3 years after today by default), so its bar overflows
    { label: 'Alpine 3.20', eolFrom: '2032-06-30', images: 8 },
  ];

  // lane-strip
  protected readonly laneColumns = Array.from({ length: 14 }, (_, i) => isoDay(i - 13));
  protected readonly laneColors = SEVERITY_COLORS.slice(0, 3);
  protected readonly lanes: LaneStripLane[] = (() => {
    const random = seeded(11);

    return ['Critical', 'High', 'Medium'].map((caption, lane) => ({
      caption,
      values: this.laneColumns.map(() => (random() < 0.35 ? 0 : Math.ceil(random() * (12 - lane * 3)))),
      // the last visit was 4 days ago; the exploited ones are a few scattered days
      emphasis1: [10, 11, 12, 13],
      emphasis2: lane === 0 ? [3, 11] : lane === 1 ? [6] : [],
    }));
  })();
  protected readonly laneSelected = signal<LaneStripSelection | null>(null);

  // matrix-heatmap
  protected readonly matrixRows = ['Critical', 'High', 'Medium', 'Low'];
  protected readonly matrixColumns = ['prod', 'stage', 'dev', 'infra', 'tools'];
  protected readonly matrixValues: (number | null)[][] = (() => {
    const random = seeded(23);

    return this.matrixRows.map((_, row) =>
      this.matrixColumns.map(() => (random() < 0.15 ? null : Math.round(random() * (40 - row * 8)))),
    );
  })();

  // meter-group
  protected readonly meterMix: MeterGroupSegment[] = [
    { label: 'Critical', value: 12, color: SEVERITY_COLORS[0], textColor: '#fff' },
    { label: 'High', value: 41, color: SEVERITY_COLORS[1], textColor: '#fff' },
    { label: 'Medium', value: 76, color: SEVERITY_COLORS[2], textColor: '#222' },
    { label: 'Low', value: 25, color: SEVERITY_COLORS[3], textColor: '#fff' },
  ];
  protected readonly meterPercent: MeterGroupSegment[] = [
    { label: 'Fixed', value: 38, color: '#2e9d5b', textColor: '#fff' },
    { label: 'Fix available', value: 24, color: '#3b82c4', textColor: '#fff' },
  ];

  // ring-gauge
  protected readonly rings: RingGaugeData[] = [
    { id: 'fixable', value: 72, color: '#2e9d5b', label: 'Fixable' },
    { id: 'exposed', value: 35, color: '#e67e22', label: 'Exposed' },
    { id: 'critical', value: 8, color: '#c0392b', label: 'Critical', centerText: '8/100' },
  ];
  protected readonly ringSelected = signal<string | undefined>('exposed');

  // scatter-select
  protected readonly scatterPoints: ScatterPoint[] = (() => {
    const random = seeded(31);
    const groups = [
      { color: '#c0392b', shape: 'circle' as const },
      { color: '#e67e22', shape: 'diamond' as const },
      { color: '#3b82c4', shape: 'square' as const },
    ];

    return Array.from({ length: 90 }, (_, i) => {
      const group = groups[i % groups.length];

      return {
        x: Math.round(1 + random() * random() * 900),
        y: Math.round((2 + random() * 8) * 10) / 10,
        color: group.color,
        shape: group.shape,
        label: `CVE-2025-${1000 + i}`,
      };
    });
  })();
  protected readonly scatterLegend: ScatterLegendItem[] = [
    { label: 'Backend', color: '#c0392b', shape: 'circle' },
    { label: 'Frontend', color: '#e67e22', shape: 'diamond' },
    { label: 'Infra', color: '#3b82c4', shape: 'square' },
  ];
  protected readonly scatterXLinear: ScatterAxis = { label: 'Age (days)', min: 0 };
  protected readonly scatterXLog: ScatterAxis = { label: 'Age (days, log)', scale: 'log' };
  protected readonly scatterY: ScatterAxis = { label: 'CVSS', min: 0, max: 10 };

  // span-timeline
  protected readonly spanRows: SpanTimelineRow[] = [
    {
      id: 'a',
      label: 'nginx:1.25',
      start: 420,
      end: 0,
      valueLabel: '420d',
      details: ['Running since the first rollout'],
      markers: [{ at: 300, kind: 'diamond', label: 'Fix available' }],
    },
    { id: 'b', label: 'redis:7.0', start: 240, end: 90, valueLabel: '150d', markers: [{ at: 120, kind: 'dot', label: 'Rebuilt' }] },
    { id: 'c', label: 'postgres:15', start: 150, end: 0, valueLabel: '150d' },
    { id: 'd', label: 'trivy-operator', start: 60, end: 10, valueLabel: '50d', markers: [{ at: 30, kind: 'tick', label: 'Scan' }] },
    { id: 'e', label: 'busybox:1.36', start: 8, valueLabel: '8d' },
  ];
  protected readonly spanLegend: SpanTimelineLegendItem[] = [
    { kind: 'bar', label: 'Running', description: 'The time an image ran' },
    { kind: 'diamond', label: 'Fix available' },
    { kind: 'dot', label: 'Rebuilt' },
    { kind: 'tick', label: 'Scan' },
  ];
  protected readonly spanColors = ['#3b82c4', '#2e9d5b', '#e67e22'];

  // stacked-bar-chart
  protected readonly stackedSeries: StackedBarSeries[] = [
    { name: 'Critical', color: SEVERITY_COLORS[0] },
    { name: 'High', color: SEVERITY_COLORS[1] },
    { name: 'Medium', color: SEVERITY_COLORS[2] },
  ];
  protected readonly stackedColumns: StackedBarColumn[] = (() => {
    const random = seeded(47);

    return Array.from({ length: 14 }, (_, i) => ({
      label: shortDay(i - 13),
      values: [Math.round(random() * 4), Math.round(random() * 9), Math.round(random() * 14)],
    }));
  })();
}
