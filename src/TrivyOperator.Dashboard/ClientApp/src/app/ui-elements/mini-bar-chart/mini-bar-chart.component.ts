import { ChangeDetectionStrategy, Component, ElementRef, ViewChild, computed, input, signal } from '@angular/core';
import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { MiniBarChartDataDto } from './mini-bar-chart.types';
import { CounterIconPipe } from '../../pipes/counter-icon.pipe';
import { SeverityDifCssStyleByIdPipe } from '../../pipes/severity-dif-css-style-by-id.pipe';
import { Tag } from 'primeng/tag';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe'

import { Overlay, OverlayModule, OverlayPositionBuilder, OverlayRef } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { MiniBarTooltipComponent } from './mini-bar-tool-tip.component';

@Component({
  selector: 'app-mini-bar-chart',
  standalone: true,
  templateUrl: './mini-bar-chart.component.html',
  styleUrls: ['./mini-bar-chart.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SeverityCssStyleByIdPipe, CounterIconPipe, SeverityDifCssStyleByIdPipe, Tag, VulnerabilityCountPipe, OverlayModule],
})
export class MiniBarChartComponent {
  dataDtos = input<MiniBarChartDataDto[]>([]);
  tooltipTitle = input<string>('');
  height = input<number>(39);
  gap = input<number>(0.5);

  @ViewChild('container', { static: true })
  containerRef!: ElementRef<HTMLDivElement>;

  hovered = signal<MiniBarChartDataDto | undefined>(undefined);
  barWidth = signal(100);

  // stack: [critical, high, medium, low, unknown]
  bars = computed(() =>
    this.dataDtos().map((d) => [
      d.criticalCount,
      d.highCount,
      d.mediumCount,
      d.lowCount,
      d.unknownCount,
    ]),
  );

  stackExtents = computed(() =>
    this.dataDtos().map((d) => {
      const values = [d.criticalCount, d.highCount, d.mediumCount, d.lowCount, d.unknownCount];

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
    const min = this.minTotal();
    const max = this.maxTotal();
    const range = max - min || 1;

    const y = (max / range) * this.height();

    // clamp inside visible area
    return Math.min(this.height() - 1, Math.max(1, y));
  });

  tooltipX = signal(0);
  tooltipY = signal(0);

  private overlayRef?: OverlayRef;

  constructor(
    private overlay: Overlay,
    private overlayPositionBuilder: OverlayPositionBuilder,
  ) {}


  ngOnChanges() {
    this.barWidth.set(100 / Math.max(1, this.dataDtos().length));
  }

  hoveredBar(row: MiniBarChartDataDto | null, event?: MouseEvent) {
    if (!row || !event) {
      this.overlayRef?.detach();
      return;
    }

    const positionStrategy = this.overlayPositionBuilder
      .flexibleConnectedTo({ x: event.clientX, y: event.clientY })
      .withPositions([
        {
          originX: 'start',
          originY: 'top',
          overlayX: 'start',
          overlayY: 'top',
        },
      ]);

    if (!this.overlayRef) {
      this.overlayRef = this.overlay.create({ positionStrategy });
    } else {
      this.overlayRef.updatePositionStrategy(positionStrategy);
    }

    const styles = getComputedStyle(this.containerRef.nativeElement);
    const background = styles.getPropertyValue('--p-highlight-background')?.trim() || 'white';
    const primaryColor = styles.getPropertyValue('--tod-text-primary-color')?.trim() || 'black';

    // attach a component portal for tooltip content
    const tooltipPortal = new ComponentPortal(MiniBarTooltipComponent);
    const tooltipRef = this.overlayRef.attach(tooltipPortal);
    tooltipRef.instance.data.set(row); // pass data to tooltip component
    tooltipRef.instance.background.set(background);
    tooltipRef.instance.primaryColor.set(primaryColor);
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
        sum += Math.abs(stack[i]); // IMPORTANT
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

  hasData(d: MiniBarChartDataDto): boolean {
    return (
      d.criticalCount !== 0 ||
      d.highCount !== 0 ||
      d.mediumCount !== 0 ||
      d.lowCount !== 0 ||
      d.unknownCount !== 0
    );
  }
}
