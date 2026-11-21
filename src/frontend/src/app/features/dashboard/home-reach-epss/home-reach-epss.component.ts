import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { TagModule } from 'primeng/tag';

import { HomeReachEpssItemDto } from '@api/models/home-reach-epss-item-dto';
import { HomeService } from '@api/services/home.service';
import { SeverityUtils } from '@shared/utils/severity.utils';
import { ScatterSelectComponent } from '@shared/ui/scatter-select/scatter-select.component';
import {
  ScatterAxis,
  ScatterLegendItem,
  ScatterPoint,
  ScatterSelection,
} from '@shared/ui/scatter-select/scatter-select.types';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// Reach vs EPSS: one point per vulnerability id, the images that have it (x, log) against its EPSS percentile (y);
// the color is the severity, a diamond is a known exploited id. Drag on the plot to select a region: the top right is
// likely to be exploited and widespread. Expanded, the selected ids are listed next to the plot.
@Component({
  selector: 'app-home-reach-epss',
  standalone: true,
  imports: [HomeWidgetFrameComponent, ScatterSelectComponent, TagModule, DecimalPipe],
  templateUrl: './home-reach-epss.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeReachEpssComponent extends HomeWidgetBase {
  private readonly homeService = inject(HomeService);

  hint = input<string>('');
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  private readonly data = rxResource({
    params: () => ({
      severities: this.severities(),
      namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
      refresh: this.refresh(),
      localRefresh: this.localRefresh(),
    }),
    stream: ({ params }) =>
      this.homeService.getHomeReachEpss({ severities: params.severities, namespaces: params.namespaces }),
  });

  protected readonly xAxis: ScatterAxis = { label: 'images (log)', scale: 'log', min: 1 };
  protected readonly yAxis: ScatterAxis = {
    label: 'EPSS',
    min: 0,
    max: 1,
    format: (value) => `${Math.round(value * 100)}%`,
  };
  protected readonly legend: ScatterLegendItem[] = [
    { label: 'known exploited', shape: 'diamond', hint: 'In a known exploited list (KEV or EUVD)' },
    { label: 'not known exploited', shape: 'circle' },
  ];

  protected readonly points = computed<ScatterPoint<HomeReachEpssItemDto>[]>(() =>
    (this.data.value()?.items ?? []).map((item) => ({
      x: item.images,
      y: item.epssPercentile,
      color: SeverityUtils.getCssColor(item.severityId),
      shape: item.isKnownExploited ? 'diamond' : 'circle',
      label: item.vulnerabilityId,
      data: item,
    })),
  );

  // the ids inside the drawn region(s), the most likely to be exploited first
  protected readonly selected = signal<HomeReachEpssItemDto[]>([]);

  protected severityColor(severityId: number): string {
    return SeverityUtils.getCssColor(severityId);
  }

  protected onSelection(selection: ScatterSelection<HomeReachEpssItemDto>): void {
    this.selected.set(
      selection.points
        .map((p) => p.data!)
        .sort((a, b) => b.epssPercentile - a.epssPercentile || b.images - a.images),
    );
  }

  protected readonly state = computed<HomeWidgetState>(() => {
    if (this.data.error()) {
      return { error: (this.data.error() as Error)?.message ?? 'request failed' };
    }

    const dto = this.data.value();
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary: `${dto.items.length} vulnerability ids`,
    };
  });
}
