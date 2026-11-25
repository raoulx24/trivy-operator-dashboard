import { LayeredGraphColumn, LayeredGraphEdge, LayeredGraphNode } from '@shared/ui/layered-graph/layered-graph.types';

// the graph of the pivot widgets (History drift, New images): vulnerabilities -> images -> namespaces, top to bottom.
// The id of a node is its kind and its key ('p:pairKey', 'i:digest', 'n:name'); the value of a node is its lines (its
// images, its vulnerabilities and namespaces, its images). A path through a node is exact: a vulnerability is on an
// image, and the image runs in the namespace.

// the box of an image, as the graph needs it
export interface PivotGraphImage {
  digest: string;
  name: string;
  rows: { pairKey: string }[];
  namespaceNames: string[];
}

// the box of a pair (vulnerability id, severity), as the graph needs it
export interface PivotGraphPair {
  key: string;
  vulnerabilityId: string;
  severityId: number;
  isKnownExploited: boolean;
  rows: { digest: string }[];
}

// the control grows by this much when it has namespaces (the nodes and the columns), so they fit
const GROWTH = 1.2;
const NODE_HEIGHT = 30;
// px; the vulnerabilities are 176 x 1.1 (they were 220, then 20% less)
const COLUMN_WIDTHS = { vulnerabilities: 194, images: 260, namespaces: 150 };

// the theme variables of a severity (the ones of Trivy, the others are shown as unknown)
function severityVariable(severityId: number): string {
  return `--tod-layered-graph-severity-${severityId >= 0 && severityId <= 3 ? severityId : 4}`;
}

// the letter of a severity, for the tag of a vulnerability: Critical, High, Medium, Low, Unknown (the others: ?)
const SEVERITY_LETTERS = ['C', 'H', 'M', 'L', 'U'];

function severityLetter(severityId: number): string {
  return SEVERITY_LETTERS[severityId] ?? '?';
}

export function pivotGraphColumns(withNamespaces: boolean): LayeredGraphColumn[] {
  const grow = (px: number): number => (withNamespaces ? Math.round(px * GROWTH) : px);
  const columns: LayeredGraphColumn[] = [
    // the colors of the vulnerabilities are their severity's, on each node
    { label: 'Vulnerabilities', width: grow(COLUMN_WIDTHS.vulnerabilities), maxPerRow: 6, subColumns: 4 },
    {
      label: 'Images',
      color: 'var(--tod-layered-graph-pivot-image)',
      textColor: 'var(--tod-layered-graph-pivot-image-text)',
      width: grow(COLUMN_WIDTHS.images),
      maxPerRow: 5,
      subColumns: 4,
    },
  ];

  if (withNamespaces) {
    // one sub-row: the namespaces are side by side (the graph scrolls when they do not fit)
    columns.push({
      label: 'Namespaces',
      color: 'var(--tod-layered-graph-pivot-namespace)',
      textColor: 'var(--tod-layered-graph-pivot-namespace-text)',
      width: grow(COLUMN_WIDTHS.namespaces),
      subColumns: 1,
    });
  }

  return columns;
}

// px: the height of the nodes
export function pivotGraphNodeHeight(withNamespaces: boolean): number {
  return withNamespaces ? Math.round(NODE_HEIGHT * GROWTH) : NODE_HEIGHT;
}

// px: the space between the sub-rows of a layer, and between the layers (the graph is given them, so the height below is
// the one it draws)
export const PIVOT_GRAPH_SUB_GAP = 8;
export const PIVOT_GRAPH_LAYER_GAP = 32;
// the room under the drawing: the scroller's padding and the bar of its horizontal scroll
const BOTTOM_ROOM = 6 + 16;

// px: the height of the box of the graph, fixed: that of a full graph (the most sub-rows of each layer, with the namespaces
// and the grown nodes), so the widget does not change its height with the day, the selection or the data. A graph with less
// is centered in it.
export function pivotGraphHeight(): number {
  const columns = pivotGraphColumns(true);
  const nodeHeight = pivotGraphNodeHeight(true);
  const layers = columns.map((c) => {
    const rows = Math.max(1, Math.floor(c.subColumns ?? 1));

    return rows * nodeHeight + (rows - 1) * PIVOT_GRAPH_SUB_GAP;
  });

  return layers.reduce((sum, h) => sum + h, 0) + (layers.length - 1) * PIVOT_GRAPH_LAYER_GAP + BOTTOM_ROOM;
}

export function buildPivotGraph(
  images: PivotGraphImage[],
  pairs: PivotGraphPair[],
): { nodes: LayeredGraphNode[]; edges: LayeredGraphEdge[]; hasNamespaces: boolean; namespaceCount: number } {
  const nodes: LayeredGraphNode[] = [];
  const edges: LayeredGraphEdge[] = [];
  const imagesOfNamespace = new Map<string, number>();

  for (const box of pairs) {
    const imageCount = new Set(box.rows.map((r) => r.digest)).size;
    const variable = severityVariable(box.severityId);

    nodes.push({
      id: `p:${box.key}`,
      column: 0,
      label: box.vulnerabilityId,
      // the tag is the severity (C, H, M, L, U), and "exploited" after it for a known exploited pair (KEV / EUVD)
      tag: box.isKnownExploited ? [severityLetter(box.severityId), 'exploited'] : [severityLetter(box.severityId)],
      value: imageCount,
      weight: imageCount,
      color: `var(${variable})`,
      textColor: `var(${variable}-text)`,
    });
    new Set(box.rows.map((r) => r.digest)).forEach((digest) => edges.push({ from: `p:${box.key}`, to: `i:${digest}` }));
  }

  for (const box of images) {
    const pairKeys = new Set(box.rows.map((r) => r.pairKey));
    const namespaces = new Set(box.namespaceNames);
    // the value of an image is its lines: its vulnerabilities and its namespaces
    nodes.push({
      id: `i:${box.digest}`,
      column: 1,
      label: box.name,
      value: pairKeys.size + namespaces.size,
      weight: pairKeys.size,
    });
    namespaces.forEach((name) => {
      imagesOfNamespace.set(name, (imagesOfNamespace.get(name) ?? 0) + 1);
      edges.push({ from: `i:${box.digest}`, to: `n:${name}` });
    });
  }

  for (const [name, count] of imagesOfNamespace) {
    nodes.push({ id: `n:${name}`, column: 2, label: name, value: count, weight: count });
  }

  return { nodes, edges, hasNamespaces: imagesOfNamespace.size > 0, namespaceCount: imagesOfNamespace.size };
}
