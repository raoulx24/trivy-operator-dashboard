import { Component, inject, OnInit } from '@angular/core';

import { InfraAssessmentReportDenormalizedDto } from '../../../api/models/infra-assessment-report-denormalized-dto';
import { InfraAssessmentReportService } from '../../../api/services/infra-assessment-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { namespacedColumns } from '../constants/generic.constants';
import { infraAssessmentReportDenormalizedColumns } from '../constants/infra-assessment-reports.constants';
import { NamespacedDataPageBase } from '../abstracts/pages/namespaced-trivy-report-data-page-base';

@Component({
  selector: 'app-infra-assessment-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './infra-assessment-reports-detailed.component.html',
  styleUrl: './infra-assessment-reports-detailed.component.scss',
})
export class InfraAssessmentReportsDetailedComponent extends NamespacedDataPageBase<InfraAssessmentReportDenormalizedDto> implements OnInit {
  csvFileName: string = 'Infra.Assessment.Reports';

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...infraAssessmentReportDenormalizedColumns];

  private readonly dataDtoService = inject(InfraAssessmentReportService);

  protected readonly dataDtosLoader = () => this.dataDtoService.getInfraAssessmentReportDenormalizedDtos();

  ngOnInit() {
    this.initialize();
  }
}
