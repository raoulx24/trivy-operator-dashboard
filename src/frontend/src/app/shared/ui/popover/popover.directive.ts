import { DestroyRef, Directive, ElementRef, HostListener, inject, input } from '@angular/core';

import { PopoverContent } from './popover.component';
import { PopoverOptions, PopoverService } from './popover.service';

/**
 * Declarative hover/focus popover, a drop-in for pTooltip:
 *   <i [todPopover]="'text'"></i>
 *   <span [todPopover]="tpl" [todPopoverContext]="items"></span>
 *
 * Empty / null content shows nothing. Default mode is 'anchor' (above the element); 'follow' tracks the mouse and
 * suits big areas.
 */
@Directive({ selector: '[todPopover]' })
export class PopoverDirective {
  readonly content = input<PopoverContent | null | undefined>(undefined, { alias: 'todPopover' });
  readonly context = input<unknown>(undefined, { alias: 'todPopoverContext' });
  readonly mode = input<'anchor' | 'follow'>('anchor', { alias: 'todPopoverMode' });
  /** Delay before showing, in ms. */
  readonly delay = input(300, { alias: 'todPopoverDelay' });
  readonly options = input<PopoverOptions | undefined>(undefined, { alias: 'todPopoverOptions' });
  /** Also open on click and keep it open (until click elsewhere / Escape / scroll); a click on the host toggles it. */
  readonly pinnable = input(false, { alias: 'todPopoverPinnable' });

  private readonly popover = inject(PopoverService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
  private timer?: ReturnType<typeof setTimeout>;
  private visible = false;

  constructor() {
    inject(DestroyRef).onDestroy(() => this.close());
  }

  @HostListener('mouseenter', ['$event'])
  protected onEnter(event: MouseEvent): void {
    this.open(event);
  }

  @HostListener('focusin')
  protected onFocus(): void {
    this.open();
  }

  @HostListener('click')
  protected onClick(): void {
    const content = this.content();
    if (!this.pinnable() || content === null || content === undefined) return;

    clearTimeout(this.timer);
    this.visible = false;
    this.popover.togglePinned(content, this.host, this.context(), this.options());
  }

  @HostListener('mousemove', ['$event'])
  protected onMove(event: MouseEvent): void {
    if (this.visible && this.mode() === 'follow') {
      this.popover.move(event);
    }
  }

  @HostListener('mouseleave')
  @HostListener('focusout')
  protected close(): void {
    clearTimeout(this.timer);
    if (this.visible) {
      this.visible = false;
      this.popover.hide();
    }
  }

  private open(event?: MouseEvent): void {
    const content = this.content();
    if (content === null || content === undefined) return;

    // a point is captured now: the mouse may move during the delay
    const at = this.mode() === 'follow' && event ? { clientX: event.clientX, clientY: event.clientY } : this.host;

    clearTimeout(this.timer);
    this.timer = setTimeout(() => {
      this.visible = true;
      this.popover.show(content, at, this.context(), this.options());
    }, this.delay());
  }
}
