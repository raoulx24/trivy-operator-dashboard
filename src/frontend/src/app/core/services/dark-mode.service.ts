import { LocalStorageUtils } from '@shared/utils/local-storage.utils';

import { Injectable, effect, signal } from '@angular/core';

export type ThemeMode = 'light' | 'system' | 'dark';

@Injectable({
  providedIn: 'root',
})
export class DarkModeService {
  static readonly DARK_MODE_SELECTOR = 'trivy-operator-dashboard-dark';
  private readonly localStorageKey = 'mainSettings.isDarkMode';

  // The single source of truth
  readonly isDarkMode = signal(false);

  // The user's choice, for the theme selector only. 'system' = no stored value (follow the OS).
  readonly themeMode = signal<ThemeMode>('system');

  constructor() {
    this.restoreMode();
    this.watchSystemDarkMode();

    let isFirstRun = true;
    effect(() => {
      const dark = this.isDarkMode();
      const root = document.documentElement;
      const apply = () => root.classList.toggle(DarkModeService.DARK_MODE_SELECTOR, dark);

      // No animation on startup, with reduced-motion, or in browsers without the View Transitions API.
      // The fade duration is set in styles.scss (::view-transition-old/new(root)).
      const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
      if (isFirstRun || reducedMotion || !document.startViewTransition) {
        isFirstRun = false;
        apply();
        return;
      }
      document.startViewTransition(apply);
    });
  }

  restoreMode() {
    const saved = LocalStorageUtils.getBoolKeyValue(this.localStorageKey);
    const prefers = window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.isDarkMode.set(saved ?? prefers);
    this.themeMode.set(saved === null ? 'system' : saved ? 'dark' : 'light');
  }

  setThemeMode(mode: ThemeMode) {
    if (mode === 'system') {
      LocalStorageUtils.removeItem(this.localStorageKey);
      this.isDarkMode.set(window.matchMedia('(prefers-color-scheme: dark)').matches);
    } else {
      LocalStorageUtils.setItem(this.localStorageKey, (mode === 'dark').toString());
      this.isDarkMode.set(mode === 'dark');
    }
    this.themeMode.set(mode);
  }

  toggleDarkMode() {
    const next = !this.isDarkMode();
    LocalStorageUtils.setItem(this.localStorageKey, next.toString());
    this.isDarkMode.set(next);
  }

  private watchSystemDarkMode() {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    mediaQuery.addEventListener('change', (event) => {
      const saved = LocalStorageUtils.getBoolKeyValue(this.localStorageKey);

      // If user manually set a theme, ignore system changes
      if (saved !== null) return;

      const newValue = event.matches;

      this.isDarkMode.set(event.matches);
    });
  }
}
