import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal, viewChild } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';

import { HomeHistoryDriftGroupDto } from '@api/models/home-history-drift-group-dto';
import { HomeService } from '@api/services/home.service';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '@shared/pipes/vulnerability-count.pipe';
import { ActivityHeatmapComponent } from '@shared/ui/activity-heatmap/activity-heatmap.component';
import { eolRelative, parseEolDay } from '@shared/ui/eol-runway/eol-runway.state';
import { PopoverService } from '@shared/ui/popover/popover.service';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableDialogComponent } from '@shared/ui/trivy-table-dialog/trivy-table-dialog.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';
import { VirtualScrollerComponent } from '@shared/ui/virtual-scroller/virtual-scroller.component';
import { VulnerabilityTagDialogsService } from '@shared/ui/vulnerability-tag-dialogs/vulnerability-tag-dialogs.service';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// a box with the images of a pair, or the pairs of an image: the namespaces are in the footer (as in the Base OS boxes)
interface NamespacesFooter {
  namespaces: number;
  namespaceNames: string[];
}

// the rows of a box that share a day: the box shows the day once, then the rows without it
interface DayGroup<T> {
  day: string;
  rows: T[];
}

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

// the selection: a click on a line of a box selects its image and its pair (both boxes are marked, the other one is
// scrolled to); a pick in a select chooses one side only; the summary shows what is selected
// day is not a selection of a box but a filter: the boxes show only the rows of that UTC day
interface DriftSelection {
  digest: string | null;
  pairKey: string | null;
  day: string | null;
}

