import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterRenderEffect,
  computed,
  effect,
  inject,
  input,
  output,
  model,
  signal,
  untracked,
  viewChild,
} from '@angular/core';

import { PopoverService } from '../popover/popover.service';
import { CHART_TIP_SOURCE, ChartTip } from '../popover/chart-tip';
import { ScrollerComponent } from '../scroller/scroller.component';

import {
  SpanTimelineBar,
  SpanTimelineLegendItem,
  SpanTimelineMarkerKind,
  SpanTimelineOrigin,
  SpanTimelineRow,
  SpanTimelineScale,
  SpanTimelineTick,
  SpanTimelineTickView,
  SpanTimelineTipContext,
} from './span-timeline.types';

// all sizes are in px: the drawing is as wide as the host, so the text keeps its size
const DEFAULT_WIDTH = 480;
const TOP_HEIGHT = 22;
const ROW_HEIGHT = 28;
// label above the bar: a taller row, the label on its first line, the bar on the second
const ROW_HEIGHT_ABOVE = 42;
const LABEL_Y_ABOVE = 12;
const BAR_Y_ABOVE = 22;
const BAR_HEIGHT = 12;
const MIN_BAR_WIDTH = 6;
const MIN_AXIS_WIDTH = 60;
// the room after the right end of the axis (the "today" label is centered on it)
const RIGHT_PAD = 20;
const VALUE_GAP = 12;
// the room between the text at either end of a row and the edge of the row (the selection outline)
const EDGE_PAD = 8;
const MAX_MARKERS = 3;
// the longest a bar takes to grow (ms)
const MAX_GROW = 600;
// ticks closer than this to each other (or to the right end) are dropped
const MIN_TICK_GAP = 52;
// candidates of the automatic ticks of a log axis
const LOG_TICKS = [1, 3, 7, 14, 30, 60, 90, 180, 365, 730, 1095, 1825, 3650];

// bars of time on one shared horizontal axis: one row per item, the oldest end at the left, now at the right
// - everything comes from the inputs: rows (label, start, end, color, value text, markers), ticks, legend, colors, scale
// - values are ages (distance back from now) in a unit of the caller; the scale is linear or log (log1p, so 0 = now works)
// - origin left turns the axis around (0 at the left, the bars grow to the right), for values that are not ages
// - a faint row (a value to take with care) has a faint bar on a track with a dashed outline
// - a row can have up to 3 markers (dot, diamond, tick); they are drawn on top of the bar and have their own popover
// - selection: a click (or Enter / Space) on a row selects it, again deselects; selectedId is the two way signal
// - the axis is drawn apart, on top; the rows are in a vertical app-scroller, as tall as they are up to maxHeight (px, the
//   axis included; null = no limit)
// - animate: when new data arrives the bars grow from the middle of each bar to both ends, one after the other in a random order of rows, all within
//   animationDuration (ms) in total; the value and the markers of a row fade in with it; the same data does not play again
//   (a resize or a selection does not); the tracks are always there
// - the control does not know what the rows or the markers mean
@Component({
  selector: 'app-span-timeline',
  standalone: true,
  imports: [ScrollerComponent],
  templateUrl: './span-timeline.component.html',
  styleUrl: './span-timeline.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: CHART_TIP_SOURCE, useExisting: SpanTimelineComponent }],
})
export class SpanTimelineComponent {
  rows = input<SpanTimelineRow[]>([]);
  scale = input<SpanTimelineScale>('linear');
  // where 0 (now) is; left: the bars grow from the left end (nowLabel is drawn there)
  origin = input<SpanTimelineOrigin>('right');
  // false: no axis (no ticks, no now label), only the rows
  showAxis = input<boolean>(true);
  // the age at the left end of the axis; default: the oldest start of the rows
  max = input<number | null>(null);
  // the labels of the axis; default: automatic (nice steps for linear, 1 3 7 14 30... for log)
  ticks = input<SpanTimelineTick[] | null>(null);
  // the text of a value in ticks and popovers
  tickFormat = input<(value: number) => string>((value) => `${value}d`);
  // the label of the right end (age 0)
  nowLabel = input<string>('today');
  // a dashed vertical line at this age; null = none
  referenceAt = input<number | null>(null);
  legend = input<SpanTimelineLegendItem[]>([]);
  // css colors for the rows without their own, in turn; empty = the primary color
  colors = input<string[]>([]);
  // left: the label is left of the bars (labelWidth is its room); above: the label is on a line of its own above each bar, with the
  // value at the right end of that line (labelWidth is then not used by the drawing)
  labelPosition = input<'left' | 'above'>('left');
  labelWidth = input<number>(160);
  // the room of the text on the right (used only when a row has a valueLabel)
  valueWidth = input<number>(48);

  // px, the axis included: taller rows scroll; null = all the rows are shown
  maxHeight = input<number | null>(null);

  // the bars grow when the data (rows) is received; the whole animation takes animationDuration ms
  animate = input<boolean>(false);
  animationDuration = input<number>(1000);
  // anything that changes when the owner receives data again: the animation plays again for the same data too (e.g. a
  // refresh); without it only changed data plays
  replay = input<unknown>(undefined);

