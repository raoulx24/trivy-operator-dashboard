import { Component, inject, OnInit } from '@angular/core';

import { ClusterRbacAssessmentReportDto } from '../../../api/models/cluster-rbac-assessment-report-dto';
import { ClusterRbacAssessmentReportService } from '../../../api/services/cluster-rbac-assessment-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import {
  rbacAssessmentReportColumns,
  rbacAssessmentReportComparedTableColumns,
  rbacAssessmentReportDetailColumns,
} from '../constants/rbac-assessment-reports.constants';
import { SecurityAssessmentReportDetailDto } from '../../../api/models/security-assessment-report-detail-dto';
import {
  TrivyReportsCompareDialogComponent
} from '../../ui-elements/trivy-reports-compare-dialog/trivy-reports-compare-dialog.component';
import {
  ClusteredScopedTrivyReportDataPageBase
} from '../abstracts/pages/clustered-scoped-trivy-report-data-page-base';

@Component({
  selector: 'app-cluster-rbac-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, TrivyReportsCompareDialogComponent],
  templateUrl: './cluster-rbac-assessment-reports.component.html',
  styleUrl: './cluster-rbac-assessment-reports.component.scss',
})
export class ClusterRbacAssessmentReportsComponent
  extends ClusteredScopedTrivyReportDataPageBase<ClusterRbacAssessmentReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  protected detailedPageRoute: string = 'cluster-rbac-assessment-reports-detailed';
  protected friendlyReportName: string = 'Cluster RBAC Assessment Reports';

  mainTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportDetailColumns];

  comparedTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(ClusterRbacAssessmentReportService);

  protected override dataDtosLoader = () => this.dataDtoService.getClusterRbacAssessmentReportDtos();

  ngOnInit() {
    this.initialize();
  }
}
