import { ChangeDetectionStrategy, Component, computed, effect, inject, linkedSignal, output } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';

import { HomeHistoryDriftDto } from '@api/models/home-history-drift-dto';
import { HomeHistoryDriftGroupDto } from '@api/models/home-history-drift-group-dto';
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

import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeDaysResult, HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';
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

// one image (digest): the pairs that were added to it, by day
interface ImageBox extends NamespacesFooter {
  digest: string;
  // repository:tag
  name: string;
  repository: string;
  tag: string;
  registry: string;
  rows: ImageBoxRow[];
  // the rows by day, the most recent first
  days: DayGroup<ImageBoxRow>[];
}

interface ImageBoxRow {
  // day and pair (the box is the digest)
  key: string;
  pairKey: string;
  day: string;
  vulnerabilityId: string;
  severityId: number;
  isKnownExploited: boolean;
  // the occurrences (package, version, target) of the pair in this digest on this day: the sum of its history entries
  count: number;
}

// one pair (vulnerability id, severity): the history entries it was added in (an image with its cves hash), by day
interface PairBox extends NamespacesFooter {
  key: string;
  vulnerabilityId: string;
  severityId: number;
  // of the vulnerability definition
  title: string | null | undefined;
  // known exploited (KEV / EUVD)
  isKnownExploited: boolean;
  // the first moment the pair was added to any image in the range (ISO)
  firstAddedAt: string | undefined;
  rows: PairBoxRow[];
  // the rows by day, the most recent first
  days: DayGroup<PairBoxRow>[];
}

interface PairBoxRow {
  // digest and day
  key: string;
  day: string;
  digest: string;
  // repository:tag
  name: string;
}

// a row of "Show all": an item of the server (a history entry and a pair), with what the table shows of its pair
interface DriftTableRow {
  addedAt: string;
  vulnerabilityId: string;
  severityId: number;
  title: string | null | undefined;
  isKnownExploited: boolean;
  count: number;
  registry: string;
  // repository:tag
  imageName: string;
  imageDigest: string;
  // comma separated, as the 'array' column (the count pill and its popover) and its filter split them
  namespaceNames: string;
}

