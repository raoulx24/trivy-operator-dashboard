import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  TemplateRef,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  model,
  output,
  signal,
  untracked,
} from '@angular/core';

import { PopoverService } from '../popover/popover.service';

import {
  ScatterAxis,
  ScatterLegendItem,
  ScatterPoint,
  ScatterPointView,
  ScatterRegion,
  ScatterSelection,
  ScatterShape,
  ScatterTick,
  ScatterTipContext,
  ScatterVertex,
} from './scatter-select.types';

const MARGIN = { top: 22, right: 12, bottom: 36, left: 44 };
// a point of the freehand line is kept when it is at least this far (px) from the last one
const MIN_STEP = 3;
// a drag smaller than this (px, both sides) is a click
const MIN_DRAG = 4;
const TARGET_TICKS = 4;
// room around the data when the axis has no min / max (linear: share of the range; log: factor)
const LINEAR_PAD = 0.05;
const LOG_PAD = 1.25;
// the cross reaches this much further than a circle of the same radius (the distance of its tips from the center)
const CROSS_REACH = 2;
// half the thickness of an arm of the cross, and the radius of the dot in the middle, as shares of the radius
const CROSS_ARM = 0.75;
const CENTER_DOT = 0.55;
// the longest a point takes to fade in (ms)
const MAX_FADE = 300;

const compact = new Intl.NumberFormat(undefined, { notation: 'compact', maximumFractionDigits: 2 });

interface Scale {
  toPx(value: number): number;
  fromPx(px: number): number;
  ticks: number[];
}

