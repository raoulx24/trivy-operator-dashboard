import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { catchError, of } from 'rxjs';

import { AppVersionsService } from '@api/services/app-versions.service';
import { HomeService } from '@api/services/home.service';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { VersionUtils } from '@shared/utils/version.utils';
import { PillButtonComponent } from '@shared/ui/pill-button/pill-button.component';
import { RingGaugeComponent } from '@shared/ui/ring-gauge/ring-gauge.component';
import { RingGaugeData } from '@shared/ui/ring-gauge/ring-gauge.types';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

type Level = 'ok' | 'warn' | 'bad' | 'off';

// no level: an old scan is normal (e.g. nothing changed in the rbac of the cluster for 200 days), so it is not judged
interface ReportRow {
  kind: string;
  count: number;
  oldest: string;
  newest: string;
  title: string;
}

// a background job of the backend (history retention, the housekeeping, the GitHub check, the KEV / EUVD download);
// level: warn = the last run failed (the error is shown), off = not run yet or skipped (switched off by its options)
interface JobRow {
  name: string;
  level: Level;
  summary: string;
  when: string;
  error: string | null;
  title: string;
}

interface SourceRow {
  source: string;
  level: Level;
  detail: string;
  title: string;
}

// Data health: can the other widgets be trusted?
// - collapsed: simple. The watchers (a ring) and the verdict, the enrichment sources (KEV, EPSS, EUVD, EOL) as dots, and where
//   the history and the fix tracking begin
// - expanded: the same facts in detail: the watchers with issues, per report kind the count and the first / last scan (not
//   judged: an old scan is normal when nothing changed), per source the last fetch and the errors
// Not filtered (it is about the data of the cluster, not about the namespaces). It loads by itself (a change of refresh
// reloads it).
@Component({
  selector: 'app-home-data-health',
  standalone: true,
  imports: [HomeWidgetFrameComponent, RingGaugeComponent, TrivyBoxComponent, PillButtonComponent],
  templateUrl: './home-data-health.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block min-w-0' },
})
export class HomeDataHealthComponent extends HomeWidgetBase {
  private static readonly maxIssues = 5;
  private static readonly hourMs = 3_600_000;
  private static readonly dayMs = 24 * HomeDataHealthComponent.hourMs;

  private readonly homeService = inject(HomeService);
  private readonly appVersionsService = inject(AppVersionsService);
  private readonly mainAppInitService = inject(MainAppInitService);
  private readonly router = inject(Router);

  hint = input<string>('not filtered');
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  protected readonly help: HomeWidgetHelp = {
    question: `<strong>Can the other widgets be trusted?</strong> What this page shows is only as good as its data: are the watchers up and the sources on?`,
    oddities: [
      `The ring is the share of the watchers that are green. <strong>To look at</strong> counts the watchers that are not green, the sources with errors and the jobs whose last run failed.`,
      `<strong>Sources</strong> (KEV, EPSS, EUVD, EOL): green when enabled without errors, amber with errors, grey when not enabled.`,
      `History and fix tracking exist only since the day they started: what is older cannot be told (e.g. when a fix appeared).`,
      `A newer release of the dashboard on GitHub is shown as a pill, "New version available" (the same check as the About page); a running version newer than the latest release as "Experimental build". Information only, not counted in "to look at".`,
    ],
    expanded: [
      `Per report kind the count and the first and last scan. An old last scan is not a problem by itself: a report is rescanned when something changes (the rbac of a cluster can stay the same for months).`,
      `Per source the newest fetch seen in the entries (the status of the feed itself is not kept) and the entries with an error.`,
      `A watcher that is not green is listed with what to do about it.`,
      `<strong>Services</strong>: the version you run against the newest GitHub release, the releases the backend cached, and how long the history is kept.`,
      `<strong>Jobs</strong>: the background jobs of the backend (history retention, the housekeeping of the details, the GitHub check, the KEV and EUVD downloads) with what the last run did, when it was and when the next one is. A job whose last run failed is amber, with the error (it counts in "to look at"); grey is not run yet (some wait a full interval before the first run) or switched off by its settings. The state is kept in memory: after a restart every job starts as not run.`,
    ],
  };

  private readonly health = rxResource({
    params: () => ({ refresh: this.refresh(), localRefresh: this.localRefresh() }),
    stream: () => this.homeService.getHomeDataHealth(),
  });

  protected readonly dto = computed(() => this.health.value());

  // the releases (the backend caches them from GitHub) and the running version, as the about page reads them; a failure is
  // an answer too (undefined), not an error of the widget
  private readonly releases = rxResource({
    params: () => ({ refresh: this.refresh(), localRefresh: this.localRefresh() }),
    stream: () => this.appVersionsService.getGitHubVersions().pipe(catchError(() => of(undefined))),
  });
  private readonly currentVersion = rxResource({
    params: () => ({ refresh: this.refresh(), localRefresh: this.localRefresh() }),
    stream: () => this.appVersionsService.getCurrentVersion().pipe(catchError(() => of(undefined))),
  });

