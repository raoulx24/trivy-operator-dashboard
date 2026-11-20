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
  LayeredGraphClick,
  LayeredGraphColumn,
  LayeredGraphColumnLabel,
  LayeredGraphEdge,
  LayeredGraphEdgeView,
  LayeredGraphNode,
  LayeredGraphNodeView,
  LayeredGraphTipContext,
} from './layered-graph.types';

// the column label row: room for the chart label text (text-sm, 14px), with its baseline at LABEL_BASELINE
const LABEL_HEIGHT = 24;
const LABEL_BASELINE = 14;
const NODE_RADIUS = 6;
// the space between the edge of a node and its texts
const NODE_PADDING = 8;
// the average width of a character of the chart label (text-sm, 14px), to shorten the texts that do not fit
const CHAR_WIDTH = 7.2;
// the least space between two columns of nodes, so the edges can be seen
const MIN_COLUMN_GAP = 32;
// the rounds of the crossing reduction (one round: left to right, then right to left)
const SWEEPS = 4;
// how many of the hidden labels the popover of a "+n more" node lists
const HIDDEN_LABELS_IN_TIP = 12;

interface Item {
  id: string;
  column: number;
  // the ids of the nodes: one for a node, the hidden ones for a "+n more" node
  members: string[];
  more: boolean;
}

interface PlacedItem extends Item {
  x: number;
  // px, the top of the node
  y: number;
}

interface PlacedEdge {
  key: string;
  from: PlacedItem;
  to: PlacedItem;
  // the edges of the input behind it (a "+n more" node merges several)
  underlying: [string, string][];
  path: string;
}

