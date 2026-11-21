// the sort of the lists of the changes widgets (new vulnerabilities, new fixes, known exploited): a pill per key, one on at
// a time; a click on a pill goes natural direction -> reversed -> off (off = the order of the server: by reach)
export type HomeListSortKey = 'moment' | 'severity' | 'name';

export interface HomeListSort {
  key: HomeListSortKey;
  reversed: boolean;
}

// the natural direction of each key: newest first, worst first, A to Z
const TITLES: Record<HomeListSortKey, { natural: string; reversed: string; label: string }> = {
  moment: { natural: 'Newest first', reversed: 'Oldest first', label: 'moment' },
  severity: { natural: 'Worst first', reversed: 'Least severe first', label: 'severity' },
  name: { natural: 'A to Z', reversed: 'Z to A', label: 'name' },
};

export function nextSort(current: HomeListSort | null, key: HomeListSortKey): HomeListSort | null {
  if (current?.key !== key) {
    return { key, reversed: false };
  }

  return current.reversed ? null : { key, reversed: true };
}

// the arrow shows the order on screen: down is the natural one (newest, worst, A), up the reverse; none while it is off
export function sortIconOf(sort: HomeListSort | null, key: HomeListSortKey): string | undefined {
  return sort?.key !== key ? undefined : sort.reversed ? 'pi-arrow-up' : 'pi-arrow-down';
}

export function sortTitleOf(sort: HomeListSort | null, key: HomeListSortKey): string {
  const titles = TITLES[key];

  return sort?.key !== key ? `Sort by ${titles.label}` : sort.reversed ? titles.reversed : titles.natural;
}

// the counter line: "by reach" (the server order) or what is on
export function sortLabelOf(sort: HomeListSort | null): string {
  if (!sort) {
    return 'by reach';
  }

  const titles = TITLES[sort.key];
  const title = sort.reversed ? titles.reversed : titles.natural;

  // a sentence case title, but the A to Z ones, in the middle of a line
  return sort.key === 'name' ? title : title.toLowerCase();
}

// a copy of the rows in the order of the sort (the rows as they are while it is off); a row without a moment goes last
// either way, and the rows that tie keep the order they had (by reach)
export function sortRows<T>(
  rows: T[],
  sort: HomeListSort | null,
  by: { moment: (row: T) => string | undefined; severity: (row: T) => number; name: (row: T) => string },
): T[] {
  if (!sort) {
    return rows;
  }

  const direction = sort.reversed ? -1 : 1;
  const collator = new Intl.Collator(undefined, { numeric: true, sensitivity: 'base' });

  const compare = (a: T, b: T): number => {
    switch (sort.key) {
      case 'moment': {
        const x = by.moment(a);
        const y = by.moment(b);
        if (x === y) {
          return 0;
        }
        if (x === undefined) {
          return 1;
        }
        if (y === undefined) {
          return -1;
        }
        // newest first is natural
        return direction * (x < y ? 1 : -1);
      }
      case 'severity':
        return direction * (by.severity(a) - by.severity(b));
      case 'name':
        return direction * collator.compare(by.name(a), by.name(b));
    }
  };

  return rows
    .map((row, index) => ({ row, index }))
    .sort((a, b) => compare(a.row, b.row) || a.index - b.index)
    .map((x) => x.row);
}
