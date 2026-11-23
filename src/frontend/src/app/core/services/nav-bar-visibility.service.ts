import { Injectable, computed, signal } from '@angular/core';

// Hides the nav bar on request (e.g. an expanded home widget wants the whole screen) and brings it back when the user
// wants it: scrolling up, or the mouse at the top edge of the window.
// - the requesters are tracked by owner (any object), so two widgets cannot undo each other; no owner left, the bar is shown
// - hideRequested: the bar leaves the layout (the content moves up, once); hidden: the bar is also out of sight
// - revealed / hovering: the temporary ways back; the bar then overlays the content, nothing moves
@Injectable({
  providedIn: 'root',
})
export class NavBarVisibilityService {
  // scrolling up by this many px reveals the bar (a wheel tick or a trackpad jitter is not enough)
  static readonly REVEAL_SCROLL_UP_PX = 60;

  private readonly owners = signal<ReadonlySet<object>>(new Set());
  private readonly revealed = signal(false);
  private readonly hovering = signal(false);
  private lastScrollY = 0;
  private scrolledUpPx = 0;

  readonly hideRequested = computed(() => this.owners().size > 0);
  readonly hidden = computed(() => this.hideRequested() && !this.revealed() && !this.hovering());

  // at the very top of the page a visible bar takes its place in the layout again (pushes the content down) instead of
  // overlaying it, which would cover the first controls of the page (e.g. the search box)
  private readonly atTop = signal(window.scrollY <= 0);
  readonly isAtTop = this.atTop.asReadonly();
  readonly collapsed =computed(() => this.hideRequested() && (this.hidden() || !this.atTop()));

  hide(owner: object) {
    if (this.owners().has(owner)) return;
    const wasRequested = this.hideRequested();
    this.owners.update((owners) => new Set(owners).add(owner));
    if (!wasRequested) this.resetTemporary();
  }

  show(owner: object) {
    if (!this.owners().has(owner)) return;
    this.owners.update((owners) => {
      const next = new Set(owners);
      next.delete(owner);
      return next;
    });
    if (!this.hideRequested()) this.resetTemporary();
  }

  // fed by the window scroll event
  onScroll(scrollY: number) {
    const delta = scrollY - this.lastScrollY;
    this.lastScrollY = scrollY;
    if (scrollY <= 0) this.atTop.set(true);
    if (!this.hideRequested() || delta === 0) return;

    if (delta > 0) {
      this.scrolledUpPx = 0;
      // the bar sits in the layout at the top: it stays while the content slides under it (leaveTop takes it away)
      if (this.atTop() && this.revealed()) return;
      this.atTop.set(false);
      this.revealed.set(false);
      return;
    }
    this.scrolledUpPx -= delta;
    if (this.scrolledUpPx >= NavBarVisibilityService.REVEAL_SCROLL_UP_PX || scrollY <= 0) {
      this.revealed.set(true);
    }
  }

  // the user stopped scrolling down with the bar in the layout at the top: once the content is scrolled past the bar's
  // height, the bar leaves. True when it did (the caller then corrects the scroll position).
  leaveTop(scrollY: number, barHeight: number): boolean {
    if (!this.hideRequested() || !this.atTop() || !this.revealed() || scrollY < barHeight) return false;
    this.atTop.set(false);
    this.revealed.set(false);
    return true;
  }

  // the page was scrolled by the app itself (not the user): the next scroll event must not count as the user's
  syncScroll(scrollY: number) {
    this.lastScrollY = scrollY;
  }

  // the mouse is at the top edge (the hot zone), or left the bar
  setHovering(value: boolean) {
    if (this.hovering() !== value) this.hovering.set(value);
  }

  private resetTemporary() {
    this.revealed.set(false);
    this.hovering.set(false);
    this.scrolledUpPx = 0;
    this.lastScrollY = window.scrollY;
    this.atTop.set(window.scrollY <= 0);
  }
}
