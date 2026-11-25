import { ChangeDetectionStrategy, Component, computed, effect, inject, linkedSignal, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';

import { HomeNewImagesDto } from '@api/models/home-new-images-dto';
import { HomeNewImagesNormalizedImageDto } from '@api/models/home-new-images-normalized-image-dto';
import { HomePivotPairDto } from '@api/models/home-pivot-pair-dto';
import { HomeService } from '@api/services/home.service';
import { DarkModeService } from '@core/services/dark-mode.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { ActivityHeatmapComponent } from '@shared/ui/activity-heatmap/activity-heatmap.component';
import { LayeredGraphComponent } from '@shared/ui/layered-graph/layered-graph.component';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableDialogComponent } from '@shared/ui/trivy-table-dialog/trivy-table-dialog.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

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
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeDaysResult, HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

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

// an image of the normalized rows, with what the boxes need of it
interface ImageRef {
  dto: HomeNewImagesNormalizedImageDto;
  // repository:tag
  name: string;
  // the UTC day the digest was first seen
  day: string;
  namespaceNames: string[];
}

interface PairRef {
  dto: HomePivotPairDto;
  key: string;
}

// a row of the pivot: an image and a pair it has now (the occurrences of the pair in the image)
interface NewImageItem {
  image: ImageRef;
  pair: PairRef;
  count: number;
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
// same data (api/home/new-images/normalized: the namespaces and the pairs once, the images pointing into them), with what
// the images have now (current state, not as they arrived); "Show all" flattens the same rows into a table, only while it is
// open:
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
  templateUrl: './home-new-images.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeNewImagesComponent extends HomePivotWidgetBase<ImageBox, PairBox> {
  // the help dialog of the frame (html, short: what is easy to get wrong is in bold)
  protected readonly help: HomeWidgetHelp = {
    question: `Which <strong>images started running</strong> in the range, and what do they carry? So a new deployment does not bring in a surprise.`,
    oddities: [
      `The vulnerabilities shown are the ones of <strong>today</strong>, not the ones the image arrived with.`,
      `A pair is (vulnerability id, severity). An image with no pair at the selected severities is <strong>not counted</strong>.`,
      `The first day of the history looks inflated: the images already running then count as first seen then.`,
    ],
    expanded: [
      `A graph: the vulnerabilities above (colored by severity), then the images, then the namespaces they run in; a line for each pair an image has. The number on a node is its lines.`,
      `Click a day of the heatmap to filter the graph.`,
    ],
    tips: [
      `<strong>Click a node</strong> to light only what is connected to it; then click a node on the other side to pick an image and a pair together.`,
    ],
  };

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

  // the days, for the widget "Since your last visit" (one call of this endpoint per page): emitted from an effect, as
  // the inputs are not set yet when a parent could read them
  readonly daysChanged = output<HomeDaysResult<HomeNewImagesDto>>();
  private readonly daysResult = computed<HomeDaysResult<HomeNewImagesDto>>(() => ({
    dto: this.daysData.value(),
    error: this.daysData.error() ? ((this.daysData.error() as Error)?.message ?? 'request failed') : undefined,
  }));

