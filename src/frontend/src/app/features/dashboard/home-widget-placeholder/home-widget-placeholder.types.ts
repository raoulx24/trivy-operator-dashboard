export interface HomeWidgetState {
  // undefined while loading (or, for a search, before the first search)
  isAvailable?: boolean;
  unavailableReason?: string | null;
  // a one line description of what arrived, e.g. "12 items, 4 vulnerability ids"
  summary?: string;
  error?: string;
  // shown instead of "Loading..." when nothing was asked yet (searches)
  idleText?: string;
}
