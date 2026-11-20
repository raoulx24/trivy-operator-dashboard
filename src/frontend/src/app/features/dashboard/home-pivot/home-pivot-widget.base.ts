import { Directive, Injector, Signal, afterNextRender, computed, effect, inject, input, signal, viewChild } from '@angular/core';

import { VirtualScrollerComponent } from '@shared/ui/virtual-scroller/virtual-scroller.component';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomePivotState } from './home-pivot-state';

// the part the pivot widgets (history drift, new images) share: the same data in two virtual rows of boxes, images and
// pairs (vulnerability id, severity), with a selection that links them (see HomePivotState); the widget makes the boxes
// and its template draws them
// the template must have the rows as #imagesRow and #pairsRow (app-virtual-scroller)
@Directive()
export abstract class HomePivotWidgetBase<
  I extends { digest: string },
  P extends { key: string },
> extends HomeWidgetBase {
  hint = input<string>('');
  namespaces = input.required<string[]>();
  severities = input.required<number[]>();
  // "yyyy-MM-dd"
  today = input.required<string>();
  // the days the home page was opened
  visitDays = input<string[]>([]);
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  // a new object on any change, so the resources reload; empty namespaces are not sent (the backend default applies)
  protected readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  // the selection and its selects, made by the widget from its rows
  protected abstract readonly pivot: HomePivotState;
  // the rows of boxes are shown (rendered, with their height)
  protected abstract readonly showBoxes: Signal<boolean>;
  protected abstract readonly imageBoxes: Signal<I[]>;
  protected abstract readonly pairBoxes: Signal<P[]>;

  // the days with a value in the heatmap (known from the days, before the rows are asked)
  protected abstract readonly valueDays: Signal<string[]>;

  // the days the heatmap lets you click: while collapsed, every day with a value (a click expands the widget); while
  // expanded, the days that have rows; none while the rows are asked (nothing is dimmed), or where it cannot expand
  protected readonly selectableDays = computed(() =>
    this.showBoxes() ? this.pivot.heatmapDays() : !this.isExpanded() && this.canExpand() ? this.valueDays() : undefined,
  );

  // two rows of boxes (virtual: only the boxes in view are rendered, so a box is scrolled to by its index)
  private readonly imagesRow = viewChild<VirtualScrollerComponent<I>>('imagesRow');
  private readonly pairsRow = viewChild<VirtualScrollerComponent<P>>('pairsRow');

  protected readonly trackImageBox = (_: number, box: I) => box.digest;
  protected readonly trackPairBox = (_: number, box: P) => box.key;

  // what the summary shows: the image box and the pair box of the selection, whichever are selected (all the boxes, not
  // only the shown ones)
  protected readonly selected = computed(() => {
    const selection = this.pivot.selection();
    if (!selection) return undefined;

    const image = selection.digest ? this.imageBoxes().find((x) => x.digest === selection.digest) : undefined;
    const pair = selection.pairKey ? this.pairBoxes().find((x) => x.key === selection.pairKey) : undefined;

    return image || pair ? { image, pair } : undefined;
  });

  // "Show all": the dialog with the denormalized rows, as the server sent them (they are not asked again)
  protected readonly showAllDialog = signal(false);

  private readonly renderInjector = inject(Injector);

  // set when the rows of this expand were revealed; reset on collapse (so a reload by a filter does not scroll)
  private revealed = false;
  // the rows have no height until their boxes are rendered and measured (virtual rows): the widget is revealed when
  // both have one (or at once when there are no boxes); 0 while the rows are not shown
  protected readonly imagesRowHeight = signal(0);
  protected readonly pairsRowHeight = signal(0);

  constructor() {
    super();

    // the rows are shown (rendered, with their height): the page may scroll to show the whole widget
    effect(() => {
      if (!this.isExpanded()) {
        this.revealed = false;
      }
      if (!this.showBoxes()) {
        // the rows (and their heights) are made again when they are shown again
        this.imagesRowHeight.set(0);
        this.pairsRowHeight.set(0);
        return;
      }

      const isShown = this.imageBoxes().length === 0 || (this.imagesRowHeight() > 0 && this.pairsRowHeight() > 0);
      if (isShown && !this.revealed) {
        this.revealed = true;
        this.revealAfterRender();
      }
    });
  }

  // a click on a line of an image box: selects the image and the pair, and goes to the box of the pair; the same line
  // again clears it
  protected selectFromImage(image: I, pairKey: string): void {
    if (this.pivot.selectLine(image.digest, pairKey)) {
      this.scrollToPairBox(pairKey);
    }
  }

  // a click on a line of a pair box: selects the pair and the image, and goes to the box of the image
  protected selectFromPair(pair: P, digest: string): void {
    if (this.pivot.selectLine(digest, pair.key)) {
      this.scrollToImageBox(digest);
    }
  }

  // a click on the title of an image box: selects the image alone (the pair side stays); on the selected one, clears it
  protected selectImageBox(digest: string): void {
    const isSelected = this.pivot.selection()?.digest === digest;
    this.pivot.pickImage(isSelected ? null : digest);
    if (!isSelected) this.scrollToImageBox(digest);
  }

  // a click on the title of a pair box: the same for the pair
  protected selectPairBox(pairKey: string): void {
    const isSelected = this.pivot.selection()?.pairKey === pairKey;
    this.pivot.pickPair(isSelected ? null : pairKey);
    if (!isSelected) this.scrollToPairBox(pairKey);
  }

  // a click on a day of the heatmap: filters by it; on a collapsed widget it expands it first (the selection is set
  // before, and it lives as long as the widget is expanded), so the boxes come already filtered
  protected onHeatmapDay(day: string): void {
    this.pivot.pickHeatmapDay(day);
    if (!this.isExpanded()) {
      this.toggleExpand();
    }
  }

  // a change in any of the three selects (date, image, pair): whatever is selected, the image and/or the pair, is brought
  // into view
  protected onDayPicked(day: string | null): void {
    this.pivot.pickDay(day);
    this.bringSelectionIntoView();
  }

  protected onImagePicked(digest: string | null): void {
    this.pivot.pickImage(digest);
    this.bringSelectionIntoView();
  }

  protected onPairPicked(pairKey: string | null): void {
    this.pivot.pickPair(pairKey);
    this.bringSelectionIntoView();
  }

  // after the render: a day filters the boxes (they are other ones, at other places), so the boxes are looked up when they
  // are there; a clear of one side still brings the other one, which is still selected
  private bringSelectionIntoView(): void {
    afterNextRender(
      () => {
        const selection = this.pivot.selection();
        if (selection?.digest) this.scrollToImageBox(selection.digest);
        if (selection?.pairKey) this.scrollToPairBox(selection.pairKey);
      },
      { injector: this.renderInjector },
    );
  }

  protected severityLabel(severityId: number): string {
    return SeverityUtils.severityDtos.find((x) => x.id === severityId)?.name ?? String(severityId);
  }

  // the box scrolls into view inside its row (centered), and the page the least needed to show the widget
  private scrollToImageBox(digest: string): void {
    this.imagesRow()?.scrollToIndex(this.imageBoxes().findIndex((x) => x.digest === digest));
    this.revealAfterRender();
  }

  private scrollToPairBox(pairKey: string): void {
    this.pairsRow()?.scrollToIndex(this.pairBoxes().findIndex((x) => x.key === pairKey));
    this.revealAfterRender();
  }
}
