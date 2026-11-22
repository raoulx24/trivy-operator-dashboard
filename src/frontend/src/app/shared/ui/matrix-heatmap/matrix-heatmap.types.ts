// what the owner says about a cell
export interface MatrixHeatmapCellDto {
  // null, undefined or 0 = empty
  value?: number | null;
  // any css color: replaces the shade of the value
  color?: string | null;
  // the css color of the value on the cell (for a cell with an own color)
  textColor?: string | null;
}

export interface MatrixHeatmapRowDto {
  label: string;
  // one per column (missing = empty)
  cells: MatrixHeatmapCellDto[];
}

export interface MatrixHeatmapCellRef {
  rowIndex: number;
  columnIndex: number;
}

// what a click says about the cell
export interface MatrixHeatmapClick extends MatrixHeatmapCellRef {
  rowLabel: string;
  columnLabel: string;
  value: number;
}

export interface MatrixHeatmapCell extends MatrixHeatmapClick {
  // 0 = empty, 1..n = quantile step
  level: number;
  // the own css color of the cell (replaces the quantile shade), if the owner gave one
  color: string | null;
  // the own css color of the value text, if the owner gave one
  textColor: string | null;
  selectable: boolean;
  selected: boolean;
  // the cell with the data it was made of: a cell with new data is a new element, so its animation runs again
  key: string;
  // ms after which the cell lights on (animate); null: not animated (an empty cell)
  litDelay: number | null;
  // px, in the svg
  x: number;
  y: number;
}

export interface MatrixHeatmapColumnLabel {
  text: string;
  // px: the center of the column
  x: number;
}

export interface MatrixHeatmapRowLabel {
  text: string;
  // px: the vertical center of the row
  y: number;
}

// the context of the popover of a cell
export type MatrixHeatmapTipContext = MatrixHeatmapCell;
