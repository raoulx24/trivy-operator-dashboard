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
  model,
  output,
  signal,
} from '@angular/core';

import { PopoverService } from '../popover/popover.service';

import {
  MatrixHeatmapCell,
  MatrixHeatmapCellRef,
  MatrixHeatmapRowDto,
  MatrixHeatmapClick,
  MatrixHeatmapColumnLabel,
  MatrixHeatmapRowLabel,
  MatrixHeatmapTipContext,
} from './matrix-heatmap.types';

const LEVELS = 4;
// the column label row: room for the chart label text (text-sm, 14px), with its baseline at LABEL_BASELINE
const LABEL_HEIGHT = 24;
const LABEL_BASELINE = 14;
// the gap between the row labels and the first column
const ROW_LABEL_GAP = 8;
const CELL_RADIUS = 6;
// the legend swatches (px), as in the scss
const LEGEND_SWATCH = 17;
// the longest a cell takes to light on (ms)
const MAX_FADE = 300;

// a matrix heatmap drawn in svg: one cell per (row, column), shaded like the activity heatmap (quantile steps of the
// non-zero values; zero or missing is empty)
// - everything comes from the inputs: the column labels, and the rows: a label and one cell per column (value, optional own
//   css color instead of the shade, optional text color)
// - animate: when new data arrives the cells with a value light on one by one, in a random order of place, all within
//   animationDuration (ms) in total; data with the same values and colors does not play again (a change of the selected
//   cell does not); the empty cells are always there
// - the cell width follows the width of the host; the height is cellHeight
// - selectable makes the cells with a value clickable (cellSelected); the selected cell (selectedCell) has a ring
// - the popover shows the labels and the value (heatmap-style: it follows the mouse)
// - the control does not know what the rows, columns or values mean
@Component({
  selector: 'app-matrix-heatmap',
  standalone: true,
  templateUrl: './matrix-heatmap.component.html',
  styleUrl: './matrix-heatmap.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MatrixHeatmapComponent {
  // the labels of the columns (top)
  columns = input<string[]>([]);
  // the rows (the label on the left, a cell per column)
  rows = input<MatrixHeatmapRowDto[]>([]);

  // the cells light on when the data is received; the whole animation takes animationDuration ms
  animate = input<boolean>(false);
  animationDuration = input<number>(600);
  // anything that changes when the owner receives data again: the animation plays again for the same data too (e.g. a
  // refresh); without it only changed data plays
  replay = input<unknown>(undefined);

  // px
  cellHeight = input<number>(36);
  cellGap = input<number>(6);
  rowLabelWidth = input<number>(56);

  showColumnLabels = input<boolean>(true);
  showRowLabels = input<boolean>(true);
  showValues = input<boolean>(true);
  showLegend = input<boolean>(false);
  valueLabel = input<string>('');
  // the popover of the legend (on hover); none for an empty text
  valueHint = input<string>('');

  // the cells with a value can be clicked (selects them, cellSelected)
  selectable = input<boolean>(false);
  // the selected cell, two-way: a click on a cell with a value selects it, a click on the selected one unselects it
  // (the owner can also set it); null = none
  selectedCell = model<MatrixHeatmapCellRef | null>(null);
  // replaces the default popover; its context is a MatrixHeatmapTipContext
  tooltipTemplate = input<TemplateRef<{ $implicit: MatrixHeatmapTipContext }> | undefined>(undefined);

  cellSelected = output<MatrixHeatmapClick>();

  protected readonly popover = inject(PopoverService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly width = signal(0);
  // how long one cell takes to light on (the last one starts at animationDuration minus this, so everything is done
  // within animationDuration)
  protected readonly fade = computed(() => Math.min(MAX_FADE, Math.max(0, this.animationDuration())));
  protected readonly cellRadius = CELL_RADIUS;
  protected readonly labelBaseline = LABEL_BASELINE;
  protected readonly legendSwatchSize = LEGEND_SWATCH;
  protected readonly levels = Array.from({ length: LEVELS }, (_, i) => i + 1);

  constructor() {
    const destroyRef = inject(DestroyRef);
    // never leave the popover behind when the heatmap goes away while hovered
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

  // the upper bounds of the quantile steps, distinct (fewer steps when the values have few distinct ones)
  private readonly thresholds = computed<number[]>(() => {
    const all = this.rows()
      .flatMap((row) => row.cells.map((c) => c.value))
      .filter((v): v is number => typeof v === 'number' && v > 0)
      .sort((a, b) => a - b);

    if (all.length === 0) return [];

    const result: number[] = [];
    for (let k = 1; k < LEVELS; k++) {
      const t = all[Math.max(0, Math.ceil((k / LEVELS) * all.length) - 1)];
      if (t < all[all.length - 1] && !result.includes(t)) result.push(t);
    }
    return result;
  });

  protected readonly legendLevels = computed(() => this.levels.slice(0, this.thresholds().length + 1));

  private readonly top = computed(() => (this.showColumnLabels() ? LABEL_HEIGHT : 0));
  private readonly left = computed(() => (this.showRowLabels() ? this.rowLabelWidth() + ROW_LABEL_GAP : 0));

  // px
  protected readonly cellWidth = computed(() => {
    const count = this.columns().length;
    if (count === 0) return 0;
    return Math.max(0, (this.width() - this.left() - this.cellGap() * (count - 1)) / count);
  });

  protected readonly height = computed(() => {
    const count = this.rows().length;
    if (count === 0) return 0;
    return this.top() + count * (this.cellHeight() + this.cellGap()) - this.cellGap();
  });

  // what the animation depends on: the values and colors, and the labels; the delays are kept while it is unchanged, as a
  // changed delay would restart the animation of a cell that has already lit on
  private delayCache?: { key: string; id: number; delays: Map<string, number> };
  private delayCacheId = 0;

  // per cell ("row-column"), in ms: a random order over the cells with a value, evenly spread over the time left after
  // the fade of the last one
  private delaysOf(rows: MatrixHeatmapRowDto[], columns: string[], key: string) {
    if (this.delayCache?.key === key) return this.delayCache;

    const ids: string[] = [];
    rows.forEach((row, r) => columns.forEach((_, c) => (row.cells[c]?.value ?? 0) > 0 && ids.push(`${r}-${c}`)));
    // Fisher-Yates
    for (let i = ids.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [ids[i], ids[j]] = [ids[j], ids[i]];
    }

    const spread = Math.max(0, this.animationDuration() - this.fade());
    const delays = new Map(ids.map((id, i) => [id, ids.length > 1 ? (i / (ids.length - 1)) * spread : 0]));
    this.delayCache = { key, id: ++this.delayCacheId, delays };
    return this.delayCache;
  }

  protected readonly cells = computed<MatrixHeatmapCell[]>(() => {
    const rows = this.rows();
    const columns = this.columns();
    const dataKey = JSON.stringify([
      this.animationDuration(),
      this.replay(),
      columns,
      rows.map((r) => [r.label, r.cells.map((c) => [c.value ?? 0, c.color ?? null])]),
    ]);
    const lit = this.animate() ? this.delaysOf(rows, columns, dataKey) : undefined;
    const selected = this.selectedCell();
    const selectable = this.selectable();
    const cellWidth = this.cellWidth();
    const stepX = cellWidth + this.cellGap();
    const stepY = this.cellHeight() + this.cellGap();

    return rows.flatMap((row, rowIndex) =>
      columns.map((columnLabel, columnIndex) => {
        const cell = row.cells[columnIndex];
        const value = cell?.value ?? 0;
        return {
          rowIndex,
          columnIndex,
          rowLabel: row.label,
          columnLabel,
          value,
          level: this.levelOf(value),
          color: value > 0 ? (cell?.color ?? null) : null,
          textColor: value > 0 ? (cell?.textColor ?? null) : null,
          selectable: selectable && value > 0,
          selected: selected?.rowIndex === rowIndex && selected?.columnIndex === columnIndex,
          key: `${lit?.id ?? 0}|${rowIndex}-${columnIndex}`,
          litDelay: lit?.delays.get(`${rowIndex}-${columnIndex}`) ?? null,
          x: this.left() + columnIndex * stepX,
          y: this.top() + rowIndex * stepY,
        };
      }),
    );
  });

  protected readonly columnLabels = computed<MatrixHeatmapColumnLabel[]>(() =>
    this.columns().map((text, i) => ({
      text,
      x: this.left() + i * (this.cellWidth() + this.cellGap()) + this.cellWidth() / 2,
    })),
  );

  protected readonly rowLabels = computed<MatrixHeatmapRowLabel[]>(() =>
    this.rows().map((row, i) => ({
      text: row.label,
      y: this.top() + i * (this.cellHeight() + this.cellGap()) + this.cellHeight() / 2,
    })),
  );

  protected onCellClick(cell: MatrixHeatmapCell): void {
    if (!cell.selectable) return;
    // a click on the selected cell unselects it
    this.selectedCell.set(cell.selected ? null : { rowIndex: cell.rowIndex, columnIndex: cell.columnIndex });
    this.cellSelected.emit({
      rowIndex: cell.rowIndex,
      columnIndex: cell.columnIndex,
      rowLabel: cell.rowLabel,
      columnLabel: cell.columnLabel,
      value: cell.value,
    });
  }

  private levelOf(value: number): number {
    if (value <= 0) return 0;
    return 1 + this.thresholds().filter((t) => value > t).length;
  }
}
