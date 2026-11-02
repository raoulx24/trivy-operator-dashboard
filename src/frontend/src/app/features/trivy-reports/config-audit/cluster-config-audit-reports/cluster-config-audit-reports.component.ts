import { Component, inject, OnInit } from '@angular/core';

import { ClusterConfigAuditReportDto } from '@api/models/cluster-config-audit-report-dto';
import { ClusterConfigAuditReportService } from '@api/services/cluster-config-audit-report.service';
import { GenericMasterDetailComponent } from '@shared/ui/generic-master-detail/generic-master-detail.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';
import {
  configAuditReportColumns,
  configAuditReportComparedTableColumns,
  configAuditReportDetailColumns,
} from '../config-audit-reports.constants';

import { SecurityAssessmentReportDetailDto } from '@api/models/security-assessment-report-detail-dto';
import {
  TrivyReportsCompareDialogComponent
} from '@shared/ui/trivy-reports-compare-dialog/trivy-reports-compare-dialog.component';
import {
  ClusteredScopedTrivyReportDataPageBase
} from '@shared/abstracts/pages/clustered-scoped-trivy-report-data-page-base';

@Component({
  selector: 'app-cluster-config-audit-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, TrivyReportsCompareDialogComponent],
  templateUrl: './cluster-config-audit-reports.component.html',
  styleUrl: './cluster-config-audit-reports.component.scss',
})
export class ClusterConfigAuditReportsComponent
  extends ClusteredScopedTrivyReportDataPageBase<ClusterConfigAuditReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  protected override detailedPageRoute: string = 'cluster-config-audit-reports-detailed';
  protected friendlyReportName: string = 'Cluster Config Audit Reports';

  mainTableColumns: TrivyTableColumn[] = [...configAuditReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...configAuditReportDetailColumns];

  comparedTableColumns: TrivyTableColumn[] = [...configAuditReportComparedTableColumns];

  private readonly dataDtoService = inject(ClusterConfigAuditReportService);

  protected override dataDtosLoader = () => this.dataDtoService.getClusterConfigAuditReportDtos();

  ngOnInit() {
    this.initialize();
  }
}
