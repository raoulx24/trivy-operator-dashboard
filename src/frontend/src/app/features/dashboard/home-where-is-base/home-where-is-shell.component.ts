import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, TemplateRef, computed, effect, inject, input, signal, viewChild } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { Scroller, ScrollerModule } from 'primeng/scroller';

import { LayeredGraphComponent } from '@shared/ui/layered-graph/layered-graph.component';
import { LayeredGraphColumn, LayeredGraphEdge, LayeredGraphNode } from '@shared/ui/layered-graph/layered-graph.types';
import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableDialogComponent } from '@shared/ui/trivy-table-dialog/trivy-table-dialog.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { ScrollerResizeDirective } from '../../../shared/ui/scroller-resize/scroller-resize.directive';
import { HomeNodeFiltersComponent, NodeFiltersDetail } from '../home-pivot/home-node-filters.component';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeWhereIsBase } from './home-where-is.base';

// the layout of the "where is" widgets that search something and show where each result runs (package, license); the
// logic is in HomeWhereIsBase (the widget), what a row, the head of the selected one and the rest of its detail look like
// is given as templates:
// - collapsed: the search box and the first rows
// - expanded, a fixed height: on the left, in a box, the search and all the rows (the width of the collapsed widget); on
//   the right the selected row: its head, the namespaces (with the extra under them, if there is one) and the images, half
//   and half, and below them the bottom (when the detail has one)
// - with selectedBox the right side is the one of the graph widgets: the head, the Selected box and a graph (graphColumns,
//   graphData; multi select), and the controls have the selects of its nodes (filterDetail); allRowsColumns adds "Show all"
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
    LayeredGraphComponent,
    PillButtonComponent,
    TrivyTableDialogComponent,
    HomeNodeFiltersComponent,
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
  help = input<HomeWidgetHelp | undefined>(undefined);
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

  // under the list of the expanded widget, in a box of one fixed line (about the selected row)
  listFooter = input<TemplateRef<unknown> | undefined>(undefined);

  // the width of the list of the expanded widget, as a part of the width of the collapsed one (none: the old width)
  listRatio = input<number | null>(null);

  // the graph layout: the body of the Selected box (the nodes selected), the graph, the title of its box, and what the
  // selects of the nodes need
  selectedBox = input<TemplateRef<unknown> | undefined>(undefined);
  graphColumns = input<LayeredGraphColumn[]>([]);
  graphData = input<{ nodes: LayeredGraphNode[]; edges: LayeredGraphEdge[] }>({ nodes: [], edges: [] });
  graphTitle = input<string>('Where it runs');
  filterDetail = input<NodeFiltersDetail | undefined>(undefined);
  // the caption of the select of the first kind of nodes
  firstSelectText = input<string>('Select a package');
  // "Show all": the columns of its table (the rows are the ones of the list); without them there is no button
  allRowsColumns = input<TrivyTableColumn[] | undefined>(undefined);
  allRowsTitle = input<string>('All rows');
  allRowsKey = input<string>('Home Where Is All');
  protected readonly showAllDialog = signal(false);

  // the selects of the namespaces and the images (without a graph) are the selections of the lists, as node ids
  protected readonly listNodeIds = computed<string[]>(() => {
    const w = this.widget();
    const namespace = w.selectedNamespace();
    const image = w.selectedImage();
    const first = w.selectedFirst();

    return [first === null ? null : `p:${first}`, image === null ? null : `i:${image}`, namespace === null ? null : `n:${namespace}`].filter((x): x is string => x !== null);
  });

  protected setListNodes(ids: string[]): void {
    const w = this.widget();
    const index = (prefix: string): number | null => {
      const id = ids.find((x) => x.startsWith(prefix));
      return id === undefined ? null : Number(id.substring(2));
    };

    w.selectedFirst.set(index('p:'));
    w.selectedNamespace.set(index('n:'));
    w.selectedImage.set(index('i:'));
  }
  // the height of the box of the graph, which scrolls inside it
  protected readonly graphHost = viewChild<ElementRef<HTMLElement>>('graphHost');
  protected readonly graphHeight = signal<number | null>(null);

  // the list: on expand, and when its order changes (a sort), it is scrolled to the selected row
  private readonly listScroller = viewChild(Scroller);
  private scrolledToSelected = false;
  private listOrder = '';

  constructor() {
    effect(() => {
      const w = this.widget();
      const scroller = this.listScroller();
      const selected = w.selected();
      const items = w.items();
      if (!w.isExpanded()) {
        this.scrolledToSelected = false;
        return;
      }

      const order = JSON.stringify(w.sort());
      if (order !== this.listOrder) {
        this.listOrder = order;
        this.scrolledToSelected = false;
      }

      const index = selected === undefined ? -1 : items.findIndex((x) => w.itemKey(x) === w.itemKey(selected));
      if (!scroller || index < 0 || !w.hasExpandedRows() || this.scrolledToSelected) {
        return;
      }

      this.scrolledToSelected = true;
      // after the scroller measured its box
      setTimeout(() => scroller.scrollToIndex(index, 'auto'), 50);
    });

    // the room of the graph: the box it is in
    const destroyRef = inject(DestroyRef);
    const graphObserver = new ResizeObserver((entries) => {
      const height = Math.floor(entries[0]?.contentRect.height ?? 0);
      this.graphHeight.set(height > 0 ? height : null);
    });
    effect(() => {
      const element = this.graphHost()?.nativeElement;
      graphObserver.disconnect();
      if (element) {
        graphObserver.observe(element);
      } else {
        this.graphHeight.set(null);
      }
    });
    destroyRef.onDestroy(() => graphObserver.disconnect());
  }

  protected key(item: unknown): string {
    return this.widget().itemKey(item);
  }
}
