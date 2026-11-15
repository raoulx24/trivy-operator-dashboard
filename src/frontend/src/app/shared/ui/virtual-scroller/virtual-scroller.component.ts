import { CdkFixedSizeVirtualScroll, CdkVirtualForOf, CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { NgTemplateOutlet } from '@angular/common';
import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  TemplateRef,
  TrackByFunction,
  computed,
  contentChild,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';

export interface VirtualScrollerItemContext<T> {
  $implicit: T;
  index: number;
}

// a horizontal row of items of the same width, as app-scroller (the same buttons, no scrollbar), but only the items in
// view (and a buffer around them) are rendered (cdk virtual scroll), so a row can have any number of items
// - the item is the ng-template in the content (let-item, let-i="index"); it gets the width, the gap is added after it
// - the height of the row is the one of its tallest rendered item, so the items should all have the same height
// - an item that leaves the buffer is destroyed, not reused for another one (a reused one would keep, e.g., the scroll
//   position of a list inside it)
// - scrollToIndex brings an item to the middle of the row, rendered or not (the row only, not the page)
@Component({
  selector: 'app-virtual-scroller',
  standalone: true,
  imports: [CdkVirtualScrollViewport, CdkFixedSizeVirtualScroll, CdkVirtualForOf, NgTemplateOutlet],
  templateUrl: './virtual-scroller.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block min-w-0' },
})
export class VirtualScrollerComponent<T> implements AfterViewInit, OnDestroy {
  // scrollToIndex: the items the row slides over to reach a far item
  private static readonly slideItems = 2;

  readonly items = input.required<readonly T[]>();
  // of an item, in rem
  readonly itemWidth = input.required<number>();
  // between the items, in rem
  readonly gap = input<number>(1);
  readonly trackBy = input<TrackByFunction<T>>((index: number) => index);

  // the row got its height (px; it has none until its items are rendered and measured), or changed it: e.g. for the
  // owner to bring the row into view once it is really shown
  readonly heightChange = output<number>();

  protected readonly itemTemplate = contentChild.required<TemplateRef<VirtualScrollerItemContext<T>>>(TemplateRef, {
    descendants: false,
  });
  private readonly viewport = viewChild.required(CdkVirtualScrollViewport);

  // the cdk works in px: an item and its gap
  private readonly remPx = parseFloat(getComputedStyle(document.documentElement).fontSize) || 16;
  protected readonly itemSize = computed(() => (this.itemWidth() + this.gap()) * this.remPx);

  // the viewport has no height of its own (cdk: contain strict): it is the height of the rendered items
  protected readonly height = signal(0);

  private readonly scrollOffset = signal(0);
  private readonly viewportSize = signal(0);
  protected readonly showLeftButton = computed(() => this.scrollOffset() > 1);
  // the total width is known from the count, not from the dom (the cdk sizes its spacer later)
  protected readonly showRightButton = computed(
    () => this.scrollOffset() + this.viewportSize() < this.items().length * this.itemSize() - 1,
  );

  private viewportObserver?: ResizeObserver;
  private itemsObserver?: ResizeObserver;
  private mutationObserver?: MutationObserver;

  ngAfterViewInit(): void {
    const viewport = this.viewport();
    const el = viewport.elementRef.nativeElement;

    el.addEventListener('scroll', this.handleScroll, { passive: true });

    // the cdk measures the viewport on window resize only: the room of the row changes with the layout too
    this.viewportObserver = new ResizeObserver(() => {
      viewport.checkViewportSize();
      this.viewportSize.set(viewport.getViewportSize());
    });
    this.viewportObserver.observe(el);

    // the rendered items change while scrolling: observe the ones that are added
    const wrapper = el.querySelector<HTMLElement>('.cdk-virtual-scroll-content-wrapper');
    if (wrapper) {
      this.itemsObserver = new ResizeObserver(() => this.measureHeight(wrapper));
      this.observeItems(wrapper);
      this.mutationObserver = new MutationObserver(() => {
        this.observeItems(wrapper);
        this.measureHeight(wrapper);
      });
      this.mutationObserver.observe(wrapper, { childList: true });
    }
  }

  ngOnDestroy(): void {
    this.viewport().elementRef.nativeElement.removeEventListener('scroll', this.handleScroll);
    this.viewportObserver?.disconnect();
    this.itemsObserver?.disconnect();
    this.mutationObserver?.disconnect();
  }

  // the item in the middle of the row (or as near as the ends allow); the page is not scrolled (the owner of the row
  // knows what to bring into view)
  // a far one (more than the row shows) is not swept to: the row jumps to a short distance before it, on the side it
  // comes from, and slides the rest, so the direction is still seen
  scrollToIndex(index: number): void {
    if (index < 0) return;

    const viewport = this.viewport();
    const size = this.itemSize();
    const room = viewport.getViewportSize();
    const offset = Math.max(0, index * size - (room - size) / 2);
    const distance = offset - viewport.measureScrollOffset('start');

    if (Math.abs(distance) > room) {
      viewport.scrollToOffset(offset - Math.sign(distance) * VirtualScrollerComponent.slideItems * size, 'instant');
    }
    viewport.scrollToOffset(offset, 'smooth');
  }

  protected scrollLeft(): void {
    this.viewport().elementRef.nativeElement.scrollBy({ left: -150, behavior: 'smooth' });
  }

  protected scrollRight(): void {
    this.viewport().elementRef.nativeElement.scrollBy({ left: 150, behavior: 'smooth' });
  }

  private readonly handleScroll = () => this.scrollOffset.set(this.viewport().measureScrollOffset('start'));

  private observeItems(wrapper: HTMLElement): void {
    // observing an element twice is a no-op; the removed ones are no longer observed
    for (const child of Array.from(wrapper.children)) this.itemsObserver?.observe(child);
  }

  private measureHeight(wrapper: HTMLElement): void {
    const children = Array.from(wrapper.children) as HTMLElement[];
    // no items: the last height stays, so a row that empties (and fills again) does not jump
    if (children.length === 0) return;

    const height = Math.max(...children.map((x) => x.offsetHeight));
    if (height !== this.height()) {
      this.height.set(height);
      this.heightChange.emit(height);
    }
  }
}
