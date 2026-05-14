import { Component, inject, OnInit } from '@angular/core';

import { ClusterRbacAssessmentReportDenormalizedDto } from '../../../api/models/cluster-rbac-assessment-report-denormalized-dto';
import { SeverityDto } from '../../../api/models/severity-dto';
import { ClusterRbacAssessmentReportService } from '../../../api/services/cluster-rbac-assessment-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { TrivyReportsDetailedBase } from '../abstracts/trivy-reports-detailed-base';
import { rbacAssessmentReportDenormalizedColumns } from '../constants/rbac-assessment-reports.constants';

@Component({
  selector: 'app-cluster-rbac-assessment-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './cluster-rbac-assessment-reports-detailed.component.html',
  styleUrl: './cluster-rbac-assessment-reports-detailed.component.scss',
})
export class ClusterRbacAssessmentReportsDetailedComponent extends TrivyReportsDetailedBase implements OnInit {
  public dataDtos: ClusterRbacAssessmentReportDenormalizedDto[] = [];
  public severityDtos: SeverityDto[] = [];
  public activeNamespaces: string[] = [];
  public isLoading: boolean = false;

  public csvFileName: string = 'Cluster.Rbac.Assessment.Reports';

  public trivyTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportDenormalizedColumns];

  private readonly dataDtoService = inject(ClusterRbacAssessmentReportService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  public getTableDataDtos() {
    this.isLoading = true;
    this.dataDtoService.getClusterRbacAssessmentReportDenormalizedDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  onGetDataDtos(dtos: ClusterRbacAssessmentReportDenormalizedDto[]) {
    this.dataDtos = dtos;
    this.isLoading = false;
  }
}
