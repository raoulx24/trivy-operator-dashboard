import { Directive, Signal, computed, effect, input, signal, viewChild } from '@angular/core';

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

  // the two rows of boxes (virtual: only the boxes in view are rendered, so a box is scrolled to by its index)
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

  // a pick in a select: the box of the pick is scrolled to
  protected onImagePicked(digest: string | null): void {
    this.pivot.pickImage(digest);
    if (digest) this.scrollToImageBox(digest);
  }

  protected onPairPicked(pairKey: string | null): void {
    this.pivot.pickPair(pairKey);
    if (pairKey) this.scrollToPairBox(pairKey);
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
