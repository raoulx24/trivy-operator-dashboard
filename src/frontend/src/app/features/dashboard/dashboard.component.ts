import { ChangeDetectionStrategy, Component, computed, effect, inject, signal, viewChild, WritableSignal } from '@angular/core';
import { Observable } from 'rxjs';

import { HomeHistoryDriftDto } from '@api/models/home-history-drift-dto';
import { HomeNewImagesDto } from '@api/models/home-new-images-dto';
import { HomeService } from '@api/services/home.service';
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
import { HomeBaseOsComponent } from './home-base-os/home-base-os.component';
import { HomeExposedSecretsComponent } from './home-exposed-secrets/home-exposed-secrets.component';
import { HomeComplianceComponent } from './home-compliance/home-compliance.component';
import { HomeFailingChecksComponent } from './home-failing-checks/home-failing-checks.component';
import { HomeFixWaitingComponent } from './home-fix-waiting/home-fix-waiting.component';
import { HomeHistoryDriftComponent } from './home-history-drift/home-history-drift.component';
import { HomeKnownExploitedComponent } from './home-known-exploited/home-known-exploited.component';
import { HomeMotionService } from './home-motion/home-motion.service';
import { HomeNewImagesComponent } from './home-new-images/home-new-images.component';
import { HomeOldestImagesComponent } from './home-oldest-images/home-oldest-images.component';
import { HomeNewFixesComponent } from './home-new-fixes/home-new-fixes.component';
import { HomeNewVulnerabilitiesComponent } from './home-new-vulnerabilities/home-new-vulnerabilities.component';
import { HomeReachEpssComponent } from './home-reach-epss/home-reach-epss.component';
import { HomePackageHotspotsComponent } from './home-package-hotspots/home-package-hotspots.component';
import { HomeSinceLastVisitComponent, SinceLastVisitClick } from './home-since-last-visit/home-since-last-visit.component';
import { HomeWhereIsLicenseComponent } from './home-where-is-license/home-where-is-license.component';
import { HomeWhereIsCheckComponent } from './home-where-is-check/home-where-is-check.component';
import { HomeWhereIsPackageComponent } from './home-where-is-package/home-where-is-package.component';
import { HomeWhereIsVulnerabilityComponent } from './home-where-is-vulnerability/home-where-is-vulnerability.component';
import { HomeWidgetExpansionService } from './home-widget-group/home-widget-expansion.service';
import { HomeWidgetGroupDirective } from './home-widget-group/home-widget-group.directive';
import { HomeWidgetPlaceholderComponent } from './home-widget-placeholder/home-widget-placeholder.component';
import { HomeDaysResult, HomeWidgetState } from './home-widget-placeholder/home-widget-placeholder.types';

