import { DestroyRef, ElementRef, afterNextRender, computed, effect, inject, input, signal, untracked, Directive } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeListSort, HomeListSortKey, nextSort, sortIconOf, sortLabelOf, sortRows, sortTitleOf } from '../home-list-sort/home-list-sort';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// what a "where is" list answer (the search) has, whatever it searches
export interface WhereIsListDto<TItem> {
  isAvailable: boolean;
  unavailableReason?: string | null;
  note?: string | null;
  totalMatches: number;
  items: TItem[];
}

// what a "where is" detailed answer has, whatever it searches
export interface WhereIsDetailDto<TDetail> {
  note?: string | null;
  item?: TDetail | null;
}

// the namespaces and the images of a detailed item, and which images run in which namespaces
export interface WhereIsDetailItem {
  namespaceNames: string[];
  imageNames: string[];
  // per image, the namespaces it runs in (indexes in namespaceNames)
  imageNamespaceIndexes: number[][];
}

// The logic of the "where is" widgets that search something and show where each result runs (package, license): the
// layout is in HomeWhereIsShellComponent, what is different (the calls, the key of a row, the rows) is in the widget
// - searches only on Enter (and on the refresh button, once something was searched); the namespaces are read when the
//   search is made, so a change of the filter does not re-run it
// - collapsed: the first rows (the server sorts them by reach, and is asked for just these); a click on a row selects it
//   and expands the widget (where it can expand)
// - expanded: all the rows (up to expandedLimit), and the selected one, all of its data (asked from the detailed
//   endpoint); the namespaces and the images of that detail filter each other
// TItem is a row of the list, TDetailItem the data of the selected row
@Directive()
export abstract class HomeWhereIsBase<
  TItem,
  TListDto extends WhereIsListDto<TItem>,
  TDetailItem extends WhereIsDetailItem,
  TDetailDto extends WhereIsDetailDto<TDetailItem>,