  // idle (and empty) while collapsed: the normalized rows (the names once, the rest points into the lists)
  private readonly rowsData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeNewImagesNormalized({
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
      data: (dto?.days ?? []).map((d) => ({ day: d.day, value: d.images, emphasis2: d.knownExploited > 0 })),
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
    (this.days()?.days ?? []).filter((d) => d.images > 0).map((d) => d.day),
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


  // the rows of the normalized response, as one row per image and pair (they share the image and pair objects: the names
  // are not copied)
  private readonly items = computed<NewImageItem[]>(() => {
    const dto = this.rowsData.value();
    if (!dto) return [];

    const pairs: PairRef[] = dto.pairs.map((p) => ({ dto: p, key: pairKey(p.vulnerabilityId, p.severityId) }));

    return dto.images.flatMap((image) => {
      const ref: ImageRef = {
        dto: image,
        name: `${image.repository}:${image.tag}`,
        day: image.firstSeenAt.substring(0, 10),
        namespaceNames: image.namespaceIndexes.map((i) => dto.namespaceNames[i]),
      };

      return image.pairs.map((p) => ({ image: ref, pair: pairs[p.pairIndex], count: p.count }));
    });
  });

  // per pair, over all the rows of the range (not of the selected day): its images and where they run
  private readonly pairStats = computed(() => {
    const digests = new Map<string, Set<string>>();
    const namespaces = new Map<string, Set<string>>();
    for (const { image, pair } of this.items()) {
      digests.set(pair.key, (digests.get(pair.key) ?? new Set()).add(image.dto.digest));
      const names = namespaces.get(pair.key) ?? new Set();
      image.namespaceNames.forEach((x) => names.add(x));
      namespaces.set(pair.key, names);
    }

    return new Map(
      [...digests].map(([key, images]) => [key, { images: images.size, namespaceNames: [...(namespaces.get(key) ?? [])].sort() }]),
    );
  });

  // the rows of the selects: one per image and pair
  private readonly entries = computed<PivotEntry[]>(() =>
    this.items().map(({ image, pair }) => ({
      day: image.day,
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

    return day ? items.filter((x) => x.image.day === day) : items;
  });

  // the pivot: the same rows, once per image and once per pair
  private readonly boxes = computed(() => {
    const stats = this.pairStats();
    const images = new Map<string, ImageBox>();
    const pairs = new Map<string, PairBox>();

    for (const { image, pair, count } of this.dayItems()) {
      // an image has one row per pair: the items are one per (image, pair)
      let imageBox = images.get(image.dto.digest);
      if (!imageBox) {
        imageBox = {
          digest: image.dto.digest,
          name: image.name,
          repository: image.dto.repository,
          tag: image.dto.tag,
          registry: image.dto.registry,
          day: image.day,
          workloads: image.dto.workloads,
          namespaces: image.namespaceNames.length,
          namespaceNames: image.namespaceNames,
          rows: [],
        };
        images.set(image.dto.digest, imageBox);
      }
      imageBox.rows.push({
        pairKey: pair.key,
        vulnerabilityId: pair.dto.vulnerabilityId,
        severityId: pair.dto.severityId,
        isKnownExploited: pair.dto.isKnownExploited,
        count,
      });

      let pairBox = pairs.get(pair.key);
      if (!pairBox) {
        const stat = stats.get(pair.key);
        pairBox = {
          key: pair.key,
          vulnerabilityId: pair.dto.vulnerabilityId,
          severityId: pair.dto.severityId,
          title: pair.dto.title,
          isKnownExploited: pair.dto.isKnownExploited,
          images: stat?.images ?? 0,
          namespaces: stat?.namespaceNames.length ?? 0,
          namespaceNames: stat?.namespaceNames ?? [],
          rows: [],
          days: [],
        };
        pairs.set(pair.key, pairBox);
      }
      pairBox.rows.push({ key: image.dto.digest, day: image.day, digest: image.dto.digest, name: image.name });
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

  // "Show all": the rows flattened from the normalized ones (one per image and pair, the namespaces in one array column);
  // made only while the dialog is open, as the template reads it inside its @if
  protected readonly tableRows = computed<NewImagesTableRow[]>(() =>
    this.items().map(({ image, pair, count }) => ({
      firstSeenAt: image.dto.firstSeenAt,
      vulnerabilityId: pair.dto.vulnerabilityId,
      severityId: pair.dto.severityId,
      title: pair.dto.title,
      isKnownExploited: pair.dto.isKnownExploited,
      count,
      registry: image.dto.registry,
      imageName: image.name,
      imageDigest: image.dto.digest,
      workloads: image.dto.workloads,
      namespaceNames: image.namespaceNames.join(','),
    })),
  );

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

  // the button of the report column: the vulnerability report of the image, as the one of the Selected box
  protected onTableRowAction(event: { row: NewImagesTableRow; col: string }): void {
    if (event.col === 'openReport') {
      this.imageHistory.openReport(event.row.imageDigest);
    }
  }
}
