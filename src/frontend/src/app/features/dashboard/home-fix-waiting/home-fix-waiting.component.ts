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

import { HomeActiveVulnerabilityDto } from '@api/models/home-active-vulnerability-dto';
import { HomeService } from '@api/services/home.service';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { CveEnrichmentStateService } from '@core/state/cve-enrichment-state.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { MeterGroupComponent } from '@shared/ui/meter-group/meter-group.component';
import { MeterGroupSegment } from '@shared/ui/meter-group/meter-group.types';
import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { SpanTimelineComponent } from '@shared/ui/span-timeline/span-timeline.component';
import { SpanTimelineRow, SpanTimelineTick } from '@shared/ui/span-timeline/span-timeline.types';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { VulnerabilityTagSource } from '@shared/ui/trivy-table/trivy-table.types';
import { VulnerabilityTagsPipe } from '@shared/ui/trivy-table/vulnerability-tags.pipe';
import { VulnerabilityScoreTagComponent } from '@shared/ui/vulnerability-score-tag/vulnerability-score-tag.component';
import { TagModule } from 'primeng/tag';

import { HomeExploitTagComponent } from '../home-pivot/home-exploit-tag.component';
import { matchesFilter } from '../home-search-box/home-list-filter';
import { HomeSearchBoxComponent } from '../home-search-box/home-search-box.component';
import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

type GroupBy = 'id' | 'package' | 'image';
type SortBy = 'wait' | 'reach';

// one entry of the list: an id, a package or an image, with the rows (id, package) it is made of
interface WaitGroup {
  // unique among the entries, the id of the timeline row: 'id:cve-2024-1', 'package:openssl', 'image:nginx:1.25'
  key: string;
  name: string;
  // the longest wait of its rows in whole days (from the oldest fix date), null when no row has a date
  days: number | null;
  oldestFixSince: string | undefined;
  ids: Set<string>;
  packages: Set<string>;
  images: Set<string>;
  namespaces: Set<string>;
  rows: HomeActiveVulnerabilityDto[];
}

// the waiting buckets of the meter and the colors of the bars, in days: [from, to)
interface WaitBucket {
  label: string;
  from: number;
  to: number;
  color: string;
  textColor: string;
}

const BUCKETS: WaitBucket[] = [
  {
    label: '< 7d',
    from: 0,
    to: 7,
    color: 'var(--tod-fix-waiting-age-1)',
    textColor: 'var(--tod-fix-waiting-age-text-1)',
  },
  {
    label: '7–30d',
    from: 7,
    to: 30,
    color: 'var(--tod-fix-waiting-age-2)',
    textColor: 'var(--tod-fix-waiting-age-text-2)',
  },
  {
    label: '30–90d',
    from: 30,
    to: 90,
    color: 'var(--tod-fix-waiting-age-3)',
    textColor: 'var(--tod-fix-waiting-age-text-3)',
  },
  {
    label: '≥ 90d',
    from: 90,
    to: Infinity,
    color: 'var(--tod-fix-waiting-age-4)',
    textColor: 'var(--tod-fix-waiting-age-text-4)',
  },
];

// the room of a character of a row label of the timeline (px, a rough size), to cut a label that does not fit
const LABEL_CHAR_WIDTH = 7;
const LABEL_PADDING = 16;

