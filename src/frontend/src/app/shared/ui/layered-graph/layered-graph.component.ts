import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  Injector,
  TemplateRef,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  model,
  output,
  signal,
  untracked,
  viewChild,
} from '@angular/core';

import { PopoverService } from '../popover/popover.service';
import { ScrollerComponent } from '../scroller/scroller.component';

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
// the room around the drawing inside the scroller (left, right, bottom, in the scss too): a node grows on hover and has a
// ring, and the scroller would clip it
const SCROLL_PADDING = 6;
const NODE_RADIUS = 6;
// the space between the edge of a node and its texts
const NODE_PADDING = 8;
// the average width of a character of the chart label (text-sm, 14px), to shorten the texts that do not fit
const CHAR_WIDTH = 7.2;
// the least space between two columns of nodes, so the edges can be seen
const MIN_COLUMN_GAP = 32;
// the narrowest a node without its own width gets in a vertical graph (it overflows rather than getting thinner)
const MIN_NODE_WIDTH = 40;
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
  width: number;
}

// where a column is, and how wide its nodes are (px)
interface ColumnGeometry {
  x: number;
  nodeWidth: number;
  subColumns: number;
  // all the sub-columns with the gaps between them
  span: number;
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
// - it scrolls in an app-scroller (scroll): vertical (default) over maxHeight, or horizontal under minNodeWidth per
//   column; the captions of the columns stay on top, and the selected node is scrolled into view
// - the control does not know what the columns, nodes or values mean
@Component({
  selector: 'app-layered-graph',
  standalone: true,
  imports: [ScrollerComponent],
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
  // the widest a node gets when its column has no width of its own (columns[].width)
  maxNodeWidth = input<number>(160);
  // the space between the sub-columns of a column (columns[].subColumns)
  subColumnGap = input<number>(8);

  // which way it scrolls when it does not fit: vertical scrolls when the graph is higher than maxHeight (null = it never
  // scrolls); horizontal scrolls when the columns would be narrower than minNodeWidth (px)
  scroll = input<'vertical' | 'horizontal'>('vertical');
  maxHeight = input<number | null>(null);
  minNodeWidth = input<number>(100);

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
  // with a selection, the nodes on a path through it (the selected one and its peers) move to the middle of the graph,
  // the others go above and below; off keeps every node where it is
  centerSelection = input<boolean>(true);

  // the nodes can be clicked (selects them, nodeSelected)
  selectable = input<boolean>(false);
  // the id of the selected node, two-way: a click on a node selects it, a click on the selected one unselects it (the
  // owner can also set it); null = none
  selectedNode = model<string | null>(null);
  // replaces the default popover; its context is a LayeredGraphTipContext
  tooltipTemplate = input<TemplateRef<{ $implicit: LayeredGraphTipContext }> | undefined>(undefined);
  // ms before the popover of a node shows, like a tooltip (none while another one is open)
  tooltipDelay = input<number>(400);

  nodeSelected = output<LayeredGraphClick>();

  protected readonly popover = inject(PopoverService);
  private readonly injector = inject(Injector);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly scroller = viewChild(ScrollerComponent);
  private readonly content = viewChild<ElementRef<HTMLElement>>('content');

  // px: the room of the drawing in the scroller (the paddings are out)
  protected readonly width = signal(0);
  protected readonly nodeRadius = NODE_RADIUS;
  protected readonly nodePadding = NODE_PADDING;
  protected readonly labelBaseline = LABEL_BASELINE;
  protected readonly labelHeight = LABEL_HEIGHT;
  protected readonly hiddenLabelsInTip = HIDDEN_LABELS_IN_TIP;

  protected readonly tipOptions = computed(() => ({ delay: this.tooltipDelay() }));

  // px: the height of the box of a vertical scroller (it scrolls inside it): the drawing, or maxHeight when that is less;
  // null = the box follows the drawing (horizontal, or no maxHeight)
  protected readonly viewportHeight = computed<number | null>(() => {
    const max = this.maxHeight();
    if (this.scroll() !== 'vertical' || max === null) return null;

    return Math.min(max, (this.showColumnLabels() ? LABEL_HEIGHT : 0) + this.height() + SCROLL_PADDING);
  });

  // the sub-columns of each column (whole, at least 1)
  private readonly subColumns = computed(() => this.columns().map((c) => Math.max(1, Math.floor(c.subColumns ?? 1))));

  // the columns: where they are and how wide their nodes are. The columns with a width of their own keep it; the nodes of
  // the others share what is left (up to maxNodeWidth), none narrower than minNodeWidth (a horizontal graph scrolls then;
  // a vertical one gets MIN_NODE_WIDTH, and overflows). The room that remains is the gap between the columns.
  private readonly geometry = computed(() => {
    const columns = this.columns();
    const subs = this.subColumns();
    const count = columns.length;
    const subGap = this.subColumnGap();
    const room = this.width();
    if (count === 0) return { contentWidth: room, columns: [] as ColumnGeometry[] };

    const minNode = this.scroll() === 'horizontal' ? this.minNodeWidth() : MIN_NODE_WIDTH;
    const spanOf = (k: number, nodeWidth: number): number => k * nodeWidth + (k - 1) * subGap;

    const required =
      columns.reduce((sum, c, i) => sum + spanOf(subs[i], c.width ?? minNode), 0) + (count - 1) * MIN_COLUMN_GAP;
    const contentWidth = Math.max(room, required);

    let fixed = 0;
    let slots = 0;
    let slotGaps = 0;
    columns.forEach((c, i) => {
      if (c.width !== undefined && c.width !== null) {
        fixed += spanOf(subs[i], c.width);
      } else {
        slots += subs[i];
        slotGaps += (subs[i] - 1) * subGap;
      }
    });
    const shared =
      slots === 0
        ? 0
        : Math.min(
            this.maxNodeWidth(),
            Math.max(minNode, (contentWidth - (count - 1) * MIN_COLUMN_GAP - fixed - slotGaps) / slots),
          );

    const nodeWidths = columns.map((c) => c.width ?? shared);
    const spans = nodeWidths.map((w, i) => spanOf(subs[i], w));
    const columnGap = count > 1 ? Math.max(MIN_COLUMN_GAP, (contentWidth - spans.reduce((a, b) => a + b, 0)) / (count - 1)) : 0;

    let x = 0;
    const result = columns.map((_, i): ColumnGeometry => {
      const column = { x, nodeWidth: nodeWidths[i], subColumns: subs[i], span: spans[i] };
      x += spans[i] + columnGap;

      return column;
    });

    return { contentWidth, columns: result };
  });

  // px: the width of the drawing: the room, or more when the columns do not fit in it (it scrolls)
  protected readonly contentWidth = computed(() => this.geometry().contentWidth);

  constructor() {
    const destroyRef = inject(DestroyRef);
    // never leave the popover behind when the graph goes away while hovered
    destroyRef.onDestroy(() => this.popover.hide());

    // the svg is drawn in px: it follows the width of the host, less the paddings of the scroller (read from it, they
    // are its own) and our own; the scroller is as wide as its content, so it cannot be the one that is measured
    afterNextRender(() => {
      const element = this.host.nativeElement;
      const measure = (): void => {
        const container = this.scroller()?.scrollContainer?.nativeElement;
        const style = container ? getComputedStyle(container) : null;
        const padding = style ? parseFloat(style.paddingLeft) + parseFloat(style.paddingRight) : 0;
        this.width.set(Math.max(0, element.clientWidth - padding - 2 * SCROLL_PADDING));
      };
      measure();
      const observer = new ResizeObserver(measure);
      observer.observe(element);
      destroyRef.onDestroy(() => observer.disconnect());
    });

    // a new selection is scrolled into view (not on every layout change, or a resize would scroll)
    effect(() => {
      if (this.selectedNode() === null) return;

      untracked(() => afterNextRender({ write: () => this.scrollToSelected() }, { injector: this.injector }));
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

  // the order of the nodes in the columns (it does not depend on the selection, or the ordering would be done again at
  // every click), and the edges between them
  private readonly arrangement = computed(() => {
    const columnItems = this.items();
    const { edges } = this.graph();
    const step = this.nodeHeight() + this.nodeGap();
    const subs = this.subColumns();
    // the rows of the biggest column (a column fills its sub-columns row by row)
    const maxRows = Math.max(0, ...columnItems.map((c, i) => Math.ceil(c.length / subs[i])));

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
    if (this.optimizeOrder()) this.reduceCrossings(orders, [...merged.values()], subs, step, maxRows);

    return { orders, merged: [...merged.entries()], maxRows };
  });

  // the drawing: where the nodes are, and the paths of the edges
  private readonly layout = computed(() => {
    const { columns: geometry } = this.geometry();
    const { orders, merged, maxRows } = this.arrangement();
    const related = this.centerSelection() ? this.related() : null;
    const nodeHeight = this.nodeHeight();
    const gap = this.nodeGap();
    const subGap = this.subColumnGap();
    const step = nodeHeight + gap;
    // the captions are not in this svg (they stay on top when it scrolls), so the nodes start at 0
    const baseHeight = maxRows === 0 ? 0 : maxRows * step - gap;
    // the line through the middle of the graph: the nodes on a path through the selected one are centered on it
    const middleLine = baseHeight / 2;

    // a column is three blocks: the nodes above the middle block, the middle block (the nodes on a path through the
    // selected one, or the whole column when nothing is selected), and the nodes below it; each block keeps its order
    // and fills the sub-columns of the column row by row
    const placed = new Map<string, PlacedItem>();
    orders.forEach((order, column) => {
      const { x, nodeWidth, subColumns: k } = geometry[column];
      const isMiddle = (item: Item): boolean => related === null || item.members.some((id) => related.has(id));
      const middle = order.filter(isMiddle);
      const others = order.filter((item) => !isMiddle(item));
      // the other nodes are split in two halves, the bigger one above
      const above = others.slice(0, Math.ceil(others.length / 2));
      const below = others.slice(above.length);

      const rows = (count: number): number => Math.ceil(count / k);
      const middleTop = middleLine - (rows(middle.length) * step - gap) / 2;
      const put = (block: Item[], top: number): void =>
        block.forEach((item, i) =>
          placed.set(item.id, {
            ...item,
            width: nodeWidth,
            x: x + (i % k) * (nodeWidth + subGap),
            y: top + Math.floor(i / k) * step,
          }),
        );

      put(above, middleTop - rows(above.length) * step);
      put(middle, middleTop);
      put(below, middleTop + rows(middle.length) * step);
    });

    // the halves can stick out of the graph by half a step: everything moves down (and the graph grows) to make room
    let shift = 0;
    let bottom = baseHeight;
    for (const item of placed.values()) {
      shift = Math.max(shift, -item.y);
      bottom = Math.max(bottom, item.y + nodeHeight);
    }
    for (const item of placed.values()) item.y += shift;

    const placedEdges: PlacedEdge[] = merged.map(([key, entry]) => {
      const from = placed.get(entry.from.id)!;
      const to = placed.get(entry.to.id)!;
      const x1 = from.x + from.width;
      const y1 = from.y + nodeHeight / 2;
      const x2 = to.x;
      const y2 = to.y + nodeHeight / 2;
      const middleX = (x1 + x2) / 2;

      return { key, from, to, underlying: entry.underlying, path: `M${x1},${y1} C${middleX},${y1} ${middleX},${y2} ${x2},${y2}` };
    });

    return {
      height: maxRows === 0 ? 0 : bottom + shift,
      items: orders.flatMap((order) => order.map((item) => placed.get(item.id)!)),
      edges: placedEdges,
    };
  });

  protected readonly height = computed(() => this.layout().height);
  protected readonly hasNodes = computed(() => this.layout().items.length > 0);

  protected readonly nodeViews = computed<LayeredGraphNodeView[]>(() => {
    const columns = this.columns();
    const { nodes } = this.graph();
    const selectedId = this.selectedNode();
    const related = this.related();
    const selectable = this.selectable();
    const moreText = this.moreText();

    return this.layout().items.map((item) => {
      const column = columns[item.column];
      const node = item.more ? null : nodes.get(item.id)!;
      const label = item.more ? `+${item.members.length} ${moreText}` : node!.label;
      const value = node?.value ?? null;
      const valueText = value === null ? '' : String(value);
      // the room for the label: the node without the paddings and the value
      const room = item.width - 2 * NODE_PADDING - (valueText ? valueText.length * CHAR_WIDTH + NODE_PADDING : 0);
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
        width: item.width,
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
    const { columns } = this.geometry();

    return this.columns().map((column, i) => ({ text: column.label, x: columns[i].x + columns[i].span / 2 }));
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

  // scrolls the selected node to the middle of what is visible (below the captions); the axis that does not scroll
  // stays where it is
  private scrollToSelected(): void {
    const element = this.scroller()?.scrollContainer?.nativeElement;
    const content = this.content()?.nativeElement;
    const node = this.nodeViews().find((n) => n.selected);
    if (!element || !content || !node) return;

    // where the content starts inside what scrolls (the scroller has paddings of its own)
    const box = element.getBoundingClientRect();
    const start = content.getBoundingClientRect();
    const offsetX = start.left - box.left + element.scrollLeft;
    const offsetY = start.top - box.top + element.scrollTop;

    const captions = this.showColumnLabels() ? LABEL_HEIGHT : 0;
    const left = offsetX + SCROLL_PADDING + node.x + node.width / 2 - element.clientWidth / 2;
    const top = offsetY + node.y + this.nodeHeight() / 2 - (element.clientHeight - captions) / 2;
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    element.scrollTo({ left, top, behavior: reduced ? 'auto' : 'smooth' });
  }

  // the barycenter method: each node goes where the mean height of its neighbors is, a column at a time, left to right
  // and back; the "+n more" node stays last in its column
  private reduceCrossings(
    orders: Item[][],
    edges: { from: Item; to: Item }[],
    subs: number[],
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
    const measure = (column: number): void => {
      const rows = Math.ceil(orders[column].length / subs[column]);
      orders[column].forEach((item, index) => {
        height.set(item.id, ((maxRows - rows) * step) / 2 + Math.floor(index / subs[column]) * step);
      });
    };
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
