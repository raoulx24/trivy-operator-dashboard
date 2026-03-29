import { ChangeDetectionStrategy, Component, ElementRef, ViewChild, computed, input, signal } from '@angular/core';
import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { MiniBarChartDataDto } from './mini-bar-chart.types';

@Component({
  selector: 'app-mini-bar-chart',
  standalone: true,
  templateUrl: './mini-bar-chart.component.html',
  styleUrls: ['./mini-bar-chart.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SeverityCssStyleByIdPipe],
})
export class MiniBarChartComponent {
  dataDtos = input<MiniBarChartDataDto[]>([]);
  title = input<string>('');
  height = input<number>(39);
  gap = input<number>(0.5);

  @ViewChild('container', { static: true })
  containerRef!: ElementRef<HTMLDivElement>;

  hovered = signal<MiniBarChartDataDto | undefined>(undefined);
  barWidth = signal(100);

  // stack: [critical, high, medium, low, unknown]
  bars = computed(() =>
    this.dataDtos().map((d) => [
      d.critical,
      d.high,
      d.medium,
      d.low,
      d.unknown,
    ]),
  );

  stackExtents = computed(() =>
    this.dataDtos().map((d) => {
      const values = [d.critical, d.high, d.medium, d.low, d.unknown];

      const positive = values.filter(v => v > 0).reduce((a, b) => a + b, 0);
      const negative = values.filter(v => v < 0).reduce((a, b) => a + b, 0);

      return { positive, negative };
    })
  );

  minTotal = computed(() =>
    Math.min(0, ...this.stackExtents().map(e => e.negative))
  );

  maxTotal = computed(() =>
    Math.max(0, ...this.stackExtents().map(e => e.positive))
  );

  // baseline Y (0 value)
  zeroY = computed(() => {
    const range = this.maxTotal() - this.minTotal();
    return this.height() * (this.maxTotal() / range);
  });

  tooltipX = signal(0);
  tooltipY = signal(0);

  ngOnChanges() {
    this.barWidth.set(100 / Math.max(1, this.dataDtos().length));
  }

  hoveredBar(row: MiniBarChartDataDto | null, event?: MouseEvent) {
    this.hovered.set(row ?? undefined);

    if (row && event && this.containerRef) {
      const rect = this.containerRef.nativeElement.getBoundingClientRect();
      const offset = 6;

      this.tooltipX.set(event.clientX - rect.left + offset);
      this.tooltipY.set(rect.height / 2); // centered vertically
    }
  }

  // split stacks
  getPositiveStack(stack: number[]) {
    return stack.map((v) => (v > 0 ? v : 0));
  }

  getNegativeStack(stack: number[]) {
    return stack.map((v) => (v < 0 ? v : 0));
  }

  // cumulative offsets
  getPositiveOffset(stack: number[], index: number) {
    let sum = 0;

    for (let i = 0; i < index; i++) {
      if (stack[i] > 0) {
        sum += stack[i];
      }
    }

    return sum;
  }

  getNegativeOffset(stack: number[], index: number) {
    let sum = 0;

    for (let i = 0; i < index; i++) {
      if (stack[i] < 0) {
        sum += Math.abs(stack[i]); // 👈 IMPORTANT
      }
    }

    return sum;
  }

  // height of a segment
  getHeight(value: number): number {
    const range = this.maxTotal() - this.minTotal();
    return (Math.abs(value) / range) * this.height();
  }

  // Y position of a segment
  getY(stack: number[], index: number): number {
    const value = stack[index];
    const min = this.minTotal();
    const max = this.maxTotal();
    const range = max - min || 1;

    if (value >= 0) {
      const offset = this.getPositiveOffset(stack, index);

      return (
        this.zeroY() -
        ((offset + value) / range) * this.height()
      );
    } else {
      const offset = this.getNegativeOffset(stack, index);

      return (
        this.zeroY() +
        (offset / range) * this.height()
      );
    }
  }

  onMouseMove(event: MouseEvent) {}
}
