import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { TitleService } from '@core/services/title.service';
import { NavMenuComponent } from '@core/layout/nav-menu/nav-menu.component';
import { VulnerabilityTagDialogsComponent } from '@shared/ui/vulnerability-tag-dialogs/vulnerability-tag-dialogs.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavMenuComponent, RouterOutlet, VulnerabilityTagDialogsComponent],
  templateUrl: './app.component.html',
})
export class AppComponent {
  constructor(private titleService: TitleService) {}
}