// a scatter plot drawn in svg, with freehand region selection
// - everything comes from the inputs: points (position, color, shape, label, own data), the axes (label, linear or log,
//   range, format) and the legend; the control does not know what any of them mean
// - selectable: drag to draw a region (free hand); the points inside keep the focus, the others dim. A new region
//   replaces the previous one; shift / ctrl / cmd adds one. A click on the plot without a drag clears
// - a click on a point (no drag) toggles it: an unselected one joins the selection (with or without regions), a selected one
//   (inside a region or picked before) leaves it
// - filter: the keys of the points an outside filter lets through; it selects them (together with the regions) until it is null
// - selectionChanged: the points selected (inside the regions, adjusted by the clicks), their indexes, and the polygons in data
//   coordinates; empty after a clear
// - animate: the points of the first data fade in from nothing one by one, in a random order, all within animationDuration
//   (ms) in total. New data is merged by the id of the points (else their index): a point that was there stays, and glides
//   to its new position if it moved; a new one fades in; one that is gone fades out. The axes are always there
// - the width follows the host; the height is height
@Component({
  selector: 'app-scatter-select',
  standalone: true,
  templateUrl: './scatter-select.component.html',
  styleUrl: './scatter-select.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScatterSelectComponent<T = unknown> {
  points = input<ScatterPoint<T>[]>([]);
  xAxis = input<ScatterAxis>({});
  yAxis = input<ScatterAxis>({});
  legend = input<ScatterLegendItem[]>([]);

  // px
  height = input<number>(260);
  // the radius, px; the same for every point
  pointRadius = input<number>(4);
  // 0..1; crowded areas show darker. The points in the selection are always opaque
  pointOpacity = input<number>(0.75);
  showTicks = input<boolean>(true);

  // the points fade in (new ones) and out (gone ones) when the data is received; the first data takes animationDuration ms
  animate = input<boolean>(false);
  animationDuration = input<number>(800);

  selectable = input<boolean>(true);
  // replaces the default popover; its context is a ScatterTipContext
  tooltipTemplate = input<TemplateRef<{ $implicit: ScatterTipContext<T> }> | undefined>(undefined);

  selectionChanged = output<ScatterSelection<T>>();

  // the picked point (its index in points; null = none): a click on a point (without a drag) picks it, a click on
  // the plot elsewhere clears it. It is drawn with a contrast border. The owner may set it (e.g. from a list)
  pickedIndex = model<number | null>(null);

  // an outside filter: the keys (ids) of the points that pass it, null for none. While it is set it is a selection: the points
  // that pass it (and are inside the regions, if there are any) are selected, the others dim, also for a single one. A click
  // on a point still wins over it. The selection is emitted again when the filter changes
  filter = input<ReadonlySet<string> | null>(null);
  private lastFilter: ReadonlySet<string> | null | undefined;

  protected readonly popover = inject(PopoverService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly width = signal(0);
  // the regions drawn, in data coordinates (they follow the axes when the control is resized); a model, so the owner can
  // keep them when the control is made again (e.g. the widget expands) and clear them
  regions = model<ScatterRegion[]>([]);
  // the points selected (true) or deselected (false) by a click, against what the regions say; by key
  // (a model, like the regions: the owner keeps them when the control is made again)
  overrides = model<ReadonlyMap<string, boolean>>(new Map());
  // the region being drawn, px
  private readonly draft = signal<ScatterVertex[] | null>(null);
  private additive = false;
  private downIndex: number | null = null;

  constructor() {
    const destroyRef = inject(DestroyRef);
    destroyRef.onDestroy(() => this.popover.hide());

    // the owner learns what the filter selects (not on the first value: nothing changed yet)
    effect(() => {
      const filter = this.filter();
      const first = this.lastFilter === undefined;
      this.lastFilter = filter;
      if (!first) {
        untracked(() => this.selectionChanged.emit(this.selection()));
      }
    });

    afterNextRender(() => {
      const element = this.host.nativeElement;
      this.width.set(element.clientWidth);
      const observer = new ResizeObserver(() => this.width.set(element.clientWidth));
      observer.observe(element);
      destroyRef.onDestroy(() => observer.disconnect());
    });
  }

  // how long one point takes to fade in (the last one starts at animationDuration minus this, so everything is done within
  // animationDuration)
  protected readonly fade = computed(() => Math.min(MAX_FADE, Math.max(0, this.animationDuration())));

  // the merge of the new points with the ones before (see the header): when the points change, which keys are new (with the
  // delay of their fade in, a random order spread over the time left after the fade of the last one) and which are gone
  // (they are drawn, fading out, until the next change). It is memoized: a selection or a resize does not run it again
  private previous = new Map<string, ScatterPoint<T>>();
  private readonly merge = computed(() => {
    const points = this.points();
    const animate = this.animate();
    const current = new Map<string, ScatterPoint<T>>(points.map((p, i) => [this.keyOf(p, i), p]));
    const before = this.previous;
    this.previous = current;
    if (!animate) return { enter: new Map<string, number>(), leaving: [] as { key: string; point: ScatterPoint<T> }[] };

    const entering = [...current.keys()].filter((key) => !before.has(key));
    // Fisher-Yates
    for (let i = entering.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [entering[i], entering[j]] = [entering[j], entering[i]];
    }

    const spread = Math.max(0, this.animationDuration() - this.fade());
    const enter = new Map(entering.map((key, i) => [key, entering.length > 1 ? (i / (entering.length - 1)) * spread : 0]));
    const leaving = [...before].filter(([key]) => !current.has(key)).map(([key, point]) => ({ key, point }));
    return { enter, leaving };
  });

  private keyOf(point: ScatterPoint<T>, index: number): string {
    return point.id ?? `#${index}`;
  }

  protected readonly plot = computed(() => ({
    left: MARGIN.left,
    right: Math.max(MARGIN.left + 1, this.width() - MARGIN.right),
    top: MARGIN.top,
    bottom: Math.max(MARGIN.top + 1, this.height() - MARGIN.bottom),
  }));

  private readonly xScale = computed(() => {
    const p = this.plot();
    return buildScale(
      this.xAxis(),
      this.points().map((pt) => pt.x),
      p.left,
      p.right,
    );
  });

  private readonly yScale = computed(() => {
    const p = this.plot();
    return buildScale(
      this.yAxis(),
      this.points().map((pt) => pt.y),
      p.bottom,
      p.top,
    );
  });

  protected readonly xTicks = computed<ScatterTick[]>(() => this.ticksOf(this.xAxis(), this.xScale()));
  protected readonly yTicks = computed<ScatterTick[]>(() => this.ticksOf(this.yAxis(), this.yScale()));

  // the same merge for the ticks of an axis (by value): a tick that was there slides to its new place when the scale changes, a
  // new one fades in, a gone one fades out where it was (until the next change); none of it on the first ticks
  private previousX = new Map<number, ScatterTick>();
  private previousY = new Map<number, ScatterTick>();

  private mergeTicks(current: ScatterTick[], previous: Map<number, ScatterTick>) {
    const keys = new Set(current.map((t) => t.value));
    const animate = this.animate() && previous.size > 0;
    return {
      entering: new Set(animate ? current.filter((t) => !previous.has(t.value)).map((t) => t.value) : []),
      leaving: animate ? [...previous.values()].filter((t) => !keys.has(t.value)) : [],
    };
  }

  protected readonly xTickMerge = computed(() => {
    const merge = this.mergeTicks(this.xTicks(), this.previousX);
    this.previousX = new Map(this.xTicks().map((t) => [t.value, t]));
    return merge;
  });

  protected readonly yTickMerge = computed(() => {
    const merge = this.mergeTicks(this.yTicks(), this.previousY);
    this.previousY = new Map(this.yTicks().map((t) => [t.value, t]));
    return merge;
  });

  // the regions in px
  private readonly regionsPx = computed<ScatterVertex[][]>(() => {
    const x = this.xScale();
    const y = this.yScale();
    return this.regions().map((region) => region.polygon.map((v) => ({ x: x.toPx(v.x), y: y.toPx(v.y) })));
  });

  protected readonly regionPaths = computed<string[]>(() => this.regionsPx().map((polygon) => polygonPath(polygon)));

  protected readonly draftPath = computed<string | null>(() => {
    const draft = this.draft();
    return draft && draft.length > 1 ? polygonPath(draft) : null;
  });

  protected readonly drawing = computed(() => this.draft() !== null);

  protected readonly views = computed<ScatterPointView<T>[]>(() => {
    const x = this.xScale();
    const y = this.yScale();
    const r = this.pointRadius();
    const polygons = this.regionsPx();
    const overrides = this.overrides();
    const points = this.points();
    // a picked point that is gone from the data does not count
    const keys = new Set(points.map((p, i) => this.keyOf(p, i)));
    const filter = this.filter();
    const hasSelection = filter !== null || polygons.length > 0 || [...overrides].some(([key, on]) => on && keys.has(key));
    const enter = this.merge().enter;

    const positions = points.map((point) => ({ px: x.toPx(point.x), py: y.toPx(point.y) }));
    const flags = points.map((point, index) => {
      const key = this.keyOf(point, index);
      const inRegion = polygons.some((polygon) => inside(positions[index].px, positions[index].py, polygon));
      // the filter and the regions both have to say yes (each one only when it is there)
      const base = (filter === null || filter.has(key)) && (polygons.length === 0 ? filter !== null : inRegion);
      return overrides.get(key) ?? base;
    });
    // one selected point is not a comparison: the others stay as they are (dimmed only for none or for several); with a
    // filter it is a selection, so they dim
    const dim = hasSelection && (filter !== null || flags.filter(Boolean).length !== 1);

    return points.map((point, index) => {
      const { px, py } = positions[index];
      const selected = flags[index];
      return {
        index,
        point,
        px,
        py,
        path: shapePath(point.shape ?? 'circle', 0, 0, r),
        color: point.color ?? null,
        centerColor: point.centerColor ?? null,
        selected,
        dimmed: dim && !selected,
        key: this.keyOf(point, index),
        litDelay: enter.get(this.keyOf(point, index)) ?? null,
      };
    });
  });

  protected readonly pickedView = computed<ScatterPointView<T> | undefined>(() => {
    const index = this.pickedIndex();
    return index === null ? undefined : this.views().find((v) => v.index === index);
  });

  // the border of the picked point: its shape; around a cross, a ring (the outline of an x would look broken)
  protected readonly pickedPath = computed<string | undefined>(() => {
    const view = this.pickedView();
    if (!view) return undefined;
    return view.point.shape === 'cross'
      ? shapePath('circle', view.px, view.py, this.pointRadius() * CROSS_REACH + 2)
      : shapePath(view.point.shape ?? 'circle', view.px, view.py, this.pointRadius());
  });

  // the points that are gone, fading out where they were (until the next change of the points)
  protected readonly leavingViews = computed(() => {
    const x = this.xScale();
    const y = this.yScale();
    const r = this.pointRadius();

    return this.merge().leaving.map(({ key, point }) => ({
      key,
      px: x.toPx(point.x),
      py: y.toPx(point.y),
      path: shapePath(point.shape ?? 'circle', 0, 0, r),
      color: point.color ?? null,
    }));
  });

  protected readonly centerRadius = computed(() => this.pointRadius() * CENTER_DOT);

  private readonly selection = computed<ScatterSelection<T>>(() => {
    const selected = this.views().filter((v) => v.selected);
    return {
      points: selected.map((v) => v.point),
      indexes: selected.map((v) => v.index),
      regions: this.regions(),
    };
  });

  // clears the regions and the points picked by hand (also callable by the owner, e.g. from a "clear" button)
  clear(): void {
    if (this.regions().length === 0 && this.overrides().size === 0) return;
    this.regions.set([]);
    this.overrides.set(new Map());
    this.selectionChanged.emit(this.selection());
  }

  // a click on a point: without shift / ctrl / cmd it selects only that point (a second click on it clears); with one of
  // them it is toggled in the selection (in -> out, out -> in, also when it is inside / outside a region)
  private toggle(index: number): void {
    const view = this.views().find((v) => v.index === index);
    if (!view) return;

    if (!this.additive) {
      const onlyThis = view.selected && this.views().filter((v) => v.selected).length === 1;
      this.regions.set([]);
      this.overrides.set(onlyThis ? new Map() : new Map([[view.key, true]]));
      this.selectionChanged.emit(this.selection());
      return;
    }

    const inRegion = this.regionsPx().some((polygon) => inside(view.px, view.py, polygon));
    const next = !view.selected;
    const overrides = new Map(this.overrides());
    // the region already says it: no override needed
    if (next === inRegion) overrides.delete(view.key);
    else overrides.set(view.key, next);
    this.overrides.set(overrides);
    this.selectionChanged.emit(this.selection());
  }

  protected tipContext(view: ScatterPointView<T>): ScatterTipContext<T> {
    return {
      view,
      xText: this.format(this.xAxis(), view.point.x),
      yText: this.format(this.yAxis(), view.point.y),
      xLabel: this.xAxis().label ?? 'x',
      yLabel: this.yAxis().label ?? 'y',
    };
  }

  protected legendPath(shape: ScatterShape | undefined): string {
    return shapePath(shape ?? 'circle', 7, 7, 5);
  }

  protected onPointerDown(event: PointerEvent): void {
    if (!this.selectable() || event.button !== 0) return;
    const p = this.local(event);
    if (!this.inPlot(p)) return;

    this.popover.hide();
    this.additive = event.shiftKey || event.ctrlKey || event.metaKey;
    // the point under the pointer, if any: a click (no drag) will pick it
    const index = (event.target as Element).getAttribute?.('data-index');
    this.downIndex = index === null || index === undefined ? null : Number(index);
    (event.currentTarget as Element).setPointerCapture(event.pointerId);
    this.draft.set([p]);
  }

  protected onPointerMove(event: PointerEvent): void {
    const draft = this.draft();
    if (!draft) return;
    const p = this.clamp(this.local(event));
    const last = draft[draft.length - 1];
    if (Math.hypot(p.x - last.x, p.y - last.y) < MIN_STEP) return;
    this.draft.set([...draft, p]);
  }

  protected onPointerUp(): void {
    const draft = this.draft();
    if (!draft) return;
    this.draft.set(null);

    const xs = draft.map((v) => v.x);
    const ys = draft.map((v) => v.y);
    const drawn =
      draft.length >= 3 &&
      Math.max(...xs) - Math.min(...xs) >= MIN_DRAG &&
      Math.max(...ys) - Math.min(...ys) >= MIN_DRAG;

    if (!drawn) {
      // a click: on a point it picks it and selects it (shift / ctrl / cmd: toggles it in the selection), elsewhere on the
      // plot it clears the selection and the pick
      if (this.downIndex !== null) {
        this.pickedIndex.set(this.downIndex);
        this.toggle(this.downIndex);
      } else if (!this.additive) {
        this.clear();
        this.pickedIndex.set(null);
      }
      return;
    }

    const x = this.xScale();
    const y = this.yScale();
    const region: ScatterRegion = { polygon: draft.map((v) => ({ x: x.fromPx(v.x), y: y.fromPx(v.y) })) };
    // a new region replaces the previous selection, the hand-picked points too; an added one keeps them
    if (!this.additive) this.overrides.set(new Map());
    this.regions.set(this.additive ? [...this.regions(), region] : [region]);
    this.selectionChanged.emit(this.selection());
  }

  protected onPointerCancel(): void {
    this.draft.set(null);
  }

  private local(event: PointerEvent): ScatterVertex {
    const rect = (event.currentTarget as Element).getBoundingClientRect();
    return { x: event.clientX - rect.left, y: event.clientY - rect.top };
  }

  private inPlot(p: ScatterVertex): boolean {
    const b = this.plot();
    return p.x >= b.left && p.x <= b.right && p.y >= b.top && p.y <= b.bottom;
  }

  private clamp(p: ScatterVertex): ScatterVertex {
    const b = this.plot();
    return { x: Math.min(b.right, Math.max(b.left, p.x)), y: Math.min(b.bottom, Math.max(b.top, p.y)) };
  }

  private ticksOf(axis: ScatterAxis, scale: Scale): ScatterTick[] {
    return scale.ticks.map((value) => ({ value, at: scale.toPx(value), text: this.format(axis, value) }));
  }

  private format(axis: ScatterAxis, value: number): string {
    return axis.format ? axis.format(value) : compact.format(value);
  }
}

function buildScale(axis: ScatterAxis, values: number[], from: number, to: number): Scale {
  const log = axis.scale === 'log';
  const usable = log ? values.filter((v) => v > 0) : values;

  let min = axis.min ?? (usable.length ? Math.min(...usable) : log ? 1 : 0);
  let max = axis.max ?? (usable.length ? Math.max(...usable) : log ? 10 : 1);

  if (log) {
    min = Math.max(min, Number.MIN_VALUE);
    if (axis.min === undefined) min /= LOG_PAD;
    if (axis.max === undefined) max *= LOG_PAD;
    if (max <= min) max = min * 10;
    const lo = Math.log(min);
    const span = Math.log(max) - lo;
    return {
      toPx: (v) => from + ((Math.log(Math.max(v, min)) - lo) / span) * (to - from),
      fromPx: (px) => Math.exp(lo + ((px - from) / (to - from)) * span),
      ticks: logTicks(min, max),
    };
  }

  if (max <= min) {
    min -= 1;
    max += 1;
  }
  const pad = (max - min) * LINEAR_PAD;
  if (axis.min === undefined) min -= pad;
  if (axis.max === undefined) max += pad;
  const span = max - min;
  return {
    toPx: (v) => from + ((v - min) / span) * (to - from),
    fromPx: (px) => min + ((px - from) / (to - from)) * span,
    ticks: linearTicks(min, max),
  };
}

function linearTicks(min: number, max: number): number[] {
  const raw = (max - min) / TARGET_TICKS;
  const magnitude = Math.pow(10, Math.floor(Math.log10(raw)));
  const step = ([1, 2, 5, 10].find((m) => m * magnitude >= raw) ?? 10) * magnitude;
  const result: number[] = [];
  for (let t = Math.ceil(min / step) * step; t <= max + step * 1e-9; t += step) {
    result.push(+t.toPrecision(12));
  }
  return result;
}

// the powers of ten inside the range; when the range holds fewer than three of them, also their 2x and 5x (1, 2, 5, 10, 20);
// when it is narrower than that, its two ends
function logTicks(min: number, max: number): number[] {
  const result: number[] = [];
  for (let e = Math.ceil(Math.log10(min)); Math.pow(10, e) <= max; e++) result.push(Math.pow(10, e));
  if (result.length >= 3) return result;

  const fine: number[] = [];
  for (let e = Math.floor(Math.log10(min)); Math.pow(10, e) <= max; e++) {
    for (const m of [1, 2, 5]) {
      const v = m * Math.pow(10, e);
      if (v >= min && v <= max) fine.push(+v.toPrecision(12));
    }
  }
  if (fine.length >= 2) return fine;
  return result.length >= 2 ? result : [min, max].map((v) => +v.toPrecision(2));
}

function shapePath(shape: ScatterShape, cx: number, cy: number, r: number): string {
  switch (shape) {
    case 'diamond': {
      const d = r * 1.4;
      return `M${cx} ${cy - d}L${cx + d} ${cy}L${cx} ${cy + d}L${cx - d} ${cy}Z`;
    }
    case 'square': {
      const s = r * 0.9;
      return `M${cx - s} ${cy - s}h${2 * s}v${2 * s}h${-2 * s}Z`;
    }
    case 'cross': {
      // an x: a plus turned 45 degrees; its tips are at CROSS_REACH x the radius
      const l = r * CROSS_REACH;
      const t = r * CROSS_ARM;
      const plus: [number, number][] = [
        [-t, -l], [t, -l], [t, -t], [l, -t], [l, t], [t, t], [t, l], [-t, l], [-t, t], [-l, t], [-l, -t], [-t, -t],
      ];
      const cos = Math.SQRT1_2;
      return (
        plus
          .map(([x, y], i) => `${i === 0 ? 'M' : 'L'}${(cx + (x - y) * cos).toFixed(2)} ${(cy + (x + y) * cos).toFixed(2)}`)
          .join('') + 'Z'
      );
    }
    case 'triangle': {
      const h = r * 1.3;
      return `M${cx} ${cy - h}L${cx + h} ${cy + h * 0.8}L${cx - h} ${cy + h * 0.8}Z`;
    }
    default:
      return `M${cx - r} ${cy}a${r} ${r} 0 1 0 ${2 * r} 0a${r} ${r} 0 1 0 ${-2 * r} 0Z`;
  }
}

function polygonPath(polygon: ScatterVertex[]): string {
  return polygon.map((v, i) => `${i === 0 ? 'M' : 'L'}${v.x.toFixed(1)} ${v.y.toFixed(1)}`).join('') + 'Z';
}

// ray casting
function inside(x: number, y: number, polygon: ScatterVertex[]): boolean {
  let result = false;
  for (let i = 0, j = polygon.length - 1; i < polygon.length; j = i++) {
    const a = polygon[i];
    const b = polygon[j];
    if (a.y > y !== b.y > y && x < ((b.x - a.x) * (y - a.y)) / (b.y - a.y) + a.x) result = !result;
  }
  return result;
}