// Fix waiting time: the fixable active (vulnerability id, package) rows that run now, by how long their fix is known
// - the filters of the page (severities, namespaces) come as inputs; the window does not apply (it is the state now); a change
//   of them, or of refresh, reloads it
// - one request, the rows (up to the server limit) as the server sorts them (the longest known fix first); everything else is
//   computed here
// - collapsed: the longest wait and the ids, a meter of the ids by waiting bucket, and the first ids as bars (a click on one
//   expands the widget, with that id selected)
// - expanded: all the entries as bars on a timeline (fix known since -> today), grouped by id, package or image and sorted by
//   wait or reach; the popover of a bar has its details
@Component({
  selector: 'app-home-fix-waiting',
  standalone: true,
  imports: [
    HomeWidgetFrameComponent,
    HomeSearchBoxComponent,
    NgTemplateOutlet,
    MeterGroupComponent,
    PillButtonComponent,
    SpanTimelineComponent,
    TagModule,
    TrivyBoxComponent,
    HomeExploitTagComponent,
    VulnerabilityScoreTagComponent,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
  ],
  templateUrl: './home-fix-waiting.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeFixWaitingComponent extends HomeWidgetBase {
  // the server allows 500; the collapsed widget needs all the rows too (the meter and the longest wait are of all of them)
  private static readonly fetchLimit = 500;
  private static readonly collapsedLimit = 3;

  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help = `
    <p class="mb-2">The <strong>active vulnerabilities that have a fix</strong> and run now, in the <strong>severities</strong> and <strong>namespaces</strong> of the filter bar, by <strong>how long the fix is known</strong>.</p>
    <p class="mb-2"><strong>Collapsed</strong>: the longest wait, the ids in waiting buckets (an id is in the bucket of its longest waiting package; the empty part of the meter is the ids without a date), and the 3 ids that wait the longest. A click on one expands the widget.</p>
    <p class="mb-2"><strong>Expanded</strong>: every id (up to 500 rows) as a bar from the day its fix is known to today, on a log scale. <strong>Group</strong> by id, package or image (the bar is the longest wait in the group) and <strong>sort</strong> by wait or by reach (images). The popover of a bar has its packages, images and namespaces.</p>
    <p class="mb-0 tod-text-muted-color"><strong>Fix known since</strong> is a lower bound of the waiting time (hence the ≥): a row without history has no date and goes last.</p>
  `;

  // the expanded widget body is at least this tall, and grows with its row of the page; the timeline takes the rest of
  // the height (timelineHeight, measured) and its rows scroll in it
  protected readonly expandedMinHeight = 'min(30rem, 70vh)';
  protected readonly expandedMaxHeight = '70vh';
  private readonly timelineHost = viewChild<ElementRef<HTMLElement>>('timelineHost');
  protected readonly timelineHeight = signal<number | null>(null);
  protected readonly buckets = BUCKETS;
  protected readonly timelineTicks: SpanTimelineTick[] = [7, 30, 90, 180, 365, 730].map((value) => ({ value }));

  private readonly homeService = inject(HomeService);
  private readonly mainAppInitService = inject(MainAppInitService);
  private readonly cveEnrichmentState = inject(CveEnrichmentStateService);
  // the tags of the vulnerability id cell of the tables: here the labels of its EPSS and exploited ones
  private readonly vulnerabilityTags = new VulnerabilityTagsPipe();

  hint = input<string>('');
  severities = input.required<number[]>();
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  // a new object on any change, so the resource reloads; empty namespaces are not sent (the backend default applies)
  private readonly scope = computed(() => ({
    severities: this.severities(),
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  private readonly data = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeFixWaiting({
        severities: params.severities,
        namespaces: params.namespaces,
        limit: HomeFixWaitingComponent.fetchLimit,
      }),
  });

  private readonly todayMs = Date.now();

  protected readonly allItems = computed(() => this.data.value()?.items ?? []);
  protected readonly totals = computed(() => this.data.value()?.totals);
  // the server sends at most fetchLimit rows: what is computed here is of those
  protected readonly partialLabel = computed(() => {
    const total = this.totals()?.activeVulnerabilities ?? 0;
    const shown = this.allItems().length;
    return total > shown ? `Computed on the first ${shown} of ${total} rows` : '';
  });

  // ---- the collapsed widget: the ids (all the rows), the longest wait first
  private readonly idGroups = computed(() => this.sortGroups(this.toGroups(this.allItems(), 'id'), 'wait'));
  protected readonly idCount = computed(() => this.idGroups().length);
  protected readonly topGroups = computed(() => this.idGroups().slice(0, HomeFixWaitingComponent.collapsedLimit));
  protected readonly longestDays = computed(() => this.idGroups()[0]?.days ?? null);
  protected readonly undatedIds = computed(() => this.idGroups().filter((x) => x.days === null).length);

  // the ids by bucket; the meter is of all the ids, so the ids without a date are its empty part
  protected readonly mix = computed<MeterGroupSegment[]>(() =>
    BUCKETS.map((b) => ({
      label: `fix known ${b.label}`,
      value: this.idGroups().filter((x) => x.days !== null && x.days >= b.from && x.days < b.to).length,
      color: b.color,
      textColor: b.textColor,
    })),
  );

  // ---- the expanded widget: the filter box, the grouping and the sort
  protected readonly filterText = signal('');
  protected readonly groupBy = signal<GroupBy>('id');
  protected readonly sortBy = signal<SortBy>('wait');
  // the selected row of the timeline (its key); set from a click on an id of the collapsed widget
  protected readonly selectedKey = signal<string | null>(null);

  private readonly items = computed(() =>
    this.allItems().filter((x) => matchesFilter(this.filterText(), x.vulnerabilityId, x.packageName)),
  );
  protected readonly noMatch = computed(
    () => this.filterText().trim().length > 0 && this.items().length === 0 && this.allItems().length > 0,
  );

  protected readonly groups = computed(() =>
    this.sortGroups(this.toGroups(this.items(), this.groupBy()), this.sortBy()),
  );

  // the labels are cut to the room the grouping gives them (an image name is long: its end, the tag, is kept)
  protected readonly labelWidth = computed(() => ({ id: 520, package: 420, image: 660 })[this.groupBy()]);

  protected readonly timelineRows = computed<SpanTimelineRow[]>(() => {
    const maxChars = Math.floor((this.labelWidth() - LABEL_PADDING) / LABEL_CHAR_WIDTH);
    return this.groups().map((g) => ({
      id: g.key,
      label: this.cut(this.rowLabel(g), maxChars, this.groupBy() === 'image'),
      start: g.days ?? 0,
      // without a date there is no bar to draw (a bar at "today" would read as a wait of 0 days)
      color: g.days === null ? 'transparent' : this.bucketOf(g.days).color,
      valueLabel: this.daysLabel(g.days),
      details: this.details(g),
    }));
  });

  // ---- the selected box: the selected entry; for an id also its detail (the installed versions and the workloads)
  protected readonly selectedGroup = computed(() => this.groups().find((g) => g.key === this.selectedKey()));
  protected readonly selectedKind = computed<GroupBy | undefined>(
    () => this.selectedGroup()?.key.split(':')[0] as GroupBy | undefined,
  );

  // a string, so a new list (a filter typed) does not ask for the same id again
  private readonly selectedDetailId = computed(() => {
    const group = this.selectedGroup();
    return this.isExpanded() && group?.key.startsWith('id:') ? group.name : undefined;
  });

  private readonly detailData = rxResource({
    params: () => {
      const id = this.selectedDetailId();
      return id ? { ...this.scope(), id } : undefined;
    },
    stream: ({ params }) =>
      this.homeService.getHomeFixWaitingDetailed({
        vulnerabilityId: params.id,
        severities: params.severities,
        namespaces: params.namespaces,
      }),
  });

  // the detail of the selected id, once it is the one that arrived (until then the box shows what the rows say)
  protected readonly selectedDetail = computed(() => {
    const item = this.detailData.value()?.item;
    const group = this.selectedGroup();
    return item && group && item.vulnerabilityId.toLowerCase() === group.name.toLowerCase() ? item : undefined;
  });

  // the images and the namespaces of the selected entry, sorted (those of its rows: the same as the detail of an id)
  protected readonly selectedImages = computed(() => [...(this.selectedGroup()?.images ?? [])].sort());
  protected readonly selectedNamespaces = computed(() => [...(this.selectedGroup()?.namespaces ?? [])].sort());

  // the packages of the selected entry: "glibc 2.35-0ubuntu3.6 → 3.7" (the installed versions only from the detail)
  protected readonly selectedFixes = computed(() => {
    const detail = this.selectedDetail();
    if (detail) {
      return detail.packages.map((p) => ({
        name: p.packageName,
        installed: p.installedVersions.join(', '),
        fixed: p.fixedVersion,
      }));
    }
    const rows = this.selectedGroup()?.rows ?? [];
    const seen = new Set<string>();
    return rows
      .filter((x) => !seen.has(x.packageKey) && !!seen.add(x.packageKey))
      .map((x) => ({ name: x.packageName, installed: '', fixed: x.fixedVersion ?? '?' }));
  });

  protected readonly state = computed<HomeWidgetState>(() => {
    if (this.data.error()) {
      return { error: (this.data.error() as Error)?.message ?? 'request failed' };
    }

    const dto = this.data.value();
    if (!dto) {
      return {};
    }

    const totals = dto.totals;
    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary:
        `${totals.vulnerabilityIds} ids, ${totals.activeVulnerabilities} rows (${this.allItems().length} read), ` +
        `${totals.images} images, ${totals.namespaces} ns`,
    };
  });

  constructor() {
    super();

    this.cveEnrichmentState.ensureLoaded();

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

    // expanded with nothing selected (the expand button): the first entry
    effect(() => {
      if (this.isExpanded()) {
        untracked(() => {
          if (!this.selectedKey()) {
            this.selectedKey.set(this.groups()[0]?.key ?? null);
          }
        });
      }
    });
  }

  // another grouping: its first entry is selected (the keys of the old one are not in it)
  protected setGroupBy(by: GroupBy): void {
    if (by !== this.groupBy()) {
      this.groupBy.set(by);
      this.selectedKey.set(this.groups()[0]?.key ?? null);
    }
  }

  // a click on an id of the collapsed widget: the timeline of the ids, with that one selected
  protected select(group: WaitGroup): void {
    this.groupBy.set('id');
    this.selectedKey.set(group.key);
    if (this.canExpand() && !this.isExpanded()) {
      this.toggleExpand();
    }
  }

  // the highest severity of the rows of an entry
  protected worstSeverity(g: WaitGroup): number {
    return Math.min(...g.rows.map((x) => x.severityId));
  }

  protected isKnownExploited(g: WaitGroup): boolean {
    return g.rows.some((x) => x.isKnownExploited) || !!this.tagLabel(g.name, 'exploited');
  }

  // the details of a CVE (the dialog behind the CVSS tag of a vulnerability id): only for CVE ids, and only when the
  // CVE details are enabled
  protected hasCveDetails(vulnerabilityId: string): boolean {
    return (
      this.mainAppInitService.backendSettingsDto().isCveDetailsEnabled &&
      vulnerabilityId.trim().toUpperCase().startsWith('CVE-')
    );
  }

  // the label of the tag of the tables for the id (the same pipe), or '': 'EPSS 46%'; 'KEV', 'EUVD' or 'KEV/EUVD'
  protected tagLabel(vulnerabilityId: string, source: VulnerabilityTagSource): string {
    return (
      this.vulnerabilityTags
        .transform(vulnerabilityId, this.cveEnrichmentState.byId(), false)
        .find((x) => x.source === source)?.label ?? ''
    );
  }

  // "≥ 94d", "no date"
  protected daysLabel(days: number | null): string {
    return days === null ? 'no date' : `≥ ${days}d`;
  }

  // the share of the bar of a collapsed row (linear, the longest is the whole track)
  protected barPercent(days: number | null): number {
    const longest = this.longestDays();
    return days === null || !longest ? 0 : Math.max(2, (days / longest) * 100);
  }

  protected bucketColor(days: number | null): string {
    return days === null ? 'transparent' : this.bucketOf(days).color;
  }

  // "CVE-2024-2961 · glibc", "CVE-2024-2961 · glibc +2", "openssl · 9 ids", "nginx:1.25 · 3 ids"
  protected rowLabel(g: WaitGroup): string {
    if (g.key.startsWith('id:')) {
      const packages = [...g.packages];
      return `${g.name} · ${packages[0] ?? ''}${packages.length > 1 ? ` +${packages.length - 1}` : ''}`;
    }
    return `${g.name} · ${g.ids.size} ${g.ids.size === 1 ? 'id' : 'ids'}`;
  }

  private toGroups(rows: HomeActiveVulnerabilityDto[], by: GroupBy): WaitGroup[] {
    const byKey = new Map<string, WaitGroup>();
    for (const row of rows) {
      const names = by === 'id' ? [row.vulnerabilityId] : by === 'package' ? [row.packageName] : row.imageNames;
      for (const name of names) {
        const key = `${by}:${name.toLowerCase()}`;
        let group = byKey.get(key);
        if (!group) {
          group = {
            key,
            name,
            days: null,
            oldestFixSince: undefined,
            ids: new Set(),
            packages: new Set(),
            images: new Set(),
            namespaces: new Set(),
            rows: [],
          };
          byKey.set(key, group);
        }

        group.rows.push(row);
        group.ids.add(row.vulnerabilityId);
        group.packages.add(row.packageName);
        row.imageNames.forEach((x) => group.images.add(x));
        row.namespaceNames.forEach((x) => group.namespaces.add(x));
        if (row.fixSince && (!group.oldestFixSince || row.fixSince < group.oldestFixSince)) {
          group.oldestFixSince = row.fixSince;
        }
      }
    }

    const groups = [...byKey.values()];
    groups.forEach((g) => (g.days = this.daysSince(g.oldestFixSince)));
    return groups;
  }

  // wait: the longest first, the ones without a date last, then by reach; reach: the most images first, then by wait
  private sortGroups(groups: WaitGroup[], by: SortBy): WaitGroup[] {
    const wait = (a: WaitGroup, b: WaitGroup) => (b.days ?? -1) - (a.days ?? -1);
    const reach = (a: WaitGroup, b: WaitGroup) => b.images.size - a.images.size;
    return [...groups].sort((a, b) => (by === 'wait' ? wait(a, b) || reach(a, b) : reach(a, b) || wait(a, b)));
  }

  // the lines of the popover of a bar
  private details(g: WaitGroup): string[] {
    const lines: string[] = [];
    if (g.key.startsWith('id:')) {
      // a package with the version that fixes it (the same package can be in several rows: one line each)
      const fixes = [...new Set(g.rows.map((x) => `${x.packageName} → ${x.fixedVersion ?? '?'}`))];
      lines.push(...fixes.slice(0, 5));
      if (fixes.length > 5) {
        lines.push(`+${fixes.length - 5} more packages`);
      }
    } else {
      lines.push(
        `${g.ids.size} ${g.ids.size === 1 ? 'id' : 'ids'}, ${g.packages.size} ${g.packages.size === 1 ? 'package' : 'packages'}`,
      );
    }
    if (g.key.startsWith('image:')) {
      lines.push(g.name);
    }
    lines.push(`${g.images.size} ${g.images.size === 1 ? 'image' : 'images'} · ${g.namespaces.size} ns`);
    lines.push(g.oldestFixSince ? `fix known since ${g.oldestFixSince.substring(0, 10)}` : 'fix known: no date');
    return lines;
  }

  private bucketOf(days: number): WaitBucket {
    return BUCKETS.find((b) => days >= b.from && days < b.to) ?? BUCKETS[BUCKETS.length - 1];
  }

  private daysSince(value: string | undefined): number | null {
    return value ? Math.max(0, Math.floor((this.todayMs - new Date(value).getTime()) / 86_400_000)) : null;
  }

  // a label that does not fit: its start ("…" at the end), or its end (for an image, whose tag is at the end)
  private cut(text: string, maxChars: number, keepEnd: boolean): string {
    if (text.length <= maxChars) {
      return text;
    }
    return keepEnd ? `…${text.slice(text.length - maxChars + 1)}` : `${text.slice(0, maxChars - 1)}…`;
  }
}