  selectable = input<boolean>(true);
  // the id of the selected row, null = none (two way: the click changes it)
  selectedId = model<string | null>(null);

  protected readonly barHeight = BAR_HEIGHT;
  protected readonly rowHeight = computed(() => (this.labelPosition() === 'above' ? ROW_HEIGHT_ABOVE : ROW_HEIGHT));
  protected readonly labelY = computed(() => (this.labelPosition() === 'above' ? LABEL_Y_ABOVE : ROW_HEIGHT / 2));
  protected readonly barY = computed(() =>
    this.labelPosition() === 'above' ? BAR_Y_ABOVE : (ROW_HEIGHT - BAR_HEIGHT) / 2,
  );
  protected readonly axisHeight = TOP_HEIGHT;
  protected readonly viewWidth = signal(DEFAULT_WIDTH);
  // the popover of an item is up to the owner (see ChartTipDirective)
  readonly tipShow = output<ChartTip<SpanTimelineTipContext>>();
  readonly tipMove = output<MouseEvent>();
  readonly tipHide = output<void>();

  protected readonly popover = inject(PopoverService);

  // how long one bar takes to grow (the last one starts at animationDuration minus this, so everything is done within
  // animationDuration)
  protected readonly grow = computed(() => Math.min(MAX_GROW, Math.max(0, this.animationDuration())));

  // what the growth depends on: the rows (not the width or the selection); the delays are kept while it is unchanged, as a
  // changed delay would restart the growth of a bar that has already grown
  private delayCache?: { key: string; id: number; delays: Map<string, number> };
  private delayCacheId = 0;

  // per row id, in ms: a random order of the rows, evenly spread over the time left after the growth of the last one
  private delaysOf(ids: string[], key: string) {
    if (this.delayCache?.key === key) return this.delayCache;

    const order = [...ids];
    // Fisher-Yates
    for (let i = order.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [order[i], order[j]] = [order[j], order[i]];
    }

    const spread = Math.max(0, this.animationDuration() - this.grow());
    const delays = new Map(order.map((id, i) => [id, order.length > 1 ? (i / (order.length - 1)) * spread : 0]));
    this.delayCache = { key, id: ++this.delayCacheId, delays };
    return this.delayCache;
  }

  private readonly axisBox = viewChild<ElementRef<HTMLElement>>('axisBox');

  constructor() {
    const destroyRef = inject(DestroyRef);
    // never leave the popover behind when the control goes away while hovered
    destroyRef.onDestroy(() => this.popover.hide());

    // the drawings are as wide as the box of the axis without its padding (the one of the scroller of the rows); the box
    // appears with the rows
    const observer = new ResizeObserver((entries) => {
      const width = Math.floor(entries[0]?.contentRect.width ?? 0);
      if (width > 0) this.viewWidth.set(width);
    });
    effect(() => {
      const element = this.axisBox()?.nativeElement;
      observer.disconnect();
      if (element) observer.observe(element);
    });
    destroyRef.onDestroy(() => observer.disconnect());

    // the selected row is brought into view in the scroller of the rows (only when it is out of it), once it is drawn: when
    // its place changes (another selection, a click or the owner's; a sort that moves it; a filter that moves it) or the
    // scroller appears. A new list that leaves it where it was does not scroll
    afterRenderEffect(() => {
      const index = this.selectedIndex();
      const container = this.scroller()?.scrollContainer?.nativeElement;
      if (index < 0 || !container) return;

      untracked(() => {
        const top = index * this.rowHeight();
        const bottom = top + this.rowHeight();
        if (top < container.scrollTop) {
          container.scrollTo({ top, behavior: 'smooth' });
        } else if (bottom > container.scrollTop + container.clientHeight) {
          container.scrollTo({ top: bottom - container.clientHeight, behavior: 'smooth' });
        }
      });
    });
  }

  private readonly scroller = viewChild(ScrollerComponent);
  // the place of the selected row in the rows (-1: none); a number, so only a move of the row is a change
  private readonly selectedIndex = computed(() => {
    const id = this.selectedId();
    return id === null ? -1 : this.rows().findIndex((r) => r.id === id);
  });

  protected readonly hasValues = computed(() => this.rows().some((r) => !!r.valueLabel));

  protected readonly barsLeft = computed(() => (this.labelPosition() === 'above' ? EDGE_PAD : this.labelWidth()));
  protected readonly barsWidth = computed(() => {
    const right =
      RIGHT_PAD + (this.hasValues() && this.labelPosition() === 'left' ? this.valueWidth() + VALUE_GAP + EDGE_PAD : 0);
    return Math.max(MIN_AXIS_WIDTH, this.viewWidth() - this.barsLeft() - right);
  });
  // the value ends EDGE_PAD before the right edge, as the label starts EDGE_PAD after the left one
  protected readonly valueX = computed(() => this.viewWidth() - EDGE_PAD);
  protected readonly nowX = computed(() =>
    this.origin() === 'left' ? this.barsLeft() : this.barsLeft() + this.barsWidth(),
  );

