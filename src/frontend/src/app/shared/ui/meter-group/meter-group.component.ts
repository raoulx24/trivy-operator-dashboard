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
// the longest a segment takes to grow (ms)
const MAX_GROW = 600;

let nextId = 0;

// a horizontal meter made of segments, side by side (as the PrimeNG MeterGroup, with a popover per segment); drawn in svg
// - two styles, from one input:
//   - without max: the segments share the whole width (a mix: each is its share of the sum)
//   - with max: a value out of another one (a percent: max = 100); the segments take value / max of the width and the
//     rest is the empty track. When the sum is above max the meter is full (the segments are scaled to fit)
// - everything comes from the inputs: segments (label, value, color, optional textColor), max
// - showValues writes the value in the middle of each segment (left out of a segment too narrow for it)
// - the popover (heatmap-style: it follows the mouse) shows the label, the value and its share
// - animate: when new data arrives each segment grows from its middle to both ends, one after the other in a random order,
//   all within animationDuration (ms) in total; the value of a segment fades in with it; the same data does not play again
//   (a resize does not); the track is always there
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

  // the segments grow when the data is received; the whole animation takes animationDuration ms
  animate = input<boolean>(false);
  animationDuration = input<number>(800);
  // anything that changes when the owner receives data again: the animation plays again for the same data too (e.g. a
  // refresh); without it only changed data plays
  replay = input<unknown>(undefined);

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

  // how long one segment takes to grow (the last one starts at animationDuration minus this, so everything is done within
  // animationDuration)
  protected readonly grow = computed(() => Math.min(MAX_GROW, Math.max(0, this.animationDuration())));

  // what the growth depends on: the segments (not the width); the delays are kept while it is unchanged, as a changed delay
  // would restart the growth of a segment that has already grown
  private delayCache?: { key: string; id: number; delays: number[] };
  private delayCacheId = 0;

  // per segment, in ms: a random order, evenly spread over the time left after the growth of the last one
  private delaysOf(count: number, key: string) {
    if (this.delayCache?.key === key) return this.delayCache;

    const order = Array.from({ length: count }, (_, i) => i);
    // Fisher-Yates
    for (let i = order.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [order[i], order[j]] = [order[j], order[i]];
    }

    const spread = Math.max(0, this.animationDuration() - this.grow());
    const delays = new Array<number>(count);
    order.forEach((segment, i) => (delays[segment] = count > 1 ? (i / (count - 1)) * spread : 0));
    this.delayCache = { key, id: ++this.delayCacheId, delays };
    return this.delayCache;
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
    const dataKey = JSON.stringify([
      this.animationDuration(),
      this.replay(),
      max ?? null,
      segments.map((s) => [s.label, s.value, s.color]),
    ]);
    const lit = this.animate() ? this.delaysOf(segments.length, dataKey) : undefined;

    let x = 0;
    return segments.map((segment, i) => {
      const w = scale > 0 ? (segment.value / scale) * width : 0;
      const bar: MeterGroupBar = {
        segment,
        x,
        width: w,
        sharePercent: whole > 0 ? (segment.value / whole) * 100 : 0,
        showValue: this.showValues() && w >= String(segment.value).length * CHAR_WIDTH + TEXT_PADDING,
        key: `${lit?.id ?? 0}|${i}`,
        growDelay: lit?.delays[i] ?? null,
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
