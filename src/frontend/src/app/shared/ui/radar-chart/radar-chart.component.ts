import { ChangeDetectionStrategy, Component, computed, input, model } from '@angular/core';

import { RadarChartAxis, RadarChartPoint } from './radar-chart.types';

// the chart is drawn in these units (the svg scales to the width of its container); the sides are wide for the labels
const RADIUS = 100;
const LABEL_GAP = 10;
const HALF_WIDTH = 212;
const HALF_HEIGHT = 128;

// a radar (spider) chart: one spoke per axis, evenly spread from 12 o'clock clockwise, a value 0..1 on each
// - knows nothing about what the axes mean
// - hoveredIndex is two-way: hovering a vertex (or label) sets it, the host may set it to emphasize an axis
@Component({
  selector: 'app-radar-chart',
  standalone: true,
  templateUrl: './radar-chart.component.html',
  styleUrl: './radar-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RadarChartComponent {
  axes = input<RadarChartAxis[]>([]);
  // the concentric grid polygons
  rings = input<number>(5);
  hoveredIndex = model<number | undefined>(undefined);

  protected readonly viewBox = `${-HALF_WIDTH} ${-HALF_HEIGHT} ${HALF_WIDTH * 2} ${HALF_HEIGHT * 2}`;

  protected readonly points = computed<RadarChartPoint[]>(() => {
    const axes = this.axes();

    return axes.map((axis, index) => {
      const angle = -Math.PI / 2 + (index * 2 * Math.PI) / axes.length;
      const cos = Math.cos(angle);
      const sin = Math.sin(angle);
      const value = Math.min(1, Math.max(0, axis.value));

      return {
        index,
        axis,
        x: round(cos * RADIUS * value),
        y: round(sin * RADIUS * value),
        endX: round(cos * RADIUS),
        endY: round(sin * RADIUS),
        labelX: round(cos * (RADIUS + LABEL_GAP)),
        labelY: round(sin * (RADIUS + LABEL_GAP)),
        // the labels on the right side start at the spoke, on the left side end at it, at the top and bottom are centred
        labelAnchor: cos > 0.2 ? 'start' : cos < -0.2 ? 'end' : 'middle',
        labelBaseline: sin > 0.2 ? 'hanging' : sin < -0.2 ? 'auto' : 'central',
      };
    });
  });

  protected readonly ringPaths = computed<string[]>(() => {
    const count = this.axes().length;
    if (count < 3) return [];

    return Array.from({ length: this.rings() }, (_, k) => {
      const radius = (RADIUS * (k + 1)) / this.rings();
      return this.polygon(
        this.points().map((p) => ({ x: (p.endX * radius) / RADIUS, y: (p.endY * radius) / RADIUS })),
      );
    });
  });

  protected readonly valuePath = computed<string>(() => this.polygon(this.points()));

  private polygon(points: { x: number; y: number }[]): string {
    return points.length === 0 ? '' : `M${points.map((p) => `${round(p.x)},${round(p.y)}`).join(' L')} Z`;
  }
}

const round = (value: number): number => Math.round(value * 100) / 100;
