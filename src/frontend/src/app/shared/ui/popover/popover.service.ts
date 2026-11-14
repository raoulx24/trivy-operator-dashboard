import { ComponentPortal } from '@angular/cdk/portal';
import {
  FlexibleConnectedPositionStrategy,
  Overlay,
  OverlayRef,
} from '@angular/cdk/overlay';
import { DestroyRef, inject, Injectable } from '@angular/core';

import { PopoverComponent, PopoverContent } from './popover.component';

export interface PopoverOptions {
  /** Fixed width (any CSS length). Default: fit content. */
  width?: string;
  /** Maximum width (any CSS length). Default: 24rem. */
  maxWidth?: string;
}

export interface PopoverPoint {
  clientX: number;
  clientY: number;
}

/**
 * Caller-agnostic popover. The caller decides when to show / move / hide
 * (typically from mouseenter / mousemove / mouseleave). One popover exists at a time:
 * a new show() replaces whatever is displayed.
 *
 * content: plain text (escaped), or a TemplateRef declared in the caller
 * (so it can use the caller's classes/styles). `context` is exposed to the template
 * as `let-x` ($implicit).
 */
@Injectable({ providedIn: 'root' })
export class PopoverService {
  private readonly overlay = inject(Overlay);

  private overlayRef?: OverlayRef;
  private positionStrategy?: FlexibleConnectedPositionStrategy;
  private popover?: PopoverComponent;
  private anchoredMode = false;

  private readonly offset = 12;

  private pinnedRef?: OverlayRef;
  private pinnedAnchor?: HTMLElement;
  private onPointerDown?: (event: PointerEvent) => void;
  private onKeydown?: (event: KeyboardEvent) => void;
  private onScroll?: () => void;

  constructor() {
    inject(DestroyRef).onDestroy(() => {
      this.dismissPinned();
      this.overlayRef?.dispose();
    });
  }

  /**
   * `at` is either a point (the popover follows the mouse via move()) or an element (the popover is anchored above it,
   * flipping below when there is no room). Nothing is shown for an empty string.
   */
  show(content: PopoverContent, at: PopoverPoint | HTMLElement, context?: unknown, options?: PopoverOptions): void {
    // a pinned popover is not replaced by hovering
    if (this.pinnedRef) return;

    // nothing to show: no text and no template
    if (typeof content === 'string' && content.trim() === '') {
      this.hide();
      return;
    }

    const anchored = at instanceof HTMLElement;
    // the strategy differs between the two modes, so the overlay is rebuilt when the mode changes
    if (this.overlayRef && this.anchoredMode !== anchored) {
      this.overlayRef.dispose();
      this.overlayRef = undefined;
    }

    if (!this.overlayRef) {
      this.anchoredMode = anchored;
      this.positionStrategy = anchored
        ? this.overlay
            .position()
            .flexibleConnectedTo(at)
            .withFlexibleDimensions(false)
            .withPush(true)
            .withPositions([
              { originX: 'center', originY: 'top', overlayX: 'center', overlayY: 'bottom', offsetY: -6 },
              { originX: 'center', originY: 'bottom', overlayX: 'center', overlayY: 'top', offsetY: 6 },
            ])
        : this.overlay
            .position()
            .flexibleConnectedTo({ x: at.clientX, y: at.clientY })
            .withFlexibleDimensions(false)
            .withPush(true)
            .withPositions([
              // below-right, then flip when near the viewport edges
              { originX: 'start', originY: 'top', overlayX: 'start', overlayY: 'top', offsetX: this.offset, offsetY: this.offset },
              { originX: 'start', originY: 'top', overlayX: 'end', overlayY: 'top', offsetX: -this.offset, offsetY: this.offset },
              { originX: 'start', originY: 'top', overlayX: 'start', overlayY: 'bottom', offsetX: this.offset, offsetY: -this.offset },
              { originX: 'start', originY: 'top', overlayX: 'end', overlayY: 'bottom', offsetX: -this.offset, offsetY: -this.offset },
            ]);
      this.overlayRef = this.overlay.create({ positionStrategy: this.positionStrategy });
      // the popover must never steal mouse events from the element being hovered
      this.overlayRef.overlayElement.style.pointerEvents = 'none';
    }

    this.positionStrategy!.setOrigin(anchored ? at : { x: at.clientX, y: at.clientY });

    if (!this.overlayRef.hasAttached()) {
      this.popover = this.overlayRef.attach(new ComponentPortal(PopoverComponent)).instance;
    }

    this.popover!.content.set(content);
    this.popover!.context.set(context);
    this.popover!.width.set(options?.width);
    this.popover!.maxWidth.set(options?.maxWidth);
    this.overlayRef.updatePosition();
  }

