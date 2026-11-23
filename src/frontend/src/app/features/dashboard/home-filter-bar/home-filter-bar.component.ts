import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  linkedSignal,
  model,
  output,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { MultiSelectModule } from 'primeng/multiselect';
import { Popover, PopoverModule } from 'primeng/popover';
import { SelectModule } from 'primeng/select';
import { ToggleSwitchModule } from 'primeng/toggleswitch';

import { SeverityUtils } from '@shared/utils/severity.utils';

import { ToolbarService } from '../../../core/layout/toolbar/toolbar.service';

import { HomeWindowMode, homeDefaultSeverityIds, homeSeverityOptions, homeWindowOptions } from './home-filter-bar.types';

// the filters of the home page: the window of the widgets about changes, the severities, the namespaces, the images of the history, and a refresh
@Component({
  selector: 'app-home-filter-bar',
  standalone: true,
  imports: [FormsModule, NgTemplateOutlet, ButtonModule, MultiSelectModule, PopoverModule, SelectModule, ToggleSwitchModule],
  templateUrl: './home-filter-bar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '(document:click)': 'closeOnOutsideClick($event)' },
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
  }));

  // below xl the controls are collapsed into a popover (see ToolbarService)
  protected readonly compact = inject(ToolbarService).compact;
  private readonly filtersPopover = viewChild(Popover);

  // The popover is not dismissed by PrimeNG: the overlays of its selects live in the body, so a click on an option would
  // count as an outside click and close it. A click closes it unless it is in the popover, on its button or in such an overlay.
  protected closeOnOutsideClick(event: Event) {
    const popover = this.filtersPopover();
    const target = event.target as Node | null;
    if (!popover?.overlayVisible || !target) {
      return;
    }
    const inside =
      popover.container?.contains(target) ||
      popover.target?.contains?.(target) ||
      (target instanceof Element && target.closest('.tod-home-filter-panel'));
    if (!inside) {
      popover.hide();
    }
  }

  // the filters that differ from their defaults (the badge of the "Filters" button counts them, its tooltip lists them)
  protected readonly changedFilters = computed(() => {
    const changes: string[] = [];

    // without an earlier visit "since previous visit" is not available, the history window stands in for it
    const defaultWindow: HomeWindowMode = this.hasPreviousVisit() ? 'lastVisit' : 'history';
    if (this.windowMode() !== defaultWindow) {
      changes.push(`Changes: ${homeWindowOptions.find((x) => x.value === this.windowMode())?.label ?? this.windowMode()}`);
    }

    const ids = this.severityIds();
    if (ids.length !== homeDefaultSeverityIds.length || !homeDefaultSeverityIds.every((id) => ids.includes(id))) {
      const names = this.severityOptions.filter((x) => ids.includes(x.value)).map((x) => x.label);
      changes.push(`Severities: ${names.join(', ')}`);
    }

    if (this.namespaces().length > 0) {
      changes.push(`Namespaces: ${this.namespaces().length}`);
    }

    if (!this.activeImagesOnly()) {
      changes.push('History: stale images included');
    }

    return changes;
  });

  // what the select shows: follows severityIds, but can be reset by itself when the last severity is unchecked
  protected readonly selectedSeverityIds = linkedSignal(() => this.severityIds());

  // the names when there are up to two, else the count (all of them: "All")
  // (the select calls its template with the selected option objects, or null while it has none)
  protected severitySummary(selected: { label: string }[] | null): string {
    if (!selected?.length) {
      return '';
    }
    if (selected.length >= this.severityOptions.length) {
      return 'All severities';
    }
    if (selected.length <= 2) {
      return selected.map((option) => option.label).join(', ');
    }
    return `${selected.length} severities`;
  }

  // at least one severity stays selected (none would mean "default" to the backend, which would be confusing); when the
  // last one is unchecked, the select is put back (a new array, so it is written to the control, without touching severityIds)
  protected onSeverityIdsChange(value: number[] | null) {
    if (value && value.length > 0) {
      this.severityIds.set([...value].sort((a, b) => a - b));
    } else {
      this.selectedSeverityIds.set([...this.severityIds()]);
    }
  }
}
