import { Signal, computed, effect, signal } from '@angular/core';

import { eolRelative, parseEolDay } from '@shared/ui/eol-runway/eol-runway.state';

import { PivotEntry, PivotOption, PivotSelection } from './home-pivot.types';

// the selection of a pivot widget and the selects built on it
// the selects depend on each other: each offers what is consistent with the other two (nothing selected: everything), so
// a day offers the days of the image and the pair selected, an image the images of the day and the pair, and so on
// created in the constructor of the widget (it has an effect)
export class HomePivotState {
  readonly selection = signal<PivotSelection | null>(null);

  // the day filter: the boxes are made of the rows of that UTC day only (none selected: all of them)
  readonly selectedDay = computed(() => this.selection()?.day ?? null);

  readonly dayOptions: Signal<PivotOption[]>;
  readonly imageOptions: Signal<PivotOption[]>;
  readonly pairOptions: Signal<PivotOption[]>;

  // the days the heatmap lets you click: every day with rows, whatever image and pair are selected (a click starts over:
  // it clears them); the day select offers fewer, only the days of the selected image and pair
  readonly heatmapDays: Signal<string[]>;

  constructor(
    private readonly entries: Signal<PivotEntry[]>,
    isExpanded: Signal<boolean>,
    private readonly today: Signal<string>,
  ) {
    // the selection does not outlive the expanded widget
    effect(() => {
      if (!isExpanded()) {
        this.selection.set(null);
      }
    });

    this.dayOptions = computed(() =>
      [...new Set(this.entriesFor('day').map((x) => x.day))]
        .sort()
        .reverse()
        .map((day) => ({ value: day, label: day, note: this.age(day), isKnownExploited: false })),
    );

    this.heatmapDays = computed(() => [...new Set(this.entries().map((x) => x.day))]);

    // an image or a pair is exploited when it has at least one entry of a pair that is
    this.imageOptions = computed(() => {
      const options = new Map<string, PivotOption>();
      for (const x of this.entriesFor('digest')) {
        const option = options.get(x.digest);
        if (option) {
          option.isKnownExploited ||= x.isKnownExploited;
        } else {
          options.set(x.digest, { value: x.digest, label: x.name, digest: x.digest, isKnownExploited: x.isKnownExploited });
        }
      }

      return [...options.values()].sort((a, b) => a.label.localeCompare(b.label) || a.value.localeCompare(b.value));
    });

    this.pairOptions = computed(() => {
      const options = new Map<string, PivotOption>();
      for (const x of this.entriesFor('pairKey')) {
        if (!options.has(x.pairKey)) {
          options.set(x.pairKey, {
            value: x.pairKey,
            label: x.vulnerabilityId,
            severityId: x.severityId,
            isKnownExploited: x.isKnownExploited,
          });
        }
      }

      return [...options.values()].sort((a, b) => a.label.localeCompare(b.label) || (a.severityId ?? 0) - (b.severityId ?? 0));
    });
  }

  // "today", "3 days ago" (the same words as the runway of the Base OS widget)
  age(day: string): string {
    const ms = parseEolDay(day);
    const today = parseEolDay(this.today());

    return ms === undefined || today === undefined ? '' : eolRelative(ms, today);
  }

  // the entries that agree with the selection of the other two selects
  private entriesFor(select: 'day' | 'digest' | 'pairKey') {
    const selection = this.selection();

    return this.entries().filter(
      (x) =>
        (select === 'day' || !selection?.day || x.day === selection.day) &&
        (select === 'digest' || !selection?.digest || x.digest === selection.digest) &&
        (select === 'pairKey' || !selection?.pairKey || x.pairKey === selection.pairKey),
    );
  }

  isSelected(digest: string, pairKey: string): boolean {
    const selection = this.selection();
    return selection?.digest === digest && selection.pairKey === pairKey;
  }

  // a click on a line of a box: selects its image and its pair; true when the line is selected now, false when it was
  // the selected one (then it is cleared)
  selectLine(digest: string, pairKey: string): boolean {
    const current = this.selection();
    if (current?.digest === digest && current.pairKey === pairKey) {
      // the same line again: image and pair are cleared, the day filter stays
      this.selectSide({ digest: null, pairKey: null, day: current.day });
      return false;
    }

    this.selection.set({ digest, pairKey, day: current?.day ?? null });
    return true;
  }

  // a pick (or a clear) in a select changes its side only: the other side stays, and the options of the selects follow
  // (an image picked narrows the pairs offered, cleared it widens them)
  pickImage(digest: string | null): void {
    const current = this.selection();
    this.selectSide({ digest, pairKey: current?.pairKey ?? null, day: current?.day ?? null });
  }

  pickPair(pairKey: string | null): void {
    const current = this.selection();
    this.selectSide({ digest: current?.digest ?? null, pairKey, day: current?.day ?? null });
  }

  // the day filters the boxes (and the other two selects); the selected image and pair stay, as the options of the
  // selects keep them consistent with the day
  pickDay(day: string | null): void {
    const current = this.selection();
    this.selectSide({ digest: current?.digest ?? null, pairKey: current?.pairKey ?? null, day });
  }

  // a click on a day of the heatmap filters by it, and starts over: the selected image and pair are cleared (the selects
  // still offer them for the day); on the selected day again, it clears the filter
  pickHeatmapDay(day: string): void {
    this.selectSide({ digest: null, pairKey: null, day: this.selection()?.day === day ? null : day });
  }

  private selectSide(selection: PivotSelection): void {
    this.selection.set(selection.digest || selection.pairKey || selection.day ? selection : null);
  }
}