> extends HomeWidgetBase {
  private static readonly collapsedLimit = 5;
  // the server allows 500
  private static readonly expandedLimit = 500;

  // what the rows are (for the summary of the frame): "purls", "licenses"
  protected abstract readonly noun: string;

  protected abstract fetchList(query: string, namespaces: string[] | undefined, limit: number): Observable<TListDto>;
  protected abstract fetchDetail(item: TItem, namespaces: string[] | undefined): Observable<TDetailDto>;
  // what identifies a row (the selection is kept by it)
  abstract itemKey(item: TItem): string;
  // is this detail the one of this row (what arrives for a row that is not selected anymore is not shown)
  protected abstract isDetailOf(dto: TDetailDto, item: TItem): boolean;

  // the part of the detail below the namespaces and the images; not there when it has nothing (the lists get the whole height)
  hasDetailBottom(_detail: TDetailItem): boolean {
    return true;
  }

  hint = input<string>('');
  namespaces = input.required<string[]>();

  readonly query = signal('');
  // what was asked: a new object at every search (so the same query runs again); undefined before the first search
  private readonly request = signal<{ query: string; namespaces: string[] | undefined } | undefined>(undefined);
  // the selected row
  readonly selected = signal<TItem | undefined>(undefined);

  // the width of the widget as it is collapsed, which is the width of the box of the list (measured while it is collapsed)
  readonly collapsedWidth = signal<number | undefined>(undefined);

  private readonly scope = computed(() => {
    const request = this.request();
    return request ? { ...request, localRefresh: this.localRefresh() } : undefined;
  });

  private readonly collapsedData = rxResource({
    params: this.scope,
    stream: ({ params }) => this.fetchList(params.query, params.namespaces, HomeWhereIsBase.collapsedLimit),
  });

  // idle (and empty) while collapsed
  private readonly expandedData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) => this.fetchList(params.query, params.namespaces, HomeWhereIsBase.expandedLimit),
  });

  // what is shown: the many rows when they are there, else the few
  private readonly shown = computed(() =>
    this.isExpanded() && this.expandedData.hasValue() ? this.expandedData : this.collapsedData,
  );

  private readonly baseItems = computed<TItem[]>(() => this.shown().value()?.items ?? []);

  // the sort of the list (null: by reach, the order of the server). A widget that can sort says so (sortable) and gives the
  // values of a row (sortValues): its severity (a lower number is worse) and its name. In the collapsed widget it sorts the
  // few rows it has
  protected readonly sortable: boolean = false;
  // the rows have a severity to sort by (the sort pill of the severity is there)
  protected readonly sortBySeverity: boolean = true;
  protected sortValues(_item: TItem): { severity: number; name: string } | undefined {
    return undefined;
  }
  readonly sort = signal<HomeListSort | null>(null);
  readonly items = computed<TItem[]>(() =>
    sortRows(this.baseItems(), this.sort(), {
      moment: () => undefined,
      severity: (x) => this.sortValues(x)?.severity ?? 99,
      name: (x) => this.sortValues(x)?.name ?? '',
    }),
  );
  // for the shell
  isSortable(): boolean {
    return this.sortable;
  }
  hasSeveritySort(): boolean {
    return this.sortBySeverity;
  }
  toggleSort(key: HomeListSortKey): void {
    this.sort.update((current) => nextSort(current, key));
  }
  sortIcon(key: HomeListSortKey): string | undefined {
    return sortIconOf(this.sort(), key);
  }
  sortTitle(key: HomeListSortKey): string {
    return sortTitleOf(this.sort(), key);
  }
  readonly sortLabel = computed(() => sortLabelOf(this.sort()));

  // the many rows are here (the expanded widget has them)
  readonly hasExpandedRows = computed(() => this.expandedData.hasValue());

  // nothing found (or nothing searched yet): there is nothing to expand to; an expanded widget can always collapse
  readonly expandable = computed(() => this.canExpand() && (this.isExpanded() || this.items().length > 0));
  readonly totalMatches = computed(() => this.shown().value()?.totalMatches ?? 0);
  readonly more = computed(() => Math.max(0, this.totalMatches() - this.items().length));
  readonly note = computed(() => this.shown().value()?.note);
  readonly searched = computed(() => this.request() !== undefined);

  // the selected row, all of its data (only while expanded); the namespaces are those of the search
  private readonly detailData = rxResource({
    params: () => {
      const scope = this.scope();
      const selected = this.selected();
      return this.isExpanded() && scope && selected !== undefined
        ? { selected, namespaces: scope.namespaces, localRefresh: scope.localRefresh }
        : undefined;
    },
    stream: ({ params }) => this.fetchDetail(params.selected, params.namespaces),
  });

  // what is on screen: the last data that arrived for the selected row; it stays while the next one loads, so a change
  // of row does not tear the layout down
  private readonly loaded = signal<TDetailDto | undefined>(undefined);
  readonly detail = computed<TDetailItem | undefined>(() => this.loaded()?.item ?? undefined);
  readonly detailNote = computed(() => this.loaded()?.note);
  readonly isDetailLoading = computed(() => this.detailData.isLoading());
  readonly detailError = computed(() => (this.detailData.error() as Error | undefined)?.message);

  // ---- the filter: the namespaces and the images filter each other (what else the detail has is fixed to the row)
  // one selection per list (an index in the list). Each list is filtered only by the selection of the other, never by its
  // own: so what is faded is what cannot go with it, and it cannot be selected (that would be an empty combination).
  readonly selectedNamespace = signal<number | null>(null);
  readonly selectedImage = signal<number | null>(null);

  // the nodes of the graph of a widget that has one (multi select: ids like 'p:2', 'i:0', 'n:1')
  readonly selectedNodes = signal<string[]>([]);

  // a selection of a first kind that a widget can have besides the namespaces and the images (the package versions of a
  // license): an index in its list, and the images it has (imagesOfFirst); the namespaces and the images follow it
  readonly selectedFirst = signal<number | null>(null);
  protected imagesOfFirst(_index: number): readonly number[] | undefined {
    return undefined;
  }
  private readonly firstImages = computed<ReadonlySet<number> | null>(() => {
    const index = this.selectedFirst();

    return index === null ? null : new Set(this.imagesOfFirst(index) ?? []);
  });

  readonly isFiltered = computed(
    () =>
      this.selectedNamespace() !== null ||
      this.selectedImage() !== null ||
      this.selectedFirst() !== null ||
      this.selectedNodes().length > 0,
  );

  // the relation of the detail as sets: the namespaces of each image
  private readonly namespacesOfImage = computed(() => this.detail()?.imageNamespaceIndexes.map((x) => new Set(x)));

  readonly allowedNamespaces = computed<ReadonlySet<number>>(() => {
    const d = this.detail();
    const relation = this.namespacesOfImage();
    const image = this.selectedImage();
    const first = this.firstImages();
    const allowed = new Set<number>();

    for (let n = 0; d && relation && n < d.namespaceNames.length; n++) {
      const inFirst = first === null || [...first].some((i) => relation[i]?.has(n));
      if (inFirst && (image === null || relation[image]?.has(n))) {
        allowed.add(n);
      }
    }
    return allowed;
  });

  readonly allowedImages = computed<ReadonlySet<number>>(() => {
    const d = this.detail();
    const relation = this.namespacesOfImage();
    const namespace = this.selectedNamespace();
    const first = this.firstImages();
    const allowed = new Set<number>();

    for (let i = 0; d && relation && i < d.imageNames.length; i++) {
      if ((first === null || first.has(i)) && (namespace === null || relation[i]?.has(namespace))) {
        allowed.add(i);
      }
    }
    return allowed;
  });

  readonly state = computed<HomeWidgetState>(() => {
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
        `${dto.totalMatches} ${this.noun} (${dto.items.length} shown)` +
        (this.isExpanded() && this.expandedData.isLoading() ? ' (loading more...)' : ''),
    };
  });

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

    // expanded without a choice (the expand button, or a new search): the first row of the list
    effect(() => {
      const first = this.items()[0];
      if (this.isExpanded() && this.selected() === undefined && first !== undefined) {
        this.selected.set(first);
      }
    });

    // other data (another row, or the same one loaded again): the indexes of the selections are of the old lists
    effect(() => {
      this.detail();
      untracked(() => this.clearFilters());
    });

    // the data of the selected row arrived (or failed): it goes on screen
    effect(() => {
      const dto = this.detailData.value();
      const selected = this.selected();
      if (this.detailData.isLoading() || selected === undefined) {
        return;
      }

      if (this.detailData.error()) {
        this.loaded.set(undefined);
      } else if (dto && this.isDetailOf(dto, selected)) {
        this.loaded.set(dto);
      }
      // else the resource did not take the new row yet
    });
  }

  onSearch(): void {
    const query = this.query().trim();
    this.selected.set(undefined);
    this.loaded.set(undefined);

    if (!query) {
      // an empty box and Enter: the widget goes back to before the first search
      this.request.set(undefined);
      return;
    }

    const namespaces = this.namespaces();
    this.request.set({ query, namespaces: namespaces.length > 0 ? namespaces : undefined });
  }

  // the reset button of the search box (the search already reset the widget): an expanded widget goes back to collapsed
  onReset(): void {
    if (this.isExpanded()) {
      this.toggleExpand();
    }
  }

  // the expand button of the frame
  toggle(): void {
    this.toggleExpand();
  }

  onRefresh(): void {
    if (this.searched()) {
      this.refreshWidget();
    }
  }

  // a click on a row (of the collapsed widget, or of the list of the expanded one)
  select(item: TItem): void {
    this.selected.set(item);
    if (this.canExpand() && !this.isExpanded()) {
      this.toggleExpand();
    }
  }

  isSelected(item: TItem): boolean {
    const selected = this.selected();
    return selected !== undefined && this.itemKey(selected) === this.itemKey(item);
  }

  // "2 of 5" while a selection filters, else "5"
  countLabel(allowed: number, total: number): string {
    return this.isFiltered() ? `${allowed} of ${total}` : `${total}`;
  }

  // a click on a namespace or an image: selects it, or clears it when it is the selected one; a faded one is disabled
  toggleNamespace(index: number): void {
    if (this.allowedNamespaces().has(index)) {
      this.selectedNamespace.update((x) => (x === index ? null : index));
    }
  }

  toggleImage(index: number): void {
    if (this.allowedImages().has(index)) {
      this.selectedImage.update((x) => (x === index ? null : index));
    }
  }

  clearFilters(): void {
    this.selectedNamespace.set(null);
    this.selectedImage.set(null);
    this.selectedNodes.set([]);
    this.selectedFirst.set(null);
  }
}
