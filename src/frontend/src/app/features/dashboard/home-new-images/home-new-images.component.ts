import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';

import { HomeNewImagesGroupDto } from '@api/models/home-new-images-group-dto';
import { HomeService } from '@api/services/home.service';
import { DarkModeService } from '@core/services/dark-mode.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '@shared/pipes/vulnerability-count.pipe';
import { ActivityHeatmapComponent } from '@shared/ui/activity-heatmap/activity-heatmap.component';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableDialogComponent } from '@shared/ui/trivy-table-dialog/trivy-table-dialog.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';
import { VirtualScrollerComponent } from '@shared/ui/virtual-scroller/virtual-scroller.component';

import { HomeExploitTagComponent } from '../home-pivot/home-exploit-tag.component';
import { HomeImageHistoryService } from '../home-pivot/home-image-history.service';
import { HomeNamespacesFooterComponent } from '../home-pivot/home-namespaces-footer.component';
import { HomePivotImageFieldsComponent } from '../home-pivot/home-pivot-image-fields.component';
import { HomePivotOptionComponent } from '../home-pivot/home-pivot-option.component';
import { HomePivotSelectedSkeletonComponent } from '../home-pivot/home-pivot-selected-skeleton.component';
import { HomePivotSkeletonComponent } from '../home-pivot/home-pivot-skeleton.component';
import { HomePivotState } from '../home-pivot/home-pivot-state';
import { HomePivotVulnerabilityFieldComponent } from '../home-pivot/home-pivot-vulnerability-field.component';
import { HomePivotWidgetBase } from '../home-pivot/home-pivot-widget.base';
import { DayGroup, NamespacesFooter, PivotEntry, groupByDay, pairKey } from '../home-pivot/home-pivot.types';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// one new image (digest): the pairs it has now; the image arrived on one day, so its rows are not grouped by day
interface ImageBox extends NamespacesFooter {
  digest: string;
  // repository:tag
  name: string;
  repository: string;
  tag: string;
  registry: string;
  // the UTC day the digest was first seen
  day: string;
  workloads: number;
  rows: ImageBoxRow[];
}

interface ImageBoxRow {
  pairKey: string;
  vulnerabilityId: string;
  severityId: number;
  isKnownExploited: boolean;
  // the occurrences (package, version, target) of the pair in the image, now
  count: number;
}

// one pair (vulnerability id, severity): the new images that have it, by the day they arrived
interface PairBox extends NamespacesFooter {
  key: string;
  vulnerabilityId: string;
  severityId: number;
  // of the vulnerability definition
  title: string | null | undefined;
  // known exploited (KEV / EUVD)
  isKnownExploited: boolean;
  // the new images (of the range) that have it
  images: number;
  rows: PairBoxRow[];
  // the rows by day, the most recent first
  days: DayGroup<PairBoxRow>[];
}

interface PairBoxRow {
  // digest
  key: string;
  day: string;
  digest: string;
  // repository:tag
  name: string;
}

// a row of "Show all": an item of the server (an image and a pair), with what the table shows of its pair
interface NewImagesTableRow {
  firstSeenAt: string;
  vulnerabilityId: string;
  severityId: number;
  title: string | null | undefined;
  isKnownExploited: boolean;
  count: number;
  registry: string;
  // repository:tag
  imageName: string;
  imageDigest: string;
  workloads: number;
  // comma separated, as the 'array' column (the count pill and its popover) and its filter split them
  namespaceNames: string;
}