  private readonly domainMax = computed(() => {
    const given = this.max();
    if (given !== null && given > 0) return given;
    return Math.max(1, ...this.rows().map((r) => r.start));
  });

  protected readonly bars = computed<SpanTimelineBar[]>(() => {
    const format = this.tickFormat();
    const now = this.nowLabel();
    const palette = this.colors();
    const text = (age: number) => (age <= 0 ? now : format(age));
    const rows = this.rows();
    const dataKey = JSON.stringify([
      this.animationDuration(),
      this.replay(),
      this.origin(),
      rows.map((r) => [r.id, r.start, r.end ?? 0, r.valueLabel ?? null, (r.markers ?? []).map((m) => m.at)]),
    ]);
    const lit = this.animate() ? this.delaysOf(rows.map((r) => r.id), dataKey) : undefined;

    return rows.map((row, i) => {
      const end = Math.max(0, row.end ?? 0);
      const start = Math.max(end, row.start);
      const xStart = this.x(start);
      const xEnd = this.x(end);
      const width = Math.max(MIN_BAR_WIDTH, Math.abs(xEnd - xStart));
      // a bar never goes past the now end
      const left = this.origin() === 'left' ? Math.min(xStart, xEnd) : Math.min(xStart, this.nowX() - width);

      return {
        row,
        y: i * this.rowHeight(),
        x: left,
        width,
        color: row.color ?? (palette.length > 0 ? palette[i % palette.length] : 'var(--p-primary-color)'),
        range: `${text(start)} → ${text(end)}`,
        key: `${lit?.id ?? 0}|${row.id}`,
        growDelay: lit?.delays.get(row.id) ?? null,
        markers: (row.markers ?? []).slice(0, MAX_MARKERS).map((m) => ({
          kind: m.kind ?? 'dot',
          x: this.x(m.at),
          color: m.color ?? null,
          tip: `${m.label ?? this.kindName(m.kind ?? 'dot')} (${text(m.at)})`,
        })),
      };
    });
  });

  protected readonly rowsBottom = computed(() => this.rows().length * this.rowHeight());
  // the height of the scroller of the rows: all of them, or what maxHeight leaves after the axis (one row at least)
  protected readonly rowsViewHeight = computed(() => {
    const max = this.maxHeight();
    const axis = this.showAxis() ? TOP_HEIGHT : 0;
    return max === null ? this.rowsBottom() : Math.min(this.rowsBottom(), Math.max(this.rowHeight(), max - axis));
  });

  protected readonly referenceX = computed(() => {
    const at = this.referenceAt();
    return at === null ? null : this.x(at);
  });

  protected readonly tickViews = computed<SpanTimelineTickView[]>(() => {
    const format = this.tickFormat();
    const ticks = this.ticks() ?? this.autoTicks();

    const views = ticks
      .filter((t) => t.value > 0 && t.value <= this.domainMax())
      .map((t) => ({ x: this.x(t.value), label: t.label ?? format(t.value) }))
      .sort((a, b) => Math.abs(a.x - this.nowX()) - Math.abs(b.x - this.nowX()));

    // from the now end: drop what would overlap the "now" label or the previous one
    const kept: SpanTimelineTickView[] = [];
    let last = this.nowX();
    for (const view of views) {
      if (Math.abs(last - view.x) < MIN_TICK_GAP) continue;
      kept.push(view);
      last = view.x;
    }
    return kept;
  });

  // age -> x (px): 0 at the now end of the axis (right, or left with origin left), the domain max at the other one;
  // values beyond are clamped
  private x(age: number): number {
    const max = this.domainMax();
    const v = Math.min(Math.max(age, 0), max);
    const ratio = this.scale() === 'log' ? Math.log1p(v) / Math.log1p(max) : v / max;
    return this.barsLeft() + (this.origin() === 'left' ? ratio : 1 - ratio) * this.barsWidth();
  }

  private autoTicks(): SpanTimelineTick[] {
    const max = this.domainMax();
    if (this.scale() === 'log') return LOG_TICKS.filter((v) => v <= max).map((value) => ({ value }));

    const raw = max / 4;
    const magnitude = Math.pow(10, Math.floor(Math.log10(raw)));
    const step = [1, 2, 5, 10].map((f) => f * magnitude).find((s) => s >= raw) ?? raw;
    const ticks: SpanTimelineTick[] = [];
    for (let value = step; value <= max; value += step) ticks.push({ value });
    return ticks;
  }

  private kindName(kind: SpanTimelineMarkerKind): string {
    return kind === 'dot' ? 'Dot' : kind === 'diamond' ? 'Diamond' : 'Tick';
  }

  protected select(row: SpanTimelineRow): void {
    if (!this.selectable()) return;
    this.selectedId.set(this.selectedId() === row.id ? null : row.id);
  }

  protected onKey(event: Event, row: SpanTimelineRow): void {
    // Space would scroll the page
    event.preventDefault();
    this.select(row);
  }

  protected tipContext(bar: SpanTimelineBar): SpanTimelineTipContext {
    return { row: bar.row, range: bar.range };
  }
}
