import { ChangeDetectionStrategy, Component, effect, inject, input, OnInit, signal } from '@angular/core';

import { EsSeveritiesByNsSummaryDto } from '../../../../api/models/es-severities-by-ns-summary-dto';
import { ExposedSecretReportService } from '../../../../api/services/exposed-secret-report.service';
import { EsTableSummary } from './dashboard-exposed-secret-reports.types';

import { SeverityCssStyleByIdPipe } from '../../../pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '../../../pipes/severity-name-by-id.pipe';
import { DarkModeService } from '../../../services/dark-mode.service';
import {
  PrimeNgChartUtils,
  PrimeNgHorizontalBarChartData,
  SeveritiesSummary,
} from '../../../utils/primeng-chart.utils';

import { ChartOptions } from 'chart.js';
import { ButtonModule } from 'primeng/button';
import { CarouselModule } from 'primeng/carousel';
import { ChartModule } from 'primeng/chart';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { VulnerabilityCountPipe } from '../../../pipes/vulnerability-count.pipe';

@Component({
  selector: 'app-dashboard-exposed-secret-reports',
  standalone: true,
  imports: [
    ButtonModule,
    CarouselModule,
    ChartModule,
    DialogModule,
    TableModule,
    TagModule,
    SeverityNameByIdPipe,
    SeverityCssStyleByIdPipe,
    VulnerabilityCountPipe,
  ],
  templateUrl: './dashboard-exposed-secret-reports.component.html',
  styleUrl: './dashboard-exposed-secret-reports.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardExposedSecretReportsComponent implements OnInit {
  exposedSecretReportSummaryDtos = signal<EsSeveritiesByNsSummaryDto[]>([]);
  esTableSummary = signal<EsTableSummary[]>([]);
  namespaceNames: string[] = [];
  severityIds: number[] = [];
  public slides: string[] = ['nsByNs', 'nsBySev'];
  barchartDataNsByNs = signal<PrimeNgHorizontalBarChartData | null>(null);
  barchartDataNsBySev = signal<PrimeNgHorizontalBarChartData | null>(null);
  horizontalBarChartOption = signal<ChartOptions | null>(null);
  public isMoreESDetailsModalVisible = signal<boolean>(false);

  showDistinctValues = input.required<boolean>();

  private readonly exposedSecretReportService = inject(ExposedSecretReportService);
  private readonly darkModeService = inject(DarkModeService);

  constructor() {
    effect(() => {
      const x = this.showDistinctValues();
      this.computeValues();
    });

    effect(() => {
      const isDark = this.darkModeService.isDarkMode();
      this.horizontalBarChartOption.set(PrimeNgChartUtils.getHorizontalBarChartOption());
    });
  }

  ngOnInit() {
    this.loadData();
  }

  loadData(): void {
    this.exposedSecretReportService.getExposedSecretReportSummaryDtos().subscribe({
      next: (res) => this.onDtos(res),
      error: (err) => console.error(err),
    });
  }

  getCountFromExposedSecretReportSummaryDtos(namespaceName: string, severityId: number): number {
    const summary = this.exposedSecretReportSummaryDtos().find((x) => x.namespaceName === namespaceName);
    if (!summary || !summary.details) {
      return 0;
    }

    const stat = summary.details.find((y) => y.id == severityId);

    return this.showDistinctValues() ? (stat?.distinctCount ?? 0) : (stat?.totalCount ?? 0);
  }

  onEsrMore(_event: MouseEvent) {
    this.isMoreESDetailsModalVisible.set(true);
  }

  // severityWrapperGetCapitalizedName(severityId: number): string {
  //   return SeverityUtils.getCapitalizedName(severityId);
  // }

  // severityWrapperGetCssColor(severityId: number): string {
  //   return SeverityUtils.getCssColor(severityId);
  // }

  private onDtos(dtos: EsSeveritiesByNsSummaryDto[]) {
    this.exposedSecretReportSummaryDtos.set(dtos.sort((a, b) => (a.namespaceName! > b.namespaceName! ? 1 : -1)));
    this.computeValues();
  }

  private computeValues() {
    const summary = this.exposedSecretReportSummaryDtos().find((x) => x.isTotal);
    if (summary && summary.details) {
      this.esTableSummary.set(
        summary.details.map((x) => {
          return { severityId: x.id!, count: this.showDistinctValues() ? (x.distinctCount ?? 0) : (x.totalCount ?? 0) };
        }),
      );
      this.severityIds = summary.details.map((x) => x.id!).sort((a, b) => a - b);
    }

    this.namespaceNames = this.exposedSecretReportSummaryDtos()
      .filter((x) => !x.isTotal)
      .filter((x) => x.namespaceName)
      .map((x) => x.namespaceName);

    this.barchartDataNsByNs.set(
      PrimeNgChartUtils.getDataForHorizontalBarChartByNamespace(
        this.exposedSecretReportSummaryDtos() as SeveritiesSummary[],
        this.showDistinctValues(),
      ),
    );
    this.barchartDataNsBySev.set(
      PrimeNgChartUtils.getDataForHorizontalBarChartBySeverity(
        this.exposedSecretReportSummaryDtos() as SeveritiesSummary[],
        this.showDistinctValues(),
      ),
    );
    this.horizontalBarChartOption.set(PrimeNgChartUtils.getHorizontalBarChartOption());
  }
}
