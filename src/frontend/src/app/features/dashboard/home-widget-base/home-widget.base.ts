import { DestroyRef, Directive, ElementRef, Injector, afterNextRender, computed, effect, inject } from '@angular/core';

import { HomeWidgetGroupDirective } from '../home-widget-group/home-widget-group.directive';

// the base of every home widget: the part that belongs to the widget class, not to its template (the template puts the
// widget in an app-home-widget-frame, which draws the "..." button)
// - registers the widget in its appHomeWidgetGroup (if it is in one), which allows at most one expanded widget
// - isExpanded / canExpand / toggleExpand are for the template and for the widget, e.g. to ask the server for more
//   rows while expanded
// - the host bindings give the expanded widget its class (the grid placement) and the name of its view transition
@Directive({
  host: {
    '[class.home-widget-expanded]': 'isExpanded()',
    // styles.scss gives the widget its view-transition-name from this, only while a transition runs
    '[style.--home-widget-vt-name]': 'transitionName',
  },
})
export abstract class HomeWidgetBase {
  private readonly group = inject(HomeWidgetGroupDirective, { optional: true });
  private readonly injector = inject(Injector);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly id = this.group?.register();

  protected readonly transitionName = this.id === undefined ? null : `home-widget-${this.id}`;

  // a widget can be expanded only inside a group, and only when the group is wide enough (lg and up)
  readonly canExpand = computed(() => !!this.group && this.group.isWide());
  readonly isExpanded = computed(() => this.id !== undefined && this.group?.expandedId() === this.id);

  constructor() {
    // every widget is brought into view when it is expanded; one that gets more data calls revealAfterRender again
    // when it is rendered (it grows)
    effect(() => {
      if (this.isExpanded()) {
        this.revealAfterRender();
      }
    });

    inject(DestroyRef).onDestroy(() => {
      if (this.id !== undefined) {
        this.group?.unregister(this.id);
      }
    });
  }

  // for a widget that got more data while expanded: once it is rendered (the widget is bigger now), the page scrolls
  // the least needed to show the whole widget (nothing if it is in view already; if it is taller than the screen, its
  // top is aligned)
  protected revealAfterRender(): void {
    afterNextRender(
      () => {
        // after the expand animation, if there is one running
        void (this.group?.whenSettled() ?? Promise.resolve()).then(() => {
          if (this.isExpanded()) {
            this.host.nativeElement.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
          }
        });
      },
      { injector: this.injector },
    );
  }

  protected toggleExpand(): void {
    if (this.id !== undefined) {
      this.group?.toggle(this.id);
    }
  }
}
