import { Component, inject, OnInit } from '@angular/core';

import { ClusterInfraAssessmentReportDenormalizedDto } from '../../../api/models/cluster-infra-assessment-report-denormalized-dto';
import { SeverityDto } from '../../../api/models/severity-dto';
import { ClusterInfraAssessmentReportService } from '../../../api/services/cluster-infra-assessment-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { TrivyReportsDetailedBase } from '../abstracts/trivy-reports-detailed-base';
import { infraAssessmentReportDenormalizedColumns } from '../constants/infra-assessment-reports.constants';

@Component({
  selector: 'app-cluster-infra-assessment-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './cluster-infra-assessment-reports-detailed.component.html',
  styleUrl: './cluster-infra-assessment-reports-detailed.component.scss',
})
export class ClusterInfraAssessmentReportsDetailedComponent extends TrivyReportsDetailedBase implements OnInit {
  dataDtos?: ClusterInfraAssessmentReportDenormalizedDto[];
  severityDtos: SeverityDto[] = [];
  isLoading: boolean = false;

  csvFileName: string = 'Cluster.Infra.Assessment.Reports';

  trivyTableColumns: TrivyTableColumn[] = [...infraAssessmentReportDenormalizedColumns];

  private readonly dataDtoService = inject(ClusterInfraAssessmentReportService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  public getTableDataDtos() {
    this.isLoading = true;
    this.dataDtoService.getClusterInfraAssessmentReportDenormalizedDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  onGetDataDtos(dtos: ClusterInfraAssessmentReportDenormalizedDto[]) {
    this.dataDtos = dtos;
    this.isLoading = false;
  }
}
