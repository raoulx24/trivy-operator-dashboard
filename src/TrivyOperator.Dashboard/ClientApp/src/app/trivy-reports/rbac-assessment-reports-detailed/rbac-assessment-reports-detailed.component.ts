import { Component, inject, OnInit } from '@angular/core';

import { RbacAssessmentReportDenormalizedDto } from '../../../api/models/rbac-assessment-report-denormalized-dto';
import { RbacAssessmentReportService } from '../../../api/services/rbac-assessment-report.service';
import { rbacAssessmentReportDenormalizedColumns } from '../constants/rbac-assessment-reports.constants';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { namespacedColumns } from '../constants/generic.constants';
import { NamespacedDataPageBase } from '../abstracts/pages/namespaced-trivy-report-data-page-base';

@Component({
  selector: 'app-rbac-assessment-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './rbac-assessment-reports-detailed.component.html',
  styleUrl: './rbac-assessment-reports-detailed.component.scss',
})
export class RbacAssessmentReportsDetailedComponent extends NamespacedDataPageBase<RbacAssessmentReportDenormalizedDto> implements OnInit {
  csvFileName: string = 'Rbac.Assessment.Reports';

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...rbacAssessmentReportDenormalizedColumns];

  private readonly dataDtoService = inject(RbacAssessmentReportService);

  protected readonly dataDtosLoader = () => this.dataDtoService.getRbacAssessmentReportDenormalizedDtos();

  ngOnInit() {
    this.initialize();
  }
}
