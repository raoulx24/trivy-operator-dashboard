import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { TagModule } from 'primeng/tag';

import { HomeService } from '@api/services/home.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// Oldest running images: the running digests by the moment the history first saw them (oldest first), with their base OS
// and what they carry. Collapsed only for now (no expand). It loads by itself (a change of a filter, or of refresh,
// reloads it); the vulnerability counts follow the severities filter, the reach follows the namespaces filter
@Component({
  selector: 'app-home-oldest-images',
  standalone: true,
  imports: [HomeWidgetFrameComponent, SeverityCssStyleByIdPipe, SeverityNameByIdPipe, TagModule],
  templateUrl: './home-oldest-images.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must wrap, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeOldestImagesComponent extends HomeWidgetBase {
  // the server allows 500
  private static readonly limit = 5;

  private readonly homeService = inject(HomeService);

  hint = input<string>('');
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  protected readonly help = `
    <p class="mb-2">The <strong>running images</strong> that the history saw first, <strong>oldest first</strong>: a long time running usually means an image nobody rebuilt.</p>
    <p class="mb-2">The age is counted from the moment the history first saw the <strong>digest</strong> (not from the build of the image), so it is never longer than the history.</p>
    <p class="mb-2">The vulnerability counts are <strong>distinct vulnerability ids</strong> of the selected severities, over the whole image: all of them, the ones with a fix for at least one package, and the known exploited ones. The severity tag is the highest of them.</p>
    <p class="mb-0 tod-text-muted-color">The <strong>severities filter</strong> changes the counts; the <strong>namespaces filter</strong> changes which images count as running and their workloads and namespaces.</p>
  `;

  private readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  private readonly data = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeOldestImages({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeOldestImagesComponent.limit,
      }),
  });

  protected readonly items = computed(() => this.data.value()?.items ?? []);

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
      summary: `${dto.items.length} images · history since ${dto.historyStartedAt?.substring(0, 10) ?? 'n/a'}`,
    };
  });

  // whole days since the digest was first seen
  protected ageDays(firstSeenAt: string): number {
    return Math.max(0, Math.floor((Date.now() - new Date(firstSeenAt).getTime()) / 86_400_000));
  }

  protected imageName(item: { repository: string; tag: string }): string {
    return `${item.repository}:${item.tag}`;
  }
}
