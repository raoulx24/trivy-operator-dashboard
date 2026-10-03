import { Component, inject, OnInit } from '@angular/core';

import { ClusterRbacAssessmentReportDenormalizedDto } from '../../../api/models/cluster-rbac-assessment-report-denormalized-dto';
import { ClusterRbacAssessmentReportService } from '../../../api/services/cluster-rbac-assessment-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

import { rbacAssessmentReportDenormalizedColumns } from '../constants/rbac-assessment-reports.constants';

import { DataPageBase } from '../abstracts/pages/data-page-base';

@Component({
  selector: 'app-cluster-rbac-assessment-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './cluster-rbac-assessment-reports-detailed.component.html',
  styleUrl: './cluster-rbac-assessment-reports-detailed.component.scss',
})
export class ClusterRbacAssessmentReportsDetailedComponent
  extends DataPageBase<ClusterRbacAssessmentReportDenormalizedDto>
  implements OnInit
{
  readonly csvFileName: string = 'Cluster.Rbac.Assessment.Reports';

  readonly trivyTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportDenormalizedColumns];

  private readonly dataDtoService = inject(ClusterRbacAssessmentReportService);

  protected readonly dataDtosLoader = () => this.dataDtoService.getClusterRbacAssessmentReportDenormalizedDtos();

  ngOnInit() {
    this.initialize();
  }
}
