// a column of the graph (left to right): its caption, and the colors its nodes get unless a node has its own
export interface LayeredGraphColumn {
  label: string;
  // any css color; null or undefined = the default node color of the theme
  color?: string | null;
  // the text on the nodes of the column (meter-group style: the owner picks one that reads on the color)
  textColor?: string | null;
  // the width (px) of each node of the column; without it, the nodes of the columns that have none share the room
  // (up to maxNodeWidth)
  width?: number | null;
  // the nodes of the column fill this many sub-columns, row by row (default 1): a column with many nodes is lower; with
  // orientation rows it is the count of sub-rows: a row of the layer is filled across, a sub-row after the other
  subColumns?: number;
  // rows only: the cutoff of a sub-row. Up to this many nodes the layer is one row, up to twice as many two rows, then
  // three, ...: the nodes are spread evenly over the rows. With subColumns as well, that is the most rows (more nodes
  // than that widen the rows beyond maxPerRow).
  maxPerRow?: number | null;
}

export interface LayeredGraphNode {
  id: string;
  // the index in columns
  column: number;
  label: string;
  // a small tag after the label (a kind, a status, ...): a darker shade of the node color, bordered in its text color;
  // left out when the node is too narrow for it
  tag?: string | null;
  // shown at the right of the node (a count, ...); also in the popover
  value?: number | string | null;
  // decides which nodes survive the cap (the heaviest stay); without it, the order of the input
  weight?: number;
  color?: string | null;
  textColor?: string | null;
}

// an edge goes from a node of a column to a node of a column further right
export interface LayeredGraphEdge {
  from: string;
  to: string;
}

// what a click says about the node
export interface LayeredGraphClick {
  id: string;
  label: string;
  column: number;
  columnLabel: string;
  value: number | string | null;
}

export interface LayeredGraphNodeView extends LayeredGraphClick {
  // the "+n more" node that stands for the nodes over the cap
  more: boolean;
  // the labels of the nodes behind a "+n more" node
  hiddenLabels: string[];
  // what is written on the node (shortened to fit)
  text: string;
  // the tag (empty when none, or when it does not fit), its width, where it starts (px in the node) and its fill (null:
  // the default of the theme)
  tag: string;
  tagWidth: number;
  tagX: number;
  tagFill: string | null;
  valueText: string;
  color: string | null;
  textColor: string | null;
  selectable: boolean;
  selected: boolean;
  // on a path through the selected node (everything is, when nothing is selected)
  active: boolean;
  // px, in the svg
  x: number;
  y: number;
  width: number;
}

export interface LayeredGraphEdgeView {
  key: string;
  // the svg path
  path: string;
  active: boolean;
}

export interface LayeredGraphColumnLabel {
  text: string;
  // px: the center of the column (all its sub-columns)
  x: number;
}

export interface LayeredGraphRowLabel {
  text: string;
  // px: the baseline of the caption of the layer, in the svg
  y: number;
}

// the context of the popover of a node
export type LayeredGraphTipContext = LayeredGraphNodeView;
