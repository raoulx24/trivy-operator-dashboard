import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { TagModule } from 'primeng/tag';

import { HomeOldestImageDto } from '@api/models/home-oldest-image-dto';
import { HomeService } from '@api/services/home.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { SpanTimelineComponent } from '@shared/ui/span-timeline/span-timeline.component';
import { SpanTimelineRow, SpanTimelineTick } from '@shared/ui/span-timeline/span-timeline.types';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';

import { severityColor, severityName } from '../home-collapsed/home-collapsed';
import { matchesFilter } from '../home-search-box/home-list-filter';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

type SortBy = 'age' | 'severity' | 'ids' | 'reach';

// an image of the list, with what is computed of it
interface AgedImage {
  item: HomeOldestImageDto;
  // the digest: unique, the id of the timeline row
  key: string;
  name: string;
  // whole days since the history first saw the digest
  days: number;
}

// the age that makes an image "old": the dashed line of the timeline, and the count of the headline
const OLD_DAYS = 90;
// the room of a character of a row label of the timeline (px, a rough size), to cut a label that does not fit
const LABEL_CHAR_WIDTH = 7;
const LABEL_PADDING = 16;

// Oldest running images: the running digests by the moment the history first saw them (oldest first), with their base OS
// and what they carry. The vulnerability counts follow the severities filter, the reach follows the namespaces filter; a
// change of a filter, or of refresh, reloads it
// - one request, the images (up to the server limit) as the server sorts them (the oldest first); the rest is computed here
// - collapsed: the oldest age and the images older than OLD_DAYS, and the first images as bars on a timeline from the
//   start of the history to today, colored by their highest severity (a click on one expands the widget on it)
// - expanded: all the images on the timeline (sorted by age, severity, ids or reach), the selected one and its namespaces
@Component({
  selector: 'app-home-oldest-images',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    HomeSearchBoxComponent,
    NgTemplateOutlet,
    PillButtonComponent,
    SpanTimelineComponent,
    TrivyBoxComponent,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
    TagModule,
  ],
  templateUrl: './home-oldest-images.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must wrap, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeOldestImagesComponent extends HomeWidgetBase {
  // the server allows 500; the collapsed widget needs all of them too (the count of the old ones)
  private static readonly fetchLimit = 500;
  private static readonly collapsedLimit = 5;

  protected readonly help: HomeWidgetHelp = {
    question: `Which <strong>running images are the oldest</strong>? An image that runs for a long time is usually one nobody rebuilt.`,
    oddities: [
      `The age counts from when the history first saw the <strong>digest</strong>, not from the build of the image, so it is never longer than the history.`,
      `The bar color is the highest severity of the image (grey: none in the selected severities). The dashed line is at ${OLD_DAYS} days.`,
      `The counts are distinct vulnerability ids of the selected severities, over the whole image.`,
      `The severities filter changes the counts; the namespaces filter changes which images count as running.`,
    ],
    expanded: [
      `Every image (up to 500), sorted by age, severity, vulnerability ids or reach (workloads).`,
      `On the right the selected image and its namespaces.`,
    ],
    tips: [
      `Collapsed: click one of the 5 oldest images to expand on it.`,
    ],
  };

  protected readonly oldDays = OLD_DAYS;

  // the expanded widget body is at least this tall, and grows with its row of the page; the timeline takes the rest of
  // the height (timelineHeight, measured) and its rows scroll in it
  protected readonly expandedMinHeight = 'min(30rem, 70vh)';
  protected readonly expandedMaxHeight = '70vh';
  private readonly timelineHost = viewChild<ElementRef<HTMLElement>>('timelineHost');
  protected readonly timelineHeight = signal<number | null>(null);

  private readonly homeService = inject(HomeService);

  hint = input<string>('');
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

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
        limit: HomeOldestImagesComponent.fetchLimit,
      }),
  });

  private readonly todayMs = Date.now();

  private readonly allImages = computed<AgedImage[]>(() =>
    (this.data.value()?.items ?? []).map((item) => ({
      item,
      key: item.digest,
      name: `${item.repository}:${item.tag}`,
      days: this.daysSince(item.firstSeenAt),
    })),
  );
  protected readonly imageCount = computed(() => this.allImages().length);
  // the server sends at most fetchLimit images: then there may be more
  protected readonly partialLabel = computed(() =>
    this.imageCount() >= HomeOldestImagesComponent.fetchLimit ? `The first ${this.imageCount()} images` : '',
  );

  // ---- the timeline: from the start of the history (or the oldest image, without a start) to today
  private readonly historyDays = computed(() => {
    const start = this.data.value()?.historyStartedAt;
    const oldest = Math.max(1, ...this.allImages().map((x) => x.days));
    return start ? Math.max(oldest, this.daysSince(start)) : oldest;
  });
  protected readonly timelineMax = computed(() => this.historyDays());
  protected readonly referenceAt = computed(() => (this.historyDays() > OLD_DAYS ? OLD_DAYS : null));
  protected readonly timelineTicks = computed<SpanTimelineTick[]>(() => [
    { value: OLD_DAYS },
    { value: this.historyDays(), label: 'history start' },
  ]);

  // ---- the collapsed widget: the oldest images
  private readonly byAge = computed(() => this.sortImages(this.allImages(), 'age'));
  protected readonly oldestDays = computed(() => this.byAge()[0]?.days ?? null);
  protected readonly oldCount = computed(() => this.allImages().filter((x) => x.days > OLD_DAYS).length);
  protected readonly collapsedRows = computed(() =>
    this.toRows(this.byAge().slice(0, HomeOldestImagesComponent.collapsedLimit), 300),
  );

  // ---- the expanded widget: the filter box, the sort, the selection
  protected readonly filterText = signal('');
  protected readonly sortBy = signal<SortBy>('age');
  protected readonly selectedKey = signal<string | null>(null);

  private readonly images = computed(() =>
    this.allImages().filter((x) =>
      matchesFilter(
        this.filterText(),
        x.name,
        x.item.registry,
        x.item.osFamily,
        x.item.osName,
        ...x.item.namespaceNames,
      ),
    ),
  );
  protected readonly noMatch = computed(
    () => this.filterText().trim().length > 0 && this.images().length === 0 && this.allImages().length > 0,
  );
  protected readonly shown = computed(() => this.sortImages(this.images(), this.sortBy()));

  protected readonly labelWidth = 450;
  protected readonly timelineRows = computed(() => this.toRows(this.shown(), this.labelWidth));

  protected readonly selected = computed(() => this.allImages().find((x) => x.key === this.selectedKey()));
  protected readonly selectedNamespaces = computed(() => [...(this.selected()?.item.namespaceNames ?? [])].sort());

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

  constructor() {
    super();

    // the room of the timeline: its box without the padding (the box appears with the expanded widget)
    const destroyRef = inject(DestroyRef);
    const observer = new ResizeObserver((entries) => {
      const height = Math.floor(entries[0]?.contentRect.height ?? 0);
      this.timelineHeight.set(height > 0 ? height : null);
    });
    effect(() => {
      const element = this.timelineHost()?.nativeElement;
      observer.disconnect();
      if (element) {
        observer.observe(element);
      } else {
        this.timelineHeight.set(null);
      }
    });
    destroyRef.onDestroy(() => observer.disconnect());

    // expanded with nothing selected (the expand button): the first image
    effect(() => {
      if (this.isExpanded()) {
        untracked(() => {
          if (!this.selectedKey()) {
            this.selectedKey.set(this.shown()[0]?.key ?? null);
          }
        });
      }
    });
  }

  // a click on a bar of the collapsed widget: expands it with that image selected (a click on the selected one too)
  protected openImage(key: string | null): void {
    if (key) {
      this.selectedKey.set(key);
    }
    if (this.canExpand() && !this.isExpanded()) {
      this.toggleExpand();
    }
  }

  protected daysLabel(days: number | null): string {
    return days === null ? '-' : `${days}d`;
  }

  protected shortDigest(digest: string): string {
    const hash = digest.includes(':') ? digest.substring(digest.indexOf(':') + 1) : digest;
    return hash.substring(0, 12);
  }

  private toRows(images: AgedImage[], labelWidth: number): SpanTimelineRow[] {
    const maxChars = Math.floor((labelWidth - LABEL_PADDING) / LABEL_CHAR_WIDTH);
    return images.map((x) => {
      const severityId = x.item.highestSeverityId;
      const hasSeverity = severityId !== undefined && severityId !== null;
      return {
        id: x.key,
        label: this.cut(x.name, maxChars),
        start: x.days,
        color: hasSeverity ? severityColor(severityId) : 'var(--tod-oldest-images-no-ids)',
        valueLabel: `${x.days}d`,
        details: [
          x.name,
          `first seen ${x.item.firstSeenAt.substring(0, 10)}`,
          hasSeverity ? `highest severity: ${severityName(severityId)}` : 'no vulnerability id',
          `${x.item.vulnerabilityIds} ids · ${x.item.fixableVulnerabilityIds} fixable · ${x.item.knownExploitedVulnerabilityIds} exploited`,
          `${x.item.workloads} ${x.item.workloads === 1 ? 'workload' : 'workloads'} · ${x.item.namespaces} ns`,
          ...(x.item.osFamily
            ? [`${x.item.osFamily} ${x.item.osName ?? ''}${x.item.isOsEndOfLife ? ' (EOL)' : ''}`.trim()]
            : []),
        ],
      };
    });
  }

  // age: the oldest first; severity: the highest first (an image without ids last); ids: the most first; reach: the most
  // workloads first. Then by age
  private sortImages(images: AgedImage[], by: SortBy): AgedImage[] {
    const age = (a: AgedImage, b: AgedImage) => b.days - a.days;
    const key = (x: AgedImage): number => {
      switch (by) {
        case 'severity':
          return -(x.item.highestSeverityId ?? Number.MAX_SAFE_INTEGER);
        case 'ids':
          return x.item.vulnerabilityIds;
        case 'reach':
          return x.item.workloads;
        default:
          return 0;
      }
    };
    return [...images].sort((a, b) => key(b) - key(a) || age(a, b));
  }

  private daysSince(value: string): number {
    return Math.max(0, Math.floor((this.todayMs - new Date(value).getTime()) / 86_400_000));
  }

  // a name that does not fit keeps its end (the tag)
  private cut(text: string, maxChars: number): string {
    return text.length <= maxChars ? text : `…${text.slice(text.length - maxChars + 1)}`;
  }
}
