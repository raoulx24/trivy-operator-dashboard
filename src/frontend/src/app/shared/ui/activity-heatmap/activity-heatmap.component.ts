import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  input,
  output,
} from '@angular/core';

import { WeekdayNamePipe } from '../../pipes/weekday-name.pipe';
import { cornerMarkerPath } from '../corner-marker/corner-marker-path';
import { PopoverService } from '../popover/popover.service';
import { ScrollerComponent } from '../scroller/scroller.component';

import {
  ActivityHeatmapCell,
  ActivityHeatmapDayDto,
  ActivityHeatmapRows,
  ActivityHeatmapWeek,
} from './activity-heatmap.types';

const DAY_MS = 86_400_000;
const WEEK_DAYS = 7;
// the sizes of the strokes and the radius are given for this cell size (px) and scale with cellSize
const BASE_CELL_SIZE = 16;
// the week label row: room for the chart label text (text-sm, 14px), with its baseline at LABEL_BASELINE
const LABEL_HEIGHT = 20;
const LABEL_BASELINE = 14;
const LEVELS = 4;
// the legend swatches (px), as in the scss
const LEGEND_SWATCH = 17;
// the longest a cell takes to light on (ms)
const MAX_FADE = 300;
// only when the browser has no Intl week info
const SUNDAY_REGIONS = new Set(['US', 'CA', 'MX', 'BR', 'JP', 'IL', 'IN', 'PH', 'ZA', 'AU']);

