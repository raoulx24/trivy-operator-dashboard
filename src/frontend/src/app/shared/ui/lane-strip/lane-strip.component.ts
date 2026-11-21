import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  TemplateRef,
  computed,
  inject,
  input,
  model,
  output,
} from '@angular/core';

import { cornerMarkerPath } from '../corner-marker/corner-marker-path';
import { PopoverService } from '../popover/popover.service';
import { ScrollerComponent } from '../scroller/scroller.component';

import { LaneStripCell, LaneStripCellClick, LaneStripLane, LaneStripRow, LaneStripSelection, LaneStripTipContext } from './lane-strip.types';

// the steps of color of a non-empty cell: the levels of the activity heatmap (quantiles of the values of the lane, the
// --tod-heatmap-level-N colors)
const LEVELS = 4;
// the longest a cell takes to light on (ms)
const MAX_FADE = 300;

// lanes of squares on one shared horizontal axis: a caption on the left and a total on the right stay put, the squares
// scroll between them (app-scroller, at the end by default)
// - everything comes from the inputs: columns (their labels), lanes (caption, values, optional total), and per lane the columns to mark with a
//   triangle (emphasis1, emphasis2)
// - the lightness of a cell is the level of its value among the values of its lane (quantiles, as in the activity
//   heatmap); zero is an empty cell
// - animate: when the data of a lane arrives (new values) its cells with a value light on one by one, in a random order of
//   place, all within animationDuration (ms) in total; a lane with the same values does not play again; the empty cells
//   are always there
// - the control does not know what the lanes or the columns mean
@Component({
  selector: 'app-lane-strip',
  standalone: true,
  imports: [ScrollerComponent],
  templateUrl: './lane-strip.component.html',
  styleUrl: './lane-strip.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LaneStripComponent {
  // the label of each column (the tooltip title), e.g. the day
  columns = input<string[]>([]);
  lanes = input<LaneStripLane[]>([]);

  // the selected cell, two-way: a click on a cell with a value selects it, a click on the selected one unselects it (the owner can also set it); null = none
  selected = model<LaneStripSelection | null>(null);

  // the cells light on when the data (lanes) is received; the whole animation takes animationDuration ms
  animate = input<boolean>(false);
  animationDuration = input<number>(600);

  cellSize = input<number>(24);
  cellGap = input<number>(4);

  // the cells with a value can be clicked (cellClicked)
  clickable = input<boolean>(true);
  // the texts of the two markers in the popover (shown on the cells that have them)
  emphasis1Label = input<string>('');
  emphasis2Label = input<string>('');
  // the text of the tooltip after the value, e.g. "vulns"
  valueLabel = input<string>('');
  // replaces the default popover (the column, "value label", the lane caption); its context is a LaneStripTipContext
  tooltipTemplate = input<TemplateRef<LaneStripTipContext> | undefined>(undefined);

  cellClicked = output<LaneStripCellClick>();

  protected readonly popover = inject(PopoverService);

  constructor() {
    // never leave the popover behind when the control goes away while hovered
    inject(DestroyRef).onDestroy(() => this.popover.hide());
  }

  protected readonly step = computed(() => this.cellSize() + this.cellGap());

  protected readonly width = computed(() => {
    const count = this.columns().length;
    return count === 0 ? 0 : count * this.step() - this.cellGap();
  });

  protected readonly height = computed(() => {
    const count = this.lanes().length;
    return count === 0 ? 0 : count * this.step() - this.cellGap();
  });

  // how long one cell takes to light on (the last one starts at animationDuration minus this, so everything is done
  // within animationDuration)
  protected readonly fade = computed(() => Math.min(MAX_FADE, Math.max(0, this.animationDuration())));

  // when each cell of a lane lights on, per lane and kept while the lane is unchanged: a changed delay would restart the
  // animation of a cell that has already lit on (only the lanes that received new values play)
  private delayCache = new Map<string, (number | null)[]>();

  // [column] in ms (null: not animated): a random order over the cells with a value or a marker, evenly spread over the
  // time left after the fade of the last one
  private delaysOf(lane: LaneStripLane, count: number): (number | null)[] {
    const delays = new Array<number | null>(count).fill(null);
    const marked = new Set([...(lane.emphasis1 ?? []), ...(lane.emphasis2 ?? [])]);
    const columns: number[] = [];
    for (let column = 0; column < count; column++) {
      if ((lane.values[column] ?? 0) > 0 || marked.has(column)) columns.push(column);
    }

    // Fisher-Yates
    for (let i = columns.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [columns[i], columns[j]] = [columns[j], columns[i]];
    }

    const spread = Math.max(0, this.animationDuration() - this.fade());
    columns.forEach((column, i) => {
      delays[column] = columns.length > 1 ? (i / (columns.length - 1)) * spread : 0;
    });
    return delays;
  }

  protected readonly rows = computed<LaneStripRow[]>(() => {
    const size = this.cellSize();
    const step = this.step();
    const selected = this.selected();
    const count = this.columns().length;
    const animate = this.animate();
    const previousDelays = this.delayCache;
    const delayCache = new Map<string, (number | null)[]>();

    const rows = this.lanes().map((lane, laneIndex) => {
      const thresholds = this.thresholdsOf(lane.values);
      // a lane is the same lane while its values are (the animation plays again only for the lanes that changed)
      const key = `${laneIndex}|${lane.caption}|${count}|${lane.values.join(',')}`;
      const delayKey = `${key}|${this.animationDuration()}`;
      const delays = animate ? (previousDelays.get(delayKey) ?? this.delaysOf(lane, count)) : [];
      if (animate) delayCache.set(delayKey, delays);
      const emphasis1 = new Set(lane.emphasis1 ?? []);
      const emphasis2 = new Set(lane.emphasis2 ?? []);

      const cells: LaneStripCell[] = Array.from({ length: count }, (_, columnIndex) => {
        const value = lane.values[columnIndex] ?? 0;
        return {
          columnIndex,
          value,
          background: value > 0 ? `var(--tod-heatmap-level-${1 + thresholds.filter((t) => value > t).length})` : null,
          emphasis1Path: emphasis1.has(columnIndex)
            ? cornerMarkerPath(columnIndex * step, laneIndex * step, size, 'bottom-left')
            : null,
          emphasis2Path: emphasis2.has(columnIndex)
            ? cornerMarkerPath(columnIndex * step, laneIndex * step, size, 'top-right')
            : null,
          selected: selected?.laneIndex === laneIndex && selected.columnIndex === columnIndex,
          litDelay: delays[columnIndex] ?? null,
        };
      });

      return { key, lane, total: lane.total ?? lane.values.reduce((sum, v) => sum + v, 0), cells };
    });

    // only the delays of the lanes still there are kept
    this.delayCache = delayCache;
    return rows;
  });

  // the upper bounds of the quantile steps, distinct (fewer steps when the values have few distinct ones); as the heatmap
  private thresholdsOf(laneValues: number[]): number[] {
    const values = laneValues.filter((v) => v > 0).sort((a, b) => a - b);
    const result: number[] = [];
    if (values.length === 0) return result;

    for (let k = 1; k < LEVELS; k++) {
      const t = values[Math.max(0, Math.ceil((k / LEVELS) * values.length) - 1)];
      if (t < values[values.length - 1] && !result.includes(t)) result.push(t);
    }
    return result;
  }

  protected onCellClick(row: LaneStripRow, laneIndex: number, cell: LaneStripCell): void {
    if (!this.clickable() || cell.value <= 0) return;
    // a click on the selected cell unselects it
    this.selected.set(cell.selected ? null : { laneIndex, columnIndex: cell.columnIndex });
    this.cellClicked.emit({
      laneIndex,
      columnIndex: cell.columnIndex,
      lane: row.lane,
      column: this.columns()[cell.columnIndex],
      value: cell.value,
    });
  }

  protected tipContext(row: LaneStripRow, laneIndex: number, cell: LaneStripCell): LaneStripTipContext {
    return {
      column: this.columns()[cell.columnIndex],
      caption: row.lane.caption,
      value: cell.value,
      emphasis1: cell.emphasis1Path !== null,
      emphasis2: cell.emphasis2Path !== null,
      laneIndex,
      columnIndex: cell.columnIndex,
    };
  }
}
