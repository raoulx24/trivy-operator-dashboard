// a column of the graph (left to right): its caption, and the colors its nodes get unless a node has its own
export interface LayeredGraphColumn {
  label: string;
  // any css color; null or undefined = the default node color of the theme
  color?: string | null;
  // the text on the nodes of the column (meter-group style: the owner picks one that reads on the color)
  textColor?: string | null;
}

export interface LayeredGraphNode {
  id: string;
  // the index in columns
  column: number;
  label: string;
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
}

export interface LayeredGraphEdgeView {
  key: string;
  // the svg path
  path: string;
  active: boolean;
}

export interface LayeredGraphColumnLabel {
  text: string;
  // px: the center of the node column
  x: number;
}

// the context of the popover of a node
export type LayeredGraphTipContext = LayeredGraphNodeView;
