import { Component, inject, OnInit } from '@angular/core';

import { InfraAssessmentReportDenormalizedDto } from '../../../api/models/infra-assessment-report-denormalized-dto';
import { SeverityDto } from '../../../api/models/severity-dto';
import { InfraAssessmentReportService } from '../../../api/services/infra-assessment-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { infraAssessmentReportDenormalizedColumns } from '../constants/infra-assessment-reports.constants';
import { namespacedColumns } from '../constants/generic.constants';

@Component({
  selector: 'app-infra-assessment-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './infra-assessment-reports-detailed.component.html',
  styleUrl: './infra-assessment-reports-detailed.component.scss',
})
export class InfraAssessmentReportsDetailedComponent implements OnInit {
  dataDtos?: InfraAssessmentReportDenormalizedDto[];
  severityDtos: SeverityDto[] = [];
  activeNamespaces: string[] = [];
  isLoading: boolean = false;

  csvFileName: string = 'Infra.Assessment.Reports';

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...infraAssessmentReportDenormalizedColumns];

  private readonly dataDtoService = inject(InfraAssessmentReportService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  public getTableDataDtos() {
    this.isLoading = true;
    this.dataDtoService.getInfraAssessmentReportDenormalizedDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  onGetDataDtos(dtos: InfraAssessmentReportDenormalizedDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace ?? 'N/A'))).sort();
    this.isLoading = false;
  }
}
