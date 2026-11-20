export interface LaneStripLane {
  // the text on the left
  caption: string;
  // one value per column (same order as the columns of the control); 0 or missing = empty cell
  values: number[];
  // the text on the right; the sum of the values when not given
  total?: string | number;
  // the columns (indexes) marked with a triangle: emphasis1 in the bottom-left corner, emphasis2 in the top-right one
  // (independent of the value; the control does not know what they mean)
  emphasis1?: number[];
  emphasis2?: number[];
}

export interface LaneStripCellClick {
  laneIndex: number;
  columnIndex: number;
  lane: LaneStripLane;
  column: string;
  value: number;
}

export interface LaneStripCell {
  columnIndex: number;
  value: number;
  // the fill: the lane color with an intensity that follows value / max of the lane; null for an empty cell
  background: string | null;
  // the path of the triangle in the bottom-left (emphasis1) / top-right (emphasis2) corner; null = none
  emphasis1Path: string | null;
  emphasis2Path: string | null;
  selected: boolean;
}

export interface LaneStripRow {
  lane: LaneStripLane;
  total: string | number;
  cells: LaneStripCell[];
}

// the context of the popover of a cell
export interface LaneStripTipContext {
  column: string;
  caption: string;
  value: number;
  emphasis1: boolean;
  emphasis2: boolean;
  laneIndex: number;
  columnIndex: number;
}

// a selected cell: the lane and the column (indexes)
export interface LaneStripSelection {
  laneIndex: number;
  columnIndex: number;
}
