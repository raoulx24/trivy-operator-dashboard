import { Component, inject } from '@angular/core';

import { ClusterInfraAssessmentReportDenormalizedDto } from '../../../api/models/cluster-infra-assessment-report-denormalized-dto';
import { ClusterInfraAssessmentReportService } from '../../../api/services/cluster-infra-assessment-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

import { infraAssessmentReportDenormalizedColumns } from '../constants/infra-assessment-reports.constants';

import { TrivyReportDataPageBase } from '../abstracts/trivy-report-data-page-base';

@Component({
  selector: 'app-cluster-infra-assessment-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './cluster-infra-assessment-reports-detailed.component.html',
  styleUrl: './cluster-infra-assessment-reports-detailed.component.scss',
})
export class ClusterInfraAssessmentReportsDetailedComponent extends TrivyReportDataPageBase<ClusterInfraAssessmentReportDenormalizedDto> {
  readonly csvFileName: string = 'Cluster.Infra.Assessment.Reports';

  readonly trivyTableColumns: TrivyTableColumn[] = [...infraAssessmentReportDenormalizedColumns];

  private readonly dataDtoService = inject(ClusterInfraAssessmentReportService);

  protected readonly dataDtosLoader =
    () => this.dataDtoService.getClusterInfraAssessmentReportDenormalizedDtos();
}
