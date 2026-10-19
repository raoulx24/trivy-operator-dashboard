import { effect, inject, Injectable, signal } from '@angular/core';
import { ActivatedRouteSnapshot, NavigationEnd, Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class RouterEventEmitterService {
  private readonly _title = signal<string>('');
  readonly title = this._title.asReadonly();

  private readonly router = inject(Router);

  constructor() {
    effect(() => {
      const sub = this.router.events.subscribe((event) => {
        if (event instanceof NavigationEnd) {
          const title = this.getTitleFromRoute(this.router.routerState.snapshot.root);
          this._title.set(title);
        }
      });

      // Cleanup when the service is destroyed
      return () => sub.unsubscribe();
    });
  }

  private getTitleFromRoute(routeSnapshot: ActivatedRouteSnapshot): string {
    let title = routeSnapshot.data['title'] || '';
    if (routeSnapshot.firstChild) {
      title = this.getTitleFromRoute(routeSnapshot.firstChild) || title;
    }
    return title;
  }
}
