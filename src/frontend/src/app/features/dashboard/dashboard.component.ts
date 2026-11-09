import { ChangeDetectionStrategy, Component, computed, effect, inject, signal, WritableSignal } from '@angular/core';
import { Observable } from 'rxjs';

import { HomeService } from '@api/services/home.service';
import { HomeTotalsDto } from '@api/models/home-totals-dto';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { AccessTrackingService } from '@features/history-reports/access-tracking.service';
import { LocalStorageUtils } from '@shared/utils/local-storage.utils';

import { HomeFilterBarComponent } from './home-filter-bar/home-filter-bar.component';
import {
  HomeWindowMode,
  homeDefaultSeverityIds,
  homeSeverityOptions,
  homeWindowOptions,
} from './home-filter-bar/home-filter-bar.types';
import { HomeWidgetPlaceholderComponent } from './home-widget-placeholder/home-widget-placeholder.component';
import { HomeWidgetState } from './home-widget-placeholder/home-widget-placeholder.types';

// the home page: a filter bar and a fixed layout of widgets, each one fed by its own api/home endpoint (each loads and
// fails alone); for now every widget is a placeholder that shows what it will display and a summary of what it got
// window ("since"), used only by the widgets about changes:
// - last visit = the previous day the home page was opened (UTC days, see AccessTrackingService); without one (first
//   visit, or only today), the history window
// - history window = vrHistoryMaxAgeDays from the backend settings
// the filters are kept in local storage (the window as a choice, not as a date)
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [HomeFilterBarComponent, HomeWidgetPlaceholderComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  private static readonly windowModeKey = 'home.windowMode';
  private static readonly severityIdsKey = 'home.severityIds';
  private static readonly namespacesKey = 'home.namespaces';

  private readonly homeService = inject(HomeService);
  private readonly accessTrackingService = inject(AccessTrackingService);
  private readonly mainAppInitService = inject(MainAppInitService);

  // the previous visit day (not today); read once, as reading also records this visit
  private readonly lastVisitUtc: string | undefined = this.getLastVisit();

  protected readonly windowMode = signal<HomeWindowMode>(this.loadWindowMode());
  protected readonly severityIds = signal<number[]>(this.loadSeverityIds());
  protected readonly namespaces = signal<string[]>(this.loadNamespaces());
  // the namespaces with data, plus the selected ones that have none now (kept visible, so they can be removed;
  // they are not dropped silently, as right after a backend start the data may be partial)
  private readonly namespacesWithData = signal<string[]>([]);
  protected readonly namespaceOptions = computed(() =>
    [...new Set([...this.namespacesWithData(), ...this.namespaces()])].sort(),
  );

  // bumped by the refresh button, so the data reloads with unchanged filters
  private readonly refreshCounter = signal(0);

  protected readonly since = computed(() => this.resolveSince(this.windowMode()));
  protected readonly sinceLabel = computed(() => `${this.since().substring(0, 10)} (UTC)`);

  // the scope shown on each widget, so a widget read alone says what it counts
  private readonly severitiesLabel = computed(() => this.toSeveritiesLabel(this.severityIds()));
  private readonly namespacesLabel = computed(() => {
    const count = this.namespaces().length;
    return count === 0 ? '' : ` · ${count} namespace${count === 1 ? '' : 's'}`;
  });
  protected readonly changesHint = computed(
    () => `since ${this.sinceLabel()} · ${this.severitiesLabel()}${this.namespacesLabel()}`,
  );
  protected readonly knownExploitedHint = computed(
    () => `now, new since ${this.sinceLabel()} · all severities${this.namespacesLabel()}`,
  );
  protected readonly stateHint = computed(() => `now · ${this.severitiesLabel()}${this.namespacesLabel()}`);
  protected readonly baseOsHint = computed(() => `now${this.namespacesLabel()}`);
  protected readonly activityHint = computed(
    () => `per UTC day · ${this.severitiesLabel()}${this.namespacesLabel()}`,
  );
  protected readonly oldestImagesHint = computed(
    () => `based on history · ${this.severitiesLabel()}${this.namespacesLabel()}`,
  );

  protected readonly dataHealth = signal<HomeWidgetState>({});
  protected readonly newVulnerabilities = signal<HomeWidgetState>({});
  protected readonly newFixes = signal<HomeWidgetState>({});
  protected readonly knownExploited = signal<HomeWidgetState>({});
  protected readonly activity = signal<HomeWidgetState>({});
  protected readonly baseOs = signal<HomeWidgetState>({});
  protected readonly fixWaiting = signal<HomeWidgetState>({});
  protected readonly failingChecks = signal<HomeWidgetState>({});
  protected readonly oldestImages = signal<HomeWidgetState>({});
  protected readonly packageHotspots = signal<HomeWidgetState>({});
  protected readonly reachEpss = signal<HomeWidgetState>({});

  constructor() {
    this.homeService.getHomeNamespaces().subscribe({
      next: (namespaces) => this.namespacesWithData.set(namespaces),
      error: () => this.namespacesWithData.set([]),
    });

    // the filters are saved and the data reloaded on any change (and once at start)
    effect(() => {
      const windowMode = this.windowMode();
      const severityIds = this.severityIds();
      const namespaces = this.namespaces();
      this.refreshCounter();

      LocalStorageUtils.setItem(DashboardComponent.windowModeKey, windowMode);
      LocalStorageUtils.setItem(DashboardComponent.severityIdsKey, JSON.stringify(severityIds));
      LocalStorageUtils.setItem(DashboardComponent.namespacesKey, JSON.stringify(namespaces));

      this.loadData(this.since(), severityIds, namespaces);
    });
  }

  protected onRefresh(): void {
    this.refreshCounter.update((x) => x + 1);
  }

  private loadData(since: string, severities: number[], namespaces: string[]): void {
    // empty arrays are not sent (the backend defaults apply)
    const scope = { severities, namespaces: namespaces.length > 0 ? namespaces : undefined };

    this.load(this.dataHealth, this.homeService.getHomeDataHealth(), (x) => ({
      isAvailable: true,
      summary:
        `${x.watchersOk}/${x.watchersTotal} watchers ok, ${x.reports.length} report kinds, ` +
        `${x.sources.filter((s) => s.isEnabled).length} sources enabled, ` +
        `history since ${this.toDay(x.historyStartedAt)}, fix tracking since ${this.toDay(x.fixTrackingStartedAt)}`,
    }));

    this.load(this.newVulnerabilities, this.homeService.getHomeNewVulnerabilities({ since, ...scope }), (x) => ({
      isAvailable: x.isAvailable,
      unavailableReason: x.unavailableReason,
      summary: `${x.items.length} items; ${this.toTotals(x.totals)}`,
    }));

    this.load(this.newFixes, this.homeService.getHomeNewFixes({ since, ...scope }), (x) => ({
      isAvailable: x.isAvailable,
      unavailableReason: x.unavailableReason,
      summary: `${x.items.length} items; ${this.toTotals(x.totals)}`,
    }));

    this.load(
      this.knownExploited,
      this.homeService.getHomeKnownExploited({ since, namespaces: scope.namespaces }),
      (x) => ({
        isAvailable: x.isAvailable,
        unavailableReason: x.unavailableReason,
        summary: `${x.items.length} items (${x.newSinceCount} new since ${this.toDay(since)}); ${this.toTotals(x.totals)}`,
      }),
    );

    this.load(this.activity, this.homeService.getHomeActivity({ ...scope }), (x) => ({
      isAvailable: x.isAvailable,
      unavailableReason: x.unavailableReason,
      summary:
        `${x.days.length} days; first seen ${this.sum(x.days.map((d) => d.firstSeen))}, ` +
        `fix appeared ${this.sum(x.days.map((d) => d.fixAppeared))}, ` +
        `became exploited ${this.sum(x.days.map((d) => d.becameExploited))}`,
    }));

    this.load(this.baseOs, this.homeService.getHomeBaseOs({ namespaces: scope.namespaces }), (x) => ({
      isAvailable: x.isAvailable,
      unavailableReason: x.unavailableReason,
      summary:
        `${x.items.length} OS cycles; ${x.items.filter((i) => i.isEol).length} end of life, ` +
        `${x.items.filter((i) => i.status === 'Pending').length} pending`,
    }));

    this.load(this.fixWaiting, this.homeService.getHomeFixWaiting({ ...scope }), (x) => ({
      isAvailable: x.isAvailable,
      unavailableReason: x.unavailableReason,
      summary: `${x.items.length} items; ${this.toTotals(x.totals)}`,
    }));

    this.load(this.failingChecks, this.homeService.getHomeFailingChecks({ ...scope }), (x) => ({
      isAvailable: x.isAvailable,
      unavailableReason: x.unavailableReason,
      summary: `${x.items.length} of ${x.totalChecks} checks`,
    }));

    this.load(this.oldestImages, this.homeService.getHomeOldestImages({ ...scope }), (x) => ({
      isAvailable: x.isAvailable,
      unavailableReason: x.unavailableReason,
      summary: `${x.items.length} images; history since ${this.toDay(x.historyStartedAt)}`,
    }));

    this.load(this.packageHotspots, this.homeService.getHomePackageHotspots({ ...scope }), (x) => ({
      isAvailable: x.isAvailable,
      unavailableReason: x.unavailableReason,
      summary: `${x.items.length} packages`,
    }));

    this.load(this.reachEpss, this.homeService.getHomeReachEpss({ ...scope }), (x) => ({
      isAvailable: x.isAvailable,
      unavailableReason: x.unavailableReason,
      summary: `${x.items.length} vulnerability ids`,
    }));
  }

  private load<T>(target: WritableSignal<HomeWidgetState>, source: Observable<T>, toState: (dto: T) => HomeWidgetState) {
    target.set({});
    source.subscribe({
      next: (dto) => target.set(toState(dto)),
      error: (err) => target.set({ error: err?.message ?? 'request failed' }),
    });
  }

  // ------------------------------------------------------------------------
  // window and stored filters
  // ------------------------------------------------------------------------

  private getLastVisit(): string | undefined {
    const trackedDays = this.accessTrackingService.trackAccess('home');
    const todayUtc = new Date().toISOString().substring(0, 10);

    // trackAccess keeps one entry per day with today first, so [1] is an earlier day; checked anyway
    return trackedDays[1]?.day === todayUtc ? undefined : trackedDays[1]?.utc;
  }

  private resolveSince(windowMode: HomeWindowMode): string {
    switch (windowMode) {
      case 'lastVisit':
        return this.lastVisitUtc ?? this.daysAgo(this.getHistoryMaxAgeDays());
      case '24h':
        return this.daysAgo(1);
      case '7d':
        return this.daysAgo(7);
      case 'history':
        return this.daysAgo(this.getHistoryMaxAgeDays());
    }
  }

  private getHistoryMaxAgeDays(): number {
    return this.mainAppInitService.backendSettingsDto().vrHistoryMaxAgeDays ?? 14;
  }

  private daysAgo(days: number): string {
    return new Date(Date.now() - days * 86_400_000).toISOString();
  }

  private loadWindowMode(): HomeWindowMode {
    const stored = LocalStorageUtils.getItem(DashboardComponent.windowModeKey);
    return homeWindowOptions.some((x) => x.value === stored) ? (stored as HomeWindowMode) : 'lastVisit';
  }

  private loadSeverityIds(): number[] {
    const stored = this.parseArray(LocalStorageUtils.getItem(DashboardComponent.severityIdsKey));
    const valid = stored.filter((x): x is number => homeSeverityOptions.some((o) => o.value === x));
    return valid.length > 0 ? valid : [...homeDefaultSeverityIds];
  }

  private loadNamespaces(): string[] {
    const stored = this.parseArray(LocalStorageUtils.getItem(DashboardComponent.namespacesKey));
    return stored.filter((x): x is string => typeof x === 'string');
  }

  private parseArray(value: string | null): unknown[] {
    if (!value) {
      return [];
    }
    try {
      const parsed = JSON.parse(value);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  // ------------------------------------------------------------------------
  // labels
  // ------------------------------------------------------------------------

  private toSeveritiesLabel(severityIds: number[]): string {
    if (severityIds.length === homeSeverityOptions.length) {
      return 'all severities';
    }
    return severityIds
      .map((id) => homeSeverityOptions.find((o) => o.value === id)?.label.toLowerCase() ?? `${id}`)
      .join('/');
  }

  private toTotals(totals: HomeTotalsDto): string {
    return (
      `${totals.vulnerabilityIds} ids, ${totals.activeVulnerabilities} (id, package), ` +
      `${totals.images} images, ${totals.namespaces} namespaces, ${totals.workloads} workloads`
    );
  }

  private toDay(value?: string | null): string {
    return value ? value.substring(0, 10) : 'n/a';
  }

  private sum(values: number[]): number {
    return values.reduce((a, b) => a + b, 0);
  }
}
