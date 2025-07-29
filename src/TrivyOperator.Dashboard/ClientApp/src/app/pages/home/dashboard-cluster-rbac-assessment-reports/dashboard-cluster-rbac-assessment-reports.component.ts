import { Component, input, OnInit } from '@angular/core';

import { ClusterRbacAssessmentReportSummaryDto } from '../../../../api/models/cluster-rbac-assessment-report-summary-dto';
import { ClusterRbacAssessmentReportService } from '../../../../api/services/cluster-rbac-assessment-report.service';
import { SeverityNameByIdPipe } from '../../../pipes/severity-name-by-id.pipe';

import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-dashboard-cluster-rbac-assessment-reports',
  standalone: true,
  imports: [TableModule, SeverityNameByIdPipe],
  templateUrl: './dashboard-cluster-rbac-assessment-reports.component.html',
  styleUrl: './dashboard-cluster-rbac-assessment-reports.component.scss',
})
export class DashboardClusterRbacAssessmentReportsComponent implements OnInit {
  clusterRbacAssessmentReportSummaryDtos: ClusterRbacAssessmentReportSummaryDto[] = [];

  showDistinctValues = input.required<boolean>();

  constructor(private clusterRbacAssessmentReportService: ClusterRbacAssessmentReportService) { }

  ngOnInit() {
    this.loadData();
  }

  private loadData(): void {
    this.clusterRbacAssessmentReportService.getClusterRbacAssessmentReportSummaryDtos().subscribe({
      next: (res) => this.onDtos(res),
      error: (err) => console.error(err),
    });
  }

  private onDtos(dtos: ClusterRbacAssessmentReportSummaryDto[]) {
    this.clusterRbacAssessmentReportSummaryDtos = dtos;
  }
}
