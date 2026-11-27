import { Splitter } from 'primeng/splitter';

const MIN_FIT_PERCENT = 10;
const MAX_FIT_PERCENT = 90;

/**
 * When the gutter of the splitter is double clicked, sizes the first panel to fit its table
 * (the natural width of the table, not stretched). Falls back to the default sizes when there is no table.
 * When the splitter is stateful, the stored sizes are overwritten.
 */
export function resetSplitterSizes(event: MouseEvent, splitter: Splitter, defaultSizes: number[]): void {
  const gutter = (event.target as HTMLElement | null)?.closest('.p-splitter-gutter');
  // ignore double clicks on the panel content, and on the gutters of nested splitters
  if (!gutter || gutter.parentElement !== event.currentTarget) {
    return;
  }

  splitter.panelSizes = fitFirstPanelSizes(gutter.parentElement as HTMLElement, splitter.layout, splitter.gutterSize) ?? [...defaultSizes];
  if (splitter.stateKey) {
    splitter.saveState();
  }
}

/** Sizes (percent) that give the first panel the natural width of its table; null when it cannot be measured. */
function fitFirstPanelSizes(splitterEl: HTMLElement, layout: string | undefined, gutterSize: number): number[] | null {
  // horizontal layout only
  if (layout === 'vertical') {
    return null;
  }
  const panels = Array.from(splitterEl.children).filter(c => c.classList.contains('p-splitterpanel')) as HTMLElement[];
  if (panels.length !== 2) {
    return null;
  }
  const panel = panels[0];
  const tables = Array.from(panel.querySelectorAll<HTMLElement>('.p-datatable-table'));
  if (tables.length === 0) {
    return null;
  }
  // the elements of the table that scroll horizontally (the container, or the virtual scroller)
  const scrollers = Array.from(panel.querySelectorAll<HTMLElement>('*')).filter(el => {
    const overflowX = getComputedStyle(el).overflowX;
    return (overflowX === 'auto' || overflowX === 'scroll') && el.clientWidth > 0;
  });

  // the width the content needs inside the scroller: what overflows now, or the natural width of the tables
  const overflowing = Math.max(0, ...scrollers.map(el => el.scrollWidth));
  const stretched = Math.max(0, ...scrollers.map(el => el.clientWidth));
  // a table with an explicit width (the style input) keeps it: that is the width it is meant to have
  const previous = tables.map(t => t.style.width);
  tables.forEach(t => {
    if (!t.style.width) t.style.width = 'max-content';
  });
  const tablesWidth = Math.max(...tables.map(t => t.offsetWidth));
  tables.forEach((t, i) => (t.style.width = previous[i]));
  const naturalWidth = overflowing > stretched ? Math.max(overflowing, tablesWidth) : tablesWidth;

  // what the panel adds around the content (padding, borders, scrollbar)
  const container = scrollers.filter(el => el.contains(tables[0])).pop() ?? tables[0];
  const chrome = Math.max(0, panel.offsetWidth - container.clientWidth);
  // primeng sets the flex-basis of a panel to calc(<size>% - <gutters>px) of the whole splitter
  const total = splitterEl.clientWidth;
  if (naturalWidth <= 0 || total <= 0) {
    return null;
  }

  const first = Math.min(MAX_FIT_PERCENT, Math.max(MIN_FIT_PERCENT, ((naturalWidth + chrome + gutterSize + 2) / total) * 100));
  return [first, 100 - first];
}
