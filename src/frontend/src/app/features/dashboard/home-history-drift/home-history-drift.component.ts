import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, computed, effect, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';

import { HomeHistoryDriftGroupDto } from '@api/models/home-history-drift-group-dto';
import { HomeService } from '@api/services/home.service';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '@shared/pipes/vulnerability-count.pipe';
import { ActivityHeatmapComponent } from '@shared/ui/activity-heatmap/activity-heatmap.component';
import { ScrollerComponent } from '@shared/ui/scroller/scroller.component';
import { eolRelative, parseEolDay } from '@shared/ui/eol-runway/eol-runway.state';
import { PopoverService } from '@shared/ui/popover/popover.service';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
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
interface DriftSelection {
  digest: string | null;
  pairKey: string | null;
}

// an entry of the selects: an image (value = digest) or a pair (value = pair key), with its exploited flag
interface DriftOption {
  value: string;
  label: string;
  // the digest of an image, to search by it
  digest?: string;
  // the severity of a pair
  severityId?: number;
  isKnownExploited: boolean;
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
    TagModule,
    ScrollerComponent,
    SeverityCssStyleByIdPipe,
    VulnerabilityCountPipe,
  ],
  templateUrl: './home-history-drift.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeHistoryDriftComponent extends HomeWidgetBase {
  // the boxes of a row; the others are counted
  private static readonly maxBoxes = 50;
  // the namespaces named in the footer of a box; the pill counts the others
  private static readonly shownNamespaces = 2;

  private readonly homeService = inject(HomeService);
  private readonly router = inject(Router);
  private readonly mainAppInitService = inject(MainAppInitService);
  private readonly cveEnrichmentState = inject(CveEnrichmentStateService);
  private readonly vulnerabilityTagDialogsService = inject(VulnerabilityTagDialogsService);
  private readonly hostElement = inject<ElementRef<HTMLElement>>(ElementRef);
  protected readonly popover = inject(PopoverService);

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
  protected readonly isLoadingBoxes = computed(
    () => this.isExpanded() && this.daysData.value()?.isAvailable === true && !this.rowsData.hasValue(),
  );

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

    for (const item of dto?.items ?? []) {
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

  protected readonly imageBoxes = computed(() => this.boxes().imageBoxes.slice(0, HomeHistoryDriftComponent.maxBoxes));
  protected readonly moreImageBoxes = computed(() => Math.max(0, this.boxes().imageBoxes.length - HomeHistoryDriftComponent.maxBoxes));
  protected readonly pairBoxes = computed(() => this.boxes().pairBoxes.slice(0, HomeHistoryDriftComponent.maxBoxes));
  protected readonly morePairBoxes = computed(() => Math.max(0, this.boxes().pairBoxes.length - HomeHistoryDriftComponent.maxBoxes));

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

  // the selects: every image, and every pair (id + severity); exploited when it has at least one pair that is
  protected readonly imageOptions = computed<DriftOption[]>(() =>
    this.boxes()
      .imageBoxes.map((box) => ({
        value: box.digest,
        label: box.name,
        digest: box.digest,
        isKnownExploited: box.rows.some((row) => row.isKnownExploited),
      }))
      .sort((a, b) => a.label.localeCompare(b.label) || a.value.localeCompare(b.value)),
  );
  protected readonly pairOptions = computed<DriftOption[]>(() =>
    this.boxes()
      .pairBoxes.map((box) => ({
        value: box.key,
        label: box.vulnerabilityId,
        severityId: box.severityId,
        isKnownExploited: box.isKnownExploited,
      }))
      .sort((a, b) => a.label.localeCompare(b.label) || (a.severityId ?? 0) - (b.severityId ?? 0)),
  );

  // set when the rows of this expand were revealed; reset on collapse (so a reload by a filter does not scroll)
  private revealed = false;

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

    // the rows arrived: after they are rendered the page may scroll to show the whole widget
    effect(() => {
      if (!this.isExpanded()) {
        this.revealed = false;
      } else if (this.rowsData.hasValue() && !this.revealed) {
        this.revealed = true;
        this.revealAfterRender();
      }
    });
  }

  // a click on a line of an image box: selects the image and the pair, and goes to the box of the pair; the same line
  // again clears it
  protected selectFromImage(image: ImageBox, row: ImageBoxRow): void {
    this.select({ digest: image.digest, pairKey: row.pairKey }, 'p:' + row.pairKey);
  }

  // a click on a line of a pair box: selects the pair and the image, and goes to the box of the image
  protected selectFromPair(pair: PairBox, row: PairBoxRow): void {
    this.select({ digest: row.digest, pairKey: pair.key }, 'i:' + row.digest);
  }

  // a pick in a select: that side only, and its box is scrolled to (a box beyond the shown ones is not there)
  protected onImagePicked(digest: string | null): void {
    this.selection.set(digest ? { digest, pairKey: null } : null);
    if (digest) this.scrollToBox('i:' + digest);
  }

  protected onPairPicked(pairKey: string | null): void {
    this.selection.set(pairKey ? { digest: null, pairKey } : null);
    if (pairKey) this.scrollToBox('p:' + pairKey);
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

  private select(selection: DriftSelection, boxToShow: string): void {
    const current = this.selection();
    if (current?.digest === selection.digest && current.pairKey === selection.pairKey) {
      this.selection.set(null);
      return;
    }

    this.selection.set(selection);
    this.scrollToBox(boxToShow);
  }

  // the box scrolls into view inside its row (centered), and the page the least needed (as a widget that expands);
  // a box beyond the shown ones is not there
  private scrollToBox(id: string): void {
    this.hostElement.nativeElement
      .querySelector(`[data-box="${CSS.escape(id)}"]`)
      ?.scrollIntoView({ block: 'nearest', inline: 'center', behavior: 'smooth' });
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
