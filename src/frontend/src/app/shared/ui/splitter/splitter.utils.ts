import { Splitter } from 'primeng/splitter';

/**
 * Restores the default panel sizes when the gutter of the splitter is double clicked.
 * When the splitter is stateful, the stored sizes are overwritten with the defaults.
 */
export function resetSplitterSizes(event: MouseEvent, splitter: Splitter, defaultSizes: number[]): void {
  const gutter = (event.target as HTMLElement | null)?.closest('.p-splitter-gutter');
  // ignore double clicks on the panel content, and on the gutters of nested splitters
  if (!gutter || gutter.parentElement !== event.currentTarget) {
    return;
  }

  splitter.panelSizes = [...defaultSizes];
  if (splitter.stateKey) {
    splitter.saveState();
  }
}