  /** Follow the mouse; no-op when nothing is shown. */
  move(at: PopoverPoint): void {
    // an anchored popover stays where it is
    if (!this.overlayRef?.hasAttached() || this.anchoredMode) return;
    this.positionStrategy!.setOrigin({ x: at.clientX, y: at.clientY });
    this.overlayRef.updatePosition();
  }

  hide(): void {
    if (this.overlayRef?.hasAttached()) {
      this.overlayRef.detach();
    }
  }

  /**
   * Click popover: opens next to `anchor` and stays until it is dismissed - another click anywhere (a click on the
   * anchor itself toggles it), Escape, or a scroll. While it is open, hover popovers are not shown.
   * Escape is swallowed, so it closes only the popover and not the dialog it may be shown above.
   */
  togglePinned(content: PopoverContent, anchor: HTMLElement, context?: unknown, options?: PopoverOptions): void {
    if (this.pinnedAnchor === anchor) {
      this.dismissPinned();
      return;
    }

    this.dismissPinned();
    this.hide();

    this.pinnedRef = this.overlay.create({
      positionStrategy: this.overlay
        .position()
        .flexibleConnectedTo(anchor)
        .withFlexibleDimensions(false)
        .withPush(true)
        .withPositions([
          { originX: 'end', originY: 'bottom', overlayX: 'end', overlayY: 'top', offsetY: 4 },
          { originX: 'end', originY: 'top', overlayX: 'end', overlayY: 'bottom', offsetY: -4 },
          { originX: 'start', originY: 'bottom', overlayX: 'start', overlayY: 'top', offsetY: 4 },
          { originX: 'start', originY: 'top', overlayX: 'start', overlayY: 'bottom', offsetY: -4 },
        ]),
    });
    this.pinnedRef.overlayElement.style.pointerEvents = 'none';

    const popover = this.pinnedRef.attach(new ComponentPortal(PopoverComponent)).instance;
    popover.content.set(content);
    popover.context.set(context);
    popover.width.set(options?.width);
    popover.maxWidth.set(options?.maxWidth);
    this.pinnedRef.updatePosition();
    this.pinnedAnchor = anchor;

    // the popover does not take pointer events, so a click lands on what is below it; a click on the anchor is left to
    // the caller (its toggle), any other one only closes
    this.onPointerDown = (event) => {
      if (!anchor.contains(event.target as Node)) {
        this.dismissPinned();
      }
    };
    this.onKeydown = (event) => {
      if (event.key === 'Escape') {
        event.stopPropagation();
        this.dismissPinned();
      }
    };
    this.onScroll = () => this.dismissPinned();
    window.addEventListener('pointerdown', this.onPointerDown, true);
    window.addEventListener('keydown', this.onKeydown, true);
    window.addEventListener('scroll', this.onScroll, true);
  }

  isPinned(): boolean {
    return this.pinnedRef !== undefined;
  }

  dismissPinned(): void {
    if (this.onPointerDown) window.removeEventListener('pointerdown', this.onPointerDown, true);
    if (this.onKeydown) window.removeEventListener('keydown', this.onKeydown, true);
    if (this.onScroll) window.removeEventListener('scroll', this.onScroll, true);
    this.onPointerDown = this.onKeydown = this.onScroll = undefined;

    this.pinnedRef?.dispose();
    this.pinnedRef = undefined;
    this.pinnedAnchor = undefined;
  }
}
