import { Component, HostListener, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';

import { CurrentPageService } from '@core/services/current-page.service';
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

  constructor(private currentPageService: CurrentPageService) {}

  @HostListener('window:scroll')
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 0);
  }
}
