import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

import { ActivityHeatmapComponent } from '@shared/ui/activity-heatmap/activity-heatmap.component';
import { ActivityHeatmapDayDto } from '@shared/ui/activity-heatmap/activity-heatmap.types';
import { EolRunwayComponent } from '@shared/ui/eol-runway/eol-runway.component';
import { EolRunwayItem } from '@shared/ui/eol-runway/eol-runway.types';
import { LaneStripComponent } from '@shared/ui/lane-strip/lane-strip.component';
import { LaneStripLane, LaneStripSelection } from '@shared/ui/lane-strip/lane-strip.types';
import { LayeredGraphComponent } from '@shared/ui/layered-graph/layered-graph.component';
import { LayeredGraphColumn, LayeredGraphEdge, LayeredGraphNode } from '@shared/ui/layered-graph/layered-graph.types';
import { MatrixHeatmapComponent } from '@shared/ui/matrix-heatmap/matrix-heatmap.component';
import { MatrixHeatmapRowDto } from '@shared/ui/matrix-heatmap/matrix-heatmap.types';
import { NetworkFillComponent } from '@shared/ui/network-fill/network-fill.component';
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

// enough days for the lane strip to scroll in its box
const LANE_DAYS = 60;

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
    LayeredGraphComponent,
    MatrixHeatmapComponent,
    MeterGroupComponent,
    NetworkFillComponent,
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
  // the seed of the network fill of each box
  protected readonly fillSeed = (name: string) => `playground ${name}`;

  // activity-heatmap
  private activitySeed = 7;
  protected readonly activityData = signal<ActivityHeatmapDayDto[]>(this.buildActivity(this.activitySeed));

  // new data for the heatmap (it lights the cells on again)
  protected reloadActivity(): void {
    this.activityData.set(this.buildActivity(++this.activitySeed));
  }

  private buildActivity(seed: number): ActivityHeatmapDayDto[] {
    const random = seeded(seed);
    const emphasis1 = [isoDay(-3), isoDay(-20), isoDay(-45)];
    const emphasis2 = [isoDay(-1), isoDay(-60)];

    return Array.from({ length: 150 }, (_, i) => {
      const day = isoDay(i - 149);
      const value = random() < 0.3 ? 0 : Math.ceil(random() * 12);
      // the days with a value can be clicked
      return { day, value, emphasis1: emphasis1.includes(day), emphasis2: emphasis2.includes(day), selectable: value > 0 };
    });
  }
  protected readonly activitySelectedDay = signal<string | null>(isoDay(-20));

  // eol-runway
  protected readonly today = isoDay(0);
  private eolImages = 0;
  protected readonly eolItems = signal<EolRunwayItem[]>(this.buildEol());

  // new data for the runway (the bars grow again)
  protected reloadEol(): void {
    this.eolItems.set(this.buildEol(++this.eolImages));
  }

  private buildEol(extra = 0): EolRunwayItem[] {
    return [
    { label: 'Debian 10', eolFrom: isoDay(-400), eoasFrom: isoDay(-700), images: 3 },
    { label: 'Ubuntu 20.04', eolFrom: isoDay(-60), eoasFrom: isoDay(-300), images: 12 },
    { label: 'Alpine 3.17', eolFrom: isoDay(25), images: 5 },
    { label: 'Debian 11', eolFrom: isoDay(300), eoasFrom: isoDay(120), images: 21 },
    { label: 'Ubuntu 22.04', eolFrom: isoDay(500), eoasFrom: isoDay(200), images: 34 },
    // beyond the window of the runway (3 years after today by default), so its bar overflows
    { label: 'Alpine 3.20', eolFrom: '2032-06-30', images: 8 },
    ].map((x) => ({ ...x, images: x.images + extra }));
  }

  // lane-strip
  protected readonly laneColumns = Array.from({ length: LANE_DAYS }, (_, i) => isoDay(i - (LANE_DAYS - 1)));
  private laneSeed = 11;
  protected readonly lanes = signal<LaneStripLane[]>(this.buildLanes(this.laneSeed));

  // new data for the lane strip (it lights the cells on again)
  protected reloadLanes(): void {
    this.lanes.set(this.buildLanes(++this.laneSeed));
  }

  private buildLanes(seed: number): LaneStripLane[] {
    const random = seeded(seed);

    return ['Critical', 'High', 'Medium'].map((caption, lane) => ({
      caption,
      values: this.laneColumns.map(() => (random() < 0.35 ? 0 : Math.ceil(random() * (12 - lane * 3)))),
      // the last visit was 4 days ago; the exploited ones are a few scattered days
      emphasis1: [LANE_DAYS - 4, LANE_DAYS - 3, LANE_DAYS - 2, LANE_DAYS - 1],
      emphasis2: lane === 0 ? [LANE_DAYS - 40, LANE_DAYS - 3] : lane === 1 ? [LANE_DAYS - 25] : [],
    }));
  }
  protected readonly laneSelected = signal<LaneStripSelection | null>(null);

  // layered-graph: package -> image -> namespace (the value of a namespace is its workloads)
  protected readonly graphColumns: LayeredGraphColumn[] = [
    { label: 'Package', color: '#c8643c', textColor: '#fff' },
    { label: 'Image', color: '#5b4fc9', textColor: '#fff' },
    { label: 'Namespace', color: '#2f8f6b', textColor: '#fff' },
  ];
  // the same, with a wider package column and the images in two sub-columns
  protected readonly graphColumnsWide: LayeredGraphColumn[] = [
    { label: 'Package', color: '#c8643c', textColor: '#fff', width: 130 },
    { label: 'Image', color: '#5b4fc9', textColor: '#fff', subColumns: 2 },
    { label: 'Namespace', color: '#2f8f6b', textColor: '#fff' },
  ];
  protected readonly graphNodes: LayeredGraphNode[] = [
    { id: 'p1', column: 0, label: 'xz-utils 5.6.0' },
    { id: 'p2', column: 0, label: 'openssl 3.0.2' },
    { id: 'i1', column: 1, label: 'api:2.0.3', weight: 9 },
    { id: 'i2', column: 1, label: 'worker:0.9', weight: 7 },
    { id: 'i3', column: 1, label: 'gw:1.1', weight: 5 },
    { id: 'i4', column: 1, label: 'billing:4.2.0', weight: 3 },
    { id: 'i5', column: 1, label: 'reports:1.8', weight: 2 },
    { id: 'i6', column: 1, label: 'legacy-batch:0.3', weight: 1 },
    { id: 'n1', column: 2, label: 'payments', value: 3, weight: 5 },
    { id: 'n2', column: 2, label: 'ingest', value: 2, weight: 4 },
    { id: 'n3', column: 2, label: 'billing-prod', value: 4, weight: 3 },
    { id: 'n4', column: 2, label: 'analytics', value: 1, weight: 2 },
    { id: 'n5', column: 2, label: 'a-namespace-with-a-long-name', value: 6, weight: 1 },
  ];
  protected readonly graphEdges: LayeredGraphEdge[] = [
    // the two packages share some images (i3, i4, i5)
    ...['i1', 'i2', 'i3', 'i4', 'i5'].map((to) => ({ from: 'p1', to })),
    ...['i3', 'i4', 'i5', 'i6'].map((to) => ({ from: 'p2', to })),
    { from: 'i1', to: 'n1' },
    { from: 'i3', to: 'n1' },
    { from: 'i2', to: 'n2' },
    { from: 'i3', to: 'n2' },
    { from: 'i4', to: 'n3' },
    { from: 'i5', to: 'n4' },
    { from: 'i6', to: 'n4' },
    { from: 'i6', to: 'n5' },
  ];
  protected readonly graphSelected = signal<string | null>('i3');

  // matrix-heatmap
  protected readonly matrixColumns = ['prod', 'stage', 'dev', 'infra', 'tools'];
  private matrixSeed = 23;
  protected readonly matrixData = signal<MatrixHeatmapRowDto[]>(this.buildMatrix(this.matrixSeed));

  // new data for the matrix heatmap (it lights the cells on again)
  protected reloadMatrix(): void {
    this.matrixData.set(this.buildMatrix(++this.matrixSeed));
  }

  private buildMatrix(seed: number): MatrixHeatmapRowDto[] {
    const random = seeded(seed);

    return ['Critical', 'High', 'Medium', 'Low'].map((label, row) => ({
      label,
      cells: this.matrixColumns.map(() => ({ value: random() < 0.15 ? null : Math.round(random() * (40 - row * 8)) })),
    }));
  }

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
  private readonly scatterGroups = [
    { color: '#c0392b', shape: 'circle' as const },
    { color: '#e67e22', shape: 'diamond' as const },
    { color: '#3b82c4', shape: 'square' as const },
  ];

  private scatterSeed = 31;
  protected readonly scatterPoints = signal<ScatterPoint[]>(this.buildScatter(this.scatterSeed));

  // new data for the scatter: most points stay (some moved), some are gone, some are new (the merge by id)
  protected reloadScatter(): void {
    const random = seeded(++this.scatterSeed);
    const groups = this.scatterGroups;

    const kept = this.scatterPoints()
      .filter(() => random() > 0.15)
      .map((p) => (random() < 0.3 ? { ...p, x: Math.max(1, Math.round(p.x * (0.6 + random() * 0.8))) } : p));
    const added = Array.from({ length: 12 }, (_, i) => {
      const group = groups[i % groups.length];
      return {
        id: `new-${this.scatterSeed}-${i}`,
        x: Math.round(1 + random() * random() * 900),
        y: Math.round((2 + random() * 8) * 10) / 10,
        color: group.color,
        shape: group.shape,
        label: `CVE-2026-${this.scatterSeed}${i}`,
      };
    });
    this.scatterPoints.set([...kept, ...added]);
  }

  private buildScatter(seed: number): ScatterPoint[] {
    const random = seeded(seed);

    return Array.from({ length: 90 }, (_, i) => {
      const group = this.scatterGroups[i % this.scatterGroups.length];

      return {
        id: `cve-${i}`,
        x: Math.round(1 + random() * random() * 900),
        y: Math.round((2 + random() * 8) * 10) / 10,
        color: group.color,
        shape: group.shape,
        label: `CVE-2025-${1000 + i}`,
      };
    });
  }
  protected readonly scatterLegend: ScatterLegendItem[] = [
    { label: 'Backend', color: '#c0392b', shape: 'circle' },
    { label: 'Frontend', color: '#e67e22', shape: 'diamond' },
    { label: 'Infra', color: '#3b82c4', shape: 'square' },
  ];
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