// New images: the heatmap of the running images first seen per UTC day; while expanded, two rows of boxes that pivot the
// same data (api/home/new-images/denormalized: a row per image and pair, and a group per pair), with what the images
// have now (current state, not as they arrived):
// - images: a box per new image (digest) with a table of its pairs: vulnerability id, severity, and the count (the
//   occurrences of the pair in the image: one per package, version and target)
// - pairs: a box per (vulnerability id, severity) with a table of the new images that have it, by the day they arrived
// it loads by itself (a change of a filter, or of refresh, reloads it); the rows are asked only while expanded
// an image without pairs (inside the severities filter) is not a new image here: the server leaves it out of the days too
// the selection, the selects and the boxes' behavior are the ones of HomePivotWidgetBase
@Component({
  selector: 'app-home-new-images',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    ActivityHeatmapComponent,
    TrivyBoxComponent,
    ButtonModule,
    FormsModule,
    SelectModule,
    SkeletonModule,
    TagModule,
    VirtualScrollerComponent,
    TrivyTableDialogComponent,
    SeverityCssStyleByIdPipe,
    VulnerabilityCountPipe,
    HomeExploitTagComponent,
    HomeNamespacesFooterComponent,
    HomePivotImageFieldsComponent,
    HomePivotOptionComponent,
    HomePivotSelectedSkeletonComponent,
    HomePivotSkeletonComponent,
    HomePivotVulnerabilityFieldComponent,
  ],
  templateUrl: './home-new-images.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeNewImagesComponent extends HomePivotWidgetBase<ImageBox, PairBox> {
  // the help dialog of the frame (html, short: what is easy to get wrong is in bold)
  protected readonly help = `
    <p>The <strong>running images</strong> that the history saw for the first time in the range, with what they have <strong>now</strong>.</p>
    <p class="mt-2"><strong>Current state:</strong> the vulnerabilities and severities are today's, not the ones the image arrived with.</p>
    <p class="mt-2"><strong>Filters:</strong> a pair is (vulnerability id, severity). An image with no pair at the selected severities is <strong>not counted</strong>.</p>
    <p class="mt-2"><strong>Heatmap:</strong> images per UTC day. While expanded, click a day to filter the boxes.</p>
    <p class="mt-2"><strong>Boxes:</strong> images (newest first) and pairs (the images that have each). A click on a line selects both sides and the other box scrolls to it.</p>
    <p class="mt-2"><strong>Count</strong> is the occurrences (package, version, target), not distinct vulnerabilities.</p>
    <p class="mt-2"><strong>First day of the history:</strong> the images already running then are first seen then, so that day can look inflated.</p>
  `;

  private readonly homeService = inject(HomeService);
  private readonly imageHistory = inject(HomeImageHistoryService);
  private readonly darkModeService = inject(DarkModeService);

  // the popover of the heatmap legend: more is darker in light mode and lighter in dark mode, so it says which
  protected readonly valueHint = computed(
    () => `Running images first seen by the history that day, with at least one vulnerability at the selected severities. ${this.darkModeService.isDarkMode() ? 'Lighter' : 'Darker'} = more.`,
  );

  private readonly daysData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeNewImages({ severities: params.severities, namespaces: params.namespaces }),
  });

  // idle (and empty) while collapsed
  private readonly rowsData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeNewImagesDenormalized({
        severities: params.severities,
        namespaces: params.namespaces,
      }),
  });

  protected readonly chart = computed(() => {
    const dto = this.daysData.value();

    return {
      from: dto?.from,
      data: (dto?.days ?? []).map((d) => ({ day: d.day, value: d.images })),
      flaggedDays: (dto?.days ?? []).filter((d) => d.knownExploited > 0).map((d) => d.day),
    };
  });

  protected readonly state = computed<HomeWidgetState>(() => {
    if (this.daysData.error()) {
      return { error: (this.daysData.error() as Error)?.message ?? 'request failed' };
    }

    const dto = this.daysData.value();
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary:
        `${dto.days.length} days from ${dto.from}; ${dto.days.reduce((sum, d) => sum + d.images, 0)} images ` +
        `on ${dto.days.filter((d) => d.images > 0).length} days` +
        (this.isExpanded() && this.rowsData.isLoading() ? ' (loading more...)' : ''),
    };
  });

  protected override readonly valueDays = computed(() =>
    (this.daysData.value()?.days ?? []).filter((d) => d.images > 0).map((d) => d.day),
  );

  protected override readonly showBoxes = computed(
    () => this.isExpanded() && this.daysData.value()?.isAvailable === true && this.rowsData.hasValue(),
  );

  // a refresh (or a change of a filter) clears the days as well as the rows: the skeleton shows from the first moment
  // of the wait, not only once the days are back; it does not for a widget that is known to be unavailable
  protected readonly isLoadingBoxes = computed(
    () =>
      this.isExpanded() &&
      this.daysData.value()?.isAvailable !== false &&
      !this.rowsData.error() &&
      !this.rowsData.hasValue(),
  );

  // the days (the heatmap) are not here: the heatmap box shows a skeleton of its own size, so it does not collapse
  protected readonly isLoadingDays = computed(() => !this.daysData.hasValue() && !this.daysData.error());

  // the rows of the selects: one per item of the server
  private readonly entries = computed<PivotEntry[]>(() => {
    const dto = this.rowsData.value();
    const exploited = new Map<string, boolean>();
    for (const group of dto?.groups ?? []) {
      exploited.set(pairKey(group.vulnerabilityId, group.severityId), group.isKnownExploited);
    }

    return (dto?.items ?? []).map((item) => {
      const key = pairKey(item.vulnerabilityId, item.severityId);

      return {
        day: item.firstSeenAt.substring(0, 10),
        digest: item.imageDigest,
        name: `${item.repository}:${item.tag}`,
        pairKey: key,
        vulnerabilityId: item.vulnerabilityId,
        severityId: item.severityId,
        isKnownExploited: exploited.get(key) ?? false,
      };
    });
  });

  protected override readonly pivot = new HomePivotState(this.entries, this.isExpanded, this.today);

  // the boxes are made of the items of the selected UTC day only (none selected: all of them)
  private readonly dayItems = computed(() => {
    const items = this.rowsData.value()?.items ?? [];
    const day = this.pivot.selectedDay();

    return day ? items.filter((x) => x.firstSeenAt.substring(0, 10) === day) : items;
  });

  // the pivot: the same rows, once per image and once per pair
  private readonly boxes = computed(() => {
    const dto = this.rowsData.value();
    const groups = new Map<string, HomeNewImagesGroupDto>();
    for (const group of dto?.groups ?? []) {
      groups.set(pairKey(group.vulnerabilityId, group.severityId), group);
    }

    const images = new Map<string, ImageBox>();
    const pairs = new Map<string, PairBox>();

    for (const item of this.dayItems()) {
      const key = pairKey(item.vulnerabilityId, item.severityId);
      const group = groups.get(key);
      const day = item.firstSeenAt.substring(0, 10);
      const name = `${item.repository}:${item.tag}`;

      // an image has one row per pair: the items are one per (image, pair)
      let image = images.get(item.imageDigest);
      if (!image) {
        image = {
          digest: item.imageDigest,
          name,
          repository: item.repository,
          tag: item.tag,
          registry: item.registry,
          day,
          workloads: item.workloads,
          namespaces: item.namespaces,
          namespaceNames: item.namespaceNames,
          rows: [],
        };
        images.set(item.imageDigest, image);
      }
      image.rows.push({
        pairKey: key,
        vulnerabilityId: item.vulnerabilityId,
        severityId: item.severityId,
        isKnownExploited: group?.isKnownExploited ?? false,
        count: item.count,
      });

      let pair = pairs.get(key);
      if (!pair) {
        pair = {
          key,
          vulnerabilityId: item.vulnerabilityId,
          severityId: item.severityId,
          title: group?.title,
          isKnownExploited: group?.isKnownExploited ?? false,
          images: group?.images ?? 0,
          namespaces: group?.namespaces ?? 0,
          namespaceNames: group?.namespaceNames ?? [],
          rows: [],
          days: [],
        };
        pairs.set(key, pair);
      }
      pair.rows.push({ key: item.imageDigest, day, digest: item.imageDigest, name });
    }

    const imageBoxes = [...images.values()];
    for (const box of imageBoxes) {
      // the worst severity first, then by id
      box.rows.sort((a, b) => a.severityId - b.severityId || a.vulnerabilityId.localeCompare(b.vulnerabilityId));
    }
    // the newest images first
    imageBoxes.sort((a, b) => b.day.localeCompare(a.day) || b.rows.length - a.rows.length || a.name.localeCompare(b.name));

    const pairBoxes = [...pairs.values()];
    for (const box of pairBoxes) {
      box.rows.sort((a, b) => b.day.localeCompare(a.day) || a.name.localeCompare(b.name));
      box.days = groupByDay(box.rows);
    }
    // the pairs with the most images first, then the worst severity
    pairBoxes.sort(
      (a, b) => b.rows.length - a.rows.length || a.severityId - b.severityId || a.vulnerabilityId.localeCompare(b.vulnerabilityId),
    );

    return { imageBoxes, pairBoxes };
  });

  protected override readonly imageBoxes = computed(() => this.boxes().imageBoxes);
  protected override readonly pairBoxes = computed(() => this.boxes().pairBoxes);

  protected readonly tableRows = computed<NewImagesTableRow[]>(() => {
    const dto = this.rowsData.value();
    const groups = new Map<string, HomeNewImagesGroupDto>();
    for (const group of dto?.groups ?? []) {
      groups.set(pairKey(group.vulnerabilityId, group.severityId), group);
    }

    return (dto?.items ?? []).map((item) => {
      const group = groups.get(pairKey(item.vulnerabilityId, item.severityId));

      return {
        firstSeenAt: item.firstSeenAt,
        vulnerabilityId: item.vulnerabilityId,
        severityId: item.severityId,
        title: group?.title,
        isKnownExploited: group?.isKnownExploited ?? false,
        count: item.count,
        registry: item.registry,
        imageName: `${item.repository}:${item.tag}`,
        imageDigest: item.imageDigest,
        workloads: item.workloads,
        namespaceNames: item.namespaceNames.join(','),
      };
    });
  });

  protected readonly tableColumns: TrivyTableColumn[] = [
    { field: 'firstSeenAt', header: 'First seen', isFilterable: false, isSortable: true, style: 'width: 9rem', renderType: 'dateTime' },
    { field: 'vulnerabilityId', header: 'Vulnerability', isFilterable: true, isSortable: true, style: 'width: 13rem', renderType: 'vulnerabilityId' },
    { field: 'severityId', header: 'Sev', isFilterable: true, isSortable: true, multiSelectType: 'severities', style: 'width: 7rem', renderType: 'severityBadge' },
    { field: 'isKnownExploited', header: 'Exploit', isFilterable: true, isSortable: true, tagSet: 'boolean', style: 'width: 7rem', renderType: 'tag' },
    { field: 'count', header: 'Count', isFilterable: false, isSortable: true, style: 'width: 5rem', renderType: 'standard' },
    { field: 'title', header: 'Title', isFilterable: true, isSortable: true, style: 'width: 20%', renderType: 'standard' },
    { field: 'imageName', header: 'Image', isFilterable: true, isSortable: true, style: 'width: 18%', renderType: 'standard' },
    { field: 'registry', header: 'Registry', isFilterable: true, autoFilter: true, isSortable: true, style: 'width: 10%', renderType: 'standard' },
    { field: 'workloads', header: 'Workloads', isFilterable: false, isSortable: true, style: 'width: 6rem', renderType: 'standard' },
    { field: 'namespaceNames', header: 'Namespaces', isFilterable: true, isSortable: true, multiSelectType: 'namespacesArrays', style: 'width: 12%', renderType: 'array' },
    { field: 'openReport', header: 'Report', isFilterable: false, isSortable: false, actionIcon: 'pi pi-external-link', style: 'width: 5rem; text-align: center;', renderType: 'action' },
  ];

  constructor() {
    super();

    // the CVE dialogs (EXPLOIT tag, details) read the enrichment of the app state, as the vulnerability id cell does
    inject(CveEnrichmentStateService).ensureLoaded();
  }

  // the button of the report column: the vulnerability report of the image, as the one of the Selected box
  protected onTableRowAction(event: { row: NewImagesTableRow; col: string }): void {
    if (event.col === 'openReport') {
      this.imageHistory.openReport(event.row.imageDigest);
    }
  }
}
