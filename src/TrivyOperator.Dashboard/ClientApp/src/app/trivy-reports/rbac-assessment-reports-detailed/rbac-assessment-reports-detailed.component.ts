import { Component, inject, OnInit } from '@angular/core';

import { RbacAssessmentReportDenormalizedDto } from '../../../api/models/rbac-assessment-report-denormalized-dto';
import { SeverityDto } from '../../../api/models/severity-dto';
import { RbacAssessmentReportService } from '../../../api/services/rbac-assessment-report.service';
import { rbacAssessmentReportDenormalizedColumns } from '../constants/rbac-assessment-reports.constants';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { namespacedColumns } from '../constants/generic.constants';
import { TrivyReportsDetailedBase } from '../abstracts/trivy-reports-detailed-base';

@Component({
  selector: 'app-rbac-assessment-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './rbac-assessment-reports-detailed.component.html',
  styleUrl: './rbac-assessment-reports-detailed.component.scss',
})
export class RbacAssessmentReportsDetailedComponent  extends TrivyReportsDetailedBase implements OnInit {
  dataDtos?: RbacAssessmentReportDenormalizedDto[] | null;
  severityDtos: SeverityDto[] = [];
  activeNamespaces: string[] = [];
  isLoading: boolean = false;

  csvFileName: string = 'Rbac.Assessment.Reports';

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...rbacAssessmentReportDenormalizedColumns];

  private readonly dataDtoService = inject(RbacAssessmentReportService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  getTableDataDtos() {
    this.isLoading = true;
    this.dataDtoService.getRbacAssessmentReportDenormalizedDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  private onGetDataDtos(dtos: RbacAssessmentReportDenormalizedDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array
      .from(new Set(dtos.map(dto => dto.resourceNamespace ?? "N/A")))
      .sort();
    this.isLoading = false;
  }
}
