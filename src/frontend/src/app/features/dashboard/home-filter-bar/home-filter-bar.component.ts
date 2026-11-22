import { ChangeDetectionStrategy, Component, computed, input, model, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { ToggleSwitchModule } from 'primeng/toggleswitch';

import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { severityTextColor } from '../home-collapsed/home-collapsed';

import { HomeWindowMode, homeSeverityOptions, homeWindowOptions } from './home-filter-bar.types';

// the filters of the home page: the window of the widgets about changes, the severities, the namespaces, the images of the history, and a refresh
@Component({
  selector: 'app-home-filter-bar',
  standalone: true,
  imports: [FormsModule, ButtonModule, MultiSelectModule, SelectModule, ToggleSwitchModule, PillButtonComponent],
  templateUrl: './home-filter-bar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeFilterBarComponent {
  windowMode = model.required<HomeWindowMode>();
  severityIds = model.required<number[]>();
  namespaces = model.required<string[]>();
  // the history widgets: only the images running now (default), or the stale ones too
  activeImagesOnly = model.required<boolean>();

  namespaceOptions = input<string[]>([]);
  // the resolved start of the window, e.g. "2026-11-08 (UTC)"
  sinceLabel = input<string>('');
  // without an earlier visit, "since previous visit" would be the history window, so it is disabled
  hasPreviousVisit = input<boolean>(true);

  refresh = output<void>();

  protected readonly windowOptions = computed(() =>
    homeWindowOptions.map((option) => ({
      ...option,
      disabled: option.value === 'lastVisit' && !this.hasPreviousVisit(),
    })),
  );
  // with the color of the severity, as everywhere else in the app
  protected readonly severityOptions = homeSeverityOptions.map((option) => ({
    ...option,
    color: SeverityUtils.getCssColor(option.value),
    textColor: severityTextColor(option.value),
  }));

  // a click on a severity pill: in or out of the selection (the last one stays, see onSeverityIdsChange)
  protected toggleSeverity(severityId: number) {
    const current = this.severityIds();
    this.onSeverityIdsChange(
      current.includes(severityId) ? current.filter((x) => x !== severityId) : [...current, severityId],
    );
  }

  // at least one severity stays selected (none would mean "default" to the backend, which would be confusing)
  protected onSeverityIdsChange(value: number[] | null) {
    if (value && value.length > 0) {
      this.severityIds.set([...value].sort((a, b) => a - b));
    }
  }
}