// a graph in layers drawn in svg: the nodes are in columns (left to right), the edges go from a column to one further
// right (the typical use: package -> image -> namespace)
// - everything comes from the inputs: columns (captions, colors), nodes (column, label, value, colors) and edges
// - the nodes of a column are ordered to reduce the crossings of the edges (optimizeOrder)
// - maxNodesPerColumn caps a column: the heaviest nodes stay, the others are one "+n more" node
// - selectable makes the nodes clickable (nodeSelected); the selected node (selectedNode) has a ring, and the nodes
//   that are not on a path through it fade (what leads to it on the left, what it leads to on the right)
// - the width follows the width of the host; the height follows the biggest column
// - the control does not know what the columns, nodes or values mean
@Component({
  selector: 'app-layered-graph',
  standalone: true,
  templateUrl: './layered-graph.component.html',
  styleUrl: './layered-graph.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayeredGraphComponent {
  columns = input<LayeredGraphColumn[]>([]);
  nodes = input<LayeredGraphNode[]>([]);
  edges = input<LayeredGraphEdge[]>([]);

  // px
  nodeHeight = input<number>(30);
  nodeGap = input<number>(10);
  maxNodeWidth = input<number>(160);

  showColumnLabels = input<boolean>(true);
  // the unit of the value of a node, for the popover
  valueLabel = input<string>('');
  // the text of the node that stands for the nodes over the cap: "+3 more"
  moreText = input<string>('more');

  // the cap of the nodes of a column: one number for all columns, or one per column (null = no cap); the nodes over it
  // are one "+n more" node (a column over the cap by one node shows it all, a "+1 more" would hide nothing)
  maxNodesPerColumn = input<number | (number | null)[] | null>(null);
  // sort the nodes of the columns to reduce the crossings of the edges; off keeps the order of the input
  optimizeOrder = input<boolean>(true);

  // the nodes can be clicked (selects them, nodeSelected)
  selectable = input<boolean>(false);
  // the id of the selected node, two-way: a click on a node selects it, a click on the selected one unselects it (the
  // owner can also set it); null = none
  selectedNode = model<string | null>(null);
  // replaces the default popover; its context is a LayeredGraphTipContext
  tooltipTemplate = input<TemplateRef<{ $implicit: LayeredGraphTipContext }> | undefined>(undefined);

  nodeSelected = output<LayeredGraphClick>();

  protected readonly popover = inject(PopoverService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly width = signal(0);
  protected readonly nodeRadius = NODE_RADIUS;
  protected readonly nodePadding = NODE_PADDING;
  protected readonly labelBaseline = LABEL_BASELINE;
  protected readonly hiddenLabelsInTip = HIDDEN_LABELS_IN_TIP;

  constructor() {
    const destroyRef = inject(DestroyRef);
    // never leave the popover behind when the graph goes away while hovered
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

  // the input, cleaned: the nodes with a known column (the first of an id wins), the edges between two known nodes, from
  // a column to one further right (once each)
  private readonly graph = computed(() => {
    const columnCount = this.columns().length;
    const nodes = new Map<string, LayeredGraphNode>();
    for (const node of this.nodes()) {
      if (node.column >= 0 && node.column < columnCount && !nodes.has(node.id)) nodes.set(node.id, node);
    }

    const out = new Map<string, string[]>();
    const into = new Map<string, string[]>();
    const seen = new Set<string>();
    const edges: [string, string][] = [];
    for (const edge of this.edges()) {
      const from = nodes.get(edge.from);
      const to = nodes.get(edge.to);
      const key = `${edge.from}\u0000${edge.to}`;
      if (!from || !to || from.column >= to.column || seen.has(key)) continue;

      seen.add(key);
      edges.push([edge.from, edge.to]);
      out.set(edge.from, [...(out.get(edge.from) ?? []), edge.to]);
      into.set(edge.to, [...(into.get(edge.to) ?? []), edge.from]);
    }

    return { nodes, edges, out, into };
  });

  // the nodes of each column that stay after the cap (and the "+n more" node), in the order of the input
  private readonly items = computed<Item[][]>(() => {
    const columnCount = this.columns().length;
    const nodes = [...this.graph().nodes.values()];
    const caps = this.maxNodesPerColumn();

    return Array.from({ length: columnCount }, (_, column) => {
      const own = nodes.filter((n) => n.column === column);
      const raw = Array.isArray(caps) ? caps[column] : caps;
      const cap = raw === null || raw === undefined || Number.isNaN(raw) ? null : Math.max(0, Math.floor(raw));
      const asItem = (n: LayeredGraphNode): Item => ({ id: n.id, column, members: [n.id], more: false });

      if (cap === null || own.length <= cap + 1) return own.map(asItem);

      // the heaviest stay (the order of the input breaks a tie)
      const keep = new Set(
        own
          .map((n, index) => ({ n, index }))
          .sort((a, b) => (b.n.weight ?? 0) - (a.n.weight ?? 0) || a.index - b.index)
          .slice(0, cap)
          .map((entry) => entry.n.id),
      );
      const hidden = own.filter((n) => !keep.has(n.id)).map((n) => n.id);

      return [
        ...own.filter((n) => keep.has(n.id)).map(asItem),
        { id: `\u0000more-${column}`, column, members: hidden, more: true },
      ];
    });
  });

  // the drawing: where the nodes are, and the paths of the edges
  private readonly layout = computed(() => {
    const columnCount = this.columns().length;
    const width = this.width();
    const columnItems = this.items();
    const { edges } = this.graph();
    const nodeHeight = this.nodeHeight();
    const step = nodeHeight + this.nodeGap();
    const top = this.showColumnLabels() ? LABEL_HEIGHT : 0;
    const maxRows = Math.max(0, ...columnItems.map((c) => c.length));

    const itemOf = new Map<string, Item>();
    for (const column of columnItems) for (const item of column) for (const id of item.members) itemOf.set(id, item);

    // the edges between the items (the ones that end in a hidden node are merged into the "+n more" node)
    const merged = new Map<string, { from: Item; to: Item; underlying: [string, string][] }>();
    for (const [from, to] of edges) {
      const a = itemOf.get(from);
      const b = itemOf.get(to);
      if (!a || !b) continue;

      const key = `${a.id}\u0000${b.id}`;
      const entry = merged.get(key) ?? { from: a, to: b, underlying: [] };
      entry.underlying.push([from, to]);
      merged.set(key, entry);
    }

    const orders = columnItems.map((c) => [...c]);
    if (this.optimizeOrder()) this.reduceCrossings(orders, [...merged.values()], step, maxRows);

    // the columns of nodes are centered on the biggest one
    const nodeWidth = this.nodeWidthFor(width, columnCount);
    const placed = new Map<string, PlacedItem>();
    orders.forEach((order, column) => {
      const x = columnCount > 1 ? (column * (width - nodeWidth)) / (columnCount - 1) : 0;
      const offset = ((maxRows - order.length) * step) / 2;
      order.forEach((item, index) => placed.set(item.id, { ...item, x, y: top + offset + index * step }));
    });

    const placedEdges: PlacedEdge[] = [...merged.entries()].map(([key, entry]) => {
      const from = placed.get(entry.from.id)!;
      const to = placed.get(entry.to.id)!;
      const x1 = from.x + nodeWidth;
      const y1 = from.y + nodeHeight / 2;
      const x2 = to.x;
      const y2 = to.y + nodeHeight / 2;
      const middle = (x1 + x2) / 2;

      return { key, from, to, underlying: entry.underlying, path: `M${x1},${y1} C${middle},${y1} ${middle},${y2} ${x2},${y2}` };
    });

    return {
      nodeWidth,
      height: maxRows === 0 ? 0 : top + maxRows * step - this.nodeGap(),
      items: orders.flatMap((order) => order.map((item) => placed.get(item.id)!)),
      edges: placedEdges,
    };
  });

  protected readonly nodeWidth = computed(() => this.layout().nodeWidth);
  protected readonly height = computed(() => this.layout().height);
  protected readonly hasNodes = computed(() => this.layout().items.length > 0);

  // the nodes on a path through the selected one: everything that leads to it, and everything it leads to; null when
  // nothing is selected
  private readonly related = computed<Set<string> | null>(() => {
    const selected = this.selectedNode();
    const { nodes, out, into } = this.graph();
    if (selected === null || !nodes.has(selected)) return null;

    const result = new Set<string>([selected]);
    for (const next of [out, into]) {
      const queue = [selected];
      for (let id = queue.shift(); id !== undefined; id = queue.shift()) {
        for (const other of next.get(id) ?? []) {
          if (!result.has(other)) {
            result.add(other);
            queue.push(other);
          }
        }
      }
    }

    return result;
  });

  protected readonly nodeViews = computed<LayeredGraphNodeView[]>(() => {
    const columns = this.columns();
    const { nodes } = this.graph();
    const selectedId = this.selectedNode();
    const related = this.related();
    const selectable = this.selectable();
    const nodeWidth = this.nodeWidth();
    const moreText = this.moreText();

    return this.layout().items.map((item) => {
      const column = columns[item.column];
      const node = item.more ? null : nodes.get(item.id)!;
      const label = item.more ? `+${item.members.length} ${moreText}` : node!.label;
      const value = node?.value ?? null;
      const valueText = value === null ? '' : String(value);
      // the room for the label: the node without the paddings and the value
      const room = nodeWidth - 2 * NODE_PADDING - (valueText ? valueText.length * CHAR_WIDTH + NODE_PADDING : 0);
      const fits = Math.max(1, Math.floor(room / CHAR_WIDTH));

      return {
        id: item.id,
        label,
        column: item.column,
        columnLabel: column.label,
        value,
        more: item.more,
        hiddenLabels: item.more ? item.members.map((id) => nodes.get(id)!.label) : [],
        text: label.length > fits ? `${label.substring(0, Math.max(1, fits - 1))}…` : label,
        valueText,
        color: node?.color ?? column.color ?? null,
        textColor: node?.textColor ?? column.textColor ?? null,
        selectable: selectable && !item.more,
        selected: !item.more && item.id === selectedId,
        active: related === null || item.members.some((id) => related.has(id)),
        x: item.x,
        y: item.y,
      };
    });
  });

  protected readonly edgeViews = computed<LayeredGraphEdgeView[]>(() => {
    const related = this.related();

    return this.layout().edges.map((edge) => ({
      key: edge.key,
      path: edge.path,
      // an edge is on a path when one of the edges behind it has both its ends on one
      active: related === null || edge.underlying.some(([from, to]) => related.has(from) && related.has(to)),
    }));
  });

  protected readonly columnLabels = computed<LayeredGraphColumnLabel[]>(() => {
    const nodeWidth = this.nodeWidth();
    const count = this.columns().length;
    const width = this.width();

    return this.columns().map((column, i) => ({
      text: column.label,
      x: (count > 1 ? (i * (width - nodeWidth)) / (count - 1) : 0) + nodeWidth / 2,
    }));
  });

  protected onNodeClick(node: LayeredGraphNodeView): void {
    if (!node.selectable) return;
    // a click on the selected node unselects it
    this.selectedNode.set(node.selected ? null : node.id);
    this.nodeSelected.emit({
      id: node.id,
      label: node.label,
      column: node.column,
      columnLabel: node.columnLabel,
      value: node.value,
    });
  }

  private nodeWidthFor(width: number, columnCount: number): number {
    if (columnCount === 0) return 0;

    return Math.max(0, Math.min(this.maxNodeWidth(), (width - (columnCount - 1) * MIN_COLUMN_GAP) / columnCount));
  }

  // the barycenter method: each node goes where the mean height of its neighbors is, a column at a time, left to right
  // and back; the "+n more" node stays last in its column
  private reduceCrossings(
    orders: Item[][],
    edges: { from: Item; to: Item }[],
    step: number,
    maxRows: number,
  ): void {
    const left = new Map<string, string[]>();
    const right = new Map<string, string[]>();
    for (const { from, to } of edges) {
      left.set(to.id, [...(left.get(to.id) ?? []), from.id]);
      right.set(from.id, [...(right.get(from.id) ?? []), to.id]);
    }

    // the height of the top of each node, as drawn (the same offset for all, so it compares)
    const height = new Map<string, number>();
    const measure = (column: number): void =>
      orders[column].forEach((item, index) => {
        height.set(item.id, ((maxRows - orders[column].length) * step) / 2 + index * step);
      });
    orders.forEach((_, column) => measure(column));

    const sort = (column: number, neighbors: Map<string, string[]>): void => {
      const keyOf = (item: Item): number => {
        const around = (neighbors.get(item.id) ?? []).map((id) => height.get(id)!);

        return around.length === 0 ? height.get(item.id)! : around.reduce((a, b) => a + b, 0) / around.length;
      };
      const keys = new Map(orders[column].map((item) => [item.id, keyOf(item)]));
      const index = new Map(orders[column].map((item, i) => [item.id, i]));

      orders[column] = orders[column].sort(
        (a, b) =>
          Number(a.more) - Number(b.more) || keys.get(a.id)! - keys.get(b.id)! || index.get(a.id)! - index.get(b.id)!,
      );
      measure(column);
    };

    for (let round = 0; round < SWEEPS; round++) {
      for (let column = 1; column < orders.length; column++) sort(column, left);
      for (let column = orders.length - 2; column >= 0; column--) sort(column, right);
    }
  }
}
