import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';

import { HomeComplianceReportDto } from '@api/models/home-compliance-report-dto';
import { HomeService } from '@api/services/home.service';
import { NetworkFillComponent } from '@shared/ui/network-fill/network-fill.component';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { RingGaugeComponent } from '@shared/ui/ring-gauge/ring-gauge.component';
import { RingGaugeData } from '@shared/ui/ring-gauge/ring-gauge.types';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// Compliance: one ring per cluster compliance report (CIS, NSA, PSS...), the share of its controls that pass.
// Not filtered (the reports are about the cluster). It loads by itself (a change of refresh reloads it).
// - no ring selected: the rings and a one line explanation
// - a ring selected (click, a second click clears it): the worst failing controls of that report, at most maxControls (the
//   server caps them; "n more" says the rest); only the controls of that report are asked for, when it is selected
// There is no "change since last visit": the backend keeps no history of the compliance reports.
@Component({
  selector: 'app-home-compliance',
  standalone: true,
  imports: [HomeWidgetFrameComponent, ButtonModule, NetworkFillComponent, RingGaugeComponent, TrivyBoxComponent, SeverityNameByIdPipe],
  templateUrl: './home-compliance.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block min-w-0' },
})
export class HomeComplianceComponent extends HomeWidgetBase {
  private static readonly maxControls = 5;

  // the height the panel of the controls takes with all of them (the border, the padding of 8px, the header of 28px, five rows
  // of 20px, the line of the rest, and the gaps of 4px), reserved also while it is closed, so the widget does not change its height
  protected readonly panelHeight = 190;

  private readonly homeService = inject(HomeService);
  private readonly router = inject(Router);

  hint = input<string>('');
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  // the id of the selected ring = the uid of the report
  protected readonly selected = signal<string | undefined>(undefined);

  protected readonly help: HomeWidgetHelp = {
    question: `How well does the cluster pass its <strong>compliance reports</strong> (CIS, NSA, PSS...)? One ring per report shows where to start.`,
    oddities: [
      `The ring is the share of controls that pass: pass / (pass + fail). A report without controls has no ring.`,
      `Not filtered: the reports are about the cluster, not about namespaces.`,
      `There is no change since the last visit: the compliance reports have no history yet.`,
    ],
    tips: [
      `<strong>Click a ring</strong> to see the five worst failing controls of that report.`,
    ],
  };

  private readonly reports = rxResource({
    params: () => ({ refresh: this.refresh(), localRefresh: this.localRefresh() }),
    stream: () => this.homeService.getHomeCompliance(),
  });

  // the controls of the selected report; no selection, no request
  private readonly worst = rxResource({
    params: () => {
      const reportUid = this.selected();
      return reportUid === undefined ? undefined : { reportUid, refresh: this.refresh(), localRefresh: this.localRefresh() };
    },
    stream: ({ params }) =>
      this.homeService.getHomeComplianceWorstControls({
        reportUid: params.reportUid,
        limit: HomeComplianceComponent.maxControls,
      }),
  });

  protected readonly dto = computed(() => this.reports.value());
  // the reports that have controls (a percent)
  private readonly measured = computed(() => (this.dto()?.items ?? []).filter((x) => x.passPercent != null));

  protected readonly gauges = computed<RingGaugeData[]>(() =>
    this.measured().map((x) => ({
      id: x.uid,
      value: x.passPercent ?? 0,
      color: 'var(--p-green-400)',
      label: this.label(x),
    })),
  );

  protected readonly selectedReport = computed(() => this.measured().find((x) => x.uid === this.selected()));
  protected readonly worstDto = computed(() => (this.selectedReport() ? this.worst.value() : undefined));
  protected readonly controls = computed(() => this.worstDto()?.items ?? []);
  protected readonly hiddenControls = computed(() => Math.max(0, (this.worstDto()?.totalFailingControls ?? 0) - this.controls().length));
  protected readonly selectedLabel = computed(() => {
    const report = this.selectedReport();
    return report ? this.label(report) : '';
  });

  protected readonly state = computed<HomeWidgetState>(() => {
    if (this.reports.error()) {
      return { error: (this.reports.error() as Error)?.message ?? 'request failed' };
    }

    const dto = this.dto();
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary: `${dto.items.length} ${dto.items.length === 1 ? 'report' : 'reports'}`,
    };
  });

  protected severityColor(severityId: number): string {
    return SeverityUtils.getCssColor(severityId);
  }

  protected openReports(): void {
    const url = this.router.serializeUrl(this.router.createUrlTree(['/cluster-compliance-reports']));
    window.open(url, '_blank');
  }

  // the resource name without the "k8s-" prefix, e.g. k8s-cis-1.23 -> "CIS 1.23", k8s-pss-restricted-0.1 -> "PSS restricted 0.1"
  private label(report: HomeComplianceReportDto): string {
    const [first, ...rest] = report.name.replace(/^k8s-/i, '').split('-');
    return [first.toUpperCase(), ...rest].join(' ');
  }
}
