import { LocalStorageUtils } from '../utils/local-storage.utils';

import { Injectable, effect, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DarkModeService {
  static readonly DARK_MODE_SELECTOR = 'trivy-operator-dashboard-dark';
  private readonly localStorageKey = 'mainSettings.isDarkMode';

  // The single source of truth
  readonly isDarkMode = signal(false);

  constructor() {
    this.restoreMode();
    this.watchSystemDarkMode();

    effect(() => {
      const dark = this.isDarkMode();
      const root = document.documentElement;
      root.classList.toggle(DarkModeService.DARK_MODE_SELECTOR, dark);
    });
  }

  restoreMode() {
    const saved = LocalStorageUtils.getBoolKeyValue(this.localStorageKey);
    const prefers = window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.isDarkMode.set(saved ?? prefers);
  }

  toggleDarkMode() {
    const next = !this.isDarkMode();
    localStorage.setItem(this.localStorageKey, next.toString());
    this.isDarkMode.set(next);
  }

  private watchSystemDarkMode() {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    mediaQuery.addEventListener('change', event => {
      const saved = LocalStorageUtils.getBoolKeyValue(this.localStorageKey);

      // If user manually set a theme, ignore system changes
      if (saved !== null) return;

      const newValue = event.matches;

      this.isDarkMode.set(event.matches);
    });
  }
}
