import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  linkedSignal,
  output,
  signal,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';

import { HomeHistoryDriftDto } from '@api/models/home-history-drift-dto';
import { HomeHistoryDriftNormalizedImageDto } from '@api/models/home-history-drift-normalized-image-dto';
import { HomePivotPairDto } from '@api/models/home-pivot-pair-dto';
import { HomeService } from '@api/services/home.service';
import { DarkModeService } from '@core/services/dark-mode.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { ActivityHeatmapComponent } from '@shared/ui/activity-heatmap/activity-heatmap.component';
import { LayeredGraphComponent } from '@shared/ui/layered-graph/layered-graph.component';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableDialogComponent } from '@shared/ui/trivy-table-dialog/trivy-table-dialog.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeDaysResult, HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';
import { HomeImageHistoryService } from '../home-pivot/home-image-history.service';
import { HomePivotImageFieldsComponent } from '../home-pivot/home-pivot-image-fields.component';
import { HomePivotOptionComponent } from '../home-pivot/home-pivot-option.component';
import { HomePivotSelectedSkeletonComponent } from '../home-pivot/home-pivot-selected-skeleton.component';
import { HomePivotSkeletonComponent } from '../home-pivot/home-pivot-skeleton.component';
import { buildPivotGraph, pivotGraphColumns, pivotGraphNodeHeight } from '../home-pivot/home-pivot-graph';
import { HomePivotState } from '../home-pivot/home-pivot-state';
import { HomePivotVulnerabilityFieldComponent } from '../home-pivot/home-pivot-vulnerability-field.component';
import { HomePivotWidgetBase } from '../home-pivot/home-pivot-widget.base';
import { DayGroup, NamespacesFooter, PivotEntry, groupByDay, pairKey } from '../home-pivot/home-pivot.types';
import { WeekdayNamePipe } from '@shared/pipes/weekday-name.pipe';
import { ChartTipDirective } from '@shared/ui/popover/chart-tip.directive';

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

// an image of the normalized rows, with what the boxes need of it
interface DriftImageRef {
  dto: HomeHistoryDriftNormalizedImageDto;
  // repository:tag
  name: string;
}

interface PairRef {
  dto: HomePivotPairDto;
  key: string;
}

