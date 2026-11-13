import { Injectable, Injector, afterNextRender, inject } from '@angular/core';

// what animates: 'widget' = a widget expands or collapses (the widgets are named, see HomeWidgetGroupDirective);
// 'items' = the things inside a widget change (the ones marked with appHomeVtItem are named)
export type HomeMotionKind = 'widget' | 'items';

// set on <html> while a transition of that kind runs; styles.scss gives the names, the duration and the easing
const transitionClasses: Record<HomeMotionKind, string> = {
  widget: 'tod-widget-vt',
  items: 'tod-items-vt',
};

// the animation of the home page: runs a change of the page (the signal update that moves things around) as a view
// transition, so the browser animates the elements that have a view-transition-name from where they were to where they
// are; elements are named only while it runs (a named element would be left out of the circular reveal of the
// light/dark switch, which uses the same root pseudo-elements)
// without support, or with reduced motion, the change is simply made
// provided by the dashboard (one per visit of the home page), used by the widget groups and, through HomeWidgetBase,
// by every widget
@Injectable()
export class HomeMotionService {
  private readonly injector = inject(Injector);

  private transitionEnd: Promise<void> = Promise.resolve();
  // the latest transition per kind: a skipped (replaced) one must not remove the class of the one that replaced it
  private readonly latest = new Map<HomeMotionKind, symbol>();

  run(change: () => void, kind: HomeMotionKind): void {
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reducedMotion || !document.startViewTransition) {
      change();
      return;
    }

    const root = document.documentElement;
    const className = transitionClasses[kind];
    const token = Symbol(kind);
    this.latest.set(kind, token);
    root.classList.add(className);

    // the new state is captured when the promise resolves, so it resolves after the render of the change (calling
    // ApplicationRef.tick here instead throws NG0101, as the callback can run while a tick is in progress)
    const transition = document.startViewTransition(
      () =>
        new Promise<void>((resolve) => {
          afterNextRender(() => resolve(), { injector: this.injector });
          change();
        }),
    );
    this.transitionEnd = transition.finished
      .catch(() => undefined) // skipped (e.g. tab hidden, or replaced by another): the layout is still changed, only the animation is lost
      .then(() => {
        if (this.latest.get(kind) === token) {
          root.classList.remove(className);
        }
      });
  }

  // resolves when the running animation is over (at once when there is none): a scroll made before it would be hidden
  // under the animation and then jump
  whenSettled(): Promise<void> {
    return this.transitionEnd;
  }
}
