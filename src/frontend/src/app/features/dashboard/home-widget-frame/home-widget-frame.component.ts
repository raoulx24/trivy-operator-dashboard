import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';

import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// the frame of a home widget: border, heading with its hint, the status line and the "..." button that expands the
// widget (shown when canExpand); the widget itself is the projected content
// the widget class extends HomeWidgetBase and binds canExpand / expanded / expandToggle to it
@Component({
  selector: 'app-home-widget-frame',
  standalone: true,
  imports: [ButtonModule],
  templateUrl: './home-widget-frame.component.html',
  styleUrl: './home-widget-frame.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeWidgetFrameComponent {
  heading = input.required<string>();
  hint = input<string>('');
  state = input.required<HomeWidgetState>();
  canExpand = input(false);
  expanded = input(false);

  expandToggle = output<void>();
}
