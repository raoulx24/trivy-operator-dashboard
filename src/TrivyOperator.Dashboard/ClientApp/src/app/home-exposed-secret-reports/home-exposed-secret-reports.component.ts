import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

import { EsSeveritiesByNsSummaryDto } from '../../api/models/es-severities-by-ns-summary-dto';
import { ExposedSecretReportService } from '../../api/services/exposed-secret-report.service';
import { EsTableSummary } from './home-exposed-secret-reports.types';

import { PrimeNgChartUtils, PrimeNgHorizontalBarChartData, SeveritiesSummary } from '../utils/primeng-chart.utils';
import { SeverityUtils } from '../utils/severity.utils';

import { ButtonModule } from 'primeng/button';
import { CarouselModule } from 'primeng/carousel';
import { ChartModule } from 'primeng/chart';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-home-exposed-secret-reports',
  standalone: true,
  imports: [CommonModule, ButtonModule, CarouselModule, ChartModule, DialogModule, TableModule, TagModule],
  templateUrl: './home-exposed-secret-reports.component.html',
  styleUrl: './home-exposed-secret-reports.component.scss',
})
export class HomeExposedSecretReportsComponent {
  exposedSecretReportSummaryDtos: EsSeveritiesByNsSummaryDto[] = [];
  esTableSummary: EsTableSummary[] = [];
  namespaceNames: string[] = [];
  severityIds: number[] = [];
  public slides: string[] = ['nsByNs', 'nsBySev'];
  barchartDataNsByNs: PrimeNgHorizontalBarChartData | null = null;
  barchartDataNsBySev: PrimeNgHorizontalBarChartData | null = null;
  public horizontalBarChartOption: any;
  public isMoreESDetailsModalVisible: boolean = false;
  private localShowDistinctValues: boolean = true;

  constructor(private exposedSecretReportService: ExposedSecretReportService) {
    this.exposedSecretReportService.getExposedSecretReportSummaryDtos().subscribe({
      next: (res) => this.onDtos(res),
      error: (err) => console.error(err),
    });
  }

  get showDistinctValues(): boolean {
    return this.localShowDistinctValues;
  }

  @Input() set showDistinctValues(value: boolean) {
    this.localShowDistinctValues = value;
    this.onDistinctSwitch();
  }

  getCountFromExposedSecretReportSummaryDtos(namespaceName: string, severityId: number): string {
    const summary = this.exposedSecretReportSummaryDtos.find((x) => x.namespaceName === namespaceName);
    if (!summary || !summary.details) {
      return '0';
    }

    const stat = summary.details.find((y) => y.id == severityId);

    const result = this.showDistinctValues ? (stat?.distinctCount ?? 0) : (stat?.totalCount ?? 0);

    return result.toString();
  }

  onEsrMore(_event: MouseEvent) {
    this.isMoreESDetailsModalVisible = true;
  }

  severityWrapperGetCapitalizedName(severityId: number): string {
    return SeverityUtils.getCapitalizedName(severityId);
  }

  severityWrapperGetCssColor(severityId: number): string {
    return SeverityUtils.getCssColor(severityId);
  }

  private onDtos(dtos: EsSeveritiesByNsSummaryDto[]) {
    this.exposedSecretReportSummaryDtos = dtos;
    this.computeValues();
    this.barchartDataNsByNs = PrimeNgChartUtils.getDataForHorizontalBarChartByNamespace(
      this.exposedSecretReportSummaryDtos as SeveritiesSummary[],
      this.showDistinctValues,
    );
    this.barchartDataNsBySev = PrimeNgChartUtils.getDataForHorizontalBarChartBySeverity(
      this.exposedSecretReportSummaryDtos as SeveritiesSummary[],
      this.showDistinctValues,
    );
    this.horizontalBarChartOption = PrimeNgChartUtils.getHorizontalBarChartOption();
  }

  private onDistinctSwitch() {
    // TODO
  }

  private computeValues() {
    const summary = this.exposedSecretReportSummaryDtos.find((x) => x.isTotal);
    if (summary && summary.details) {
      this.esTableSummary = summary.details.map((x) => {
        return { severityId: x.id!, count: this.showDistinctValues ? (x.distinctCount ?? 0) : (x.totalCount ?? 0) };
      });
      this.severityIds = summary.details.map((x) => x.id!);
    }

    this.namespaceNames = this.exposedSecretReportSummaryDtos
      .filter((x) => !x.isTotal)
      .filter((x) => x.namespaceName)
      .map((x) => x.namespaceName);
  }
}
