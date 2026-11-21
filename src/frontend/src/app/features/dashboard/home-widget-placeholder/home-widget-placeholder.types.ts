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

// the answer of an endpoint a widget loads (or its error), for another widget that uses the same data
export interface HomeDaysResult<T> {
  // undefined while loading
  dto: T | undefined;
  error: string | undefined;
}
