import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { TagModule } from 'primeng/tag';
import { forkJoin, map, of, switchMap } from 'rxjs';

import { HomePackageHotspotDto } from '@api/models/home-package-hotspot-dto';
import { HomeWhereIsPackageItemDto } from '@api/models/home-where-is-package-item-dto';
import { HomeWhereIsPackageVulnerabilityDto } from '@api/models/home-where-is-package-vulnerability-dto';
import { HomeService } from '@api/services/home.service';
import { MeterGroupComponent } from '@shared/ui/meter-group/meter-group.component';
import { MeterGroupSegment } from '@shared/ui/meter-group/meter-group.types';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// the versions of a package whose vulnerability ids are read (the most widespread first)
const MAX_VERSIONS_WITH_IDS = 8;

interface HotspotRow {
  item: HomePackageHotspotDto;
  segments: MeterGroupSegment[];
}

interface VersionRow {
  version: string;
  images: number;
  segments: MeterGroupSegment[];
}

interface IdRow {
  vulnerabilityId: string;
  severityId: number;
  isKnownExploited: boolean;
  // the fixed versions (empty: no fix yet)
  fixedVersions: string[];
  images: number;
}

interface PackageDetail {
  versions: VersionRow[];
  // the ids of the (up to MAX_VERSIONS_WITH_IDS) most widespread versions
  ids: IdRow[];
  // the versions that were not read for ids
  versionsWithoutIds: number;
}

