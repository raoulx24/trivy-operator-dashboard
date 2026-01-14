import { Component, inject, OnInit } from '@angular/core';

import { ClusterComplianceReportDto } from '../../../api/models/cluster-compliance-report-dto';
import { ClusterComplianceReportService } from '../../../api/services/cluster-compliance-report.service';
import { DataPageBase } from '../../abstracts/data-page-base';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import {
  TrivyFilterData,
  TrivyTableColumn,
  TrivyTableExpandRowData,
} from '../../ui-elements/trivy-table/trivy-table.types';
import {
  clusterComplianceReportColumns,
  clusterComplianceReportDetailColumns,
} from '../constants/cluster-compliance-reports.constants';

@Component({
  selector: 'app-cluster-compliance-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent],
  templateUrl: './cluster-compliance-reports.component.html',
  styleUrl: './cluster-compliance-reports.component.scss',
})
export class ClusterComplianceReportsComponent extends DataPageBase implements OnInit {
  dataDtos: ClusterComplianceReportDto[] = [];

  mainTableColumns: TrivyTableColumn[] = [...clusterComplianceReportColumns];
  isMainTableLoading: boolean = true;

  detailsTableColumns: TrivyTableColumn[] = [...clusterComplianceReportDetailColumns];

  private readonly dataDtoService = inject(ClusterComplianceReportService);

  ngOnInit() {
    this.getDataDtos();
  }

  private getDataDtos() {
    this.isMainTableLoading = true;
    this.dataDtoService.getClusterComplianceReportDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  private onGetDataDtos(dtos: ClusterComplianceReportDto[]) {
    this.dataDtos = dtos;
    this.isMainTableLoading = false;
  }

  public onRefreshRequested(_event: TrivyFilterData) {
    this.getDataDtos();
  }

  rowExpandResponse?: TrivyTableExpandRowData<ClusterComplianceReportDto>;
  onRowExpandChange(dto: ClusterComplianceReportDto) {
    this.rowExpandResponse = {
      rowKey: dto,
      colStyles: [
        { width: '70px', 'min-width': '70px', height: '50px' },
        { 'white-space': 'normal', display: 'flex', 'align-items': 'center', height: '50px' },
      ],
      details: [
        [
          { label: 'Description' },
          { label: dto.description ?? '' },
        ],
        [
          { label: 'Platform' },
          { label: dto.platform ?? '' },
        ],
        [
          { label: 'Type' },
          { label: dto.type ?? '' },
        ],
        [
          { label: 'Version' },
          { label: dto.version ?? '' },
        ],
        [
          { label: 'Report Type' },
          { label: dto.reportType ?? '' },
        ],
        [
          { label: 'Checks' },
          { label: `${dto.totalFailCount ?? 0} failed vs ${dto.totalPassCount ?? 0} passed` },
        ],
        [
          { label: 'Cron' },
          { label: '', cron: dto.cron ?? undefined },
        ],
        [
          { label: 'Updated' },
          { label: '', localTime: dto.updateTimestamp ?? undefined },
        ],
        [
          { label: 'Related Resources' },
          {
            label: 'on',
            url: {
              text: this.getHostname(dto.relatedResources?.[0] ?? '') ?? 'link',
              link: dto.relatedResources?.[0] ?? '',
            },
          },
        ],
      ],
    };
  }

  getHostname(urlString: string): string | undefined {
    try {
      const url = new URL(urlString);
      return url.hostname;
    } catch (error) {
      console.error('Invalid URL: ', urlString, error);
      return undefined;
    }
  }
}
