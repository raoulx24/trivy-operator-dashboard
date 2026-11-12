import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { HomeService } from '@api/services/home.service';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// Package hotspots: packages (purl without version) by the number of vulnerability ids, with the fixable ones and the images
// it loads by itself (a change of a filter, or of refresh, reloads it): a few rows normally, and while it is expanded
// many more, asked from the server with a bigger limit (the few rows are shown until they arrive)
@Component({
  selector: 'app-home-package-hotspots',
  standalone: true,
  imports: [HomeWidgetFrameComponent],
  templateUrl: './home-package-hotspots.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomePackageHotspotsComponent extends HomeWidgetBase {
  // the server allows 500
  private static readonly collapsedLimit = 5;
  private static readonly expandedLimit = 100;

  private readonly homeService = inject(HomeService);

  hint = input<string>('');
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  // a new object on any change, so the resources reload; empty namespaces are not sent (the backend default applies)
  private readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
  }));

  private readonly collapsedData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomePackageHotspots({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomePackageHotspotsComponent.collapsedLimit,
      }),
  });

  // idle (and empty) while collapsed
  private readonly expandedData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomePackageHotspots({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomePackageHotspotsComponent.expandedLimit,
      }),
  });

  // set when the many rows of this expand were revealed; reset on collapse (so a reload by a filter does not scroll)
  private revealed = false;

  constructor() {
    super();

    // the many rows arrived and are in the signal: after they are rendered the page may scroll to show the whole widget
    effect(() => {
      if (!this.isExpanded()) {
        this.revealed = false;
      } else if (this.expandedData.hasValue() && !this.revealed) {
        this.revealed = true;
        this.revealAfterRender();
      }
    });
  }

  // what is shown: the many rows when they are there, else the few
  private readonly shown = computed(() =>
    this.isExpanded() && this.expandedData.hasValue() ? this.expandedData : this.collapsedData,
  );

  protected readonly items = computed(() => this.shown().value()?.items ?? []);

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
        `${dto.items.length} packages` + (this.isExpanded() && this.expandedData.isLoading() ? ' (loading more...)' : ''),
    };
  });
}
