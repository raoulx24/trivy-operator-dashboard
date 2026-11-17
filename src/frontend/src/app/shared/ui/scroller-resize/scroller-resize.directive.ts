import { DestroyRef, Directive, ElementRef, afterNextRender, inject } from '@angular/core';
import { Scroller } from 'primeng/scroller';

// for a p-scroller whose box changes size without the window changing (a widget that grows or shrinks with its row):
// PrimeNG measures its box at the start and on a window resize only, so it keeps the old height (and the old count of
// rendered rows). Use it with scrollHeight="100%", so the height itself follows the box; this directive makes the
// scroller measure again.
@Directive({
  selector: 'p-scroller[appScrollerResize]',
  standalone: true,
})
export class ScrollerResizeDirective {
  private readonly scroller = inject(Scroller);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  constructor() {
    const destroyRef = inject(DestroyRef);

    afterNextRender(() => {
      let frame = 0;
      let first = true;
      const observer = new ResizeObserver(() => {
        // the first call is the start, which the scroller does itself
        if (first) {
          first = false;
          return;
        }
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => this.scroller.init());
      });
      observer.observe(this.host.nativeElement);

      destroyRef.onDestroy(() => {
        cancelAnimationFrame(frame);
        observer.disconnect();
      });
    });
  }
}