// a per-day heatmap, one block per week (first day of the week of the locale, Monday in EU), the days of a week running top down, then left right
// - cells have a fixed size: if the blocks do not fit, they scroll horizontally (app-scroller, with its buttons)
// - fill: quantile shading (blue) of the non-zero values; zero is empty
// - everything comes from data, one item per day: value, emphasis1 (a triangle in the bottom-left corner), emphasis2 (a
//   triangle in the top-right corner; both independent of the fill) and selectable (the day can be clicked, daySelected;
//   when no day sets it, nothing is clickable)
// - the selected day (selectedDay) has a ring around the cell
// - animate: when new data arrives the cells with a value or a marker light on one by one, in a random order of place, all
//   within animationDuration (ms) in total; data with the same values, markers and range does not play again (a change of
//   selectable or selectedDay does not); the empty cells are always there
// - days are "yyyy-MM-dd" (UTC); the control does not know what the emphasis days mean
@Component({
  selector: 'app-activity-heatmap',
  standalone: true,
  imports: [ScrollerComponent, WeekdayNamePipe],
  templateUrl: './activity-heatmap.component.html',
  styleUrl: './activity-heatmap.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActivityHeatmapComponent {
  data = input<ActivityHeatmapDayDto[]>([]);
  // the range shown; by default the first and last day of data
  from = input<string | undefined>(undefined);
  to = input<string | undefined>(undefined);

  rows = input<ActivityHeatmapRows>(3);
  cellSize = input<number>(23);
  cellGap = input<number>(4);
  weekGap = input<number>(34);  // 'locale': the first day of the week of the browser's locale (Monday if it cannot be told)
  weekStart = input<'locale' | 'monday' | 'sunday'>('locale');

  showWeekLabels = input<boolean>(true);
  showLegend = input<boolean>(true);
  valueLabel = input<string>('');
  emphasis1Label = input<string>('');
  emphasis2Label = input<string>('');
  // the caption of the selected day in the legend (shown while a day is selected)
  selectedLabel = input<string>('selected day');
  // the popover of each legend item (on hover); none for an empty text
  valueHint = input<string>('');
  emphasis1Hint = input<string>('');
  emphasis2Hint = input<string>('');
  selectedHint = input<string>('');

  // the cells light on when the data is received; the whole animation takes animationDuration ms
  animate = input<boolean>(false);
  animationDuration = input<number>(600);
  // anything that changes when the owner receives data again: the animation plays again for the same data too (e.g. a
  // refresh); without it only changed data plays
  replay = input<unknown>(undefined);

  // the day marked as selected, by the owner (a click only says which day: daySelected)
  selectedDay = input<string | null>(null);

  daySelected = output<string>();

  protected readonly popover = inject(PopoverService);

  constructor() {
    // never leave the popover behind when the heatmap goes away while hovered
    inject(DestroyRef).onDestroy(() => this.popover.hide());
  }

  // how long one cell takes to light on (the last one starts at animationDuration minus this, so everything is done
  // within animationDuration)
  protected readonly fade = computed(() => Math.min(MAX_FADE, Math.max(0, this.animationDuration())));

  // 1 at the base cell size; scales the radius, strokes and labels with the cells
  protected readonly labelBaseline = LABEL_BASELINE;
  protected readonly unit = computed(() => this.cellSize() / BASE_CELL_SIZE);

  protected readonly levels =Array.from({ length: LEVELS }, (_, i) => i + 1);

  // the upper bounds of the quantile steps, distinct (fewer steps when the values have few distinct ones)
  private readonly thresholds = computed<number[]>(() => {
    const values = this.data()
      .map((d) => d.value)
      .filter((v) => v > 0)
      .sort((a, b) => a - b);

    if (values.length === 0) return [];

    const result: number[] = [];
    for (let k = 1; k < LEVELS; k++) {
      const t = values[Math.max(0, Math.ceil((k / LEVELS) * values.length) - 1)];
      if (t < values[values.length - 1] && !result.includes(t)) result.push(t);
    }
    return result;
  });

  protected readonly usedLevels = computed(() => this.thresholds().length + 1);

  protected readonly legendLevels = computed(() => this.levels.slice(0, this.usedLevels()));

  // what the animation depends on: the values and markers of the days (not selectable) and the range; the delays are kept
  // while it is unchanged, as a changed delay would restart the animation of a cell that has already lit on
  private delayCache?: { key: string; id: number; delays: Map<string, number> };
  private delayCacheId = 0;

  // per day, in ms: a random order over the days with a value or a marker, evenly spread over the time left after the
  // fade of the last one
  private delaysOf(data: ActivityHeatmapDayDto[], key: string): { id: number; delays: Map<string, number> } {
    if (this.delayCache?.key === key) return this.delayCache;

    const days = data.filter((d) => d.value > 0 || d.emphasis1 || d.emphasis2).map((d) => d.day);
    // Fisher-Yates
    for (let i = days.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [days[i], days[j]] = [days[j], days[i]];
    }

    const spread = Math.max(0, this.animationDuration() - this.fade());
    const delays = new Map(days.map((day, i) => [day, days.length > 1 ? (i / (days.length - 1)) * spread : 0]));
    this.delayCache = { key, id: ++this.delayCacheId, delays };
    return this.delayCache;
  }

  protected readonly weeks = computed<ActivityHeatmapWeek[]>(() => {
    const data = this.data();
    const range = this.range(data);
    if (!range) return [];

    const byDay = new Map(data.map((d) => [d.day, d]));
    const interactive = data.some((d) => d.selectable !== undefined);
    const dataKey = `${range.join('-')}|${this.animationDuration()}|${String(this.replay())}|${data.map((d) => `${d.day}:${d.value}:${d.emphasis1 ? 1 : 0}${d.emphasis2 ? 1 : 0}`).join(',')}`;
    const lit = this.animate() ? this.delaysOf(data, dataKey) : undefined;
    const selectedDay = this.selectedDay();
    const rows = this.rows();
    const step = this.cellSize() + this.cellGap();
    const weekWidth = Math.ceil(WEEK_DAYS / rows) * step - this.cellGap();
    const top = this.showWeekLabels() ? LABEL_HEIGHT : 0;

    const [fromMs, toMs] = range;
    const firstDay = this.firstDayOfWeek();
    // the first day (Monday, Sunday...) of the first week
    const firstWeekStart = fromMs - this.weekdayIndex(fromMs, firstDay) * DAY_MS;

    const weeks: ActivityHeatmapWeek[] = [];
    for (let weekStart = firstWeekStart; weekStart <= toMs; weekStart += WEEK_DAYS * DAY_MS) {
      const cells: ActivityHeatmapCell[] = [];
      for (let i = 0; i < WEEK_DAYS; i++) {
        const ms = weekStart + i * DAY_MS;
        if (ms < fromMs || ms > toMs) continue;

        const day = this.toDay(ms);
        const item = byDay.get(day);
        const value = item?.value ?? 0;
        const e1 = item?.emphasis1 ?? false;
        const e2 = item?.emphasis2 ?? false;
        const isSelectable = interactive && (item?.selectable ?? false);
        cells.push({
          day,
          value,
          level: this.levelOf(value),
          emphasis1: e1,
          emphasis2: e2,
          selectable: isSelectable,
          inactive: interactive && !isSelectable && value > 0,
          selected: day === selectedDay,
          litDelay: lit?.delays.get(day) ?? null,
          x: Math.floor(i / rows) * step,
          y: top + (i % rows) * step,
        });
      }

      weeks.push({
        key: `${lit?.id ?? 0}|${weekStart}`,
        label: cells[0].day.slice(5),
        x: weeks.length * (weekWidth + this.weekGap()),
        cells,
      });
    }
    return weeks;
  });

  protected readonly width = computed(() => {
    const count = this.weeks().length;
    if (count === 0) return 0;
    const weekWidth = Math.ceil(WEEK_DAYS / this.rows()) * (this.cellSize() + this.cellGap()) - this.cellGap();
    return count * weekWidth + (count - 1) * this.weekGap();
  });

  protected readonly height = computed(
    () =>
      (this.showWeekLabels() ? LABEL_HEIGHT : 0) + this.rows() * (this.cellSize() + this.cellGap()) - this.cellGap(),
  );

  protected onCellClick(cell: ActivityHeatmapCell): void {
    if (cell.selectable) {
      this.daySelected.emit(cell.day);
    }
  }

  // the corner marker: a right triangle in the top-right of the cell, its three corners rounded
  protected cornerPath(cell: ActivityHeatmapCell): string {
    return cornerMarkerPath(cell.x, cell.y, this.cellSize(), 'top-right');
  }

  // the marker of emphasis1: the same triangle in the opposite corner (bottom-left)
  protected visitPath(cell: ActivityHeatmapCell): string {
    return cornerMarkerPath(cell.x, cell.y, this.cellSize(), 'bottom-left');
  }

  // the same marker for the legend swatch (a square of LEGEND_SWATCH px)
  protected readonly legendCornerPath = cornerMarkerPath(0, 0, LEGEND_SWATCH, 'top-right');
  protected readonly legendVisitPath = cornerMarkerPath(0, 0, LEGEND_SWATCH, 'bottom-left');
  protected readonly legendSwatchSize = LEGEND_SWATCH;

  private range(data: ActivityHeatmapDayDto[]): [number, number] | undefined {
    const days = data.map((d) => d.day).sort();
    const from = this.parse(this.from() ?? days[0]);
    const to = this.parse(this.to() ?? days[days.length - 1]);
    return from === undefined || to === undefined || to < from ? undefined : [from, to];
  }

  private levelOf(value: number): number {
    if (value <= 0) return 0;
    return 1 + this.thresholds().filter((t) => value > t).length;
  }

  // 0 = the first day of the week
  private weekdayIndex(ms: number, firstDay: number): number {
    return (new Date(ms).getUTCDay() - firstDay + 7) % 7;
  }

  // 0 = Sunday, 1 = Monday, ... 6 = Saturday
  private firstDayOfWeek(): number {
    const mode = this.weekStart();
    if (mode === 'monday') return 1;
    if (mode === 'sunday') return 0;

    try {
      const locale = new Intl.Locale(navigator.language) as Intl.Locale & {
        getWeekInfo?: () => { firstDay: number };
        weekInfo?: { firstDay: number };
      };
      // Intl week info: 1 = Monday ... 7 = Sunday
      const info = locale.getWeekInfo?.() ?? locale.weekInfo;
      if (info) return info.firstDay % 7;
      return SUNDAY_REGIONS.has(locale.maximize().region ?? '') ? 0 : 1;
    } catch {
      return 1;
    }
  }

  private parse(day: string | undefined): number | undefined {
    if (!day) return undefined;
    const ms = Date.parse(`${day.slice(0, 10)}T00:00:00Z`);
    return Number.isNaN(ms) ? undefined : ms;
  }

  private toDay(ms: number): string {
    return new Date(ms).toISOString().slice(0, 10);
  }
}
