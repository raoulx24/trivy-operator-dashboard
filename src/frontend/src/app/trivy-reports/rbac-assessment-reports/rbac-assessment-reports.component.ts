import { Component, inject, OnInit } from '@angular/core';

import { RbacAssessmentReportDto } from '../../../api/models/rbac-assessment-report-dto';
import { RbacAssessmentReportService } from '../../../api/services/rbac-assessment-report.service';
import { namespacedColumns } from '../constants/generic.constants';
import {
  rbacAssessmentReportColumns,
  rbacAssessmentReportComparedTableColumns,
  rbacAssessmentReportDetailColumns,
} from '../constants/rbac-assessment-reports.constants';

import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

import { DialogModule } from 'primeng/dialog';
import {NamespacedResourceTrivyReportDataPageBase} from "../abstracts/pages/namespaced-trivy-report-data-page-base";
import {SecurityAssessmentReportDetailDto} from "../../../api/models/security-assessment-report-detail-dto";
import {
  TrivyReportsCompareDialogComponent
} from '../../ui-elements/trivy-reports-compare-dialog/trivy-reports-compare-dialog.component';

@Component({
  selector: 'app-rbac-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, TrivyReportsCompareDialogComponent],
  templateUrl: './rbac-assessment-reports.component.html',
  styleUrl: './rbac-assessment-reports.component.scss',
})
export class RbacAssessmentReportsComponent
  extends NamespacedResourceTrivyReportDataPageBase<RbacAssessmentReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  protected override detailedPageRoute: string = 'rbac-assessment-reports-detailed';
  protected override friendlyReportName: string = 'RBAC Assessment Reports';
  mainTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...rbacAssessmentReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportDetailColumns];

  comparedTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(RbacAssessmentReportService);

  protected override dataDtosLoader = () => this.dataDtoService.getRbacAssessmentReportDtos();

  ngOnInit() {
    this.initialize();
  }
}
