import { Component } from '@angular/core';

import { ClusterComplianceReportDto } from '../../api/models/cluster-compliance-report-dto';
import { SeverityDto } from '../../api/models/severity-dto';
import { ClusterComplianceReportService } from '../../api/services/cluster-compliance-report.service';

import { ClusterComplianceReportDenormalizedDto } from '../../api/models';
import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../trivy-table/trivy-table.types';

@Component({
  selector: 'app-cluster-compliance-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './cluster-compliance-reports-detailed.component.html',
  styleUrl: './cluster-compliance-reports-detailed.component.scss',
})
export class ClusterComplianceReportsDetailedComponent {
  public dataDtos?: ClusterComplianceReportDto[] | null;
  public severityDtos: SeverityDto[] = [];
  public isLoading: boolean = false;

  public csvFileName: string = 'Cluster.Compliance.Reports';

  public trivyTableColumns: TrivyTableColumn[];

  constructor(private dataDtoService: ClusterComplianceReportService) {
    this.getTableDataDtos();

    this.trivyTableColumns = [
      {
        field: 'name',
        header: 'Name',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 120px; max-width: 120px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'description',
        header: 'Description',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 260px; max-width: 260px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'platform',
        header: 'Platf',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 130px; max-width: 130px; white-space: normal;',
        renderType: 'standard',
      },
      //{
      //  field: 'relatedResources',
      //  header: 'RelatedResources',
      //  isFilterable: true,
      //  isSortable: true,
      //  multiSelectType: 'none',
      //  style: 'width: 240px; max-width: 240px; white-space: normal;',
      //  renderType: 'standard',
      //},
      {
        field: 'title',
        header: 'Title',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 290px; max-width: 290px; white-space: normal;',
        renderType: 'link',
        extraFields: ['relatedResources'],
      },
      {
        field: 'type',
        header: 'Type',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 110px; max-width: 110px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'version',
        header: 'Ver',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 100px; max-width: 100px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'cron',
        header: 'Cron',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 110px; max-width: 110px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'reportType',
        header: 'RepType',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 140px; max-width: 140px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'totalPassCount',
        header: 'PassCount',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 150px; max-width: 150px; white-space: normal; text-align: right;',
        renderType: 'standard',
      },
      {
        field: 'totalFailCount',
        header: 'FailCount',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 150px; max-width: 150px; white-space: normal; text-align: right;',
        renderType: 'standard',
      },
      {
        field: 'updateTimestamp',
        header: 'Updated',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 140px; max-width: 140px; white-space: normal;',
        renderType: 'date',
      },
      {
        field: 'detailId',
        header: 'Id',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 110px; max-width: 110px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'detailName',
        header: 'Detail Name',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 340px; max-width: 340px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'detailDescription',
        header: 'Detail Description',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 380px; max-width: 380px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'severityId',
        header: 'Sev',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'severities',
        style: 'width: 90px; max-width: 90px;',
        renderType: 'severityBadge',
      },
      {
        field: 'checks',
        header: 'Checks',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 140px; max-width: 140px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'commands',
        header: 'Commands',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 150px; max-width: 150px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'totalFail',
        header: 'TotFail',
        isFilterable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 140px; max-width: 140px; white-space: normal; text-align: right;',
        renderType: 'severityValue',
        extraFields: ['-1'],
      },
    ];
  }

  getTableDataDtos() {
    this.isLoading = true;
    this.dataDtoService.getClusterComplianceReportDenormalizedDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  onGetDataDtos(dtos: ClusterComplianceReportDenormalizedDto[]) {
    this.dataDtos = dtos;
    this.isLoading = false;
  }
}
