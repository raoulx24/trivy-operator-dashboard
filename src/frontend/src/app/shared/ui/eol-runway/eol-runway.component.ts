import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';

import { PopoverService } from '../popover/popover.service';

import {
  EOL_RISK_DAYS,
  EOL_STATE_LABELS,
  EOL_STATE_ORDER,
  eolRelative,
  eolStateDescription,
  eolStateOf,
  parseEolDay,
} from './eol-runway.state';
import { EolRunwayBar, EolRunwayItem, EolRunwayState, EolRunwayTick, EolRunwayTip } from './eol-runway.types';

// all sizes are in px: the drawing is as wide as the host, so the text keeps its size (text-sm)
const DEFAULT_WIDTH = 480;
// the label (130) and, at its right end, the room for the state icon (EOL, risky)
const LABEL_WIDTH = 144;
const ICON_SIZE = 10;
const ICON_GUTTER = 14;
// one pattern id per runway (the ids of a page are shared)
let nextHatchId = 0;
const COUNT_WIDTH = 64;
// the room after the axis end for the "goes on" arrow
const ARROW_WIDTH = 18;
const MIN_AXIS_WIDTH = 60;
const TOP_HEIGHT = 18;
const ROW_HEIGHT = 24;
const BAR_HEIGHT = 10;
const AXIS_HEIGHT = 22;
const MIN_BAR_WIDTH = 6;

// the runway of OS cycles: one bar per cycle, from the left edge of the axis to its end of life, with today marked
// - the axis shows monthsBefore months before today and yearsAfter years after; a later end is a bar to the axis end, then a dashed line and an arrow
// - an end older than the axis start is a short stub at the left; the stub tells it is over, not when
// - colors: supported, only security fixes (end of active support passed), risky (end of life in less than riskDays
//   days, whatever the support phase), end of life passed
// - "today" is above the chart, the years below it, so they never overlap
// - dates are "yyyy-MM-dd" (UTC); items are drawn sorted by end of life
@Component({
  selector: 'app-eol-runway',
  standalone: true,
  templateUrl: './eol-runway.component.html',
  styleUrl: './eol-runway.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EolRunwayComponent {
  items = input<EolRunwayItem[]>([]);
  // "yyyy-MM-dd"
  today = input.required<string>();
  monthsBefore = input<number>(6);
  yearsAfter = input<number>(3);
  // an end of life in less than this many days is risky: it has its own color, whatever the support phase
  riskDays = input<number>(EOL_RISK_DAYS);
  protected readonly barHeight = BAR_HEIGHT;
  protected readonly rowHeight = ROW_HEIGHT;
  protected readonly top = TOP_HEIGHT;
  protected readonly barsLeft = LABEL_WIDTH;
  protected readonly iconSize = ICON_SIZE;
  protected readonly iconX = LABEL_WIDTH - ICON_GUTTER;
  protected readonly hatchId = `eol-runway-hatch-${nextHatchId++}`;

  protected readonly viewWidth = signal(DEFAULT_WIDTH);

  private readonly barsWidth = computed(() =>
    Math.max(MIN_AXIS_WIDTH, this.viewWidth() - LABEL_WIDTH - COUNT_WIDTH - ARROW_WIDTH),
  );

  protected readonly countX = computed(() => this.barsLeft + this.barsWidth() + ARROW_WIDTH + COUNT_WIDTH);
  protected readonly arrowX = computed(() => this.barsLeft + this.barsWidth());

  protected readonly popover = inject(PopoverService);

  constructor() {
    const host = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
    const destroyRef = inject(DestroyRef);
    // never leave the popover behind when the runway goes away while hovered
    destroyRef.onDestroy(() => this.popover.hide());

    afterNextRender(() => {
      const observer = new ResizeObserver(() => {
        const width = Math.floor(host.clientWidth);
        if (width > 0) this.viewWidth.set(width);
      });
      observer.observe(host);
      destroyRef.onDestroy(() => observer.disconnect());
    });
  }

  // [axis start, axis end, today], in ms
  private readonly domain = computed<[number, number, number] | undefined>(() => {
    const today = this.parse(this.today());
    if (today === undefined) return undefined;

    const start = new Date(today);
    start.setUTCMonth(start.getUTCMonth() - this.monthsBefore());
    const end = new Date(today);
    end.setUTCFullYear(end.getUTCFullYear() + this.yearsAfter());
    return [start.getTime(), end.getTime(), today];
  });

  protected readonly bars = computed<EolRunwayBar[]>(() => {
    const domain = this.domain();
    if (!domain) return [];
    const [start, end, today] = domain;

    return this.items()
      .map((item) => ({ item, eol: this.parse(item.eolFrom), eoas: this.parse(item.eoasFrom ?? undefined) }))
      .filter((x): x is { item: EolRunwayItem; eol: number; eoas: number | undefined } => x.eol !== undefined)
      .sort((a, b) => a.eol - b.eol)
      .map(({ item, eol, eoas }, i) => {
        const state = eolStateOf(eol, eoas, today, this.riskDays());
        const width = Math.max(MIN_BAR_WIDTH, this.scale(Math.min(eol, end), start, end));

        return {
          label: item.label,
          state,
          y: TOP_HEIGHT + i * ROW_HEIGHT,
          width,
          clipped: eol > end,
          overdueWidth: state === 'eol' ? Math.max(0, this.scale(today, start, end) - width) : 0,
          imagesLabel: `${item.images} img`,
          tip: this.tipOf(item, state, eol, eoas, today),
        };
      });
  });

  protected readonly legend = computed(() =>
    EOL_STATE_ORDER.map((state) => ({
      state,
      label: EOL_STATE_LABELS[state],
      description: eolStateDescription(state, this.riskDays()),
    })),
  );

  protected readonly todayX = computed(() => {
    const domain = this.domain();
    return domain ? LABEL_WIDTH + this.scale(domain[2], domain[0], domain[1]) : 0;
  });

  protected readonly ticks = computed<EolRunwayTick[]>(() => {
    const domain = this.domain();
    if (!domain) return [];
    const [start, end] = domain;

    const ticks: EolRunwayTick[] = [];
    for (let year = new Date(start).getUTCFullYear() + 1; Date.UTC(year, 0, 1) < end; year++) {
      ticks.push({ label: String(year), x: LABEL_WIDTH + this.scale(Date.UTC(year, 0, 1), start, end) });
    }
    return ticks;
  });

  protected readonly rowsBottom = computed(() => TOP_HEIGHT + this.bars().length * ROW_HEIGHT);
  protected readonly height = computed(() => this.rowsBottom() + AXIS_HEIGHT);

  private scale(ms: number, start: number, end: number): number {
    return ((ms - start) / (end - start)) * this.barsWidth();
  }

  private tipOf(
    item: EolRunwayItem,
    state: EolRunwayState,
    eol: number,
    eoas: number | undefined,
    today: number,
  ): EolRunwayTip {
    return {
      label: item.label,
      eol: `${item.eolFrom} (${this.relative(eol, today)})`,
      eoas: eoas === undefined ? undefined : `${item.eoasFrom} (${this.relative(eoas, today)})`,
      securityFixesOnly: state === 'eoas',
      endsSoon: state === 'risky',
    };
  }

  private relative(ms: number, today: number): string {
    return eolRelative(ms, today);
  }

  private parse(day: string | undefined): number | undefined {
    return parseEolDay(day);
  }
}
