import { computed, effect, inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRouteSnapshot, NavigationEnd, Router } from '@angular/router';
import { filter, map } from 'rxjs';

export interface CurrentPage {
  // routeConfig.path of the deepest activated route ('' for home); null until the first navigation ends.
  // Matches NavEntry.route in nav-menu.items.ts. Never contains params/query, unlike router.url.
  route: string | null;
  // data.title of the deepest route that defines one
  title: string;
}

// Single source of truth for "which page is the user on": feeds the browser tab title,
// the header title, the logger source and the active entry of the nav menu.
@Injectable({
  providedIn: 'root',
})
export class CurrentPageService {
  private readonly defaultTitle: string = 'Trivy Operator Dashboard';

  private readonly router = inject(Router);
  private readonly browserTitle = inject(Title);

  readonly page = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map(() => this.readPage(this.router.routerState.snapshot.root)),
    ),
    { initialValue: { route: null, title: '' } as CurrentPage },
  );

  readonly title = computed(() => this.page().title);
  readonly route = computed(() => this.page().route);

  constructor() {
    effect(() => this.updateBrowserTitle(this.title()));
  }

  private readPage(root: ActivatedRouteSnapshot): CurrentPage {
    let route: string | null = null;
    let title = '';

    for (let snapshot: ActivatedRouteSnapshot | null = root; snapshot; snapshot = snapshot.firstChild) {
      title = snapshot.data['title'] || title;
      if (snapshot.routeConfig) {
        route = snapshot.routeConfig.path ?? '';
      }
    }

    return { route, title };
  }

  // 'Reports' is shortened so the title does not wrap on mobile
  private updateBrowserTitle(routeTitle: string) {
    const fullTitle = [routeTitle.replace('Reports', 'Reps'), this.defaultTitle].filter(Boolean).join(' - ');
    this.browserTitle.setTitle(fullTitle);
  }
}