// Package hotspots: packages (purl without version) by the number of vulnerability ids, with the fixable ones and the images
// it loads by itself (a change of a filter, or of refresh, reloads it): a few rows normally, and while it is expanded
// many more, asked from the server with a bigger limit (the few rows are shown until they arrive)
// - a click on a row expands the widget with that package selected; expanded, the selected package shows its versions
//   (version sprawl: images per installed version) and its vulnerability ids, from the where-is endpoints
@Component({
  selector: 'app-home-package-hotspots',
  standalone: true,
  imports: [HomeWidgetFrameComponent, MeterGroupComponent, TagModule, NgTemplateOutlet],
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

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help = `
    <p class="mb-2">The <strong>packages</strong> (without their version) with the most <strong>vulnerability ids</strong> among the active vulnerabilities of the running images, in the <strong>severities</strong> and <strong>namespaces</strong> of the filter bar.</p>
    <p class="mb-2">The <strong>bar</strong> is the number of ids of the package, on one scale for all: <strong>teal</strong> are the ids that have a fix, <strong>grey</strong> the ids without one yet. At the right: the <strong>images</strong> (and, expanded, the <strong>namespaces</strong>) that have the package.</p>
    <p class="mb-2"><strong>Collapsed</strong>: the 5 worst packages. A click on one expands the widget on it.</p>
    <p class="mb-2"><strong>Expanded</strong>: up to 100 packages on the left (a click selects one). On the right the selected package: its <strong>versions</strong> and the images that run each (the version sprawl: one version fewer to run is one fewer to patch), and its <strong>vulnerability ids</strong> with the fix.</p>
    <p class="mb-0 tod-text-muted-color">The ids of a package are read from its <strong>${MAX_VERSIONS_WITH_IDS} most widespread versions</strong> (the images of an id are summed over them).</p>
  `;

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
    localRefresh: this.localRefresh(),
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

  // the selected package (its key); set by a click on a row (of the collapsed widget too), by default the first
  protected readonly selectedKey = signal<string | null>(null);

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

  // the most ids of a package: the meters share this scale, so a longer one is a worse package
  protected readonly maxIds = computed(() => Math.max(1, ...this.items().map((x) => x.vulnerabilityIds)));

  protected readonly rows = computed<HotspotRow[]>(() =>
    this.items().map((item) => ({
      item,
      segments: [
        { label: 'fixable ids', value: item.fixableVulnerabilityIds, color: 'var(--tod-package-hotspots-fixable)' },
        {
          label: 'no fix yet',
          value: Math.max(0, item.vulnerabilityIds - item.fixableVulnerabilityIds),
          color: 'var(--tod-package-hotspots-no-fix)',
        },
      ],
    })),
  );

  // the selected package: the one clicked if it is in the list, else the first
  protected readonly selected = computed<HomePackageHotspotDto | undefined>(
    () => this.items().find((x) => x.packageKey === this.selectedKey()) ?? this.items()[0],
  );

  // a click on a row: the widget is expanded (when it is not) with that package selected
  protected select(item: HomePackageHotspotDto): void {
    this.selectedKey.set(item.packageKey);
    this.expand();
  }

  // the versions and ids of the selected package; idle while collapsed
  private readonly detailData = rxResource({
    params: () => {
      const selected = this.selected();
      return this.isExpanded() && selected
        ? { packageKey: selected.packageKey, namespaces: this.scope().namespaces, refresh: this.scope().refresh }
        : undefined;
    },
    stream: ({ params }) =>
      this.homeService
        .getHomeWhereIsPackage({ query: params.packageKey, namespaces: params.namespaces, limit: 100 })
        .pipe(
          switchMap((list) => {
            const versions = list.items
              .filter((x) => x.packageKey === params.packageKey)
              .sort((a, b) => b.images - a.images);
            const read = versions.slice(0, MAX_VERSIONS_WITH_IDS);
            if (read.length === 0) {
              return of(this.toDetail(versions, [], 0));
            }

            return forkJoin(
              read.map((v) =>
                this.homeService.getHomeWhereIsPackageDetailed({
                  packageKey: v.packageKey,
                  version: v.version,
                  namespaces: params.namespaces,
                }),
              ),
            ).pipe(
              map((details) =>
                this.toDetail(
                  versions,
                  details.flatMap((d) => d.item?.vulnerabilities ?? []),
                  versions.length - read.length,
                ),
              ),
            );
          }),
        ),
  });

  protected readonly detail = computed<PackageDetail | undefined>(() =>
    this.detailData.hasValue() ? this.detailData.value() : undefined,
  );
  protected readonly detailLoading = computed(() => this.detailData.isLoading());
  protected readonly detailError = computed(() => (this.detailData.error() as Error | undefined)?.message);

  protected readonly maxVersionImages = computed(() =>
    Math.max(1, ...(this.detail()?.versions ?? []).map((v) => v.images)),
  );

  private toDetail(
    versions: HomeWhereIsPackageItemDto[],
    vulnerabilities: HomeWhereIsPackageVulnerabilityDto[],
    versionsWithoutIds: number,
  ): PackageDetail {
    const ids = new Map<string, IdRow>();
    for (const v of vulnerabilities) {
      const row = ids.get(v.vulnerabilityId) ?? {
        vulnerabilityId: v.vulnerabilityId,
        severityId: v.severityId,
        isKnownExploited: v.isKnownExploited,
        fixedVersions: [],
        images: 0,
      };
      row.images += v.imageIndexes.length;
      if (v.fixedVersion && !row.fixedVersions.includes(v.fixedVersion)) {
        row.fixedVersions.push(v.fixedVersion);
      }
      ids.set(v.vulnerabilityId, row);
    }

    return {
      versions: versions.map((v) => {
        // the color is the highest severity of the version
        const color =
          v.maxSeverityId == null ? 'var(--tod-package-hotspots-no-fix)' : SeverityUtils.getCssColor(v.maxSeverityId);
        return { version: v.version, images: v.images, segments: [{ label: v.version, value: v.images, color }] };
      }),
      ids: [...ids.values()].sort((a, b) => a.severityId - b.severityId || b.images - a.images),
      versionsWithoutIds,
    };
  }

  protected severityColor(severityId: number): string {
    return SeverityUtils.getCssColor(severityId);
  }

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
