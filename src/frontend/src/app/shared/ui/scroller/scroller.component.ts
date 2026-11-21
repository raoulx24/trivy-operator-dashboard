import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  computed,
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
  // the styles of the buttons and of the hidden scrollbar are in styles.scss (shared with app-virtual-scroller)
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScrollerComponent implements AfterViewInit, OnDestroy {
  @ViewChild('scrollContainer') scrollContainer?: ElementRef<HTMLElement>;

  // the scrolling axis. Vertical needs a parent with a height (the scroller fills it and scrolls inside it)
  readonly direction = input<'horizontal' | 'vertical'>('horizontal');
  protected readonly vertical = computed(() => this.direction() === 'vertical');

  // where the content is scrolled to: the beginning (default), the middle or the end (the middle and the end are kept
  // while content grows or the room shrinks, until the user scrolls away from them)
  readonly startAt = input<'start' | 'middle' | 'end'>('start');
  private following = true;

  // left / right when horizontal, up / down when vertical
  readonly showStartButton = signal(false);
  readonly showEndButton = signal(false);

  private resizeObserver?: ResizeObserver;
  private mutationObserver?: MutationObserver;

  private readonly cdr = inject(ChangeDetectorRef);

  ngAfterViewInit() {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;

    this.scrollToAnchor(el);
    this.updateScrollState();
    el.addEventListener('scroll', this.handleScroll);
    this.resizeObserver = new ResizeObserver(() => {
      // content grew or the room shrank: stay at the middle / end, until the user scrolls away from it
      if (this.following) this.scrollToAnchor(el);
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

  // the offset the content is anchored at: the middle or the end (none for the start)
  private anchorOffset(el: HTMLElement): number | null {
    const { client, scroll } = this.metrics(el);
    switch (this.startAt()) {
      case 'end':
        return scroll - client;
      case 'middle':
        return (scroll - client) / 2;
      default:
        return null;
    }
  }

  private scrollToAnchor(el: HTMLElement) {
    const target = this.anchorOffset(el);
    if (target === null) return;
    if (this.vertical()) el.scrollTop = target;
    else el.scrollLeft = target;
  }

  handleScroll = () => {
    const el = this.scrollContainer?.nativeElement;
    // following the anchor holds while the scroll is at it (also after our own scrolling to it)
    if (el) {
      const target = this.anchorOffset(el);
      if (target !== null) this.following = Math.abs(this.metrics(el).offset - target) <= 1;
    }
    this.updateScrollState();
  };

  scrollBackward() {
    this.scrollByStep(-150);
  }

  scrollForward() {
    this.scrollByStep(150);
  }

  private scrollByStep(step: number) {
    this.scrollContainer?.nativeElement.scrollBy(
      this.vertical() ? { top: step, behavior: 'smooth' } : { left: step, behavior: 'smooth' },
    );
  }

  // the sizes along the scrolling axis
  private metrics(el: HTMLElement) {
    return this.vertical()
      ? { offset: el.scrollTop, client: el.clientHeight, scroll: el.scrollHeight }
      : { offset: el.scrollLeft, client: el.clientWidth, scroll: el.scrollWidth };
  }

  private updateScrollState() {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;

    const { offset, client, scroll } = this.metrics(el);

    // 1px of tolerance: the offset can be fractional (zoom, display scaling) while the sizes are rounded, so at the
    // true end the sum can be a fraction short
    this.showStartButton.set(offset > 1);
    this.showEndButton.set(offset + client < scroll - 1);

    this.cdr.markForCheck();
  }
}
