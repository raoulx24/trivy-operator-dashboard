import { Component, inject, OnInit } from '@angular/core';

import { ConfigAuditReportDto } from '../../../api/models/config-audit-report-dto';
import { ConfigAuditReportService } from '../../../api/services/config-audit-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import {
  configAuditReportColumns,
  configAuditReportComparedTableColumns,
  configAuditReportDetailColumns,
} from '../constants/config-audit-reports.constants';
import { namespacedColumns } from '../constants/generic.constants';

import { DialogModule } from 'primeng/dialog';
import { NamespacedResourceTrivyReportDataPageBase } from '../abstracts/pages/namespaced-trivy-report-data-page-base';
import { SecurityAssessmentReportDetailDto } from '../../../api/models/security-assessment-report-detail-dto';
import {
  TrivyReportsCompareDialogComponent
} from '../../ui-elements/trivy-reports-compare-dialog/trivy-reports-compare-dialog.component';
import { PairedOptionsDto } from '../../ui-elements/paired-options-selector/paired-options-selector.types';

@Component({
  selector: 'app-config-audit-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, TrivyReportsCompareDialogComponent],
  templateUrl: './config-audit-reports.component.html',
  styleUrl: './config-audit-reports.component.scss',
})
export class ConfigAuditReportsComponent
  extends NamespacedResourceTrivyReportDataPageBase<ConfigAuditReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  protected detailedPageRoute: string = 'config-audit-reports-detailed';
  protected friendlyReportName: string = 'Config Audit Reports';

  mainTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...configAuditReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...configAuditReportDetailColumns];

  comparedTableColumns: TrivyTableColumn[] = [...configAuditReportComparedTableColumns];

  private readonly dataDtoService = inject(ConfigAuditReportService);

  protected override dataDtosLoader = () => this.dataDtoService.getConfigAuditReportDtos();

  ngOnInit() {
    this.initialize();
  }

  protected override prepareCompareDataDtos(): PairedOptionsDto[] {
  return this.dataDtos
    .filter((tr) => this.hasSeverities(tr))
    .map((tr) => ({
      uid: tr.uid ?? '',
      firstOption: tr.resourceNamespace ?? '',
      secondOption: tr.resourceName,
      group: tr.resourceKind,
    }));
}
}
