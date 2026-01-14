import { ChangeDetectionStrategy, Component, effect, inject, input, OnInit, signal } from '@angular/core';

import { ConfigAuditReportSummaryDto } from '../../../../api/models/config-audit-report-summary-dto';
import { ConfigAuditReportService } from '../../../../api/services/config-audit-report.service';
import {
  PrimeNgChartUtils,
  PrimeNgHorizontalBarChartData,
  SeveritiesSummary,
} from '../../../utils/primeng-chart.utils';
import { CarDetailsDto, CarSeveritySummary } from './dashboard-config-audit-reports.types';

import { ChartOptions } from 'chart.js';
import { ButtonModule } from 'primeng/button';
import { CarouselModule } from 'primeng/carousel';
import { ChartModule } from 'primeng/chart';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';

import { SeverityCssStyleByIdPipe } from '../../../pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '../../../pipes/severity-name-by-id.pipe';
import { VulnerabilityCountPipe } from '../../../pipes/vulnerability-count.pipe';
import { DarkModeService } from '../../../services/dark-mode.service';
import { SeverityUtils } from '../../../utils/severity.utils';

@Component({
  selector: 'app-dashboard-config-audit-reports',
  standalone: true,
  imports: [
    ButtonModule,
    CarouselModule,
    ChartModule,
    DialogModule,
    TableModule,
    TagModule,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
    VulnerabilityCountPipe,
  ],
  templateUrl: './dashboard-config-audit-reports.component.html',
  styleUrl: './dashboard-config-audit-reports.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardConfigAuditReportsComponent implements OnInit {
  configAuditReportSummaryDtos?: ConfigAuditReportSummaryDto[];
  namespaceNames: string[] = [];
  kinds: string[] = [];
  severities: number[] = SeverityUtils.severityShortDtos.map((x) => x.id);
  carSeveritySummaries = signal<CarSeveritySummary[]>([]);
  carDetailsDtos = signal<CarDetailsDto[]>([]);
  carDetailsDtoFooter = signal<CarDetailsDto>({ namespaceName: '', values: [], isTotal: true });
  public slides: string[] = ['nsByNs', 'nsBySev', 'kindByNs', 'kindBySev'];
  severitiesSummariesNamespace: SeveritiesSummary[] = [];
  barchartDataNsByNs = signal<PrimeNgHorizontalBarChartData | null>(null);
  barchartDataNsBySev = signal<PrimeNgHorizontalBarChartData | null>(null);
  barchartDataKindByNs = signal<PrimeNgHorizontalBarChartData | null>(null);
  barchartDataKindBySev = signal<PrimeNgHorizontalBarChartData | null>(null);
  horizontalBarChartOption = signal<ChartOptions | null>(null);
  isCarDetailsDialogVisible = signal<boolean>(false);

  showDistinctValues = input.required<boolean>();

  private readonly configAuditReportService = inject(ConfigAuditReportService);
  private readonly darkModeService = inject(DarkModeService);

  constructor() {
    effect(() => {
      const x = this.showDistinctValues();
      this.computeCarSeveritySummaries();
      this.computeStatisticsByNs();
      this.computeStatisticsByKind();
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
    this.configAuditReportService.getConfigAuditReportSummaryDtos().subscribe({
      next: (res) => this.onDtos(res),
      error: (err) => console.error(err),
    });
  }

  onCarsMore(_event: MouseEvent) {
    this.isCarDetailsDialogVisible.set(true);
  }

  private onDtos(dtos: ConfigAuditReportSummaryDto[]) {
    this.configAuditReportSummaryDtos = dtos.sort((a, b) => (a.namespaceName! > b.namespaceName! ? 1 : -1));

    this.getArraysFromDtos();
    this.computeCarSeveritySummaries();
    this.computeStatisticsByNs();
    this.computeStatisticsByKind();
    this.horizontalBarChartOption.set(PrimeNgChartUtils.getHorizontalBarChartOption());
  }

  private getArraysFromDtos() {
    if (!this.configAuditReportSummaryDtos) {
      return;
    }
    const result = this.configAuditReportSummaryDtos.reduce(
      (acc, item) => {
        if (item.namespaceName && !acc.namespaceNames.includes(item.namespaceName)) {
          acc.namespaceNames.push(item.namespaceName);
        }
        if (item.kind && !acc.kinds.includes(item.kind)) {
          acc.kinds.push(item.kind);
        }
        if (!acc.severities.includes(item.severityId!)) {
          acc.severities.push(item.severityId!);
        }
        return acc;
      },
      { namespaceNames: [] as string[], kinds: [] as string[], severities: [] as number[] },
    );

    const { namespaceNames, kinds, severities } = result;
    this.namespaceNames = namespaceNames.sort();
    this.kinds = kinds.sort();
    //this.severities = severities.sort((a, b) => a - b);

    const localCarDetailsDto: CarDetailsDto[] = [];
    this.namespaceNames.forEach((namespaceName) => {
      const values: { severityId: number; count: number }[] = [];
      this.severities.forEach((severityId) => {
        this.kinds.forEach((kind) => {
          const dto = this.configAuditReportSummaryDtos?.find(
            (x) => x.namespaceName == namespaceName && x.severityId === severityId && x.kind == kind,
          );
          const count = this.showDistinctValues() ? (dto?.distinctCount ?? -1) : (dto?.totalCount ?? -1);
          values.push({ severityId: severityId, count: count });
        });
      });
      localCarDetailsDto.push({ namespaceName: namespaceName, values: values, isTotal: false });
    });
    this.carDetailsDtos.set(localCarDetailsDto);
    const values: { severityId: number; count: number }[] = [];
    this.severities.forEach((severityId) => {
      this.kinds.forEach((kind) => {
        const dto = this.configAuditReportSummaryDtos?.find(
          (x) => x.namespaceName === '' && x.severityId === severityId && x.kind == kind,
        );
        const count = this.showDistinctValues() ? (dto?.distinctCount ?? -1) : (dto?.totalCount ?? -1);
        values.push({ severityId: severityId, count: count });
      });
    });
    this.carDetailsDtoFooter.set({ namespaceName: '', values: values, isTotal: true });
  }

  private computeCarSeveritySummaries() {
    if (!this.configAuditReportSummaryDtos) {
      return;
    }
    const groupedSumForCarSeverities = this.configAuditReportSummaryDtos
      .filter((dto) => dto.namespaceName === '')
      .reduce(
        (acc, item) => {
          const severityName: string = SeverityUtils.getCapitalizedName(item.severityId!);
          if (!acc[severityName]) {
            acc[severityName] = 0;
          }
          acc[severityName] += this.showDistinctValues() ? (item.distinctCount ?? 0) : (item.totalCount ?? 0);
          return acc;
        },
        {} as Record<string, number>,
      );

    this.carSeveritySummaries.set(
      Object.keys(groupedSumForCarSeverities).map((key) => ({
        severityName: key,
        count: groupedSumForCarSeverities[key],
      })),
    );
  }

  private computeStatisticsByNs() {
    if (!this.configAuditReportSummaryDtos) {
      return;
    }

    const summaryMap: { [key: string]: SeveritiesSummary } = {};
    this.configAuditReportSummaryDtos
      .filter((dto) => dto.namespaceName !== '')
      .forEach((item) => {
        if (!summaryMap[item.kind!]) {
          summaryMap[item.kind!] = {
            namespaceName: item.kind,
            details: [],
            isTotal: false,
          };
        }
        const existingDetail = summaryMap[item.kind!].details!.find((detail) => detail.id === item.severityId);
        if (existingDetail) {
          existingDetail.totalCount! += item.totalCount ?? 0;
          existingDetail.distinctCount! += item.distinctCount ?? 0;
        } else {
          summaryMap[item.kind!].details!.push({
            id: item.severityId,
            totalCount: item.totalCount,
            distinctCount: item.distinctCount,
          });
        }
      });

    this.severitiesSummariesNamespace = Object.values(summaryMap);
    this.barchartDataKindByNs.set(
      PrimeNgChartUtils.getDataForHorizontalBarChartByNamespace(
        this.severitiesSummariesNamespace,
        this.showDistinctValues(),
      ),
    );
    this.barchartDataKindBySev.set(
      PrimeNgChartUtils.getDataForHorizontalBarChartBySeverity(
        this.severitiesSummariesNamespace,
        this.showDistinctValues(),
      ),
    );
  }

  private computeStatisticsByKind() {
    if (!this.configAuditReportSummaryDtos) {
      return;
    }

    const summaryMap: { [key: string]: SeveritiesSummary } = {};
    this.configAuditReportSummaryDtos
      .filter((dto) => dto.namespaceName !== '')
      .forEach((item) => {
        if (!summaryMap[item.namespaceName!]) {
          summaryMap[item.namespaceName!] = {
            namespaceName: item.namespaceName,
            details: [],
            isTotal: false,
          };
        }
        const existingDetail = summaryMap[item.namespaceName!].details!.find((detail) => detail.id === item.severityId);
        if (existingDetail) {
          existingDetail.totalCount! += item.totalCount ?? 0;
          existingDetail.distinctCount! += item.distinctCount ?? 0;
        } else {
          summaryMap[item.namespaceName!].details!.push({
            id: item.severityId,
            totalCount: item.totalCount,
            distinctCount: item.distinctCount,
          });
        }
      });

    this.severitiesSummariesNamespace = Object.values(summaryMap);
    this.barchartDataNsByNs.set(
      PrimeNgChartUtils.getDataForHorizontalBarChartByNamespace(
        this.severitiesSummariesNamespace,
        this.showDistinctValues(),
      ),
    );
    this.barchartDataNsBySev.set(
      PrimeNgChartUtils.getDataForHorizontalBarChartBySeverity(
        this.severitiesSummariesNamespace,
        this.showDistinctValues(),
      ),
    );
  }
}
