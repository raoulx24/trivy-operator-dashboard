import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, TemplateRef, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ScrollerModule } from 'primeng/scroller';

import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';

import { ScrollerResizeDirective } from '../../../shared/ui/scroller-resize/scroller-resize.directive';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWhereIsBase } from './home-where-is.base';

// the layout of the "where is" widgets that search something and show where each result runs (package, license); the
// logic is in HomeWhereIsBase (the widget), what a row, the head of the selected one and the rest of its detail look like
// is given as templates:
// - collapsed: the search box and the first rows
// - expanded, a fixed height: on the left, in a box, the search and all the rows (the width of the collapsed widget); on
//   the right the selected row: its head, the namespaces (with the extra under them, if there is one) and the images, half
//   and half, and below them the bottom (when the detail has one)
// - the expanded widget is at least expandedHeight tall and grows with its row of the page, what does not fit scrolls inside
@Component({
  selector: 'app-home-where-is-shell',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    HomeSearchBoxComponent,
    ButtonModule,
    NgTemplateOutlet,
    ScrollerModule,
    ScrollerResizeDirective,
    TrivyBoxComponent,
  ],
  templateUrl: './home-where-is-shell.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'contents' },
})
export class HomeWhereIsShellComponent {
  // the widget that owns this layout (its state, its selection, its filters)
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  widget = input.required<HomeWhereIsBase<any, any, any, any>>();

  heading = input.required<string>();
  help = input<string>('');
  searchPlaceholder = input.required<string>();
  // what to say when the search found nothing, and when there is nothing to show of the selected row
  emptyText = input.required<string>();
  selectText = input<string>('Select a row');
  searchText = input<string>('Search for something');

  // the height of a row of the list (a scroller needs the same for all)
  rowHeight = input.required<number>();
  // the height of the expanded widget body (it needs a fixed one: the lists and the table scroll in it)
  expandedHeight = input<string>('40rem');

  // a row of the list, in the collapsed widget (inList false) and in the list of the expanded one (inList true)
  rowBody = input.required<TemplateRef<unknown>>();
  // the box above the details of the selected row: the row itself, e.g. with a button
  detailHead = input.required<TemplateRef<unknown>>();
  // under the namespaces (fixed to the selected row: nothing selects or fades it)
  detailExtra = input<TemplateRef<unknown> | undefined>(undefined);
  // the box on the second half of the height (fixed to the selected row too)
  detailBottom = input<TemplateRef<unknown> | undefined>(undefined);

  protected key(item: unknown): string {
    return this.widget().itemKey(item);
  }
}
