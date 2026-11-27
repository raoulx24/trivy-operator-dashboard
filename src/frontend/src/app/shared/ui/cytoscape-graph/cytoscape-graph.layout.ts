import { EdgeSingular, LayoutOptions, Position } from 'cytoscape';
import { FcoseLayoutOptions } from 'cytoscape-fcose';

import { GraphLayout, NodeDataDto } from './cytoscape-graph.types';

// the layered layout: a cell per node, wide enough for the widest node (250px, see the styles) plus a gap
const LAYERED_CELL_WIDTH = 280;
const LAYERED_ROW_HEIGHT = 50;
// between the bands (parents, the main node, children): the same as between the rows of a band, so the picture reads as
// one evenly spaced grid; the bands are told apart by the edges (to / from the main node)
const LAYERED_BAND_GAP = LAYERED_ROW_HEIGHT;

/**
 * The cytoscape layout options. fcose: the free force layout (the fcose options apply); layered: a preset layout with
 * the positions from buildLayeredPositions (the fcose options are ignored).
 */
export function buildLayoutOptions(
  layout: GraphLayout,
  getIdealEdgeLength: () => number,
  getLayeredPositions: () => Record<string, Position>,
): LayoutOptions {
  if (layout === 'layered') {
    const positions = getLayeredPositions();
    return {
      name: 'preset',
      positions,
      animate: false,
      // the host fits it to the width, not to the whole graph (see CytoscapeGraphComponent.fitLayered)
      fit: false,
    } as LayoutOptions;
  }

  return buildFcoseLayoutOptions(getIdealEdgeLength);
}

function buildFcoseLayoutOptions(getIdealEdgeLength: () => number): FcoseLayoutOptions {
  return {
    name: 'fcose',
    nodeRepulsion: () => 20000,
    numIter: 2500,
    animate: false,
    fit: true,
    padding: 10,
    sampleSize: 50,
    nodeSeparation: 50,
    tilingPaddingHorizontal: 100,
    tilingPaddingVertical: 100,
    // pull group members toward their group's center so group boxes stay compact (defaults: 1 / 1.5)
    gravityCompound: 5,
    gravityRangeCompound: 1,
    idealEdgeLength: (edge: EdgeSingular) => {
      let denominator = 1;
      if (edge.source().parent().nonempty()) denominator = 8;
      if (edge.target().parent().nonempty()) denominator = 4;

      return getIdealEdgeLength() / denominator;
    },
    edgeElasticity: () => 0.15,
  };
}

/**
 * Parents in rows above the main node, children in rows below it; each band wraps to the width (in model units, so
 * the same input always gives the same picture). A node that is both a parent and a child goes with the children;
 * a node that is neither (not expected) too. Each band is sorted by name and centered.
 */
export function buildLayeredPositions(nodeDataDtos: NodeDataDto[], width: number): Record<string, Position> {
  const main = nodeDataDtos.find((x) => x.isMain);
  if (!main?.id) {
    return {};
  }
  const childIds = new Set(main.dependsOn ?? []);
  const byName = (a: NodeDataDto, b: NodeDataDto) => (a.name ?? '').localeCompare(b.name ?? '');
  const others = nodeDataDtos.filter((x) => x.id && x !== main);
  const parents = others.filter((x) => !childIds.has(x.id!) && x.dependsOn?.includes(main.id!)).sort(byName);
  const parentIds = new Set(parents.map((x) => x.id));
  const children = others.filter((x) => !parentIds.has(x.id)).sort(byName);

  const columns = Math.max(1, Math.floor(width / LAYERED_CELL_WIDTH));
  const positions: Record<string, Position> = {};

  // the parents' band ends right above the main node (its last row is the closest one)
  const parentRows = Math.ceil(parents.length / columns);
  placeBand(parents, columns, -LAYERED_BAND_GAP - (parentRows - 1) * LAYERED_ROW_HEIGHT, positions);
  positions[main.id] = { x: 0, y: 0 };
  placeBand(children, columns, LAYERED_BAND_GAP, positions);

  return positions;
}

function placeBand(nodes: NodeDataDto[], columns: number, top: number, positions: Record<string, Position>) {
  nodes.forEach((node, index) => {
    const row = Math.floor(index / columns);
    const inRow = Math.min(columns, nodes.length - row * columns);
    const column = index % columns;
    positions[node.id!] = {
      x: (column - (inRow - 1) / 2) * LAYERED_CELL_WIDTH,
      y: top + row * LAYERED_ROW_HEIGHT,
    };
  });
}
