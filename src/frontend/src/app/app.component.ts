import { Component, ElementRef, HostListener, signal, viewChild } from '@angular/core';
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
export class AppComponent {
  // true once the page is scrolled, i.e. there is content passing under the nav bar
  isScrolled = signal(false);

  private readonly navHost = viewChild<ElementRef<HTMLElement>>('navHost');

  constructor(
    private currentPageService: CurrentPageService,
    protected navBar: NavBarVisibilityService,
  ) {}

  @HostListener('window:scroll')
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 0);
    this.navBar.onScroll(window.scrollY);
  }

  // a bar shown by the mouse at the top edge goes away when the mouse leaves it (the bar is out of the layout, so it
  // is measured, not listened to)
  @HostListener('document:mousemove', ['$event'])
  onMouseMove(event: MouseEvent) {
    if (!this.navBar.hideRequested()) return;
    const bar = this.navHost()?.nativeElement.firstElementChild;
    if (!bar) return;
    const hoverGapPx = 12;
    if (event.clientY > bar.getBoundingClientRect().bottom + hoverGapPx) this.navBar.setHovering(false);
  }
}
