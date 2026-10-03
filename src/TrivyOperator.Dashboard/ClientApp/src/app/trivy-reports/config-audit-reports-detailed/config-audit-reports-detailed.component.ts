import { Component, inject } from '@angular/core';

import { ConfigAuditReportDenormalizedDto } from '../../../api/models/config-audit-report-denormalized-dto';
import { ConfigAuditReportService } from '../../../api/services/config-audit-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { configAuditReportDenormalizedColumns } from '../constants/config-audit-reports.constants';
import { namespacedColumns } from '../constants/generic.constants';
import { NamespacedDataTrivyReportDataPageBase } from '../abstracts/namespaced-trivy-report-data-page-base';

@Component({
  selector: 'app-config-audit-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './config-audit-reports-detailed.component.html',
  styleUrl: './config-audit-reports-detailed.component.scss',
})
export class ConfigAuditReportsDetailedComponent extends NamespacedDataTrivyReportDataPageBase<ConfigAuditReportDenormalizedDto> {
  csvFileName: string = 'Config.Audit.Reports';

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...configAuditReportDenormalizedColumns];

  private readonly dataDtoService = inject(ConfigAuditReportService);

  protected readonly dataDtosLoader =
    () => this.dataDtoService.getConfigAuditReportDenormalizedDtos();
}
