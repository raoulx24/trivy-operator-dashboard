import { BreakpointObserver } from '@angular/cdk/layout';
import { inject, Injectable, signal, TemplateRef } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

// The contextual part of the nav bar: the page that is on screen registers a template (see ToolbarDirective), the nav bar
// renders it. The template stays bound to the page that declared it, so the state remains in the page.
@Injectable({
  providedIn: 'root',
})
export class ToolbarService {
  private readonly breakpoints = inject(BreakpointObserver);
  private readonly current = signal<TemplateRef<unknown> | null>(null);

  readonly template = this.current.asReadonly();

  // Below Tailwind's xl (1280px) the controls do not fit the nav bar: the page shows them collapsed (e.g. in a popover).
  readonly compact = toSignal(this.matches('(max-width: 1279.98px)'), { initialValue: false });

  register(template: TemplateRef<unknown>) {
    this.current.set(template);
  }

  // only the owner clears it: when navigating, the next page may have registered already
  unregister(template: TemplateRef<unknown>) {
    if (this.current() === template) {
      this.current.set(null);
    }
  }

  private matches(query: string) {
    return this.breakpoints.observe(query).pipe(map((state) => state.matches));
  }
}