// an entry of the selects: a day, an image (value = digest) or a pair (value = pair key), with its exploited flag
interface DriftOption {
  value: string;
  label: string;
  // a day: how long ago
  note?: string;
  // the digest of an image, to search by it
  digest?: string;
  // the severity of a pair
  severityId?: number;
  isKnownExploited: boolean;
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
@Component({
  selector: 'app-home-history-drift',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    ActivityHeatmapComponent,
    TrivyBoxComponent,
    NgTemplateOutlet,
    ButtonModule,
    FormsModule,
    SelectModule,
    SkeletonModule,
    TagModule,
    VirtualScrollerComponent,
    TrivyTableDialogComponent,
    SeverityCssStyleByIdPipe,
    VulnerabilityCountPipe,
  ],
  templateUrl: './home-history-drift.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeHistoryDriftComponent extends HomeWidgetBase {
  // the namespaces named in the footer of a box; the pill counts the others
  private static readonly shownNamespaces = 2;

  private readonly homeService = inject(HomeService);
  private readonly router = inject(Router);
  private readonly mainAppInitService = inject(MainAppInitService);
  private readonly cveEnrichmentState = inject(CveEnrichmentStateService);
  private readonly vulnerabilityTagDialogsService = inject(VulnerabilityTagDialogsService);
  protected readonly popover = inject(PopoverService);

  // the two rows of boxes (virtual: only the boxes in view are rendered, so a box is scrolled to by its index)
  private readonly imagesRow = viewChild<VirtualScrollerComponent<ImageBox>>('imagesRow');
  private readonly pairsRow = viewChild<VirtualScrollerComponent<PairBox>>('pairsRow');

  hint = input<string>('');
  namespaces = input.required<string[]>();
  severities = input.required<number[]>();
  // "yyyy-MM-dd"
  today = input.required<string>();
  // the days the home page was opened
  visitDays = input<string[]>([]);
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  // a new object on any change, so the resources reload; empty namespaces are not sent (the backend default applies)
  private readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  private readonly daysData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeHistoryDrift({ severities: params.severities, namespaces: params.namespaces }),
  });

  // idle (and empty) while collapsed
  private readonly rowsData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeHistoryDriftDenormalized({
        severities: params.severities,
        namespaces: params.namespaces,
      }),
  });

  protected readonly chart = computed(() => {
    const dto = this.daysData.value();

    return {
      from: dto?.from,
      data: (dto?.days ?? []).map((d) => ({ day: d.day, value: d.added })),
      flaggedDays: (dto?.days ?? []).filter((d) => d.addedKnownExploited > 0).map((d) => d.day),
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

  protected readonly showBoxes = computed(
    () => this.isExpanded() && this.daysData.value()?.isAvailable === true && this.rowsData.hasValue(),
  );
  // the skeleton of the boxes while the rows are asked: two rows of boxes, as the real ones
  protected readonly skeletonRows = [0, 1];
  protected readonly skeletonBoxes = [0, 1, 2, 3];
  protected readonly skeletonLines = [0, 1, 2, 3, 4, 5];

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

  // the day filter: the boxes are made of the items of that UTC day only (none selected: all of them)
  private readonly selectedDay = computed(() => this.selection()?.day ?? null);
  private readonly dayItems = computed(() => {
    const items = this.rowsData.value()?.items ?? [];
    const day = this.selectedDay();

    return day ? items.filter((x) => x.addedAt.substring(0, 10) === day) : items;
  });

  // the pivot: the same rows, once per image and once per pair
  private readonly boxes = computed(() => {
    const dto = this.rowsData.value();
    const groups = new Map<string, HomeHistoryDriftGroupDto>();
    for (const group of dto?.groups ?? []) {
      groups.set(this.pairKey(group.vulnerabilityId, group.severityId), group);
    }

    const images = new Map<string, ImageBox>();
    // the namespaces of an image: the union of the ones of its history entries
    const imageNamespaces = new Map<string, Set<string>>();
    const pairs = new Map<string, PairBox>();
    // the rows of the boxes by (digest, day, pair): the items are per history entry (digest + cves hash)
    const imageRows = new Map<string, ImageBoxRow>();
    const pairRows = new Set<string>();

    for (const item of this.dayItems()) {
      const key = this.pairKey(item.vulnerabilityId, item.severityId);
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
      box.days = this.groupByDay(box.rows);
    }
    // the images with the most pairs first
    imageBoxes.sort((a, b) => b.rows.length - a.rows.length || a.name.localeCompare(b.name));

    const pairBoxes = [...pairs.values()];
    for (const box of pairBoxes) {
      box.rows.sort((a, b) => b.day.localeCompare(a.day) || a.name.localeCompare(b.name));
      box.days = this.groupByDay(box.rows);
    }
    // the pairs with the most rows first, then the worst severity
    pairBoxes.sort(
      (a, b) => b.rows.length - a.rows.length || a.severityId - b.severityId || a.vulnerabilityId.localeCompare(b.vulnerabilityId),
    );

    return { imageBoxes, pairBoxes };
  });

  protected readonly imageBoxes = computed(() => this.boxes().imageBoxes);
  protected readonly pairBoxes = computed(() => this.boxes().pairBoxes);

  protected readonly trackImageBox = (_: number, box: ImageBox) => box.digest;
  protected readonly trackPairBox = (_: number, box: PairBox) => box.key;

  // "Show all": the dialog with the denormalized rows, as the server sent them (they are not asked again)
  protected readonly showAllDialog = signal(false);

  protected readonly tableRows = computed<DriftTableRow[]>(() => {
    const dto = this.rowsData.value();
    const groups = new Map<string, HomeHistoryDriftGroupDto>();
    for (const group of dto?.groups ?? []) {
      groups.set(this.pairKey(group.vulnerabilityId, group.severityId), group);
    }

    return (dto?.items ?? []).map((item) => {
      const group = groups.get(this.pairKey(item.vulnerabilityId, item.severityId));

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

  // the button of the history column: the history of the image, as the one of the Selected box
  protected onTableRowAction(event: { row: DriftTableRow; col: string }): void {
    if (event.col === 'openHistory') {
      this.openHistory(event.row.imageDigest);
    }
  }

  protected readonly selection = signal<DriftSelection | null>(null);

  // what the summary shows: the image box and the pair box of the selection, whichever are selected (all the boxes, not
  // only the shown ones)
  protected readonly selected = computed(() => {
    const selection = this.selection();
    if (!selection) return undefined;

    const image = selection.digest ? this.boxes().imageBoxes.find((x) => x.digest === selection.digest) : undefined;
    const pair = selection.pairKey ? this.boxes().pairBoxes.find((x) => x.key === selection.pairKey) : undefined;

    return image || pair ? { image, pair } : undefined;
  });

  // the selects: the days, the images and the pairs (id + severity); an image or a pair is exploited when it has at least
  // one entry of a pair that is
  // all three depend on each other: each offers what is consistent with the other two (nothing selected: everything), so
  // a day offers the days of the image and the pair selected, an image the images of the day and the pair, and so on
  private readonly entries = computed(() => {
    const dto = this.rowsData.value();
    const exploited = new Map<string, boolean>();
    for (const group of dto?.groups ?? []) {
      exploited.set(this.pairKey(group.vulnerabilityId, group.severityId), group.isKnownExploited);
    }

    return (dto?.items ?? []).map((item) => {
      const pairKey = this.pairKey(item.vulnerabilityId, item.severityId);

      return {
        day: item.addedAt.substring(0, 10),
        digest: item.imageDigest,
        name: `${item.repository}:${item.tag}`,
        pairKey,
        vulnerabilityId: item.vulnerabilityId,
        severityId: item.severityId,
        isKnownExploited: exploited.get(pairKey) ?? false,
      };
    });
  });

  // the entries that agree with the selection of the other two selects
  private entriesFor(select: 'day' | 'digest' | 'pairKey') {
    const selection = this.selection();

    return this.entries().filter(
      (x) =>
        (select === 'day' || !selection?.day || x.day === selection.day) &&
        (select === 'digest' || !selection?.digest || x.digest === selection.digest) &&
        (select === 'pairKey' || !selection?.pairKey || x.pairKey === selection.pairKey),
    );
  }

  protected readonly dayOptions = computed<DriftOption[]>(() =>
    [...new Set(this.entriesFor('day').map((x) => x.day))]
      .sort()
      .reverse()
      .map((day) => ({ value: day, label: day, note: this.age(day), isKnownExploited: false })),
  );

  // the days the heatmap lets you click: every day with rows, whatever image and pair are selected (a click starts over:
  // it clears them); the day select offers fewer, only the days of the selected image and pair
  protected readonly heatmapDays = computed(() => [...new Set(this.entries().map((x) => x.day))]);

  protected readonly imageOptions = computed<DriftOption[]>(() => {
    const options = new Map<string, DriftOption>();
    for (const x of this.entriesFor('digest')) {
      const option = options.get(x.digest);
      if (option) {
        option.isKnownExploited ||= x.isKnownExploited;
      } else {
        options.set(x.digest, { value: x.digest, label: x.name, digest: x.digest, isKnownExploited: x.isKnownExploited });
      }
    }

    return [...options.values()].sort((a, b) => a.label.localeCompare(b.label) || a.value.localeCompare(b.value));
  });

  protected readonly pairOptions = computed<DriftOption[]>(() => {
    const options = new Map<string, DriftOption>();
    for (const x of this.entriesFor('pairKey')) {
      if (!options.has(x.pairKey)) {
        options.set(x.pairKey, {
          value: x.pairKey,
          label: x.vulnerabilityId,
          severityId: x.severityId,
          isKnownExploited: x.isKnownExploited,
        });
      }
    }

    return [...options.values()].sort((a, b) => a.label.localeCompare(b.label) || (a.severityId ?? 0) - (b.severityId ?? 0));
  });

  // set when the rows of this expand were revealed; reset on collapse (so a reload by a filter does not scroll)
  private revealed = false;
  // the rows have no height until their boxes are rendered and measured (virtual rows): the widget is revealed when
  // both have one (or at once when there are no boxes); 0 while the rows are not shown
  protected readonly imagesRowHeight = signal(0);
  protected readonly pairsRowHeight = signal(0);

  constructor() {
    super();

    // the CVE dialogs (EXPLOIT tag, details) read the enrichment of the app state, as the vulnerability id cell does
    this.cveEnrichmentState.ensureLoaded();

    // the selection does not outlive the expanded widget
    effect(() => {
      if (!this.isExpanded()) {
        this.selection.set(null);
      }
    });

    // the rows are shown (rendered, with their height): the page may scroll to show the whole widget
    effect(() => {
      if (!this.isExpanded()) {
        this.revealed = false;
      }
      if (!this.showBoxes()) {
        // the rows (and their heights) are made again when they are shown again
        this.imagesRowHeight.set(0);
        this.pairsRowHeight.set(0);
        return;
      }

      const isShown = this.boxes().imageBoxes.length === 0 || (this.imagesRowHeight() > 0 && this.pairsRowHeight() > 0);
      if (isShown && !this.revealed) {
        this.revealed = true;
        this.revealAfterRender();
      }
    });
  }

  // a click on a line of an image box: selects the image and the pair, and goes to the box of the pair; the same line
  // again clears it
  protected selectFromImage(image: ImageBox, row: ImageBoxRow): void {
    if (this.select({ digest: image.digest, pairKey: row.pairKey, day: this.selection()?.day ?? null })) {
      this.scrollToPairBox(row.pairKey);
    }
  }

  // a click on a line of a pair box: selects the pair and the image, and goes to the box of the image
  protected selectFromPair(pair: PairBox, row: PairBoxRow): void {
    if (this.select({ digest: row.digest, pairKey: pair.key, day: this.selection()?.day ?? null })) {
      this.scrollToImageBox(row.digest);
    }
  }

  // a pick (or a clear) in a select changes its side only: the other side stays, and the options of the selects follow
  // (an image picked narrows the pairs offered, cleared it widens them); the box of the pick is scrolled to
  protected onImagePicked(digest: string | null): void {
    const current = this.selection();
    this.selectSide({ digest, pairKey: current?.pairKey ?? null, day: current?.day ?? null });
    if (digest) this.scrollToImageBox(digest);
  }

  protected onPairPicked(pairKey: string | null): void {
    const current = this.selection();
    this.selectSide({ digest: current?.digest ?? null, pairKey, day: current?.day ?? null });
    if (pairKey) this.scrollToPairBox(pairKey);
  }

  // the day filters the boxes (and the other two selects); the selected image and pair stay, as the options of the
  // selects keep them consistent with the day
  protected onDayPicked(day: string | null): void {
    const current = this.selection();
    this.selectSide({ digest: current?.digest ?? null, pairKey: current?.pairKey ?? null, day });
  }

  // a click on a day of the heatmap filters by it, and starts over: the selected image and pair are cleared (the selects
  // still offer them for the day); on the selected day again, it clears the filter
  protected onHeatmapDay(day: string): void {
    this.selectSide({ digest: null, pairKey: null, day: this.selection()?.day === day ? null : day });
  }

  private selectSide(selection: DriftSelection): void {
    this.selection.set(selection.digest || selection.pairKey || selection.day ? selection : null);
  }

  protected isImageRowSelected(image: ImageBox, row: ImageBoxRow): boolean {
    const selection = this.selection();
    return selection?.digest === image.digest && selection.pairKey === row.pairKey;
  }

  protected isPairRowSelected(pair: PairBox, row: PairBoxRow): boolean {
    const selection = this.selection();
    return selection?.pairKey === pair.key && selection.digest === row.digest;
  }

  // "today", "3 days ago" (the same words as the runway of the Base OS widget)
  protected age(day: string): string {
    const ms = parseEolDay(day);
    const today = parseEolDay(this.today());

    return ms === undefined || today === undefined ? '' : eolRelative(ms, today);
  }

  protected severityLabel(severityId: number): string {
    return SeverityUtils.severityDtos.find((x) => x.id === severityId)?.name ?? String(severityId);
  }

  protected shownNamespaces(box: NamespacesFooter): string[] {
    return box.namespaceNames.slice(0, HomeHistoryDriftComponent.shownNamespaces);
  }

  // what the popover of the "+N" pill lists: the namespaces that were sent and are not in the footer, and how many
  // were not sent (the server caps the list)
  protected namespacesTipData(box: NamespacesFooter): { names: string[]; more: number } {
    return {
      names: box.namespaceNames.slice(HomeHistoryDriftComponent.shownNamespaces),
      more: Math.max(0, box.namespaces - box.namespaceNames.length),
    };
  }

  // the details of a CVE (the dialog behind the Details tag of a vulnerability id): only for CVE ids, and only when the
  // CVE details are enabled
  protected hasCveDetails(vulnerabilityId: string): boolean {
    return (
      this.mainAppInitService.backendSettingsDto().isCveDetailsEnabled &&
      vulnerabilityId.trim().toUpperCase().startsWith('CVE-')
    );
  }

  // the dialog behind the EXPLOIT tag (KEV / EUVD data of the CVE)
  protected openExploited(vulnerabilityId: string): void {
    this.vulnerabilityTagDialogsService.open(vulnerabilityId, 'exploited');
  }

  protected openCveDetails(vulnerabilityId: string): void {
    this.vulnerabilityTagDialogsService.open(vulnerabilityId, 'cveDetails');
  }

  // the history of the image, in a new tab (the same way the image dependency tree opens it)
  protected openHistory(digest: string): void {
    const url = this.router.serializeUrl(
      this.router.createUrlTree(['/open-trivy-rep'], { queryParams: { page: 'vulnerability-reports-history', digest } }),
    );
    window.open(url, '_blank');
  }

  // true when the line is selected now, false when it was the selected one (then it is cleared)
  private select(selection: DriftSelection): boolean {
    const current = this.selection();
    if (current?.digest === selection.digest && current.pairKey === selection.pairKey) {
      // the same line again: image and pair are cleared, the day filter stays
      this.selectSide({ digest: null, pairKey: null, day: current.day });
      return false;
    }

    this.selection.set(selection);
    return true;
  }

  // the box scrolls into view inside its row (centered), and the page the least needed to show the widget
  private scrollToImageBox(digest: string): void {
    this.imagesRow()?.scrollToIndex(this.imageBoxes().findIndex((x) => x.digest === digest));
    this.revealAfterRender();
  }

  private scrollToPairBox(pairKey: string): void {
    this.pairsRow()?.scrollToIndex(this.pairBoxes().findIndex((x) => x.key === pairKey));
    this.revealAfterRender();
  }

  // the rows are sorted by day, so the ones of a day are next to each other
  private groupByDay<T extends { day: string }>(rows: T[]): DayGroup<T>[] {
    const groups: DayGroup<T>[] = [];
    for (const row of rows) {
      const last = groups.at(-1);
      if (last?.day === row.day) {
        last.rows.push(row);
      } else {
        groups.push({ day: row.day, rows: [row] });
      }
    }
    return groups;
  }

  private pairKey(vulnerabilityId: string, severityId: number): string {
    return `${vulnerabilityId}|${severityId}`;
  }
}
