import { Component, inject, OnInit } from '@angular/core';

import { ConfigAuditReportDenormalizedDto } from '../../../api/models/config-audit-report-denormalized-dto';
import { SeverityDto } from '../../../api/models/severity-dto';
import { ConfigAuditReportService } from '../../../api/services/config-audit-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { TrivyReportsDetailedBase } from '../abstracts/trivy-reports-detailed-base';
import { configAuditReportDenormalizedColumns } from '../constants/config-audit-reports.constants';
import { namespacedColumns } from '../constants/generic.constants';

@Component({
  selector: 'app-config-audit-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './config-audit-reports-detailed.component.html',
  styleUrl: './config-audit-reports-detailed.component.scss',
})
export class ConfigAuditReportsDetailedComponent extends TrivyReportsDetailedBase implements OnInit {
  dataDtos: ConfigAuditReportDenormalizedDto[] = [];
  severityDtos: SeverityDto[] = [];
  activeNamespaces: string[] = [];
  isLoading: boolean = false;

  csvFileName: string = 'Config.Audit.Reports';

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...configAuditReportDenormalizedColumns];

  private readonly dataDtoService = inject(ConfigAuditReportService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  public getTableDataDtos() {
    this.isLoading = true;
    this.dataDtoService.getConfigAuditReportDenormalizedDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  onGetDataDtos(dtos: ConfigAuditReportDenormalizedDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace ?? 'N/A'))).sort();
    this.isLoading = false;
  }
}
