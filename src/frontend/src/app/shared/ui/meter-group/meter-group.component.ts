import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  TemplateRef,
  afterNextRender,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';

import { PopoverService } from '../popover/popover.service';

import { MeterGroupBar, MeterGroupSegment, MeterGroupTipContext } from './meter-group.types';

// px per character of the value text (a rough size at the text-sm of the segments), to hide a value that does not fit
const CHAR_WIDTH = 8;
const TEXT_PADDING = 8;
const DEFAULT_HEIGHT = 10;
const DEFAULT_HEIGHT_WITH_VALUES = 30;

let nextId = 0;

// a horizontal meter made of segments, side by side (as the PrimeNG MeterGroup, with a popover per segment); drawn in svg
// - two styles, from one input:
//   - without max: the segments share the whole width (a mix: each is its share of the sum)
//   - with max: a value out of another one (a percent: max = 100); the segments take value / max of the width and the
//     rest is the empty track. When the sum is above max the meter is full (the segments are scaled to fit)
// - everything comes from the inputs: segments (label, value, color, optional textColor), max
// - showValues writes the value in the middle of each segment (left out of a segment too narrow for it)
// - the popover (heatmap-style: it follows the mouse) shows the label, the value and its share
// - the control does not know what the segments mean
@Component({
  selector: 'app-meter-group',
  standalone: true,
  templateUrl: './meter-group.component.html',
  styleUrl: './meter-group.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MeterGroupComponent {
  segments = input<MeterGroupSegment[]>([]);
  // the value that fills the meter; undefined = the sum of the segments (always full)
  max = input<number | undefined>(undefined);

  // the value in the middle of each segment
  showValues = input<boolean>(false);
  // px; by default thin, taller when the values are shown
  height = input<number | undefined>(undefined);

  // the text of the popover after the value, e.g. "ids"
  valueLabel = input<string>('');
  // replaces the default popover; its context is a MeterGroupTipContext
  tooltipTemplate = input<TemplateRef<{ $implicit: MeterGroupTipContext }> | undefined>(undefined);

  protected readonly popover = inject(PopoverService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly clipId = `meter-group-clip-${nextId++}`;
  protected readonly width = signal(0);

  constructor() {
    // never leave the popover behind when the meter goes away while hovered
    const destroyRef = inject(DestroyRef);
    destroyRef.onDestroy(() => this.popover.hide());

    // the svg is drawn in px: it follows the width of the host
    afterNextRender(() => {
      const element = this.host.nativeElement;
      this.width.set(element.clientWidth);
      const observer = new ResizeObserver(() => this.width.set(element.clientWidth));
      observer.observe(element);
      destroyRef.onDestroy(() => observer.disconnect());
    });
  }

  protected readonly barHeight = computed(
    () => this.height() ?? (this.showValues() ? DEFAULT_HEIGHT_WITH_VALUES : DEFAULT_HEIGHT),
  );

  protected readonly bars = computed<MeterGroupBar[]>(() => {
    const segments = this.segments().filter((s) => s.value > 0);
    const sum = segments.reduce((total, s) => total + s.value, 0);
    const max = this.max();
    const whole = max !== undefined && max > 0 ? max : sum;
    // above max the meter is full: the widths are scaled, the shares stay against max
    const scale = Math.max(whole, sum);
    const width = this.width();

    let x = 0;
    return segments.map((segment) => {
      const w = scale > 0 ? (segment.value / scale) * width : 0;
      const bar: MeterGroupBar = {
        segment,
        x,
        width: w,
        sharePercent: whole > 0 ? (segment.value / whole) * 100 : 0,
        showValue: this.showValues() && w >= String(segment.value).length * CHAR_WIDTH + TEXT_PADDING,
      };
      x += w;
      return bar;
    });
  });

  // the width of the fill (the segments together): less than the whole when the meter is not full
  protected readonly fillWidth = computed(() => this.bars().reduce((sum, b) => sum + b.width, 0));

  protected tipContext(bar: MeterGroupBar, segmentIndex: number): MeterGroupTipContext {
    return {
      label: bar.segment.label,
      value: bar.segment.value,
      max: this.max(),
      sharePercent: bar.sharePercent,
      color: bar.segment.color,
      segmentIndex,
    };
  }
}
