import { Component, inject, OnInit } from '@angular/core';

import { ClusterConfigAuditReportDenormalizedDto } from '@api/models/cluster-config-audit-report-denormalized-dto';
import { ClusterConfigAuditReportService } from '@api/services/cluster-config-audit-report.service';

import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { configAuditReportDenormalizedColumns } from '../config-audit-reports.constants';

import { DataPageBase } from '@shared/abstracts/pages/data-page-base';

@Component({
  selector: 'app-cluster-config-audit-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './cluster-config-audit-reports-detailed.component.html',
  styleUrl: './cluster-config-audit-reports-detailed.component.scss',
})
export class ClusterConfigAuditReportsDetailedComponent
  extends DataPageBase<ClusterConfigAuditReportDenormalizedDto>
  implements OnInit
{
  readonly csvFileName: string = 'Cluster.Config.Audit.Reports';

  readonly trivyTableColumns: TrivyTableColumn[] = [...configAuditReportDenormalizedColumns];

  private readonly dataDtoService = inject(ClusterConfigAuditReportService);

  protected readonly dataDtosLoader = () => this.dataDtoService.getClusterConfigAuditReportDenormalizedDtos();

  ngOnInit() {
    this.initialize();
  }
}
