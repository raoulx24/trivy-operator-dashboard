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
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ScrollerModule } from 'primeng/scroller';
import { TagModule } from 'primeng/tag';

import { HomeFailingCheckDetailDto } from '@api/models/home-failing-check-detail-dto';
import { HomeFailingCheckDto } from '@api/models/home-failing-check-dto';
import { HomeService } from '@api/services/home.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { ScrollerResizeDirective } from '@shared/ui/scroller-resize/scroller-resize.directive';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

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

// Failing checks: the failing config audit / RBAC / infra checks, grouped by check id
// - it loads by itself (a change of a filter, or of refresh, reloads it); only the severities and the namespaces apply
// - collapsed: the first checks with their counts (the server sorts them by severity, then reach, and is asked for just
//   these); a click on a check selects it and expands the widget (where it can expand)
// - expanded: on the left all the checks (a list that scrolls), on the right one check at a time (asked from the
//   detailed endpoint): its data and a table with one row per failing resource
@Component({
  selector: 'app-home-failing-checks',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    ScrollerResizeDirective,
    NgTemplateOutlet,
    ScrollerModule,
    TagModule,
    TrivyBoxComponent,
    TrivyTableComponent,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
  ],
  templateUrl: './home-failing-checks.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeFailingChecksComponent extends HomeWidgetBase {
  private static readonly collapsedLimit = 5;
  // the server allows 500
  private static readonly expandedLimit = 500;

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help = `
    <p class="mb-2">The <strong>security assessment checks</strong> (config audit, RBAC, infra) that fail now, grouped by check id, in the selected <strong>severities</strong> and <strong>namespaces</strong>.</p>
    <p class="mb-2"><strong>Collapsed</strong>: the 5 most severe checks (then the ones failing in most resources), with the number of resources and namespaces. A click on a check expands the widget on it.</p>
    <p class="mb-2"><strong>Expanded</strong>: on the left every check (up to 500; a click selects one), on the right its description and remediation, and one row per failing <strong>resource</strong>: namespace, resource kind and name, and the report type. The columns can be filtered and sorted.</p>
    <p class="mb-0 tod-text-muted-color">Resources are counted once per report; the resource is the owner of the report (e.g. the Deployment) when there is one. Trivy reports a check per resource, not per container. With a namespaces filter the cluster scoped resources are out.</p>
  `;

  // the height of a row of the list (a scroller needs the same for all): the id, two lines of title, the counts
  protected readonly rowHeight = 110;
  // the height of the expanded widget body (it needs a fixed one: the list and the table scroll in it)
  protected readonly expandedHeight = '40rem';

  private readonly homeService = inject(HomeService);

  protected readonly resourceColumns: TrivyTableColumn[] = [
    { field: 'namespaceName', header: 'Namespace', isFilterable: true, isSortable: true, style: 'width: 22%', renderType: 'standard' },
    { field: 'resourceKind', header: 'Resource type', isFilterable: true, autoFilter: true, isSortable: true, style: 'width: 18%', renderType: 'standard' },
    { field: 'resourceName', header: 'Resource', isFilterable: true, isSortable: true, style: 'width: 38%', renderType: 'standard' },
    { field: 'reportKind', header: 'Report', isFilterable: true, autoFilter: true, isSortable: true, style: 'width: 22%', renderType: 'standard' },
  ];

  hint = input<string>('');
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  protected readonly selected = signal<CheckKey | undefined>(undefined);

  // the width of the widget as it is collapsed, which is the width of the box of the list (measured while it is collapsed)
  protected readonly collapsedWidth = signal<number | undefined>(undefined);

  // a new object on any change, so the resources reload; empty namespaces are not sent (the backend default applies)
  private readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  private readonly collapsedData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeFailingChecks({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeFailingChecksComponent.collapsedLimit,
      }),
  });

  // idle (and empty) while collapsed
  private readonly expandedData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeFailingChecks({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeFailingChecksComponent.expandedLimit,
      }),
  });

  // what is shown: the many rows when they are there, else the few
  private readonly shown = computed(() =>
    this.isExpanded() && this.expandedData.hasValue() ? this.expandedData : this.collapsedData,
  );

  protected readonly items = computed(() => this.shown().value()?.items ?? []);
  protected readonly totalChecks = computed(() => this.shown().value()?.totalChecks ?? 0);
  protected readonly more = computed(() => Math.max(0, this.totalChecks() - this.items().length));

  // the selected check, all of its data (only while expanded); the namespaces are those of the filter
  private readonly detailData = rxResource({
    params: () => {
      const key = this.selected();
      return this.isExpanded() && key ? { ...key, ...this.scope() } : undefined;
    },
    stream: ({ params }) =>
      this.homeService.getHomeFailingCheckDetailed({
        family: params.family,
        checkId: params.checkId,
        namespaces: params.namespaces,
      }),
  });

  // what is on screen: the last data that arrived for the selected check; it stays while the next one loads, so a change
  // of check does not tear the layout down
  private readonly loaded = signal<HomeFailingCheckDetailDto | undefined>(undefined);
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
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary:
        `${dto.items.length} of ${dto.totalChecks} checks` +
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

    // expanded without a choice (the expand button): the first check of the list
    effect(() => {
      const first = this.items()[0];
      if (this.isExpanded() && !this.selected() && first) {
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

  // a click on a check (of the collapsed widget, or of the list of the expanded one)
  protected select(item: HomeFailingCheckDto): void {
    this.selected.set({ family: item.family, checkId: item.checkId });
    if (this.canExpand() && !this.isExpanded()) {
      this.toggleExpand();
    }
  }

  protected isSelected(item: HomeFailingCheckDto): boolean {
    const key = this.selected();
    return key?.family === item.family && key.checkId === item.checkId;
  }
}
