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

  private readonly offset = 12;

  constructor() {
    inject(DestroyRef).onDestroy(() => this.overlayRef?.dispose());
  }

  show(content: PopoverContent, at: PopoverPoint, context?: unknown, options?: PopoverOptions): void {
    if (!this.overlayRef) {
      this.positionStrategy = this.overlay
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

    this.positionStrategy!.setOrigin({ x: at.clientX, y: at.clientY });

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
    if (!this.overlayRef?.hasAttached()) return;
    this.positionStrategy!.setOrigin({ x: at.clientX, y: at.clientY });
    this.overlayRef.updatePosition();
  }

  hide(): void {
    if (this.overlayRef?.hasAttached()) {
      this.overlayRef.detach();
    }
  }
}
