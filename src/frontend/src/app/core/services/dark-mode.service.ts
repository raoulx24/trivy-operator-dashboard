import { LocalStorageUtils } from '@shared/utils/local-storage.utils';

import { Injectable, effect, signal } from '@angular/core';

export type ThemeMode = 'light' | 'system' | 'dark';

@Injectable({
  providedIn: 'root',
})
export class DarkModeService {
  static readonly DARK_MODE_SELECTOR = 'trivy-operator-dashboard-dark';
  static readonly REVEAL_DURATION_MS = 2000;
  // fast start, slow middle, fast end (the reverse of ease-in-out). Roughly: the first ~10% of the time covers ~40% of
  // the distance, the last ~10% another ~40%, and the middle 80% crawls through the remaining ~20%.
  static readonly REVEAL_EASING = 'cubic-bezier(0.2, 0.8, 0.7, 0.3)';
  static readonly REVEAL_FEATHER_PX = 1200; // width of the zone where old and new theme blend
  private readonly localStorageKey = 'mainSettings.isDarkMode';
  private revealOrigin?: { x: number; y: number };

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
      const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
      if (isFirstRun || reducedMotion || !document.startViewTransition) {
        isFirstRun = false;
        this.revealOrigin = undefined;
        apply();
        return;
      }

      // Circular reveal with a soft edge: the new theme grows as a circle from `origin` (the theme button).
      // Inside the feather band at the edge, old and new theme blend (see the mask in styles.scss, which reads the
      // --tod-reveal-* properties). The default crossfade is switched off there too.
      // Without an origin (e.g. the OS theme changed) it grows from the top-right corner, where the button is.
      const { x, y } = this.revealOrigin ?? { x: window.innerWidth, y: 0 };
      this.revealOrigin = undefined;
      // feather: at most REVEAL_FEATHER_PX, but never more than 25% of the viewport diagonal (small windows)
      const feather = Math.min(DarkModeService.REVEAL_FEATHER_PX, 0.5 * Math.hypot(window.innerWidth, window.innerHeight));
      // the fully opaque part (radius - feather) must reach the farthest corner
      const maxRadius =
        Math.hypot(Math.max(x, window.innerWidth - x), Math.max(y, window.innerHeight - y)) + feather;

      root.style.setProperty('--tod-reveal-x', `${x}px`);
      root.style.setProperty('--tod-reveal-y', `${y}px`);
      root.style.setProperty('--tod-reveal-feather', `${feather}px`);

      const transition = document.startViewTransition(apply);
      transition.ready
        .then(() =>
          root.animate(
            { '--tod-reveal-radius': ['0px', `${maxRadius}px`] },
            {
              duration: DarkModeService.REVEAL_DURATION_MS,
              easing: DarkModeService.REVEAL_EASING,
              fill: 'forwards', // without it the radius falls back to 0 (new theme hidden) when the animation ends
              pseudoElement: '::view-transition-new(root)',
            },
          ),
        )
        .catch(() => {
          // transition was skipped (e.g. tab hidden): the theme is still applied, only the animation is lost
        });
      transition.finished
        .catch(() => undefined)
        .then(() => {
          root.style.removeProperty('--tod-reveal-x');
          root.style.removeProperty('--tod-reveal-y');
          root.style.removeProperty('--tod-reveal-feather');
        });
    });
  }

  restoreMode() {
    const saved = LocalStorageUtils.getBoolKeyValue(this.localStorageKey);
    const prefers = window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.isDarkMode.set(saved ?? prefers);
    this.themeMode.set(saved === null ? 'system' : saved ? 'dark' : 'light');
  }

  // `origin` (viewport px) is where the circular reveal starts, when the theme actually changes.
  setThemeMode(mode: ThemeMode, origin?: { x: number; y: number }) {
    this.revealOrigin = origin;
    if (mode === 'system') {
      LocalStorageUtils.removeItem(this.localStorageKey);
      this.isDarkMode.set(window.matchMedia('(prefers-color-scheme: dark)').matches);
    } else {
      LocalStorageUtils.setItem(this.localStorageKey, (mode === 'dark').toString());
      this.isDarkMode.set(mode === 'dark');
    }
    this.themeMode.set(mode);
  }

  private watchSystemDarkMode() {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    mediaQuery.addEventListener('change', (event) => {
      const saved = LocalStorageUtils.getBoolKeyValue(this.localStorageKey);

      // If user manually set a theme, ignore system changes
      if (saved !== null) return;

      this.isDarkMode.set(event.matches);
    });
  }
}