// a row of the pivot: a pair added to an image in a history entry (the occurrences of the pair in the entry)
interface DriftItem {
  image: DriftImageRef;
  pair: PairRef;
  // UTC day of the entry, "yyyy-MM-dd"
  day: string;
  // ISO
  addedAt: string;
  count: number;
  // of the entry
  namespaceNames: string[];
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
// data (api/home/history-drift/normalized: the namespaces, the pairs and the images once, and the history entries
// (digest + cves hash) pointing into them; "Show all" flattens the same rows into a table, only while it is open):
// - images: a box per image (digest) with a table of its pairs: day, vulnerability id, severity, and the count (the
//   occurrences of the pair in the digest that day, as the history counts them: one per package, version and target)
// - pairs: a box per (vulnerability id, severity) with a table of its rows: day, image, and the button of its report
// it loads by itself (a change of a filter, or of refresh, reloads it); the rows are asked only while expanded
// the selection, the selects and the boxes' behavior are the ones of HomePivotWidgetBase
@Component({
  selector: 'app-home-history-drift',
  standalone: true,
  imports: [
    WeekdayNamePipe,
    ChartTipDirective,
    HomeWidgetFrameComponent,
    ActivityHeatmapComponent,
    TrivyBoxComponent,
    LayeredGraphComponent,
    ButtonModule,
    FormsModule,
    SelectModule,
    SkeletonModule,
    TagModule,
    TrivyTableDialogComponent,
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
  protected readonly help: HomeWidgetHelp = {
    question: `Did <strong>images that were already running</strong> get new vulnerabilities, without anyone deploying anything? These are the ones that appear while you sleep.`,
    goodToKnow: [
      `A pair is (vulnerability id, severity), counted per UTC day, and only while still active.`,
      `A pair counts on <strong>every day</strong> it is added to some image, so the days do not sum to the distinct pairs.`,
      `Not here: what a new image brings with its first scan (see <em>New images</em>).`,
      `Only the history retention is complete.`,
    ],
    expanded: [
      `A graph: the vulnerabilities above (colored by severity), then the images, then the namespaces they run in; a line for each pair added to an image. The number on a node is its lines.`,
      `The filters (date, image, vulnerability) depend on each other: each offers only what fits the other two.`,
      `<strong>Show all</strong> opens every row in a table.`,
    ],
    tips: [
      `<strong>Click a node</strong> to light only what is connected to it; then click a node on the other side to pick an image and a pair together.`,
      `<strong>Click a day</strong> of the heatmap to filter by it.`,
    ],
  };

  private readonly homeService = inject(HomeService);
  private readonly imageHistory = inject(HomeImageHistoryService);
  private readonly darkModeService = inject(DarkModeService);

  // the popover of the heatmap legend: more is darker in light mode and lighter in dark mode, so it says which
  protected readonly valueHint = computed(
    () => `Distinct (vulnerability id, severity) pairs added that day to images that were already running (also to the stale ones, with the History switch off), at the selected severities. A new image's first snapshot is not here (see New images). A pair added on several days counts on each. ${this.darkModeService.isDarkMode() ? 'Lighter' : 'Darker'} = more.`,
  );

  // only the images running now (default), or the stale ones too
  readonly activeImagesOnly = input<boolean>(true);
  private readonly driftScope = computed(() => ({ ...this.scope(), activeImagesOnly: this.activeImagesOnly() }));

  private readonly daysData = rxResource({
    params: this.driftScope,
    stream: ({ params }) =>
      this.homeService.getHomeHistoryDrift({
        severities: params.severities,
        namespaces: params.namespaces,
        activeImagesOnly: params.activeImagesOnly,
      }),
  });

  // the days, for the widget "Since your last visit" (one call of this endpoint per page): emitted from an effect, as
  // the inputs are not set yet when a parent could read them
  readonly daysChanged = output<HomeDaysResult<HomeHistoryDriftDto>>();
  private readonly daysResult = computed<HomeDaysResult<HomeHistoryDriftDto>>(() => ({
    dto: this.daysData.value(),
    error: this.daysData.error() ? ((this.daysData.error() as Error)?.message ?? 'request failed') : undefined,
  }));

  // idle (and empty) while collapsed: the normalized rows (the names once, the rest points into the lists)
  private readonly rowsData = rxResource({
    params: () => (this.isExpanded() ? this.driftScope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeHistoryDriftNormalized({
        severities: params.severities,
        namespaces: params.namespaces,
        activeImagesOnly: params.activeImagesOnly,
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


  // the rows of the normalized response, as one row per history entry and pair (they share the image and pair objects:
  // the names are not copied)
  private readonly items = computed<DriftItem[]>(() => {
    const dto = this.rowsData.value();
    if (!dto) return [];

    const images: DriftImageRef[] = dto.images.map((image) => ({ dto: image, name: `${image.repository}:${image.tag}` }));
    const pairs: PairRef[] = dto.pairs.map((p) => ({ dto: p, key: pairKey(p.vulnerabilityId, p.severityId) }));

    return dto.entries.flatMap((entry) => {
      const day = entry.addedAt.substring(0, 10);
      const namespaceNames = entry.namespaceIndexes.map((i) => dto.namespaceNames[i]);

      return entry.pairs.map((p) => ({
        image: images[entry.imageIndex],
        pair: pairs[p.pairIndex],
        day,
        addedAt: entry.addedAt,
        count: p.count,
        namespaceNames,
      }));
    });
  });

  // per pair, over all the rows of the range (not of the selected day): the first moment it was added to any image, and
  // the namespaces of the entries it was added in
  private readonly pairStats = computed(() => {
    const stats = new Map<string, { firstAddedAt: string; namespaces: Set<string> }>();
    for (const { pair, addedAt, namespaceNames } of this.items()) {
      const stat = stats.get(pair.key) ?? { firstAddedAt: addedAt, namespaces: new Set<string>() };
      if (addedAt < stat.firstAddedAt) stat.firstAddedAt = addedAt;
      namespaceNames.forEach((x) => stat.namespaces.add(x));
      stats.set(pair.key, stat);
    }

    return stats;
  });

  // the rows of the selects: one per history entry and pair
  private readonly entries = computed<PivotEntry[]>(() =>
    this.items().map(({ image, pair, day }) => ({
      day,
      digest: image.dto.digest,
      name: image.name,
      pairKey: pair.key,
      vulnerabilityId: pair.dto.vulnerabilityId,
      severityId: pair.dto.severityId,
      isKnownExploited: pair.dto.isKnownExploited,
    })),
  );

  protected override readonly pivot = new HomePivotState(this.entries, this.isExpanded, this.today);

  // the boxes are made of the rows of the selected UTC day only (none selected: all of them)
  private readonly dayItems = computed(() => {
    const items = this.items();
    const day = this.pivot.selectedDay();

    return day ? items.filter((x) => x.day === day) : items;
  });

  // the pivot: the same rows, once per image and once per pair
  private readonly boxes = computed(() => {
    const stats = this.pairStats();
    const images = new Map<string, ImageBox>();
    // the namespaces of an image: the union of the ones of its history entries
    const imageNamespaces = new Map<string, Set<string>>();
    const pairs = new Map<string, PairBox>();
    // the rows of the boxes by (digest, day, pair): the items are per history entry (digest + cves hash)
    const imageRows = new Map<string, ImageBoxRow>();
    const pairRows = new Set<string>();

    for (const { image, pair, day, count, namespaceNames } of this.dayItems()) {
      const digest = image.dto.digest;

      let imageBox = images.get(digest);
      if (!imageBox) {
        imageBox = {
          digest,
          name: image.name,
          repository: image.dto.repository,
          tag: image.dto.tag,
          registry: image.dto.registry,
          namespaces: 0,
          namespaceNames: [],
          rows: [],
          days: [],
        };
        images.set(digest, imageBox);
        imageNamespaces.set(digest, new Set());
      }
      namespaceNames.forEach((x) => imageNamespaces.get(digest)!.add(x));
      // the entries of a digest that added the same pair on the same day are one row, with their counts summed
      const imageRowKey = `${digest}|${day}|${pair.key}`;
      const imageRow = imageRows.get(imageRowKey);
      if (imageRow) {
        imageRow.count += count;
      } else {
        const row: ImageBoxRow = {
          key: `${day}|${pair.key}`,
          pairKey: pair.key,
          day,
          vulnerabilityId: pair.dto.vulnerabilityId,
          severityId: pair.dto.severityId,
          isKnownExploited: pair.dto.isKnownExploited,
          count,
        };
        imageRows.set(imageRowKey, row);
        imageBox.rows.push(row);
      }

      let pairBox = pairs.get(pair.key);
      if (!pairBox) {
        const stat = stats.get(pair.key);
        const names = [...(stat?.namespaces ?? [])].sort();
        pairBox = {
          key: pair.key,
          vulnerabilityId: pair.dto.vulnerabilityId,
          severityId: pair.dto.severityId,
          title: pair.dto.title,
          isKnownExploited: pair.dto.isKnownExploited,
          firstAddedAt: stat?.firstAddedAt,
          namespaces: names.length,
          namespaceNames: names,
          rows: [],
          days: [],
        };
        pairs.set(pair.key, pairBox);
      }
      // one row per digest and day
      const pairRowKey = `${pair.key}|${digest}|${day}`;
      if (!pairRows.has(pairRowKey)) {
        pairRows.add(pairRowKey);
        pairBox.rows.push({ key: `${digest}|${day}`, day, digest, name: image.name });
      }
    }

    const imageBoxes = [...images.values()];
    for (const box of imageBoxes) {
      box.namespaceNames = [...(imageNamespaces.get(box.digest) ?? [])].sort();
      box.namespaces = box.namespaceNames.length;
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

  // ---- the graph: vulnerabilities (id, severity) -> images -> namespaces, top to bottom, of the same rows as the boxes (so
  // of the selected day); see home-pivot-graph.ts
  private readonly graph = computed(() => buildPivotGraph(this.imageBoxes(), this.pairBoxes()));
  protected readonly graphColumns = computed(() => pivotGraphColumns(this.graph().hasNamespaces));
  protected readonly graphNodeHeight = computed(() => pivotGraphNodeHeight(this.graph().hasNamespaces));
  protected readonly graphData = this.graph;

  // a namespace is not part of the selection of the boxes (they have no select for it): the graph keeps it
  private readonly selectedNamespace = signal<string | null>(null);

  // multi select of the graph: at most one node per column, a peer of the other: the pair, the image and the namespace
  protected readonly selectedNodes = computed<string[]>(() => {
    const selection = this.pivot.selection();
    const namespace = this.selectedNamespace();

    return [
      selection?.pairKey ? `p:${selection.pairKey}` : null,
      selection?.digest ? `i:${selection.digest}` : null,
      namespace ? `n:${namespace}` : null,
    ].filter((id): id is string => id !== null);
  });

  // the graph says the selected nodes: each side of the selection is set from its own (none: cleared, the day stays)
  protected onGraphSelect(ids: string[]): void {
    const side = (prefix: string): string | null => ids.find((id) => id.startsWith(prefix))?.substring(prefix.length) ?? null;

    this.selectedNamespace.set(side('n:'));
    this.pivot.pickImage(side('i:'));
    this.pivot.pickPair(side('p:'));
  }

  // "Show all": the rows flattened from the normalized ones (one per history entry and pair, the namespaces in one array
  // column); made only while the dialog is open, as the template reads it inside its @if
  protected readonly tableRows = computed<DriftTableRow[]>(() =>
    this.items().map(({ image, pair, addedAt, count, namespaceNames }) => ({
      addedAt,
      vulnerabilityId: pair.dto.vulnerabilityId,
      severityId: pair.dto.severityId,
      title: pair.dto.title,
      isKnownExploited: pair.dto.isKnownExploited,
      count,
      registry: image.dto.registry,
      imageName: image.name,
      imageDigest: image.dto.digest,
      namespaceNames: namespaceNames.join(','),
    })),
  );

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

    // the base reveals the widget once its two rows of boxes have a height; there are none here, the graph takes their place
    effect(() => {
      const height = this.showBoxes() ? 1 : 0;
      this.imagesRowHeight.set(height);
      this.pairsRowHeight.set(height);
    });

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
