// the filter box of the lists of the home widgets: the text (trimmed, any case) must be contained in at least one of the keys
// of the row; an empty text matches every row
export function matchesFilter(text: string, ...keys: (string | null | undefined)[]): boolean {
  const needle = text.trim().toLowerCase();
  return needle.length === 0 || keys.some((key) => !!key && key.toLowerCase().includes(needle));
}
