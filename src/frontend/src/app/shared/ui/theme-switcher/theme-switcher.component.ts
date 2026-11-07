import { Overlay, OverlayRef } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import {
  ChangeDetectionStrategy,
  Component,
  ComponentRef,
  ElementRef,
  OnDestroy,
  computed,
  inject,
} from '@angular/core';

import { ButtonModule } from 'primeng/button';

import { DarkModeService } from '@core/services/dark-mode.service';
import { IconComponent } from '../icon/icon.component';
import { THEME_MODES, ThemePopoverComponent } from './theme-popover.component';

// keep in sync with the closing animation in theme-popover.component.css
const CLOSE_ANIMATION_MS = 120;

// Round icon button showing the current theme mode; opens the theme popover on click.
@Component({
  selector: 'app-theme-switcher',
  imports: [ButtonModule, IconComponent],
  templateUrl: './theme-switcher.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ThemeSwitcherComponent implements OnDestroy {
  private readonly overlay = inject(Overlay);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly darkModeService = inject(DarkModeService);

  private overlayRef?: OverlayRef;
  private popoverRef?: ComponentRef<ThemePopoverComponent>;
  private closeTimer?: number;

  protected readonly currentIcon = computed(
    () => THEME_MODES.find((m) => m.value === this.darkModeService.themeMode())?.iconName ?? 'routine',
  );

  protected toggle() {
    if (this.overlayRef?.hasAttached()) {
      this.close();
    } else {
      this.open();
    }
  }

  private open() {
    if (!this.overlayRef) {
      this.overlayRef = this.overlay.create({
        positionStrategy: this.overlay
          .position()
          .flexibleConnectedTo(this.host)
          .withPositions([
            { originX: 'end', originY: 'bottom', overlayX: 'end', overlayY: 'top', offsetY: 4 },
            { originX: 'end', originY: 'top', overlayX: 'end', overlayY: 'bottom', offsetY: -4 },
          ]),
        scrollStrategy: this.overlay.scrollStrategies.reposition(),
        hasBackdrop: true,
        backdropClass: 'cdk-overlay-transparent-backdrop',
      });
      this.overlayRef.backdropClick().subscribe(() => this.close());
      this.overlayRef.keydownEvents().subscribe((event) => {
        if (event.key === 'Escape') this.close();
      });
    }

    this.popoverRef = this.overlayRef.attach(new ComponentPortal(ThemePopoverComponent));
    this.popoverRef.instance.selected.subscribe(() => this.close());
  }

  // Plays the closing animation, then removes the popover.
  private close() {
    if (!this.overlayRef?.hasAttached() || this.closeTimer !== undefined) return;

    this.popoverRef?.instance.closing.set(true);
    this.closeTimer = window.setTimeout(() => {
      this.closeTimer = undefined;
      this.overlayRef?.detach();
    }, CLOSE_ANIMATION_MS);
  }

  ngOnDestroy() {
    window.clearTimeout(this.closeTimer);
    this.overlayRef?.dispose();
  }
}
