import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  Injector,
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

import { CHART_TIP_SOURCE, ChartTip } from '../popover/chart-tip';
import { ScrollerComponent } from '../scroller/scroller.component';

import {
  LayeredGraphClick,
  LayeredGraphColumn,
  LayeredGraphColumnLabel,
  LayeredGraphEdge,
  LayeredGraphEdgeView,
  LayeredGraphNode,
  LayeredGraphNodeView,
  LayeredGraphRowLabel,
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
// the narrowest a node without its own width gets in a vertical graph (it overflows rather than getting thinner)
const MIN_NODE_WIDTH = 40;
// px between the captions of the layers (rows) and the nodes
const LABEL_GUTTER_GAP = 12;
// the tag of a node (a small label after its text): the average width of a character (10px), the room at its sides, the
// space before it, its height, and how many characters the label must keep for the tag to be shown
const TAG_CHAR_WIDTH = 5.8;
const TAG_PADDING = 5;
const TAG_GAP = 10;
const MIN_LABEL_WITH_TAG = 3;

// a color a step (100) darker: var(--p-red-500) is var(--p-red-600); any other color is mixed with black
function darker(color: string): string {
  const match = /^var\(--p-([a-z]+)-(\d{2,3})\)$/.exec(color.trim());
  const shade = match ? Number(match[2]) : 0;
  if (match && shade >= 50 && shade < 900) return `var(--p-${match[1]}-${shade === 50 ? 100 : shade + 100})`;

  return `color-mix(in srgb, ${color}, black 15%)`;
}

// the rounds of the crossing reduction (one round: left to right, then right to left)
const SWEEPS = 4;

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

// where a layer is along the way the layers go (x for columns, y for rows), and how wide its nodes are (px)
interface ColumnGeometry {
  start: number;
  nodeWidth: number;
  // the sub-columns (columns) or the sub-rows (rows)
  subColumns: number;
  // the size of the layer along the way the layers go: all the sub-columns with the gaps between them (columns); the
  // caption and all the sub-rows (rows)
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
  providers: [{ provide: CHART_TIP_SOURCE, useExisting: LayeredGraphComponent }],
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

  // the way the layers go: 'columns' = left to right (the nodes of a layer are one under the other, the sub-columns of
  // a layer are side by side); 'rows' = top to bottom (the nodes of a layer are side by side, a layer is split in
  // sub-rows: columns[].subColumns is then the count of sub-rows, and the caption of a layer is above it)
  orientation = input<'columns' | 'rows'>('columns');
  // the least space between two layers, where the edges run; in rows it is the space (px)
  layerGap = input<number>(32);

  // which way it scrolls when it does not fit: vertical scrolls when the graph is higher than maxHeight (null = it never
  // scrolls); horizontal scrolls when the columns would be narrower than minNodeWidth (px). Without it: vertical for
  // columns, horizontal for rows
  scroll = input<'vertical' | 'horizontal' | null>(null);
  maxHeight = input<number | null>(null);
  minNodeWidth = input<number>(100);

  showColumnLabels = input<boolean>(true);
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
  // several nodes can be selected (selectedNodes instead of selectedNode): at most one per column, and each one a direct
  // peer (an edge) of the selected node in the next column. A click on:
  // - a selected node unselects it, and the selected nodes left with no selected peer go too (a lone one stays);
  // - a node in a column that has a selection replaces it: the selected nodes that are not peers of it go (and the ones
  //   that were only connected through them), so a click on a node that is a peer of none starts a new selection;
  // - a node in a column with no selection adds it when it is a peer of a selected node, and is ignored when not.
  multiSelect = input<boolean>(false);
  selectedNodes = model<string[]>([]);

  nodeSelected = output<LayeredGraphClick>();

  // the popover of an item is up to the owner (see ChartTipDirective)
  readonly tipShow = output<ChartTip<LayeredGraphTipContext>>();
  readonly tipMove = output<MouseEvent>();
  readonly tipHide = output<void>();

  private readonly injector = inject(Injector);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  // the node that was just clicked (multi select): the one that is scrolled into view
  private focusId: string | null = null;
  // the data (columns, nodes, edges) that the graph was last centered for
  private centeredFor: unknown[] | null = null;
  private readonly scroller = viewChild(ScrollerComponent);
  private readonly content = viewChild<ElementRef<HTMLElement>>('content');

  // px: the room of the drawing in the scroller (the paddings are out)
  protected readonly width = signal(0);
  protected readonly nodeRadius = NODE_RADIUS;
  protected readonly nodePadding = NODE_PADDING;
  protected readonly labelBaseline = LABEL_BASELINE;
  protected readonly labelHeight = LABEL_HEIGHT;

  // px: the height of the tag of a node (a little less than the node)
  protected readonly tagHeight = computed(() => Math.max(10, Math.min(16, this.nodeHeight() - 6)));


  // px: the height of the box of a vertical scroller (it scrolls inside it): the drawing, or maxHeight when that is less;
  // null = the box follows the drawing (horizontal, or no maxHeight)
  protected readonly direction = computed<'vertical' | 'horizontal'>(
    () => this.scroll() ?? (this.orientation() === 'rows' ? 'horizontal' : 'vertical'),
  );

  protected readonly viewportHeight = computed<number | null>(() => {
    const max = this.maxHeight();
    if (this.direction() !== 'vertical' || max === null) return null;

    return Math.min(max, (this.captionsOnTop() ? LABEL_HEIGHT : 0) + this.height() + SCROLL_PADDING);
  });

  // the captions stay on top (their own svg) when the layers are columns; with rows they are in the svg, above the layers
  protected readonly captionsOnTop = computed(() => this.showColumnLabels() && this.orientation() === 'columns');

  // the sub-columns of each column (whole, at least 1)
  // (rows: with maxPerRow, as many as the nodes need: up to maxPerRow nodes are one sub-row, up to twice that two, ...)
  private readonly subColumns = computed(() => {
    const rowsMode = this.orientation() === 'rows';
    const items = rowsMode ? this.items() : [];

    return this.columns().map((c, i) => {
      const cutoff = c.maxPerRow ?? 0;
      if (rowsMode && cutoff >= 1) {
        const needed = Math.max(1, Math.ceil((items[i]?.length ?? 0) / Math.floor(cutoff)));

        // with subColumns too, it is the most sub-rows: over that the rows get more than maxPerRow nodes
        return c.subColumns ? Math.min(needed, Math.max(1, Math.floor(c.subColumns))) : needed;
      }

      return Math.max(1, Math.floor(c.subColumns ?? 1));
    });
  });

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

    const minNode = this.direction() === 'horizontal' ? this.minNodeWidth() : MIN_NODE_WIDTH;
    if (this.orientation() === 'rows') return this.rowsGeometry(room, minNode);

    const layerGap = this.layerGap();
    const spanOf = (k: number, nodeWidth: number): number => k * nodeWidth + (k - 1) * subGap;

    const required =
      columns.reduce((sum, c, i) => sum + spanOf(subs[i], c.width ?? minNode), 0) + (count - 1) * layerGap;
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
            Math.max(minNode, (contentWidth - (count - 1) * layerGap - fixed - slotGaps) / slots),
          );

    const nodeWidths = columns.map((c) => c.width ?? shared);
    const spans = nodeWidths.map((w, i) => spanOf(subs[i], w));
    const columnGap = count > 1 ? Math.max(layerGap, (contentWidth - spans.reduce((a, b) => a + b, 0)) / (count - 1)) : 0;

    let x = 0;
    const result = columns.map((_, i): ColumnGeometry => {
      const column = { start: x, nodeWidth: nodeWidths[i], subColumns: subs[i], span: spans[i] };
      x += spans[i] + columnGap;

      return column;
    });

    return { contentWidth, columns: result };
  });

  // px: the width of the strip at the left where the captions of the layers are (rows only), as wide as the longest
  protected readonly labelGutter = computed(() => {
    if (this.orientation() !== 'rows' || !this.showColumnLabels()) return 0;
    const counts = this.items().map((layer) => layer.length);
    const longest = Math.max(0, ...this.columns().map((c, i) => (counts[i] > 0 ? c.label.length : 0)));

    return longest === 0 ? 0 : Math.ceil(longest * CHAR_WIDTH) + LABEL_GUTTER_GAP;
  });

  // the layers as rows: each layer is as high as its sub-rows; the nodes of the layers without a width
  // of their own share the room, so that the layer with the most nodes in a sub-row fits (up to maxNodeWidth, none
  // narrower than minNode: a horizontal graph scrolls then)
  private rowsGeometry(room: number, minNode: number): { contentWidth: number; columns: ColumnGeometry[] } {
    const columns = this.columns();
    const subs = this.subColumns();
    const counts = this.items().map((layer) => layer.length);
    const nodeGap = this.nodeGap();
    const subGap = this.subColumnGap();
    const layerGap = this.layerGap();
    // the captions are at the left (labelGutter), so the room of the nodes is what is left of it
    room = Math.max(0, room - this.labelGutter());

    // the nodes side by side in a sub-row (the layer is filled sub-row after sub-row, across)
    const across = counts.map((n, i) => Math.ceil(n / subs[i]));
    const extentOf = (n: number, nodeWidth: number): number => n * nodeWidth + Math.max(0, n - 1) * nodeGap;

    const required = Math.max(0, ...across.map((n, i) => extentOf(n, columns[i].width ?? minNode)));
    const contentWidth = Math.max(room, required);

    const widest = Math.max(0, ...across.filter((_, i) => columns[i].width === undefined || columns[i].width === null));
    const shared =
      widest === 0
        ? 0
        : Math.min(this.maxNodeWidth(), Math.max(minNode, (contentWidth - (widest - 1) * nodeGap) / widest));

    let y = 0;
    const result = columns.map((column, i): ColumnGeometry => {
      // an empty layer takes no room
      if (counts[i] === 0) return { start: y, nodeWidth: column.width ?? shared, subColumns: subs[i], span: 0 };

      const used = Math.min(subs[i], counts[i]);
      const span = used * this.nodeHeight() + (used - 1) * subGap;
      const layer = { start: y, nodeWidth: column.width ?? shared, subColumns: subs[i], span };
      y += span + layerGap;

      return layer;
    });

    return { contentWidth, columns: result };
  }

  // px: the width of the drawing: the room, or more when the columns do not fit in it (it scrolls)
  protected readonly contentWidth = computed(() => this.geometry().contentWidth);

  constructor() {
    const destroyRef = inject(DestroyRef);

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
      if (this.selection().length === 0) return;

      untracked(() => afterNextRender({ write: () => this.scrollToSelected() }, { injector: this.injector }));
    });

    // the first time something is drawn (the width is known only after the first render), and each time new data is fed
    // (other columns, nodes or edges), it is centered: the selected node, or the middle of the graph. A resize or a
    // selection does not do it.
    effect(() => {
      const data = [this.columns(), this.nodes(), this.edges()];
      if (this.width() <= 0 || !this.hasNodes()) return;
      if (this.centeredFor?.every((part, i) => part === data[i])) return;

      this.centeredFor = data;
      untracked(() => afterNextRender({ write: () => this.scrollToSelected(true) }, { injector: this.injector }));
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

  // the ids that are selected (the ones that are in the graph)
  private readonly selection = computed<string[]>(() => {
    const { nodes } = this.graph();
    const ids = this.multiSelect() ? this.selectedNodes() : [this.selectedNode()];

    return ids.filter((id): id is string => id !== null && nodes.has(id));
  });

  // the nodes on a path through the selected ones (through all of them): everything that leads to a selected node, and
  // everything it leads to, for each of them; null when nothing is selected
  private readonly onPath = computed<Set<string> | null>(() => {
    const selected = this.selection();
    if (selected.length === 0) return null;

    const { out, into } = this.graph();
    const reach = (start: string): Set<string> => {
      const result = new Set<string>([start]);
      for (const next of [out, into]) {
        const queue = [start];
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
    };

    const [first, ...others] = selected.map(reach);

    return new Set([...first].filter((id) => others.every((set) => set.has(id))));
  });

  // what stays lit with a selection: the nodes on a path through the selected ones, and the direct peers of each selected
  // node (with several selected, the peers of one are not on a path through all, but they are what can be clicked next)
  private readonly related = computed<Set<string> | null>(() => {
    const path = this.onPath();
    if (path === null) return null;

    const { out, into } = this.graph();
    const result = new Set(path);
    for (const id of this.selection()) {
      for (const other of [...(out.get(id) ?? []), ...(into.get(id) ?? [])]) result.add(other);
    }

    return result;
  });

  // two nodes are peers when an edge joins them (either way)
  private isPeer(a: string, b: string): boolean {
    const { out } = this.graph();

    return (out.get(a)?.includes(b) ?? false) || (out.get(b)?.includes(a) ?? false);
  }

  // the selected nodes, in groups of nodes that are peers of one another (through the others of the group)
  private groups(ids: string[]): string[][] {
    const left = new Set(ids);
    const result: string[][] = [];
    for (const start of ids) {
      if (!left.has(start)) continue;

      left.delete(start);
      const group = [start];
      for (let i = 0; i < group.length; i++) {
        for (const other of [...left]) {
          if (this.isPeer(group[i], other)) {
            left.delete(other);
            group.push(other);
          }
        }
      }
      result.push(group);
    }

    return result;
  }

  // the selection after a click on a node (multi select); null when the click is ignored
  private nextSelection(current: string[], id: string): string[] | null {
    const { nodes } = this.graph();
    const columnOf = (other: string): number => nodes.get(other)!.column;

    if (current.includes(id)) {
      // the nodes left with no peer go, unless only one is left; when what is left is cut in pieces, the biggest stays
      const rest = current.filter((other) => other !== id);
      if (rest.length <= 1) return rest;

      const groups = this.groups(rest).filter((group) => group.length > 1);

      return groups.sort((a, b) => b.length - a.length || Math.min(...a.map(columnOf)) - Math.min(...b.map(columnOf)))[0] ?? [];
    }

    if (current.length === 0) return [id];

    const rest = current.filter((other) => columnOf(other) !== columnOf(id));
    const occupied = rest.length < current.length;
    if (!occupied && !rest.some((other) => this.isPeer(other, id))) return null;

    // what stays is what is connected to the clicked node through the selected nodes
    return this.groups([id, ...rest])[0];
  }

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
    const rowsMode = this.orientation() === 'rows';
    const { columns: geometry, contentWidth } = this.geometry();
    const { orders, merged, maxRows } = this.arrangement();
    const related = this.centerSelection() ? this.related() : null;
    const nodeHeight = this.nodeHeight();
    const gap = this.nodeGap();
    const subGap = this.subColumnGap();

    // The nodes of a layer go in three blocks along the cross axis (y for columns, x for rows): the nodes before the
    // middle block, the middle block (the nodes on a path through the selected one, or the whole layer when nothing is
    // selected), and the nodes after it. Each block keeps its order and fills the sub-columns (sub-rows) of the layer.
    // `pitch` is the size of a node on the cross axis with the gap, `middleLine` the line the middle blocks are centered
    // on: in columns the captions are not in this svg (they stay on top when it scrolls), so the nodes start at 0.
    const pitch = rowsMode ? 0 : nodeHeight + gap;
    const crossSize = rowsMode ? contentWidth : maxRows === 0 ? 0 : maxRows * pitch - gap;
    const middleLine = crossSize / 2;

    const placed = new Map<string, PlacedItem>();
    orders.forEach((order, layer) => {
      const { start, nodeWidth, subColumns: k } = geometry[layer];
      const isMiddle = (item: Item): boolean => related === null || item.members.some((id) => related.has(id));
      const middle = order.filter(isMiddle);
      const others = order.filter((item) => !isMiddle(item));
      // the other nodes are split in two halves, the bigger one first (above, or at the left)
      const before = others.slice(0, Math.ceil(others.length / 2));
      const after = others.slice(before.length);

      // the places across a block (the rows of a column, the nodes side by side of a row) and their pitch
      const places = (count: number): number => Math.ceil(count / k);
      const size = rowsMode ? nodeWidth + gap : pitch;
      const middleStart = middleLine - (places(middle.length) * size - gap) / 2;
      const put = (block: Item[], from: number): void =>
        block.forEach((item, i) => {
          const across = from + Math.floor(i / k) * size;
          const along = i % k;
          placed.set(
            item.id,
            rowsMode
              ? { ...item, width: nodeWidth, x: across, y: start + along * (nodeHeight + subGap) }
              : { ...item, width: nodeWidth, x: start + along * (nodeWidth + subGap), y: across },
          );
        });

      put(before, middleStart - places(before.length) * size);
      put(middle, middleStart);
      put(after, middleStart + places(middle.length) * size);
    });

    // the blocks can stick out of the graph by half a step: everything moves (and the graph grows) to make room
    let shift = 0;
    let far = rowsMode ? Math.max(0, ...geometry.map((g) => g.start + g.span)) : crossSize;
    let wide = contentWidth;
    for (const item of placed.values()) {
      if (rowsMode) {
        shift = Math.max(shift, -item.x);
        wide = Math.max(wide, item.x + item.width);
      } else {
        shift = Math.max(shift, -item.y);
        far = Math.max(far, item.y + nodeHeight);
      }
    }
    // rows: the nodes start after the strip of the captions
    if (rowsMode) shift += this.labelGutter();
    for (const item of placed.values()) {
      if (rowsMode) item.x += shift;
      else item.y += shift;
    }

    const placedEdges: PlacedEdge[] = merged.map(([key, entry]) => {
      const from = placed.get(entry.from.id)!;
      const to = placed.get(entry.to.id)!;
      // columns: from the right of a node to the left of the next; rows: from the bottom to the top
      const x1 = rowsMode ? from.x + from.width / 2 : from.x + from.width;
      const y1 = rowsMode ? from.y + nodeHeight : from.y + nodeHeight / 2;
      const x2 = rowsMode ? to.x + to.width / 2 : to.x;
      const y2 = rowsMode ? to.y : to.y + nodeHeight / 2;
      const middleX = (x1 + x2) / 2;
      const middleY = (y1 + y2) / 2;
      const path = rowsMode
        ? `M${x1},${y1} C${x1},${middleY} ${x2},${middleY} ${x2},${y2}`
        : `M${x1},${y1} C${middleX},${y1} ${middleX},${y2} ${x2},${y2}`;

      return { key, from, to, underlying: entry.underlying, path };
    });

    const empty = maxRows === 0;

    return {
      width: rowsMode ? wide + shift : contentWidth,
      height: empty ? 0 : rowsMode ? far : far + shift,
      items: orders.flatMap((order) => order.map((item) => placed.get(item.id)!)),
      edges: placedEdges,
    };
  });

  // px: the size of the svg of the nodes
  protected readonly drawingWidth = computed(() => this.layout().width);
  protected readonly height = computed(() => this.layout().height);
  protected readonly hasNodes = computed(() => this.layout().items.length > 0);

  protected readonly nodeViews = computed<LayeredGraphNodeView[]>(() => {
    const columns = this.columns();
    const { nodes } = this.graph();
    const selected = new Set(this.selection());
    const related = this.related();
    const selectable = this.selectable();
    const moreText = this.moreText();
    // multi select: a node in a column with no selection can be clicked when it is a peer of a selected node
    const multi = this.multiSelect() && selected.size > 0;
    const takenColumns = new Set([...selected].map((id) => nodes.get(id)!.column));

    return this.layout().items.map((item) => {
      const column = columns[item.column];
      const node = item.more ? null : nodes.get(item.id)!;
      const label = item.more ? `+${item.members.length} ${moreText}` : node!.label;
      const value = node?.value ?? null;
      const valueText = value === null ? '' : String(value);
      // the room for the label: the node without the paddings and the value
      const room = item.width - 2 * NODE_PADDING - (valueText ? valueText.length * CHAR_WIDTH + NODE_PADDING : 0);
      // the tag is at the right of the label, one part for each text; the parts are taken in order while the label would
      // keep a few characters, so a node too narrow for the first one has no tag
      const texts = (Array.isArray(node?.tag) ? node.tag : [node?.tag ?? ''])
        .map((x) => x.trim())
        .filter((x) => x !== '');
      const drawn: { text: string; width: number; offset: number }[] = [];
      let tagWidth = 0;
      for (const part of texts) {
        const width = Math.ceil(part.length * TAG_CHAR_WIDTH) + 2 * TAG_PADDING;
        if (room - tagWidth - width - TAG_GAP < MIN_LABEL_WITH_TAG * CHAR_WIDTH) break;

        drawn.push({ text: part, width, offset: tagWidth });
        tagWidth += width;
      }
      const fits = Math.max(1, Math.floor((tagWidth > 0 ? room - tagWidth - TAG_GAP : room) / CHAR_WIDTH));
      const text = label.length > fits ? `${label.substring(0, Math.max(1, fits - 1))}…` : label;
      // the tag is at the end of the node, just before the value: its place does not depend on the width of the label
      const tagX = item.width - NODE_PADDING - (valueText ? valueText.length * CHAR_WIDTH + NODE_PADDING : 0) - tagWidth;
      const color = node?.color ?? column.color ?? null;

      return {
        id: item.id,
        label,
        column: item.column,
        columnLabel: column.label,
        value,
        more: item.more,
        hiddenLabels: item.more ? item.members.map((id) => nodes.get(id)!.label) : [],
        text,
        tag: texts.join(' · '),
        tagParts: drawn.map((part, i) => ({
          text: part.text,
          x: tagX + part.offset + part.width / 2,
          divider: i === 0 ? null : tagX + part.offset,
        })),
        tagWidth,
        tagX,
        tagFill: color === null ? null : darker(color),
        valueText,
        color,
        textColor: node?.textColor ?? column.textColor ?? null,
        selectable:
          selectable &&
          !item.more &&
          (!multi || takenColumns.has(item.column) || [...selected].some((id) => this.isPeer(id, item.id))),
        selected: !item.more && selected.has(item.id),
        active: related === null || item.members.some((id) => related.has(id)),
        x: item.x,
        y: item.y,
        width: item.width,
      };
    });
  });

  protected readonly edgeViews = computed<LayeredGraphEdgeView[]>(() => {
    const path = this.onPath();
    const related = this.related();
    const selected = new Set(this.selection());

    return this.layout().edges.map((edge) => ({
      key: edge.key,
      path: edge.path,
      // an edge is lit when one of the edges behind it is on a path through the selected nodes, or joins a selected node
      // to one of its peers
      active:
        path === null ||
        related === null ||
        edge.underlying.some(
          ([from, to]) =>
            (path.has(from) && path.has(to)) ||
            (selected.has(from) && related.has(to)) ||
            (selected.has(to) && related.has(from)),
        ),
    }));
  });

  // the captions on top (columns): centered over their column
  protected readonly columnLabels = computed<LayeredGraphColumnLabel[]>(() => {
    const { columns } = this.geometry();

    return this.columns().map((column, i) => ({ text: column.label, x: columns[i].start + columns[i].span / 2 }));
  });

  // the captions of the layers (rows): at the left, in the middle of their layer (the empty layers have none)
  protected readonly rowLabels = computed<LayeredGraphRowLabel[]>(() => {
    const { columns } = this.geometry();
    const counts = this.items().map((layer) => layer.length);

    return this.columns()
      .map((column, i) => ({ text: column.label, y: columns[i].start + columns[i].span / 2 }))
      .filter((_, i) => counts[i] > 0);
  });

  protected onNodeClick(node: LayeredGraphNodeView): void {
    if (!node.selectable) return;
    if (this.multiSelect()) {
      const next = this.nextSelection(this.selection(), node.id);
      if (next === null) return;

      this.focusId = next.includes(node.id) ? node.id : null;
      this.selectedNodes.set(next);
    } else {
      // a click on the selected node unselects it
      this.selectedNode.set(node.selected ? null : node.id);
    }
    this.nodeSelected.emit({
      id: node.id,
      label: node.label,
      column: node.column,
      columnLabel: node.columnLabel,
      value: node.value,
    });
  }

  // scrolls the selected node to the middle of what is visible (below the captions); the axis that does not scroll
  // stays where it is. The first time the graph is drawn it jumps there (no animation), and with nothing selected it is
  // the middle of the graph that is centered.
  private scrollToSelected(initial = false): void {
    const element = this.scroller()?.scrollContainer?.nativeElement;
    const content = this.content()?.nativeElement;
    // the one just clicked, or the first selected
    const views = this.nodeViews();
    const selected = views.find((n) => n.selected && n.id === this.focusId) ?? views.find((n) => n.selected);
    const node = selected ?? (initial ? { x: 0, y: this.height() / 2 - this.nodeHeight() / 2, width: this.drawingWidth() } : null);
    if (!element || !content || !node) return;

    // where the content starts inside what scrolls (the scroller has paddings of its own)
    const box = element.getBoundingClientRect();
    const start = content.getBoundingClientRect();
    const offsetX = start.left - box.left + element.scrollLeft;
    const offsetY = start.top - box.top + element.scrollTop;

    const captions = this.captionsOnTop() ? LABEL_HEIGHT : 0;
    const left = offsetX + SCROLL_PADDING + node.x + node.width / 2 - element.clientWidth / 2;
    const top = offsetY + node.y + this.nodeHeight() / 2 - (element.clientHeight - captions) / 2;
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    element.scrollTo({ left, top, behavior: reduced || initial ? 'auto' : 'smooth' });
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
