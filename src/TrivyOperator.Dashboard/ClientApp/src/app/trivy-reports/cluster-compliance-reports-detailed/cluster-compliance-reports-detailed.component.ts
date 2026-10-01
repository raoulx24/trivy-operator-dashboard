import { Component, inject } from '@angular/core';

import { ClusterComplianceReportDenormalizedDto } from '../../../api/models';
import { ClusterComplianceReportService } from '../../../api/services/cluster-compliance-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

import { clusterComplianceReportDenormalizedColumns } from '../constants/cluster-compliance-reports.constants';

import { TrivyReportDataPageBase } from '../abstracts/trivy-report-data-page-base';

@Component({
  selector: 'app-cluster-compliance-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './cluster-compliance-reports-detailed.component.html',
  styleUrl: './cluster-compliance-reports-detailed.component.scss',
})
export class ClusterComplianceReportsDetailedComponent extends TrivyReportDataPageBase<ClusterComplianceReportDenormalizedDto> {
  readonly csvFileName: string = 'Cluster.Compliance.Reports';

  readonly trivyTableColumns: TrivyTableColumn[] = [...clusterComplianceReportDenormalizedColumns];

  private readonly dataDtoService = inject(ClusterComplianceReportService);

  protected readonly dataDtosLoader =
    () => this.dataDtoService.getClusterComplianceReportDenormalizedDtos();
}