// History drift: the heatmap of the pairs added per UTC day; while expanded, two rows of boxes that pivot the same
// data (api/home/history-drift/denormalized: a row per history entry (digest + cves hash) and pair, and a group per
// pair):
// - images: a box per image (digest) with a table of its pairs: day, vulnerability id, severity, and the count (the
//   occurrences of the pair in the digest that day, as the history counts them: one per package, version and target)
// - pairs: a box per (vulnerability id, severity) with a table of its rows: day, image, and the button of its report
// it loads by itself (a change of a filter, or of refresh, reloads it); the rows are asked only while expanded
// the selection, the selects and the boxes' behavior are the ones of HomePivotWidgetBase
@Component({
  selector: 'app-home-history-drift',
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
  templateUrl: './home-history-drift.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeHistoryDriftComponent extends HomePivotWidgetBase<ImageBox, PairBox> {
  // the help dialog of the frame (html, short: what is easy to get wrong is in bold)
  protected readonly help = `
    <p>The <strong>pairs</strong> (vulnerability id, severity) <strong>added</strong> to images that were <strong>already running</strong>, per UTC day, and still active now.</p>
    <p class="mt-2"><strong>Not here:</strong> what a new image brings with its first scan (see <em>New images</em>).</p>
    <p class="mt-2"><strong>Heatmap:</strong> pairs per day. A pair counts on <strong>every day</strong> it is added to some image, so the days do not sum to the distinct pairs. Only the history retention is complete.</p>
    <p class="mt-2"><strong>Boxes</strong> (expanded): images with the pairs added to each, and pairs with the images they were added to, by day. <strong>Count</strong> is the occurrences (package, version, target), not distinct vulnerabilities.</p>
    <p class="mt-2"><strong>Click a line</strong> to select an image and a pair together: the other box scrolls to it and <strong>Selected</strong> shows the details. <strong>Click a day</strong> to filter by it; that clears the selection.</p>
    <p class="mt-2"><strong>Filters</strong> (date, image, vulnerability) depend on each other: each offers only what fits the other two. <strong>Show all</strong> opens every row in a table.</p>
  `;

  private readonly homeService = inject(HomeService);
  private readonly imageHistory = inject(HomeImageHistoryService);
  private readonly darkModeService = inject(DarkModeService);

  // the popover of the heatmap legend: more is darker in light mode and lighter in dark mode, so it says which
  protected readonly valueHint = computed(
    () => `Distinct (vulnerability id, severity) pairs added that day to images that were already running, at the selected severities. A new image's first snapshot is not here (see New images). A pair added on several days counts on each. ${this.darkModeService.isDarkMode() ? 'Lighter' : 'Darker'} = more.`,
  );

  private readonly daysData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeHistoryDrift({ severities: params.severities, namespaces: params.namespaces }),
  });

  // the days, for the widget "Since your last visit" (one call of this endpoint per page): emitted from an effect, as
  // the inputs are not set yet when a parent could read them
  readonly daysChanged = output<HomeDaysResult<HomeHistoryDriftDto>>();
  private readonly daysResult = computed<HomeDaysResult<HomeHistoryDriftDto>>(() => ({
    dto: this.daysData.value(),
    error: this.daysData.error() ? ((this.daysData.error() as Error)?.message ?? 'request failed') : undefined,
  }));

  // idle (and empty) while collapsed
  private readonly rowsData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeHistoryDriftDenormalized({
        severities: params.severities,
        namespaces: params.namespaces,
      }),
  });

  // the days of the last answer, kept while the next one is asked (the heatmap does not go blank on a reload)
  private readonly days = linkedSignal<ReturnType<typeof this.daysData.value>, ReturnType<typeof this.daysData.value>>({
    source: this.daysData.value,
    computation: (value, previous) => value ?? previous?.value,
  });

  // counts the answers received: the heatmap plays its animation again for each one, also for the same days (a refresh)
  protected readonly answers = linkedSignal<ReturnType<typeof this.daysData.value>, number>({
    source: this.daysData.value,
    computation: (value, previous) => (previous?.value ?? 0) + (value ? 1 : 0),
  });

  // no days yet: an empty chart (zeros), so the heatmap has its size from the start
  protected override readonly chart = computed(() => {
    const dto = this.days();
    if (!dto) return this.emptyChart();

    return {
      from: dto.from,
      data: (dto?.days ?? []).map((d) => ({ day: d.day, value: d.added, emphasis2: d.addedKnownExploited > 0 })),
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

    const added = dto.days.reduce((sum, d) => sum + d.added, 0);
    const exploited = dto.days.reduce((sum, d) => sum + d.addedKnownExploited, 0);

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary:
        `${dto.days.length} days from ${dto.from}; added ${added} (${exploited} known exploited), ` +
        `on ${dto.days.filter((d) => d.added > 0).length} days` +
        (this.isExpanded() && this.rowsData.isLoading() ? ' (loading more...)' : ''),
    };
  });

  protected override readonly valueDays = computed(() =>
    (this.days()?.days ?? []).filter((d) => d.added > 0).map((d) => d.day),
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
        day: item.addedAt.substring(0, 10),
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

    return day ? items.filter((x) => x.addedAt.substring(0, 10) === day) : items;
  });

  // the pivot: the same rows, once per image and once per pair
  private readonly boxes = computed(() => {
    const dto = this.rowsData.value();
    const groups = new Map<string, HomeHistoryDriftGroupDto>();
    for (const group of dto?.groups ?? []) {
      groups.set(pairKey(group.vulnerabilityId, group.severityId), group);
    }

    const images = new Map<string, ImageBox>();
    // the namespaces of an image: the union of the ones of its history entries
    const imageNamespaces = new Map<string, Set<string>>();
    const pairs = new Map<string, PairBox>();
    // the rows of the boxes by (digest, day, pair): the items are per history entry (digest + cves hash)
    const imageRows = new Map<string, ImageBoxRow>();
    const pairRows = new Set<string>();

    for (const item of this.dayItems()) {
      const key = pairKey(item.vulnerabilityId, item.severityId);
      const group = groups.get(key);
      const day = item.addedAt.substring(0, 10);
      const name = `${item.repository}:${item.tag}`;

      let image = images.get(item.imageDigest);
      if (!image) {
        image = {
          digest: item.imageDigest,
          name,
          repository: item.repository,
          tag: item.tag,
          registry: item.registry,
          namespaces: 0,
          namespaceNames: [],
          rows: [],
          days: [],
        };
        images.set(item.imageDigest, image);
        imageNamespaces.set(item.imageDigest, new Set());
      }
      item.namespaceNames.forEach((x) => imageNamespaces.get(item.imageDigest)!.add(x));
      // the lists are capped by the server, the counts are not
      image.namespaces = Math.max(image.namespaces, item.namespaces);
      // the entries of a digest that added the same pair on the same day are one row, with their counts summed
      const imageRowKey = `${item.imageDigest}|${day}|${key}`;
      const imageRow = imageRows.get(imageRowKey);
      if (imageRow) {
        imageRow.count += item.count;
      } else {
        const row: ImageBoxRow = {
          key: `${day}|${key}`,
          pairKey: key,
          day,
          vulnerabilityId: item.vulnerabilityId,
          severityId: item.severityId,
          isKnownExploited: group?.isKnownExploited ?? false,
          count: item.count,
        };
        imageRows.set(imageRowKey, row);
        image.rows.push(row);
      }

      let pair = pairs.get(key);
      if (!pair) {
        pair = {
          key,
          vulnerabilityId: item.vulnerabilityId,
          severityId: item.severityId,
          title: group?.title,
          isKnownExploited: group?.isKnownExploited ?? false,
          firstAddedAt: group?.firstAddedAt,
          namespaces: group?.namespaces ?? 0,
          namespaceNames: group?.namespaceNames ?? [],
          rows: [],
          days: [],
        };
        pairs.set(key, pair);
      }
      // one row per digest and day
      const pairRowKey = `${key}|${item.imageDigest}|${day}`;
      if (!pairRows.has(pairRowKey)) {
        pairRows.add(pairRowKey);
        pair.rows.push({ key: `${item.imageDigest}|${day}`, day, digest: item.imageDigest, name });
      }
    }

    const imageBoxes = [...images.values()];
    for (const box of imageBoxes) {
      box.namespaceNames = [...(imageNamespaces.get(box.digest) ?? [])].sort();
      box.namespaces = Math.max(box.namespaces, box.namespaceNames.length);
      // the most recent day first, then by severity (the worst first) and id
      box.rows.sort(
        (a, b) => b.day.localeCompare(a.day) || a.severityId - b.severityId || a.vulnerabilityId.localeCompare(b.vulnerabilityId),
      );
      box.days = groupByDay(box.rows);
    }
    // the images with the most pairs first
    imageBoxes.sort((a, b) => b.rows.length - a.rows.length || a.name.localeCompare(b.name));

    const pairBoxes = [...pairs.values()];
    for (const box of pairBoxes) {
      box.rows.sort((a, b) => b.day.localeCompare(a.day) || a.name.localeCompare(b.name));
      box.days = groupByDay(box.rows);
    }
    // the pairs with the most rows first, then the worst severity
    pairBoxes.sort(
      (a, b) => b.rows.length - a.rows.length || a.severityId - b.severityId || a.vulnerabilityId.localeCompare(b.vulnerabilityId),
    );

    return { imageBoxes, pairBoxes };
  });

  protected override readonly imageBoxes = computed(() => this.boxes().imageBoxes);
  protected override readonly pairBoxes = computed(() => this.boxes().pairBoxes);

  protected readonly tableRows = computed<DriftTableRow[]>(() => {
    const dto = this.rowsData.value();
    const groups = new Map<string, HomeHistoryDriftGroupDto>();
    for (const group of dto?.groups ?? []) {
      groups.set(pairKey(group.vulnerabilityId, group.severityId), group);
    }

    return (dto?.items ?? []).map((item) => {
      const group = groups.get(pairKey(item.vulnerabilityId, item.severityId));

      return {
        addedAt: item.addedAt,
        vulnerabilityId: item.vulnerabilityId,
        severityId: item.severityId,
        title: group?.title,
        isKnownExploited: group?.isKnownExploited ?? false,
        count: item.count,
        registry: item.registry,
        imageName: `${item.repository}:${item.tag}`,
        imageDigest: item.imageDigest,
        namespaceNames: item.namespaceNames.join(','),
      };
    });
  });

  protected readonly tableColumns: TrivyTableColumn[] = [
    { field: 'addedAt', header: 'Added', isFilterable: false, isSortable: true, style: 'width: 9rem', renderType: 'dateTime' },
    { field: 'vulnerabilityId', header: 'Vulnerability', isFilterable: true, isSortable: true, style: 'width: 13rem', renderType: 'vulnerabilityId' },
    { field: 'severityId', header: 'Sev', isFilterable: true, isSortable: true, multiSelectType: 'severities', style: 'width: 7rem', renderType: 'severityBadge' },
    { field: 'isKnownExploited', header: 'Exploit', isFilterable: true, isSortable: true, tagSet: 'boolean', style: 'width: 7rem', renderType: 'tag' },
    { field: 'count', header: 'Count', isFilterable: false, isSortable: true, style: 'width: 5rem', renderType: 'standard' },
    { field: 'title', header: 'Title', isFilterable: true, isSortable: true, style: 'width: 20%', renderType: 'standard' },
    { field: 'imageName', header: 'Image', isFilterable: true, isSortable: true, style: 'width: 18%', renderType: 'standard' },
    { field: 'registry', header: 'Registry', isFilterable: true, autoFilter: true, isSortable: true, style: 'width: 10%', renderType: 'standard' },
    { field: 'namespaceNames', header: 'Namespaces', isFilterable: true, isSortable: true, multiSelectType: 'namespacesArrays', style: 'width: 12%', renderType: 'array' },
    { field: 'openHistory', header: 'History', isFilterable: false, isSortable: false, actionIcon: 'pi pi-external-link', style: 'width: 5rem; text-align: center;', renderType: 'action' },
  ];

  constructor() {
    super();

    effect(() => this.daysChanged.emit(this.daysResult()));

    // the CVE dialogs (KEV/EUVD tag, details) read the enrichment of the app state, as the vulnerability id cell does
    inject(CveEnrichmentStateService).ensureLoaded();
  }

  // the button of the history column: the history of the image, as the one of the Selected box
  protected onTableRowAction(event: { row: DriftTableRow; col: string }): void {
    if (event.col === 'openHistory') {
      this.imageHistory.open(event.row.imageDigest);
    }
  }
}
