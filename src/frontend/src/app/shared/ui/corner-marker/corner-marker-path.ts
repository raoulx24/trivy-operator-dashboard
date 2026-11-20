// the side of the triangle, as a share of the cell
const MARKER_SIZE = 0.44;

export type CornerMarkerCorner = 'top-right' | 'bottom-left';

// a marker for the square at (x, y) of `size`: a right triangle in one of two opposite corners, its three corners
// rounded (shared by the activity heatmap and the lane strip, so their markers look the same)
export function cornerMarkerPath(x: number, y: number, size: number, corner: CornerMarkerCorner): string {
  const s = Math.round(size * MARKER_SIZE);
  const right = x + size - 1;
  const top = y + 1;
  const left = x + 1;
  const bottom = y + size - 1;
  const points: [number, number][] =
    corner === 'top-right'
      ? [
          [right - s, top],
          [right, top],
          [right, top + s],
        ]
      : [
          [left + s, bottom],
          [left, bottom],
          [left, bottom - s],
        ];
  // how far from each corner the rounding starts along the edges (a quadratic curve with the corner as control)
  const trim = s * 0.28;

  return points
    .map((v, i) => {
      const from = towards(v, points[(i + 2) % 3], trim);
      const to = towards(v, points[(i + 1) % 3], trim);
      return `${i === 0 ? 'M' : 'L'}${from[0]},${from[1]} Q${v[0]},${v[1]} ${to[0]},${to[1]}`;
    })
    .join(' ')
    .concat(' Z');
}

// the point `distance` away from `from`, on the way to `to`
function towards(from: [number, number], to: [number, number], distance: number): [number, number] {
  const length = Math.hypot(to[0] - from[0], to[1] - from[1]);
  return [from[0] + ((to[0] - from[0]) / length) * distance, from[1] + ((to[1] - from[1]) / length) * distance];
}
