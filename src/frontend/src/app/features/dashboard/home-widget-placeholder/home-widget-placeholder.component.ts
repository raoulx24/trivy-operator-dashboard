import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { HomeWidgetState } from './home-widget-placeholder.types';

// a stand-in for a home page widget: says what the widget will show and from which DTO, plus the state of the data
// (loading / unavailable with reason / a short summary of what arrived); replace each usage with the real widget
@Component({
  selector: 'app-home-widget-placeholder',
  standalone: true,
  templateUrl: './home-widget-placeholder.component.html',
  styleUrl: './home-widget-placeholder.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeWidgetPlaceholderComponent {
  title = input.required<string>();
  hint = input<string>('');
  description = input.required<string>();
  source = input.required<string>();
  state = input.required<HomeWidgetState>();
}
