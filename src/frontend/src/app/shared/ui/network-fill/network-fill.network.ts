// a loose network of nodes and links (a cluster seen from afar): nodes spread on a jittered grid, each linked to its
// nearest neighbours; two layers, the back one fainter and denser, for depth; sparser to the left, so the drawing fades
// in from the right corner, where it is anchored
// everything follows the seed: the same seed gives the same network on every render

export interface NetworkNode {
  x: number;
  y: number;
  r: number;
  // a thin circle around the node
  hasRing: boolean;
}

export interface NetworkLayer {
  // all the links of the layer as one path (one path is stroked once: no darker dots where links meet)
  links: string;
  nodes: NetworkNode[];
  isBack: boolean;
}

interface LayerSettings {
  // the side of a cell of the jittered grid, in px (one node at most per cell)
  cellSize: number;
  // links per node, to the nearest ones
  linksPerNode: number;
  maxLinkLength: number;
  ringChance: number;
  bigChance: number;
  isBack: boolean;
}

const backLayer: LayerSettings = {
  cellSize: 70,
  linksPerNode: 2,
  maxLinkLength: 150,
  ringChance: 0.06,
  bigChance: 0.05,
  isBack: true,
};

const frontLayer: LayerSettings = {
  cellSize: 105,
  linksPerNode: 3,
  maxLinkLength: 210,
  ringChance: 0.16,
  bigChance: 0.1,
  isBack: false,
};

export function buildNetwork(seed: string, width: number, height: number): NetworkLayer[] {
  const random = mulberry32(hashString(seed));
  return [backLayer, frontLayer].map((settings) => buildLayer(settings, width, height, random));
}

function buildLayer(settings: LayerSettings, width: number, height: number, random: () => number): NetworkLayer {
  const { cellSize } = settings;
  const nodes: NetworkNode[] = [];

  // one row and one column more than fit: the nodes just outside the edge pull links in from beyond it
  for (let top = -cellSize / 2; top < height + cellSize / 2; top += cellSize) {
    for (let left = -cellSize / 2; left < width + cellSize / 2; left += cellSize) {
      const x = left + (0.15 + random() * 0.7) * cellSize;
      const y = top + (0.15 + random() * 0.7) * cellSize;
      // the further left, the fewer nodes
      if (random() > 0.25 + 0.75 * Math.max(0, x / width)) {
        continue;
      }
      const isBig = random() < settings.bigChance;
      nodes.push({
        x,
        y,
        r: isBig ? 4 + random() * 2 : 1.5 + random() * 1.5,
        hasRing: random() < settings.ringChance,
      });
    }
  }

  const linked = new Set<string>();
  const parts: string[] = [];
  nodes.forEach((node, index) => {
    const nearest = nodes
      .map((other, otherIndex) => ({ otherIndex, distance: Math.hypot(other.x - node.x, other.y - node.y) }))
      .filter(({ otherIndex, distance }) => otherIndex !== index && distance <= settings.maxLinkLength)
      .sort((a, b) => a.distance - b.distance)
      .slice(0, settings.linksPerNode);
    for (const { otherIndex } of nearest) {
      const key = index < otherIndex ? `${index}-${otherIndex}` : `${otherIndex}-${index}`;
      if (linked.has(key)) {
        continue;
      }
      linked.add(key);
      const other = nodes[otherIndex];
      parts.push(`M${node.x.toFixed(1)} ${node.y.toFixed(1)}L${other.x.toFixed(1)} ${other.y.toFixed(1)}`);
    }
  });

  return { links: parts.join(''), nodes, isBack: settings.isBack };
}

// FNV-1a
function hashString(text: string): number {
  let hash = 0x811c9dc5;
  for (let index = 0; index < text.length; index++) {
    hash ^= text.charCodeAt(index);
    hash = Math.imul(hash, 0x01000193);
  }
  return hash >>> 0;
}

// a small seeded random generator: numbers in [0, 1)
function mulberry32(seed: number): () => number {
  let state = seed;
  return () => {
    state = (state + 0x6d2b79f5) | 0;
    let t = Math.imul(state ^ (state >>> 15), 1 | state);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}
