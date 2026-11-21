import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  inject,
  input,
  model,
  signal,
} from '@angular/core';

// px kept free at both ends of the svg, so the dot and the first and last legend do not touch the edge
const PADDING = 14;
const LINE_Y = 10;
const THIN = 2;
const THICK = 4;
const DOT_RADIUS = 5;
const TICK_HEIGHT = 3;
const LEGEND_Y = 30;
const HEIGHT = 36;

interface StepTick {
  value: number;
  // px
  x: number;
  text: string;
  selectable: boolean;
}

// a line to pick one value on, in steps (a slider without the handle)
// - the line goes from min to max, thin; from min to the selected value it is thicker, with a dot at the value
// - a click on the line (or near it) selects the nearest value that can be selected: the steps from min, kept between
//   minSelectable and maxSelectable. What cannot be selected is drawn dashed. The arrow keys, Home and End also work
// - the legend (a text under the line) is written every legendScale, from min
// - everything comes from the inputs: min, max, minSelectable, maxSelectable, step, legendScale, valueFormat for the
//   text of the legend; selected is the model. The control does not know what the value means
@Component({
  selector: 'app-step-select',
  standalone: true,
  templateUrl: './step-select.component.html',
  styleUrl: './step-select.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StepSelectComponent {
  min = input<number>(0);
  max = input<number>(100);
  // the part of the line that can be selected; by default all of it
  minSelectable = input<number | undefined>(undefined);
  maxSelectable = input<number | undefined>(undefined);
  step = input<number>(1);
  // the distance between two texts of the legend, from min; none = no legend
  legendScale = input<number | undefined>(undefined);
  // the text of a value in the legend, e.g. (v) => v + '%'
  valueFormat = input<(value: number) => string>((value) => String(value));
  ariaLabel = input<string>('');

  selected = model<number>(0);

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly width = signal(0);
  protected readonly height = HEIGHT;
  protected readonly lineY = LINE_Y;
  protected readonly legendY = LEGEND_Y;
  protected readonly dotRadius = DOT_RADIUS;
  protected readonly tickHeight = TICK_HEIGHT;
  protected readonly thin = THIN;
  protected readonly thick = THICK;

  constructor() {
    // inject() works only here, not in the callback below
    const destroyRef = inject(DestroyRef);

    // the svg is drawn in px: it follows the width of the host
    afterNextRender(() => {
      const element = this.host.nativeElement;
      this.width.set(element.clientWidth);
      const observer = new ResizeObserver(() => this.width.set(element.clientWidth));
      observer.observe(element);
      destroyRef.onDestroy(() => observer.disconnect());
    });
  }

  private readonly lower = computed(() => Math.max(this.min(), this.minSelectable() ?? this.min()));
  private readonly upper = computed(() => Math.min(this.max(), this.maxSelectable() ?? this.max()));

  protected readonly left = PADDING;
  protected readonly right = computed(() => Math.max(PADDING + 1, this.width() - PADDING));

  protected toPx(value: number): number {
    const span = this.max() - this.min();
    return span > 0 ? this.left + ((value - this.min()) / span) * (this.right() - this.left) : this.left;
  }

  private fromPx(px: number): number {
    const span = this.right() - this.left;
    return span > 0 ? this.min() + ((px - this.left) / span) * (this.max() - this.min()) : this.min();
  }

  protected readonly dotX = computed(() => this.toPx(this.selected()));
  protected readonly lowerX = computed(() => this.toPx(this.lower()));
  protected readonly upperX = computed(() => this.toPx(this.upper()));

  protected readonly ticks = computed<StepTick[]>(() => {
    const scale = this.legendScale();
    if (!scale || scale <= 0 || this.width() === 0) return [];

    const format = this.valueFormat();
    const result: StepTick[] = [];
    // the count is computed (not a float sum) so 0.1 steps do not drift
    const count = Math.floor((this.max() - this.min()) / scale + 1e-9);
    for (let i = 0; i <= count; i++) {
      const value = +(this.min() + i * scale).toPrecision(12);
      result.push({
        value,
        x: this.toPx(value),
        text: format(value),
        selectable: value >= this.lower() && value <= this.upper(),
      });
    }
    return result;
  });

  // the nearest step from min inside the selectable part
  private snap(value: number): number {
    const step = this.step() > 0 ? this.step() : 1;
    const snapped = this.min() + Math.round((value - this.min()) / step) * step;
    const lowest = this.min() + Math.ceil((this.lower() - this.min()) / step - 1e-9) * step;
    const highest = this.min() + Math.floor((this.upper() - this.min()) / step + 1e-9) * step;
    return +Math.min(highest, Math.max(lowest, snapped)).toPrecision(12);
  }

  protected onClick(event: MouseEvent): void {
    const rect = (event.currentTarget as Element).getBoundingClientRect();
    this.selected.set(this.snap(this.fromPx(event.clientX - rect.left)));
  }

  protected onKeydown(event: KeyboardEvent): void {
    const step = this.step() > 0 ? this.step() : 1;
    let next: number | undefined;
    switch (event.key) {
      case 'ArrowRight':
      case 'ArrowUp':
        next = this.selected() + step;
        break;
      case 'ArrowLeft':
      case 'ArrowDown':
        next = this.selected() - step;
        break;
      case 'Home':
        next = this.lower();
        break;
      case 'End':
        next = this.upper();
        break;
    }
    if (next !== undefined) {
      event.preventDefault();
      this.selected.set(this.snap(next));
    }
  }
}
