import { ChangeDetectionStrategy, Component, inject, output, signal } from '@angular/core';

import { DarkModeService, ThemeMode } from '@core/services/dark-mode.service';
import { IconComponent } from '../icon/icon.component';

export const THEME_MODES: { value: ThemeMode; label: string; iconName: string }[] = [
  { value: 'system', label: 'System', iconName: 'routine' },
  { value: 'dark', label: 'Dark', iconName: 'dark_mode' },
  { value: 'light', label: 'Light', iconName: 'sunny' },
];

// Content of the theme popover: a horizontal row of icon-over-label choices.
@Component({
  selector: 'app-theme-popover',
  imports: [IconComponent],
  templateUrl: './theme-popover.component.html',
  styleUrl: './theme-popover.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ThemePopoverComponent {
  private readonly darkModeService = inject(DarkModeService);

  protected readonly modes = THEME_MODES;
  protected readonly themeMode = this.darkModeService.themeMode;

  readonly selected = output<ThemeMode>();

  // set by the host right before it removes the popover, to play the closing animation
  readonly closing = signal(false);

  protected select(mode: ThemeMode) {
    this.darkModeService.setThemeMode(mode);
    this.selected.emit(mode);
  }
}