// the home page: a filter bar and a fixed layout of widgets, each one fed by its own api/home endpoint (each loads and
// fails alone); for now every widget is a placeholder that shows what it will display and a summary of what it got
// window ("since"), used only by the widgets about changes:
// - previous visit = the previous day the home page was opened (UTC days, see AccessTrackingService), at most as far
//   back as the history window; without one (first visit, or only today) the option is disabled, and the history
//   window stands in
// - history window = vrHistoryMaxAgeDays from the backend settings
// the filters are kept in local storage (the window as a choice, not as a date)
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    HomeFilterBarComponent,
    HomeWidgetGroupDirective,
    HomePackageHotspotsComponent,
    HomeReachEpssComponent,
    HomeWidgetPlaceholderComponent,
    HomeBaseOsComponent,
    HomeExposedSecretsComponent,
    HomeComplianceComponent,
    HomeFailingChecksComponent,
    HomeFixWaitingComponent,
    HomeHistoryDriftComponent,
    HomeKnownExploitedComponent,
    HomeNewImagesComponent,
    HomeNewFixesComponent,
    HomeNewVulnerabilitiesComponent,
    HomeOldestImagesComponent,
    HomeSinceLastVisitComponent,
    HomeWhereIsCheckComponent,
    HomeWhereIsLicenseComponent,
    HomeWhereIsPackageComponent,
    HomeWhereIsVulnerabilityComponent,
  ],
  // one expanded widget per page, shared by the widget groups
  providers: [HomeWidgetExpansionService, HomeMotionService],
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

  // read once, as reading also records this visit
  private readonly trackedDays = this.accessTrackingService.trackAccess('home');
  // the previous visit day (not today)
  protected readonly lastVisitUtc: string | undefined = this.getLastVisit();
  // the days the home page was opened (UTC, today included), for the heatmaps
  protected readonly visitDays: string[] = this.trackedDays.map((x) => x.day);

  protected windowMode = signal<HomeWindowMode>(this.loadWindowMode());
  protected severityIds = signal<number[]>(this.loadSeverityIds());
  protected namespaces = signal<string[]>(this.loadNamespaces());
  // the namespaces with data, plus the selected ones that have none now (kept visible, so they can be removed;
  // they are not dropped silently, as right after a backend start the data may be partial)
  private readonly namespacesWithData = signal<string[]>([]);
  protected readonly namespaceOptions = computed(() =>
    [...new Set([...this.namespacesWithData(), ...this.namespaces()])].sort(),
  );

  // bumped by the refresh button, so the data reloads with unchanged filters
  protected readonly refreshCounter = signal(0);

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
  protected readonly exposedSecretsHint = computed(() => `running images${this.namespacesLabel()}`);
  protected readonly historyDriftHint = computed(
    () => `per UTC day, history retention · ${this.severitiesLabel()}${this.namespacesLabel()}`,
  );
  protected readonly newImagesHint = computed(
    () => `per UTC day, current state · ${this.severitiesLabel()}${this.namespacesLabel()}`,
  );
  // the searches look at everything running now; only the namespaces filter applies to them
  protected readonly searchHint = computed(() => `now${this.namespacesLabel()}`);

  protected readonly oldestImagesHint = computed(
    () => `based on history · ${this.severitiesLabel()}${this.namespacesLabel()}`,
  );

  protected readonly sinceLastVisitHint = computed(
    () => `history range, per UTC day · ${this.severitiesLabel()}${this.namespacesLabel()}`,
  );
  // the previous visit day ("yyyy-MM-dd"), for the strip of the last visit
  protected readonly lastVisitDay: string | undefined = this.lastVisitUtc?.substring(0, 10);

  private readonly newVulnerabilitiesWidget = viewChild<HomeNewVulnerabilitiesComponent>('newVulnerabilities');
  private readonly newFixesWidget = viewChild<HomeNewFixesComponent>('newFixes');
  private readonly knownExploitedWidget = viewChild<HomeKnownExploitedComponent>('knownExploited');
  // the days of the two widgets, for the strip (see daysChanged of the widgets)
  protected readonly driftDays = signal<HomeDaysResult<HomeHistoryDriftDto> | undefined>(undefined);
  protected readonly newImagesDays = signal<HomeDaysResult<HomeNewImagesDto> | undefined>(undefined);
  private readonly historyDriftWidget = viewChild<HomeHistoryDriftComponent>('historyDrift');
  private readonly newImagesWidget = viewChild<HomeNewImagesComponent>('newImages');

  // the refresh of the strip: the two widgets whose days it shows reload too
  protected onSinceLastVisitRefresh(): void {
    this.historyDriftWidget()?.refreshWidget();
    this.newImagesWidget()?.refreshWidget();
  }

  // a square of the strip: the widget of its lane opens on that day (the changes widgets select the first id of the day)
  protected onSinceLastVisitClick(event: SinceLastVisitClick): void {
    switch (event.lane) {
      case 'vulnerabilities':
        this.newVulnerabilitiesWidget()?.openDay(event.day);
        break;
      case 'fixes':
        this.newFixesWidget()?.openDay(event.day);
        break;
      case 'exploited':
        this.knownExploitedWidget()?.openDay(event.day);
        break;
      case 'drift':
        this.historyDriftWidget()?.openDay(event.day);
        break;
      case 'images':
        this.newImagesWidget()?.openDay(event.day);
        break;
    }
  }

  protected readonly dataHealth = signal<HomeWidgetState>({});
  protected readonly todayUtc = new Date().toISOString().substring(0, 10);

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

      this.loadData();
    });
  }

  protected onRefresh(): void {
    this.refreshCounter.update((x) => x + 1);
  }

  private loadData(): void {
    this.reloadDataHealth();
  }

  protected reloadDataHealth(): void {
    this.load(this.dataHealth, this.homeService.getHomeDataHealth(), (x) => ({
      isAvailable: true,
      summary:
        `${x.watchersOk}/${x.watchersTotal} watchers ok, ${x.reports.length} report kinds, ` +
        `${x.sources.filter((s) => s.isEnabled).length} sources enabled, ` +
        `history since ${this.toDay(x.historyStartedAt)}, fix tracking since ${this.toDay(x.fixTrackingStartedAt)}`,
    }));
  }

  private load<T>(
    target: WritableSignal<HomeWidgetState>,
    source: Observable<T>,
    toState: (dto: T) => HomeWidgetState,
    onData?: (dto: T) => void,
  ) {
    target.set({});
    source.subscribe({
      next: (dto) => {
        target.set(toState(dto));
        onData?.(dto);
      },
      error: (err) => target.set({ error: err?.message ?? 'request failed' }),
    });
  }

  // ------------------------------------------------------------------------
  // window and stored filters
  // ------------------------------------------------------------------------

  private getLastVisit(): string | undefined {
    const trackedDays = this.trackedDays;
    const todayUtc = new Date().toISOString().substring(0, 10);

    // trackAccess keeps one entry per day with today first, so [1] is an earlier day; checked anyway
    return trackedDays[1]?.day === todayUtc ? undefined : trackedDays[1]?.utc;
  }

  private resolveSince(windowMode: HomeWindowMode): string {
    switch (windowMode) {
      case 'lastVisit': {
        // the history window is the earliest start (the history keeps no older data), so an old visit is clamped to it
        const historyStart = this.daysAgo(this.getHistoryMaxAgeDays());
        return this.lastVisitUtc && this.lastVisitUtc > historyStart ? this.lastVisitUtc : historyStart;
      }
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
    const mode: HomeWindowMode = homeWindowOptions.some((x) => x.value === stored)
      ? (stored as HomeWindowMode)
      : 'lastVisit';
    // "since previous visit" is disabled without an earlier visit, so the history window stands in
    return mode === 'lastVisit' && this.lastVisitUtc === undefined ? 'history' : mode;
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

  private toDay(value?: string | null): string {
    return value ? value.substring(0, 10) : 'n/a';
  }
}