  // the newest release by version (the same rule as the about page), undefined while loading or without releases
  protected readonly latestRelease = computed(() =>
    [...(this.releases.value() ?? [])].sort(
      (a, b) => VersionUtils.parseVersion(b.tagName ?? '') - VersionUtils.parseVersion(a.tagName ?? ''),
    )[0],
  );
  protected readonly releaseCount = computed(() => this.releases.value()?.length);
  // true / false once both answers are there and comparable
  protected readonly versionInfo = computed(() => {
    const current = this.currentVersion.value()?.fileVersion;
    const latest = this.latestRelease();
    if (!current || !latest) {
      return undefined;
    }
    const diff = VersionUtils.parseVersion(latest.tagName) - VersionUtils.parseVersion(current);
    return { current, latest: latest.tagName.replace('v', ''), isNewer: diff > 0, isExperimental: diff < 0 };
  });
  protected readonly releasesLoaded = computed(() => !this.releases.isLoading());

  // the days the history is kept (the backend settings), when the history is on
  protected readonly retentionDays = computed(() => this.mainAppInitService.backendSettingsDto().vrHistoryMaxAgeDays);

  private readonly now = computed(() => {
    const dto = this.dto();
    return dto ? new Date(dto.generatedAt).getTime() : Date.now();
  });

  protected readonly watchersOkPercent = computed(() => {
    const dto = this.dto();
    return !dto || dto.watchersTotal === 0 ? 0 : (dto.watchersOk / dto.watchersTotal) * 100;
  });

  protected readonly watcherGauge = computed<RingGaugeData>(() => {
    const dto = this.dto();
    const percent = this.watchersOkPercent();
    const level: Level = !dto || dto.watchersTotal === 0 ? 'off' : percent === 100 ? 'ok' : percent >= 50 ? 'warn' : 'bad';

    return {
      id: 'watchers',
      value: percent,
      color: this.color(level),
      label: 'watchers',
      centerText: dto ? `${dto.watchersOk}/${dto.watchersTotal}` : '',
    };
  });

  protected readonly reports = computed<ReportRow[]>(() =>
    // ascending by the name shown, so the order does not change with the data
    [...(this.dto()?.reports ?? [])].sort((a, b) => this.compareKinds(a.kind, b.kind)).map((x) => ({
      kind: this.kindLabel(x.kind),
      count: x.count,
      oldest: this.dayAgo(x.oldestScanAt),
      newest: this.dayAgo(x.newestScanAt),
      title: `${x.kind}: first scan ${this.stamp(x.oldestScanAt)}, last scan ${this.stamp(x.newestScanAt)}`,
    })),
  );

  protected readonly sources = computed<SourceRow[]>(() =>
    (this.dto()?.sources ?? []).map((x) => {
      const level: Level = !x.isEnabled ? 'off' : x.errorCount > 0 ? 'warn' : 'ok';
      const detail = !x.isEnabled
        ? 'off'
        : x.lastFetchedAt
          ? this.ago(x.lastFetchedAt)
          : 'no fetch yet';

      return {
        source: x.source,
        level,
        detail,
        title:
          `${x.source}: ${x.isEnabled ? 'enabled' : 'not enabled'}, newest fetch ${this.stamp(x.lastFetchedAt)}` +
          (x.errorCount > 0 ? `, ${x.errorCount} ${x.errorCount === 1 ? 'entry' : 'entries'} with an error` : ''),
      };
    }),
  );

  protected readonly jobs = computed<JobRow[]>(() =>
    (this.dto()?.jobs ?? []).map((x) => {
      const level: Level = x.state === 'Failed' ? 'warn' : x.state === 'NotRun' || x.state === 'Skipped' ? 'off' : 'ok';
      const lastOk = x.lastSuccessAt ? `last ok ${this.ago(x.lastSuccessAt)}` : 'never succeeded';

      let when: string;
      switch (x.state) {
        case 'Failed':
          when = `failed ${x.lastFinishedAt ? this.ago(x.lastFinishedAt) : ''}`.trim();
          break;
        case 'Running':
          when = 'running';
          break;
        case 'NotRun':
          when = x.nextRunAt ? `first run ${this.until(x.nextRunAt)}` : 'not run yet';
          break;
        case 'Skipped':
          when = 'skipped';
          break;
        default:
          when = x.lastFinishedAt ? this.ago(x.lastFinishedAt) : '';
      }

      return {
        name: x.name,
        level,
        summary: x.state === 'Failed' ? '' : (x.summary ?? ''),
        when,
        error: x.state === 'Failed' ? (x.lastError ?? 'unknown error') : null,
        title:
          `${x.name}${x.members > 1 ? ` (${x.members} jobs)` : ''}: ${x.state}, last run ${this.stamp(x.lastFinishedAt)}, ${lastOk}` +
          (x.nextRunAt ? `, next ${this.stamp(x.nextRunAt)}` : '') +
          (x.consecutiveFailures > 0 ? `, ${x.consecutiveFailures} failures in a row` : ''),
      };
    }),
  );

