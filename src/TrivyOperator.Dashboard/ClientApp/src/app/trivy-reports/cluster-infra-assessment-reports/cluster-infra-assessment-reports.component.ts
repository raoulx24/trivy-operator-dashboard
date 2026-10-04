import { Component, inject, OnInit } from '@angular/core';

import { ClusterInfraAssessmentReportDto } from '../../../api/models/cluster-infra-assessment-report-dto';
import { ClusterInfraAssessmentReportService } from '../../../api/services/cluster-infra-assessment-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import {
  infraAssessmentReportColumns,
  infraAssessmentReportComparedTableColumns,
  infraAssessmentReportDetailColumns,
} from '../constants/infra-assessment-reports.constants';

import { SecurityAssessmentReportDetailDto } from '../../../api/models/security-assessment-report-detail-dto';
import {
  TrivyReportsCompareDialogComponent
} from '../../ui-elements/trivy-reports-compare-dialog/trivy-reports-compare-dialog.component';
import {
  ClusteredScopedTrivyReportDataPageBase
} from '../abstracts/pages/clustered-scoped-trivy-report-data-page-base';

@Component({
  selector: 'app-cluster-infra-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, TrivyReportsCompareDialogComponent],
  templateUrl: './cluster-infra-assessment-reports.component.html',
  styleUrl: './cluster-infra-assessment-reports.component.scss',
})
export class ClusterInfraAssessmentReportsComponent
  extends ClusteredScopedTrivyReportDataPageBase<ClusterInfraAssessmentReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  protected override detailedPageRoute: string = 'cluster-infra-assessment-reports-detailed';
  protected friendlyReportName: string = 'Cluster Infra Assessment Reports';

  mainTableColumns: TrivyTableColumn[] = [...infraAssessmentReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...infraAssessmentReportDetailColumns];

  comparedTableColumns: TrivyTableColumn[] = [...infraAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(ClusterInfraAssessmentReportService);

  protected override dataDtosLoader = () => this.dataDtoService.getClusterInfraAssessmentReportDtos();

  ngOnInit() {
    this.initialize();
  }
}
