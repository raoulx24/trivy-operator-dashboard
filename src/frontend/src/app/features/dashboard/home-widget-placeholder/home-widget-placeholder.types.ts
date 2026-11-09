export interface HomeWidgetState {
  // undefined while loading
  isAvailable?: boolean;
  unavailableReason?: string | null;
  // a one line description of what arrived, e.g. "12 items, 4 vulnerability ids"
  summary?: string;
  error?: string;
}
