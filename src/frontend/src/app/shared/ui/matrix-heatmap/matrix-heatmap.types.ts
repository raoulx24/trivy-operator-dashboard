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
  selectable: boolean;
  selected: boolean;
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
