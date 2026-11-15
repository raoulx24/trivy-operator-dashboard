import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from './home-widget-placeholder.types';

// a stand-in for a home page widget: says what the widget will show and from which DTO, plus the state of the data
// (loading / unavailable with reason / a short summary of what arrived); replace each usage with the real widget
// (a HomeWidgetBase in an app-home-widget-frame, see HomePackageHotspotsComponent)
// with searchPlaceholder set, it also has a search box (Enter searches)
@Component({
  selector: 'app-home-widget-placeholder',
  standalone: true,
  imports: [HomeWidgetFrameComponent],
  templateUrl: './home-widget-placeholder.component.html',
  styleUrl: './home-widget-placeholder.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeWidgetPlaceholderComponent extends HomeWidgetBase {
  heading = input.required<string>();
  hint = input<string>('');
  description = input.required<string>();
  source = input.required<string>();
  state = input.required<HomeWidgetState>();
  searchPlaceholder = input<string>('');

  search = output<string>();
  // the refresh button: the dashboard reloads the data of this widget (a search widget runs its search again)
  refresh = output<void>();

  protected readonly query = signal('');

  protected onRefresh(): void {
    if (this.searchPlaceholder()) {
      // nothing to run again before the first search
      if (this.query().trim()) this.onSearch();
    } else {
      this.refresh.emit();
    }
  }

  protected onSearch(): void {
    this.search.emit(this.query().trim());
  }
}
