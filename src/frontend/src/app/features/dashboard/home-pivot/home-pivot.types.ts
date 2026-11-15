// what the pivot widgets (history drift, new images) share: two rows of boxes (images, pairs) that pivot the same rows

// a box with the images of a pair, or the pairs of an image: the namespaces are in the footer (as in the Base OS boxes)
export interface NamespacesFooter {
  namespaces: number;
  namespaceNames: string[];
}

// the rows of a box that share a day: the box shows the day once, then the rows without it
export interface DayGroup<T> {
  day: string;
  rows: T[];
}

// the selection: a click on a line of a box selects its image and its pair (both boxes are marked, the other one is
// scrolled to); a pick in a select chooses one side only; the summary shows what is selected
// day is not a selection of a box but a filter: the boxes show only the rows of that UTC day
export interface PivotSelection {
  digest: string | null;
  pairKey: string | null;
  day: string | null;
}

// an entry of the selects: a day, an image (value = digest) or a pair (value = pair key), with its exploited flag
export interface PivotOption {
  value: string;
  label: string;
  // a day: how long ago
  note?: string;
  // the digest of an image, to search by it
  digest?: string;
  // the severity of a pair
  severityId?: number;
  isKnownExploited: boolean;
}

// a row of the data of a pivot widget, as the selects need it
export interface PivotEntry {
  // UTC day, "yyyy-MM-dd"
  day: string;
  digest: string;
  // repository:tag
  name: string;
  pairKey: string;
  vulnerabilityId: string;
  severityId: number;
  isKnownExploited: boolean;
}

// the pair of a box or of the selection, as the shared parts show it
export interface PivotPair {
  vulnerabilityId: string;
  severityId: number;
  isKnownExploited: boolean;
}

// the image of a box or of the selection, as the shared parts show it
export interface PivotImage {
  digest: string;
  // repository:tag
  name: string;
  registry: string;
}

export function pairKey(vulnerabilityId: string, severityId: number): string {
  return `${vulnerabilityId}|${severityId}`;
}

// the rows are sorted by day, so the ones of a day are next to each other
export function groupByDay<T extends { day: string }>(rows: T[]): DayGroup<T>[] {
  const groups: DayGroup<T>[] = [];
  for (const row of rows) {
    const last = groups.at(-1);
    if (last?.day === row.day) {
      last.rows.push(row);
    } else {
      groups.push({ day: row.day, rows: [row] });
    }
  }
  return groups;
}
