import { ChangeDetectionStrategy, Component, inject, input, OnInit, signal } from '@angular/core';

import { ClusterRbacAssessmentReportSummaryDto } from '@api/models/cluster-rbac-assessment-report-summary-dto';
import { ClusterRbacAssessmentReportService } from '@api/services/cluster-rbac-assessment-report.service';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';

import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-dashboard-cluster-rbac-assessment-reports',
  standalone: true,
  imports: [TableModule, SeverityNameByIdPipe],
  templateUrl: './cluster-rbac.component.html',
  styleUrl: './cluster-rbac.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardClusterRbacAssessmentReportsComponent implements OnInit {
  clusterRbacAssessmentReportSummaryDtos = signal<ClusterRbacAssessmentReportSummaryDto[]>([]);

  showDistinctValues = input.required<boolean>();

  private readonly clusterRbacAssessmentReportService = inject(ClusterRbacAssessmentReportService);

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
    this.clusterRbacAssessmentReportSummaryDtos.set(dtos);
  }
}
