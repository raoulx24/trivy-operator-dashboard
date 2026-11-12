import { Injectable, signal } from '@angular/core';

// which home widget is expanded: at most one on the page, so it is shared by all the groups (provided by the dashboard,
// not in root, so each visit of the page starts with none expanded)
// the ids are given by HomeWidgetGroupDirective.register and are unique on the page
@Injectable()
export class HomeWidgetExpansionService {
  readonly expandedId = signal<number | null>(null);
}
