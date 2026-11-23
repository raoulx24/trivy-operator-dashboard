import {
  AfterViewInit,
  Component,
  ElementRef,
  HostListener,
  Injector,
  afterNextRender,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';

import { CurrentPageService } from '@core/services/current-page.service';
import { NavBarVisibilityService } from '@core/services/nav-bar-visibility.service';
import { NavMenuComponent } from '@core/layout/nav-menu/nav-menu.component';
import { VulnerabilityTagDialogsComponent } from '@shared/ui/vulnerability-tag-dialogs/vulnerability-tag-dialogs.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ToastModule, NavMenuComponent, RouterOutlet, VulnerabilityTagDialogsComponent],
  templateUrl: './app.component.html',
})
export class AppComponent implements AfterViewInit {
  // true once the page is scrolled, i.e. there is content passing under the nav bar
  isScrolled = signal(false);

  private readonly navHost = viewChild<ElementRef<HTMLElement>>('navHost');
  private readonly content = viewChild<ElementRef<HTMLElement>>('content');
  private readonly injector = inject(Injector);

  private readonly currentPageService = inject(CurrentPageService);
  protected readonly navBar = inject(NavBarVisibilityService);

  // the height of the bar (px): the space it takes in the layout when it is shown while a hide is requested
  protected readonly barHeight = signal(0);

  constructor() {
    effect(() => {
      if (this.navBar.hideRequested()) this.measureBar();
    });
  }

  ngAfterViewInit() {
    this.measureBar();
  }

  @HostListener('window:resize')
  measureBar() {
    this.barHeight.set(this.menubar()?.getBoundingClientRect().height ?? 0);
  }

  // the bar itself: the hosts around it (app-nav-menu, p-menubar) are inline elements, their rect is not the bar's
  private menubar(): Element | null {
    return this.navHost()?.nativeElement.querySelector('.p-menubar') ?? null;
  }

  @HostListener('window:scroll')
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 0);
    this.navBar.onScroll(window.scrollY);

    // a bar in the layout at the top stays (pinned) while the user scrolls down, and leaves only when the scrolling
    // stops: leaving takes its height out of the page, and doing that under a running scroll makes the page jump
    clearTimeout(this.scrollIdleTimer);
    if (this.navBar.hideRequested()) this.scrollIdleTimer = setTimeout(() => this.onScrollIdle(), 150);
  }

  private scrollIdleTimer?: ReturnType<typeof setTimeout>;

  private onScrollIdle() {
    const contentTop = this.content()?.nativeElement.getBoundingClientRect().top ?? 0;
    if (!this.navBar.leaveTop(window.scrollY, this.menubar()?.getBoundingClientRect().height ?? 0)) return;

    // the bar just left the layout: the page is shorter by its height and the content moves up. Scroll up by the same
    // amount once the page is rendered, so the content stays where the user sees it. The browser's own scroll
    // anchoring is off meanwhile (it would do the same and the two would add up). That scroll is not the user's: the
    // service must not read it as a scroll up.
    const root = document.documentElement;
    root.style.overflowAnchor = 'none';
    afterNextRender(
      () => {
        const shift = (this.content()?.nativeElement.getBoundingClientRect().top ?? contentTop) - contentTop;
        if (Math.abs(shift) > 0.5) window.scrollBy({ top: shift, behavior: 'instant' });
        this.navBar.syncScroll(window.scrollY);
        requestAnimationFrame(() => root.style.removeProperty('overflow-anchor'));
      },
      { injector: this.injector },
    );
  }

  // a bar shown by the mouse at the top edge goes away when the mouse leaves it (the bar is out of the layout, so it
  // is measured, not listened to)
  @HostListener('document:mousemove', ['$event'])
  onMouseMove(event: MouseEvent) {
    if (!this.navBar.hideRequested()) return;
    const bar = this.menubar();
    if (!bar) return;
    const hoverGapPx = 12;
    if (event.clientY > bar.getBoundingClientRect().bottom + hoverGapPx) this.navBar.setHovering(false);
  }
}
