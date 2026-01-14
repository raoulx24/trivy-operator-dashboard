import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { TitleService } from './services/title.service';
import { NavMenuComponent } from './ui-elements/nav-menu/nav-menu.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavMenuComponent, RouterOutlet],
  templateUrl: './app.component.html',
})
export class AppComponent {
  constructor(private titleService: TitleService) {}
}
