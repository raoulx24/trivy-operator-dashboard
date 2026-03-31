import {
  ChangeDetectionStrategy,
  Component,
  ViewChild,
  computed,
  input,
  signal,
  inject,
  ComponentRef,
} from '@angular/core';
import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { MiniBarChartDataDto } from './mini-bar-chart.types';

import { Overlay, OverlayModule, OverlayPositionBuilder, OverlayRef } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { MiniBarTooltipComponent } from './mini-bar-tool-tip.component';

@Component({
  selector: 'app-mini-bar-chart',
  standalone: true,
  templateUrl: './mini-bar-chart.component.html',
  styleUrls: ['./mini-bar-chart.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SeverityCssStyleByIdPipe, OverlayModule],
})
export class MiniBarChartComponent {
  dataDtos = input<MiniBarChartDataDto[]>([]);
  tooltipTitle = input<string>('');
  height = input<number>(39);
  gap = input<number>(0.5);

  @ViewChild('miniBarChart', { static: false })
  private tooltipComponentRef?: ComponentRef<MiniBarTooltipComponent>;

  barWidth = signal(100);

  // stack: [critical, high, medium, low, unknown]
  bars = computed(() =>
    this.dataDtos().map((d) => {
      const added = d.newCount;
      const removed = d.removedCount.map(v => -v); // convert to negative

      return [...added, ...removed]; // 10 values total
    })
  );


  stackExtents = computed(() =>
    this.dataDtos().map((d) => {
      const values = [
        ...d.newCount,
        ...d.removedCount.map(v => -v)
      ];

      const positive = values.filter(v => v > 0).reduce((a, b) => a + b, 0);
      const negative = values.filter(v => v < 0).reduce((a, b) => a + b, 0);

      return { positive, negative };
    })
  );


  minTotal = computed(() => Math.min(0, ...this.stackExtents().map(e => e.negative)));
  maxTotal = computed(() => Math.max(0, ...this.stackExtents().map(e => e.positive)));

  // baseline Y (0 value)
  zeroY = computed(() => {
    const min = this.minTotal();
    const max = this.maxTotal();
    const range = max - min || 1;

    const y = (max / range) * this.height();
    return Math.min(this.height() - 1, Math.max(1, y));
  });

  private overlayRef?: OverlayRef;

  private readonly overlay = inject(Overlay);
  private readonly overlayPositionBuilder = inject(OverlayPositionBuilder);

  private containerStyles!: CSSStyleDeclaration;

  hoveredIndex?: number;

  ngAfterViewInit() {
    this.containerStyles = getComputedStyle(document.documentElement);
  }

  ngOnChanges() {
    this.barWidth.set(100 / Math.max(1, this.dataDtos().length));
  }

  tooltipLocked = signal(false);
  // Handle both mouseenter and mouseleave
  hoveredBar(row: MiniBarChartDataDto | null, index: number, event?: MouseEvent) {
    // If tooltip is locked, ignore all hover events
    if (this.tooltipLocked()) return;
    this.hoveredIndex = index ?? undefined;

    if (!row || !event) {
      this.hideTooltip(index);
      return;
    }

    this.showTooltip(row, event);
  }


  private showTooltip(row: MiniBarChartDataDto, event: MouseEvent) {
    this.tooltipLocked.set(true);

    const positionStrategy = this.overlayPositionBuilder
      .flexibleConnectedTo({ x: event.clientX + 3, y: event.clientY + 3 })
      .withPositions([
        { originX: 'start', originY: 'top', overlayX: 'start', overlayY: 'top' }
      ]);

    // Create overlay if needed
    if (!this.overlayRef) {
      this.overlayRef = this.overlay.create({ positionStrategy });
    } else {
      this.overlayRef.updatePositionStrategy(positionStrategy);
    }

    // Attach tooltip if not already attached
    if (!this.overlayRef.hasAttached()) {
      const portal = new ComponentPortal(MiniBarTooltipComponent);
      this.tooltipComponentRef = this.overlayRef.attach(portal);
    }

    // Update tooltip instance safely
    if (this.tooltipComponentRef) {
      const instance = this.tooltipComponentRef.instance;

      instance.data.set(row);

      instance.background.set(this.containerStyles.getPropertyValue('--p-highlight-background')?.trim() || 'white');
      instance.primaryColor.set(this.containerStyles.getPropertyValue('--tod-text-primary-color')?.trim() || 'black');
    }
  }

  protected hideTooltip(index: number) {
    if (this.overlayRef?.hasAttached()) {
      this.overlayRef.detach();
    }
    this.tooltipLocked.set(false);
    if (this.hoveredIndex === index)
      this.hoveredIndex = undefined;
  }






  // Helper methods for stacking bars
  getPositiveStack(stack: number[]) {
    return stack.map((v) => (v > 0 ? v : 0));
  }

  getNegativeStack(stack: number[]) {
    return stack.map((v) => (v < 0 ? v : 0));
  }

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

  getHeight(value: number): number {
    const range = this.maxTotal() - this.minTotal();
    return (Math.abs(value) / range) * this.height();
  }

  getY(stack: number[], index: number): number {
    const value = stack[index];
    const min = this.minTotal();
    const max = this.maxTotal();
    const range = max - min || 1;

    if (value >= 0) {
      const offset = this.getPositiveOffset(stack, index);
      return this.zeroY() - ((offset + value) / range) * this.height();
    } else {
      const offset = this.getNegativeOffset(stack, index);
      return this.zeroY() + (offset / range) * this.height();
    }
  }

  hasData(d: MiniBarChartDataDto): boolean {
    return (
      d.newCount.some(v => v !== 0) ||
      d.removedCount.some(v => v !== 0)
    );
  }
}
