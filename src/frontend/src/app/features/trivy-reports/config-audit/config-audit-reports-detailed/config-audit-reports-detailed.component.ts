import { Component, inject, OnInit } from '@angular/core';

import { ConfigAuditReportDenormalizedDto } from '@api/models/config-audit-report-denormalized-dto';
import { ConfigAuditReportService } from '@api/services/config-audit-report.service';

import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';
import { configAuditReportDenormalizedColumns } from '../config-audit-reports.constants';
import { namespacedColumns } from '@shared/constants/generic.constants';
import { NamespacedDataPageBase } from '@shared/abstracts/pages/namespaced-trivy-report-data-page-base';

@Component({
  selector: 'app-config-audit-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './config-audit-reports-detailed.component.html',
  styleUrl: './config-audit-reports-detailed.component.scss',
})
export class ConfigAuditReportsDetailedComponent
  extends NamespacedDataPageBase<ConfigAuditReportDenormalizedDto>
  implements OnInit
{
  csvFileName: string = 'Config.Audit.Reports';

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...configAuditReportDenormalizedColumns];

  private readonly dataDtoService = inject(ConfigAuditReportService);

  protected readonly dataDtosLoader = () => this.dataDtoService.getConfigAuditReportDenormalizedDtos();

  ngOnInit() {
    this.initialize();
  }
}
