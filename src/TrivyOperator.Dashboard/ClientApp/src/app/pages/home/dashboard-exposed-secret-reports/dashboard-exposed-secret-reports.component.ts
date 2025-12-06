
import { Component, effect, input, OnInit } from '@angular/core';

import { EsSeveritiesByNsSummaryDto } from '../../../../api/models/es-severities-by-ns-summary-dto';
import { ExposedSecretReportService } from '../../../../api/services/exposed-secret-report.service';
import { EsTableSummary } from './dashboard-exposed-secret-reports.types';

import { PrimeNgChartUtils, PrimeNgHorizontalBarChartData, SeveritiesSummary } from '../../../utils/primeng-chart.utils';
import { DarkModeService } from '../../../services/dark-mode.service';
import { SeverityCssStyleByIdPipe } from '../../../pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '../../../pipes/severity-name-by-id.pipe';

import { ButtonModule } from 'primeng/button';
import { CarouselModule } from 'primeng/carousel';
import { ChartModule } from 'primeng/chart';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-dashboard-exposed-secret-reports',
  standalone: true,
  imports: [ButtonModule, CarouselModule, ChartModule, DialogModule, TableModule, TagModule, SeverityNameByIdPipe, SeverityCssStyleByIdPipe],
  templateUrl: './dashboard-exposed-secret-reports.component.html',
  styleUrl: './dashboard-exposed-secret-reports.component.scss',
})
export class DashboardExposedSecretReportsComponent implements OnInit {
  exposedSecretReportSummaryDtos: EsSeveritiesByNsSummaryDto[] = [];
  esTableSummary: EsTableSummary[] = [];
  namespaceNames: string[] = [];
  severityIds: number[] = [];
  public slides: string[] = ['nsByNs', 'nsBySev'];
  barchartDataNsByNs: PrimeNgHorizontalBarChartData | null = null;
  barchartDataNsBySev: PrimeNgHorizontalBarChartData | null = null;
  public horizontalBarChartOption: any;
  public isMoreESDetailsModalVisible: boolean = false;

  private darkLightMode: 'Dark' | 'Light' = 'Dark';

  showDistinctValues = input.required<boolean>();

  constructor(private exposedSecretReportService: ExposedSecretReportService, private darkModeService: DarkModeService) {
    effect(() => {
      const x = this.showDistinctValues();
      this.computeValues();
    });
  }

  ngOnInit() {
    this.loadData();
    this.darkModeService.isDarkMode$.subscribe((isDarkMode) => {
      const oldDarkLightMode = this.darkLightMode;
      this.darkLightMode = isDarkMode ? 'Dark' : 'Light';
      if (oldDarkLightMode != this.darkLightMode) {
        setTimeout(() => {
          this.horizontalBarChartOption = PrimeNgChartUtils.getHorizontalBarChartOption();
        }, 0);
      }
    });
    this.horizontalBarChartOption = PrimeNgChartUtils.getHorizontalBarChartOption();
  }

  loadData(): void {
    this.exposedSecretReportService.getExposedSecretReportSummaryDtos().subscribe({
      next: (res) => this.onDtos(res),
      error: (err) => console.error(err),
    });
  }

  getCountFromExposedSecretReportSummaryDtos(namespaceName: string, severityId: number): string {
    const summary = this.exposedSecretReportSummaryDtos.find((x) => x.namespaceName === namespaceName);
    if (!summary || !summary.details) {
      return '0';
    }

    const stat = summary.details.find((y) => y.id == severityId);

    const result = this.showDistinctValues() ? (stat?.distinctCount ?? 0) : (stat?.totalCount ?? 0);

    return result.toString();
  }

  onEsrMore(_event: MouseEvent) {
    this.isMoreESDetailsModalVisible = true;
  }

  // severityWrapperGetCapitalizedName(severityId: number): string {
  //   return SeverityUtils.getCapitalizedName(severityId);
  // }

  // severityWrapperGetCssColor(severityId: number): string {
  //   return SeverityUtils.getCssColor(severityId);
  // }

  private onDtos(dtos: EsSeveritiesByNsSummaryDto[]) {
    this.exposedSecretReportSummaryDtos = dtos.sort((a, b) => a.namespaceName! > b.namespaceName! ? 1 : -1);
    this.computeValues();
  }

  private computeValues() {
    const summary = this.exposedSecretReportSummaryDtos.find((x) => x.isTotal);
    if (summary && summary.details) {
      this.esTableSummary = summary.details.map((x) => {
        return { severityId: x.id!, count: this.showDistinctValues() ? (x.distinctCount ?? 0) : (x.totalCount ?? 0) };
      });
      this.severityIds = summary.details.map((x) => x.id!);
    }

    this.namespaceNames = this.exposedSecretReportSummaryDtos
      .filter((x) => !x.isTotal)
      .filter((x) => x.namespaceName)
      .map((x) => x.namespaceName);

    this.barchartDataNsByNs = PrimeNgChartUtils.getDataForHorizontalBarChartByNamespace(
      this.exposedSecretReportSummaryDtos as SeveritiesSummary[],
      this.showDistinctValues(),
    );
    this.barchartDataNsBySev = PrimeNgChartUtils.getDataForHorizontalBarChartBySeverity(
      this.exposedSecretReportSummaryDtos as SeveritiesSummary[],
      this.showDistinctValues(),
    );
    this.horizontalBarChartOption = PrimeNgChartUtils.getHorizontalBarChartOption();
  }
}
