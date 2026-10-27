import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { TitleService } from '@core/services/title.service';
import { NavMenuComponent } from '@core/layout/nav-menu/nav-menu.component';
import { NvdCveDialogComponent } from '@shared/ui/nvd-cve-dialog/nvd-cve-dialog.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavMenuComponent, NvdCveDialogComponent, RouterOutlet],
  templateUrl: './app.component.html',
})
export class AppComponent {
  constructor(private titleService: TitleService) {}
}