  protected readonly offSources = computed(() => this.sources().filter((x) => x.level === 'off').length);

  protected readonly issues = computed(() => (this.dto()?.watcherIssues ?? []).slice(0, HomeDataHealthComponent.maxIssues));
  protected readonly hiddenIssues = computed(() => Math.max(0, (this.dto()?.watcherIssues.length ?? 0) - this.issues().length));

  // what needs a look: watchers that are not green, sources with errors, jobs whose last run failed (the age of a scan is not
  // judged)
  protected readonly attention = computed(
    () =>
      (this.dto()?.watcherIssues.length ?? 0) +
      this.sources().filter((x) => x.level === 'warn').length +
      this.jobs().filter((x) => x.level === 'warn').length,
  );

  protected readonly historyDays = computed(() => this.daysSince(this.dto()?.historyStartedAt));
  protected readonly fixTrackingDays = computed(() => this.daysSince(this.dto()?.fixTrackingStartedAt));

  protected readonly state = computed<HomeWidgetState>(() => {
    if (this.health.error()) {
      return { error: (this.health.error() as Error)?.message ?? 'request failed' };
    }

    const dto = this.dto();
    if (!dto) {
      return {};
    }

    return {
      isAvailable: true,
      summary: this.attention() === 0 ? 'all good' : `${this.attention()} to look at`,
    };
  });

  // the about page has the release notes; in a new tab, so the home page stays as it is
  protected openAbout(): void {
    window.open(this.router.serializeUrl(this.router.createUrlTree(['/about'])), '_blank');
  }

  protected color(level: Level): string {
    return `var(--tod-data-health-${level})`;
  }

  protected day(value?: string | null): string {
    return value ? value.substring(0, 10) : 'n/a';
  }

  protected stamp(value?: string | null): string {
    return value ? value.substring(0, 16).replace('T', ' ') + ' UTC' : 'n/a';
  }

  // "in 5m", "in 3h", "in 2d"; "soon" when it is due
  private until(value: string): string {
    const left = new Date(value).getTime() - this.now();
    if (left < 60_000) {
      return 'soon';
    }
    if (left < HomeDataHealthComponent.hourMs) {
      return `in ${Math.round(left / 60_000)}m`;
    }
    if (left < HomeDataHealthComponent.dayMs) {
      return `in ${Math.round(left / HomeDataHealthComponent.hourMs)}h`;
    }
    return `in ${Math.round(left / HomeDataHealthComponent.dayMs)}d`;
  }

  // "2026-11-22 · 5m ago"
  private dayAgo(value?: string | null): string {
    return value ? `${this.day(value)} · ${this.ago(value)}` : 'n/a';
  }

  private ago(value: string): string {
    const age = Math.max(0, this.now() - new Date(value).getTime());
    if (age < HomeDataHealthComponent.hourMs) {
      return `${Math.max(1, Math.round(age / 60_000))}m ago`;
    }
    if (age < HomeDataHealthComponent.dayMs) {
      return `${Math.round(age / HomeDataHealthComponent.hourMs)}h ago`;
    }
    return `${Math.round(age / HomeDataHealthComponent.dayMs)}d ago`;
  }

  private daysSince(value?: string | null): number | undefined {
    return value ? Math.max(0, Math.floor((this.now() - new Date(value).getTime()) / HomeDataHealthComponent.dayMs)) : undefined;
  }

  // the type name of the report as the page calls it (the wording of the nav menu); a type that is not here (a new one) gets
  // its name split into words: NewThingReport -> "New Thing"
  private static readonly kindLabels: Record<string, string> = {
    VulnerabilityReport: 'Vulnerabilities',
    SbomReport: 'SBOMs',
    ConfigAuditReport: 'Config Audits',
    ExposedSecretReport: 'Exposed Secrets',
    RbacAssessmentReport: 'RBAC Assessments',
    InfraAssessmentReport: 'Infra Assessments',
    ClusterVulnerabilityReport: 'Cluster Vulnerabilities',
    ClusterSbomReport: 'Cluster SBOMs',
    ClusterRbacAssessmentReport: 'Cluster RBAC Assessments',
    ClusterComplianceReport: 'Cluster Compliance',
    ClusterConfigAuditReport: 'Cluster Config Audits',
    ClusterInfraAssessmentReport: 'Cluster Infra Assessments',
  };

  private kindLabel(kind: string): string {
    return (
      HomeDataHealthComponent.kindLabels[kind] ??
      kind
        .replace(/Report$/, '')
        .replace(/([a-z])([A-Z])/g, '$1 $2')
    );
  }

  // ascending by the label as shown
  private compareKinds(a: string, b: string): number {
    return this.kindLabel(a).localeCompare(this.kindLabel(b));
  }
}
