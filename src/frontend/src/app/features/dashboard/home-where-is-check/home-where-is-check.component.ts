import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Scroller, ScrollerModule } from 'primeng/scroller';
import { TagModule } from 'primeng/tag';

import { HomeWhereIsCheckDetailDto } from '@api/models/home-where-is-check-detail-dto';
import { HomeWhereIsCheckItemDto } from '@api/models/home-where-is-check-item-dto';
import { HomeService } from '@api/services/home.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { TrivyTableDialogComponent } from '@shared/ui/trivy-table-dialog/trivy-table-dialog.component';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { HomeListSort, HomeListSortKey, nextSort, sortIconOf, sortLabelOf, sortRows, sortTitleOf } from '../home-list-sort/home-list-sort';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';
import { ScrollerResizeDirective } from '../../../shared/ui/scroller-resize/scroller-resize.directive';

// a failing resource of the selected check: namespace and resource denormalized (one row each)
interface ResourceRow {
  namespaceName: string;
  resourceKind: string;
  resourceName: string;
  reportKind: string;
}

interface CheckKey {
  family: string;
  checkId: string;
}

// Where is... check: a security assessment check (config audit, RBAC, infra) by id or title, and where it fails
// - searches only on Enter (and on the refresh button, once something was searched); the namespaces are read when the
//   search is made, so a change of the filter does not re-run it
// - collapsed: the first checks with their counts only (the server sorts them by severity, then reach, and is asked for
//   just these); a click on a check selects it and expands the widget (where it can expand)
// - expanded: on the left, in a box, the search and all the checks it found (a list that scrolls); on the right one check
//   at a time (asked from the detailed endpoint): its data and a table with one row per failing resource
@Component({
  selector: 'app-home-where-is-check',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    HomeSearchBoxComponent,
    ScrollerResizeDirective,
    NgTemplateOutlet,
    ScrollerModule,
    TagModule,
    TrivyBoxComponent,
    TrivyTableComponent,
    PillButtonComponent,
    TrivyTableDialogComponent,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
  ],
  templateUrl: './home-where-is-check.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeWhereIsCheckComponent extends HomeWidgetBase {
  private static readonly collapsedLimit = 5;
  // the server allows 500
  private static readonly expandedLimit = 500;

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help: HomeWidgetHelp = {
    question: `Where does a <strong>security check</strong> (config audit, RBAC, infra) fail in my cluster? Type an id or a part of the title.`,
    oddities: [
      `At least 3 characters, then <strong>Enter</strong>. <em>KSV014</em>, <em>KSV-0014</em> and <em>AVD-KSV-0014</em> are the same check; case does not matter.`,
      `Only the <strong>namespaces filter</strong> applies (cluster scoped resources are then out). A change of it does not search again; the refresh button does.`,
      `The resource is the owner of the report (e.g. the Deployment). Trivy reports per resource, not per container.`,
    ],
    expanded: [
      `Every check found (up to 500) on the left; on the right its description, remediation and one row per failing resource (namespace, kind, name, report type); the columns can be filtered and sorted.`,
    ],
    tips: [
      `Search by a word of the title, e.g. <em>root</em>.`,
      `Collapsed: click one of the 5 most severe checks to expand on it.`,
    ],
  };

  // the height of a row of the list (a scroller needs the same for all): the id, two lines of title, the counts
  protected readonly rowHeight = 110;
  // the height of the expanded widget body (it needs a fixed one: the list and the table scroll in it)
  protected readonly expandedHeight = '56rem';

  private readonly homeService = inject(HomeService);

  protected readonly resourceColumns: TrivyTableColumn[] = [
    { field: 'namespaceName', header: 'Namespace', isFilterable: true,isSortable: true, style: 'width: 22%', renderType: 'standard' },
    { field: 'resourceKind', header: 'Resource type', isFilterable: true, autoFilter: true, isSortable: true, style: 'width: 18%', renderType: 'standard' },
    { field: 'resourceName', header: 'Resource', isFilterable: true, isSortable: true, style: 'width: 38%', renderType: 'standard' },
    { field: 'reportKind', header: 'Report', isFilterable: true, autoFilter: true, isSortable: true, style: 'width: 22%', renderType: 'standard' },
  ];

  hint = input<string>('');
  namespaces = input.required<string[]>();

  protected readonly query = signal('');
  // what was asked: a new object at every search (so the same query runs again); undefined before the first search
  private readonly request = signal<{ query: string; namespaces: string[] | undefined } | undefined>(undefined);
  protected readonly selected = signal<CheckKey | undefined>(undefined);

  // the width of the widget as it is collapsed, which is the width of the box of the list (measured while it is collapsed)
  protected readonly collapsedWidth = signal<number | undefined>(undefined);

  private readonly scope = computed(() => {
    const request = this.request();
    return request ? { ...request, localRefresh: this.localRefresh() } : undefined;
  });

  private readonly collapsedData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeWhereIsCheck({
        query: params.query,
        namespaces: params.namespaces,
        limit: HomeWhereIsCheckComponent.collapsedLimit,
      }),
  });

  // idle (and empty) while collapsed
  private readonly expandedData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeWhereIsCheck({
        query: params.query,
        namespaces: params.namespaces,
        limit: HomeWhereIsCheckComponent.expandedLimit,
      }),
  });

  // what is shown: the many rows when they are there, else the few
  private readonly shown = computed(() =>
    this.isExpanded() && this.expandedData.hasValue() ? this.expandedData : this.collapsedData,
  );

  private readonly baseItems = computed(() => this.shown().value()?.items ?? []);

  // the sort of the list (null: the order of the server, by severity then reach); in the collapsed widget it sorts the few
  // rows it has
  protected readonly sort = signal<HomeListSort | null>(null);
  protected readonly items = computed(() =>
    sortRows(this.baseItems(), this.sort(), {
      moment: () => undefined,
      severity: (x) => x.severityId,
      name: (x) => x.checkId,
    }),
  );

  // nothing found (or nothing searched yet): there is nothing to expand to; an expanded widget can always collapse
  protected readonly expandable = computed(() => this.canExpand() && (this.isExpanded() || this.items().length > 0));

  protected toggleSort(key: HomeListSortKey): void {
    this.sort.update((current) => nextSort(current, key));
  }

  protected sortIcon(key: HomeListSortKey): string | undefined {
    return sortIconOf(this.sort(), key);
  }

  protected sortTitle(key: HomeListSortKey): string {
    return sortTitleOf(this.sort(), key);
  }

  protected readonly sortLabel = computed(() => sortLabelOf(this.sort()));

  // "Show all": every check found (the rows of the list, up to 500), flat, in a table
  protected readonly showAllDialog = signal(false);
  protected readonly allRowsColumns: TrivyTableColumn[] = [
    { field: 'checkId', header: 'Check', isFilterable: true, isSortable: true, style: 'width: 10rem', renderType: 'standard' },
    { field: 'severityId', header: 'Sev', isFilterable: true, isSortable: true, multiSelectType: 'severities', style: 'width: 7rem', renderType: 'severityBadge' },
    { field: 'family', header: 'Family', isFilterable: true, autoFilter: true, isSortable: true, style: 'width: 10rem', renderType: 'standard' },
    { field: 'title', header: 'Title', isFilterable: true, isSortable: true, style: 'width: 40%', renderType: 'standard' },
    { field: 'resources', header: 'Resources', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'standard' },
    { field: 'namespaces', header: 'Namespaces', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'standard' },
  ];

  // the list: on expand, and when its order changes (a sort), it is scrolled to the selected check
  private readonly listScroller = viewChild(Scroller);
  private scrolledToSelected = false;
  private listOrder = '';
  protected readonly totalMatches = computed(() => this.shown().value()?.totalMatches ?? 0);
  protected readonly more = computed(() => Math.max(0, this.totalMatches() - this.items().length));
  protected readonly searched = computed(() => this.request() !== undefined);

  // the selected check, all of its data (only while expanded); the namespaces are those of the search
  private readonly detailData = rxResource({
    params: () => {
      const scope = this.scope();
      const key = this.selected();
      return this.isExpanded() && scope && key
        ? { ...key, namespaces: scope.namespaces, localRefresh: scope.localRefresh }
        : undefined;
    },
    stream: ({ params }) =>
      this.homeService.getHomeWhereIsCheckDetailed({
        family: params.family,
        checkId: params.checkId,
        namespaces: params.namespaces,
      }),
  });

  // what is on screen: the last data that arrived for the selected check; it stays while the next one loads, so a change
  // of check does not tear the layout down
  private readonly loaded = signal<HomeWhereIsCheckDetailDto | undefined>(undefined);
  protected readonly detail = computed(() => this.loaded()?.item ?? undefined);
  protected readonly detailNote = computed(() => this.loaded()?.note);
  protected readonly displayedId = computed(() => this.detail()?.checkId ?? this.selected()?.checkId);
  protected readonly isDetailLoading = computed(() => this.detailData.isLoading());
  protected readonly detailError = computed(() => (this.detailData.error() as Error | undefined)?.message);

  // the table: namespaced resources by namespace; a cluster scoped one has no namespace
  protected readonly resourceRows = computed<ResourceRow[]>(() =>
    (this.detail()?.rows ?? []).map((x) => ({
      namespaceName: x.namespaceName ?? '(cluster scoped)',
      resourceKind: x.resourceKind,
      resourceName: x.resourceName,
      reportKind: x.reportKind,
    })),
  );

  protected readonly state = computed<HomeWidgetState>(() => {
    const resource = this.shown();
    if (resource.error()) {
      return { error: (resource.error() as Error)?.message ?? 'request failed' };
    }

    const dto = resource.value();
    if (!this.searched()) {
      return { idleText: 'Type and press Enter to search' };
    }
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary:
        `${dto.totalMatches} checks (${dto.items.length} shown)` +
        (this.isExpanded() && this.expandedData.isLoading() ? ' (loading more...)' : ''),
    };
  });

  // the first check is selected on its own once per expand and per search: a deselect (a click on the selected row) leaves the
  // empty state
  private autoSelect = true;

  constructor() {
    super();

    // the width of the collapsed widget (what is measured while it is expanded is not kept)
    const host = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
    const destroyRef = inject(DestroyRef);
    afterNextRender(() => {
      const observer = new ResizeObserver(() => {
        if (!this.isExpanded() && host.offsetWidth > 0) {
          this.collapsedWidth.set(host.offsetWidth);
        }
      });
      observer.observe(host);
      destroyRef.onDestroy(() => observer.disconnect());
    });

    // expanded with a check selected: the list scrolls to it, once (when the rows of the expanded widget are there and the
    // list is drawn); a collapse or a new order re-arms it
    effect(() => {
      const scroller = this.listScroller();
      const key = this.selected();
      const items = this.items();
      if (!this.isExpanded()) {
        this.scrolledToSelected = false;
        return;
      }

      const order = JSON.stringify(this.sort());
      if (order !== this.listOrder) {
        this.listOrder = order;
        this.scrolledToSelected = false;
      }

      const index = key ? items.findIndex((x) => x.family === key.family && x.checkId === key.checkId) : -1;
      if (!scroller || index < 0 || !this.expandedData.hasValue() || this.scrolledToSelected) {
        return;
      }

      this.scrolledToSelected = true;
      // after the scroller measured its box
      setTimeout(() => scroller.scrollToIndex(index, 'auto'), 50);
    });

    // expanded without a choice (the expand button, or a new search): the first check of the list
    effect(() => {
      const first = this.items()[0];
      const key = this.selected();
      if (!this.isExpanded()) {
        this.autoSelect = true;
        return;
      }
      if (key) {
        this.autoSelect = false;
      }
      if (this.autoSelect && !key && first) {
        this.autoSelect = false;
        this.selected.set({ family: first.family, checkId: first.checkId });
      }
    });

    // the data of the selected check arrived (or failed): it goes on screen
    effect(() => {
      const dto = this.detailData.value();
      const key = this.selected();
      if (this.detailData.isLoading() || !key) {
        return;
      }

      if (this.detailData.error()) {
        this.loaded.set(undefined);
      } else if (dto?.checkId === key.checkId && dto.family === key.family) {
        this.loaded.set(dto);
      }
      // else the resource did not take the new check yet
    });
  }

  protected onSearch(): void {
    const query = this.query().trim();
    if (!query) {
      // an empty box and Enter: the widget goes back to before the first search
      this.request.set(undefined);
      this.selected.set(undefined);
      this.loaded.set(undefined);
      return;
    }

    const namespaces = this.namespaces();
    // a new search selects its first check again
    this.autoSelect = true;
    this.selected.set(undefined);
    this.loaded.set(undefined);
    this.request.set({ query, namespaces: namespaces.length > 0 ? namespaces : undefined });
  }

  // the reset button of the search box (the search already reset the widget): an expanded widget goes back to collapsed
  protected onReset(): void {
    if (this.isExpanded()) {
      this.toggleExpand();
    }
  }

  protected onRefresh(): void {
    if (this.searched()) {
      this.refreshWidget();
    }
  }

  // a click on a check (of the collapsed widget, or of the list of the expanded one); on the selected row of the expanded
  // list it deselects (the empty state)
  protected select(item: HomeWhereIsCheckItemDto): void {
    if (this.isExpanded() && this.isSelected(item)) {
      this.selected.set(undefined);
      this.loaded.set(undefined);
      return;
    }

    this.selected.set({ family: item.family, checkId: item.checkId });
    if (this.canExpand() && !this.isExpanded()) {
      this.toggleExpand();
    }
  }

  protected isSelected(item: HomeWhereIsCheckItemDto): boolean {
    const key = this.selected();
    return key?.family === item.family && key.checkId === item.checkId;
  }
}
