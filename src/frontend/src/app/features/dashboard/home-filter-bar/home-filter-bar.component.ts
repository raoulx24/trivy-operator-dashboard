import { ChangeDetectionStrategy, Component, input, model, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { SelectButtonModule } from 'primeng/selectbutton';

import { HomeWindowMode, homeSeverityOptions, homeWindowOptions } from './home-filter-bar.types';

// the filters of the home page: the window of the widgets about changes, the severities, the namespaces, and a refresh
@Component({
  selector: 'app-home-filter-bar',
  standalone: true,
  imports: [FormsModule, ButtonModule, MultiSelectModule, SelectModule, SelectButtonModule],
  templateUrl: './home-filter-bar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeFilterBarComponent {
  windowMode = model.required<HomeWindowMode>();
  severityIds = model.required<number[]>();
  namespaces = model.required<string[]>();

  namespaceOptions = input<string[]>([]);
  // the resolved start of the window, e.g. "2026-11-08 (UTC)"
  sinceLabel = input<string>('');

  refresh = output<void>();

  protected readonly windowOptions = homeWindowOptions;
  protected readonly severityOptions = homeSeverityOptions;

  // at least one severity stays selected (none would mean "default" to the backend, which would be confusing)
  protected onSeverityIdsChange(value: number[] | null) {
    if (value && value.length > 0) {
      this.severityIds.set([...value].sort((a, b) => a - b));
    }
  }
}
