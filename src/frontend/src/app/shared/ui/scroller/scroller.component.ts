import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  inject,
  input,
  OnDestroy,
  signal,
  ViewChild,
} from '@angular/core';

@Component({
  selector: 'app-scroller',
  standalone: true,
  imports: [],
  templateUrl: './scroller.component.html',
  styleUrl: './scroller.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScrollerComponent implements AfterViewInit, OnDestroy {
  @ViewChild('scrollContainer') scrollContainer?: ElementRef;

  // where the content is scrolled to: the beginning (default), or the end (kept while content grows or the room
  // shrinks, until the user scrolls away from the end)
  readonly startAt = input<'start' | 'end'>('start');
  private stickToEnd = true;

  readonly showLeftButton = signal(false);
  readonly showRightButton = signal(false);

  private resizeObserver?: ResizeObserver;
  private mutationObserver?: MutationObserver;

  private readonly cdr = inject(ChangeDetectorRef);

  ngAfterViewInit() {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;

    if (this.startAt() === 'end') el.scrollLeft = el.scrollWidth;
    this.updateScrollState();
    el.addEventListener('scroll', this.handleScroll);
    this.resizeObserver = new ResizeObserver(() => {
      // content grew or the room shrank: stay at the end, until the user scrolls away from it
      if (this.startAt() === 'end' && this.stickToEnd) el.scrollLeft = el.scrollWidth;
      this.updateScrollState();
    });
    this.resizeObserver.observe(el);

    // the content can grow after the first render (data arriving later): watch the children's size too
    this.observeChildren(el);
    this.mutationObserver = new MutationObserver(() => this.observeChildren(el));
    this.mutationObserver.observe(el, { childList: true });
  }

  ngOnDestroy() {
    const el = this.scrollContainer?.nativeElement;
    if (el) el.removeEventListener('scroll', this.handleScroll);
    this.resizeObserver?.disconnect();
    this.mutationObserver?.disconnect();
  }

  private observeChildren(el: HTMLElement) {
    // observing an element twice is a no-op
    for (const child of Array.from(el.children)) this.resizeObserver?.observe(child);
  }

  handleScroll = () => {
    const el = this.scrollContainer?.nativeElement;
    // following the end holds while the scroll is at the end (also after our own scrolling to it)
    if (el) this.stickToEnd = el.scrollLeft + el.clientWidth >= el.scrollWidth - 1;
    this.updateScrollState();
  };

  scrollLeft() {
    this.scrollContainer?.nativeElement.scrollBy({ left: -150, behavior: 'smooth' });
  }

  scrollRight() {
    this.scrollContainer?.nativeElement.scrollBy({ left: 150, behavior: 'smooth' });
  }

  private updateScrollState() {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;

    const { scrollWidth, clientWidth, scrollLeft } = el;

    this.showLeftButton.set(scrollLeft > 0);
    this.showRightButton.set(scrollLeft + clientWidth < scrollWidth);

    this.cdr.markForCheck();
  }
}
