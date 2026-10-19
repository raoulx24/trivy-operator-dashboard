import { Component, inject, OnInit } from '@angular/core';

import { InfraAssessmentReportDto } from '@api/models/infra-assessment-report-dto';
import { InfraAssessmentReportService } from '@api/services/infra-assessment-report.service';
import { GenericMasterDetailComponent } from '@shared/ui/generic-master-detail/generic-master-detail.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';
import { namespacedColumns } from '@shared/constants/generic.constants';
import {
  infraAssessmentReportColumns,
  infraAssessmentReportComparedTableColumns,
  infraAssessmentReportDetailColumns,
} from '../infra-assessment-reports.constants';

import { DialogModule } from 'primeng/dialog';
import { NamespacedResourceTrivyReportDataPageBase } from '@shared/abstracts/pages/namespaced-trivy-report-data-page-base';
import { SecurityAssessmentReportDetailDto } from '@api/models/security-assessment-report-detail-dto';
import {
  TrivyReportsCompareDialogComponent
} from '@shared/ui/trivy-reports-compare-dialog/trivy-reports-compare-dialog.component';

@Component({
  selector: 'app-infra-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, TrivyReportsCompareDialogComponent],
  templateUrl: './infra-assessment-reports.component.html',
  styleUrl: './infra-assessment-reports.component.scss',
})
export class InfraAssessmentReportsComponent
  extends NamespacedResourceTrivyReportDataPageBase<InfraAssessmentReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  protected override detailedPageRoute: string = 'infra-assessment-reports-detailed';
  protected override friendlyReportName: string = 'Infra Assessment Reports';
  mainTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...infraAssessmentReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...infraAssessmentReportDetailColumns];

  comparedTableColumns: TrivyTableColumn[] = [...infraAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(InfraAssessmentReportService);

  protected override dataDtosLoader = () => this.dataDtoService.getInfraAssessmentReportDtos();

  ngOnInit() {
    this.initialize();
  }
}
