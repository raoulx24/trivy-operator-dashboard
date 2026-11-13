import { DestroyRef, Directive, computed, effect, inject, signal } from '@angular/core';

import { HomeMotionService } from '../home-motion/home-motion.service';
import { HomeWidgetExpansionService } from './home-widget-expansion.service';

// a group of home widgets laid out in a grid (one row, or a few that wrap by breakpoint); at most one widget on the
// whole page is expanded (HomeWidgetExpansionService): the group that has it puts it in the left ~70% and stacks the
// others, in their order, in the right ~30% (see dashboard.component.scss); the other groups stay as they are
// the DOM order never changes, the placement is done with the grid, so the widgets keep their state; the change is
// animated with a view transition; the widgets are named (view-transition-name) only while it runs, see styles.scss,
// as a named element would be left out of the circular reveal of the light/dark switch
// the expanded layout exists only from the "lg" breakpoint up; below it the group stays as it is (no button, and a
// widget expanded when the window gets narrower collapses)
@Directive({
  selector: '[appHomeWidgetGroup]',
  standalone: true,
  host: {
    class: 'home-group',
    '[class.home-group-open]': 'isOpen()',
    '[style.--home-group-rows]': 'stackedCount()',
  },
})
export class HomeWidgetGroupDirective {
  // the same as the tailwind "lg" breakpoint
  private static readonly wideQuery = '(min-width: 1024px)';
  private static nextId = 0;

  private readonly destroyRef = inject(DestroyRef);
  private readonly expansion = inject(HomeWidgetExpansionService);
  private readonly motion = inject(HomeMotionService);

  // the ids of the widgets of this group
  private readonly widgetIds = signal<ReadonlySet<number>>(new Set());

  // the expanded widget of the page (not necessarily of this group)
  readonly expandedId = this.expansion.expandedId.asReadonly();
  readonly isWide = signal(false);

  protected readonly isOpen = computed(() => {
    const id = this.expandedId();
    return id !== null && this.isWide() && this.widgetIds().has(id);
  });
  // how many widgets stack beside the expanded one (the rows the expanded one spans)
  protected readonly stackedCount = computed(() => Math.max(1, this.widgetIds().size - 1));

  constructor() {
    const query = window.matchMedia(HomeWidgetGroupDirective.wideQuery);
    this.isWide.set(query.matches);
    const onChange = (e: MediaQueryListEvent) => this.isWide.set(e.matches);
    query.addEventListener('change', onChange);
    this.destroyRef.onDestroy(() => query.removeEventListener('change', onChange));

    // narrower than lg: back to the normal layout (no animation, the window is being resized)
    effect(() => {
      if (!this.isWide() && this.expandedId() !== null) {
        this.expansion.expandedId.set(null);
      }
    });
  }

  // called by each widget of the group; returns its id (also the name of its view transition)
  register(): number {
    const id = HomeWidgetGroupDirective.nextId++;
    this.widgetIds.update((x) => new Set(x).add(id));
    return id;
  }

  unregister(id: number): void {
    this.widgetIds.update((x) => {
      const ids = new Set(x);
      ids.delete(id);
      return ids;
    });
    if (this.expandedId() === id) {
      this.expansion.expandedId.set(null);
    }
  }

  // expands the widget (the expanded one of any group collapses); when it is the expanded one already, collapses it
  toggle(id: number): void {
    const next = this.expandedId() === id ? null : id;
    this.motion.run(() => this.expansion.expandedId.set(next), 'widget');
  }

  // resolves when the running animation (expand / collapse, or of the items of a widget) is over (at once when there
  // is none): a scroll made before it would be hidden under the animation and then jump
  whenSettled(): Promise<void> {
    return this.motion.whenSettled();
  }
}
