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

import { PopoverService } from '../popover/popover.service';
import { ScrollerComponent } from '../scroller/scroller.component';

import { LaneStripCell, LaneStripCellClick, LaneStripLane, LaneStripRow, LaneStripSelection, LaneStripTipContext } from './lane-strip.types';

// the weakest fill of a non-empty cell (percent of the lane color), the strongest being 100
const MIN_INTENSITY = 35;

// lanes of squares on one shared horizontal axis: a caption on the left and a total on the right stay put, the squares
// scroll between them (app-scroller, at the end by default)
// - everything comes from the inputs: columns (their labels), lanes (caption, values, optional total), colors (one per
//   lane, taken in turn when there are fewer), highlightFrom (first highlighted column: outlined cells)
// - the intensity of a cell follows its value against the largest value of its lane; zero is an empty cell
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
  // css colors, one per lane (the first for the first lane...); used in turn when there are fewer than lanes
  colors = input<string[]>([]);
  // the first column of the highlighted range (to the end); null = no highlight
  highlightFrom = input<number | null>(null);

  // the selected cell, two-way: a click on a cell with a value selects it (the owner can also set it); null = none
  selected = model<LaneStripSelection | null>(null);

  cellSize = input<number>(24);
  cellGap = input<number>(4);

  // the cells with a value can be clicked (cellClicked)
  clickable = input<boolean>(true);
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

  protected readonly rows = computed<LaneStripRow[]>(() => {
    const colors = this.colors();
    const from = this.highlightFrom();
    const selected = this.selected();
    const count = this.columns().length;

    return this.lanes().map((lane, laneIndex) => {
      const color = colors.length > 0 ? colors[laneIndex % colors.length] : 'currentColor';
      const max = Math.max(0, ...lane.values);

      const cells: LaneStripCell[] = Array.from({ length: count }, (_, columnIndex) => {
        const value = lane.values[columnIndex] ?? 0;
        const intensity = max > 0 ? MIN_INTENSITY + (100 - MIN_INTENSITY) * (value / max) : 0;
        return {
          columnIndex,
          value,
          background: value > 0 ? `color-mix(in srgb, ${color} ${intensity.toFixed(1)}%, transparent)` : null,
          highlighted: from !== null && columnIndex >= from,
          selected: selected?.laneIndex === laneIndex && selected.columnIndex === columnIndex,
        };
      });

      return { lane, total: lane.total ?? lane.values.reduce((sum, v) => sum + v, 0), cells };
    });
  });

  protected onCellClick(row: LaneStripRow, laneIndex: number, cell: LaneStripCell): void {
    if (!this.clickable() || cell.value <= 0) return;
    this.selected.set({ laneIndex, columnIndex: cell.columnIndex });
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
      laneIndex,
      columnIndex: cell.columnIndex,
    };
  }
}
