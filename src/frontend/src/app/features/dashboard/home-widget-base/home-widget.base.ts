import { DestroyRef, Directive, ElementRef, Injector, afterNextRender, computed, effect, inject } from '@angular/core';

import { HomeMotionService } from '../home-motion/home-motion.service';
import { HomeWidgetGroupDirective } from '../home-widget-group/home-widget-group.directive';

// the base of every home widget: the part that belongs to the widget class, not to its template (the template puts the
// widget in an app-home-widget-frame, which draws the "..." button)
// - registers the widget in its appHomeWidgetGroup (if it is in one), which allows at most one expanded widget
// - isExpanded / canExpand / toggleExpand are for the template and for the widget, e.g. to ask the server for more
//   rows while expanded
// - the host bindings give the expanded widget its class (the grid placement) and the name of its view transition
// - animateItems(change) is for a change inside the widget (a filter, a sort): mark the things that move with
//   appHomeVtItem (see HomeMotionService, HomeVtItemDirective)
@Directive({
  host: {
    // styles.scss names the widgets (view-transition-name) with this while one expands or collapses
    class: 'tod-home-widget',
    '[class.home-widget-expanded]': 'isExpanded()',
    // styles.scss gives the widget its view-transition-name from this, only while a transition runs
    '[style.--home-widget-vt-name]': 'transitionName',
  },
})
export abstract class HomeWidgetBase {
  private readonly group = inject(HomeWidgetGroupDirective, { optional: true });
  private readonly injector = inject(Injector);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly motion = inject(HomeMotionService, { optional: true });
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

  // makes a change of what the widget shows (a filter, a sort, ...) with an animation: the things marked with
  // appHomeVtItem move from where they were to where they are, new ones fade in, removed ones fade out
  // without HomeMotionService (outside the home page), or without support, the change is simply made
  protected animateItems(change: () => void): void {
    if (this.motion) {
      this.motion.run(change, 'items');
    } else {
      change();
    }
  }

  protected toggleExpand(): void {
    if (this.id !== undefined) {
      this.group?.toggle(this.id);
    }
  }
}
