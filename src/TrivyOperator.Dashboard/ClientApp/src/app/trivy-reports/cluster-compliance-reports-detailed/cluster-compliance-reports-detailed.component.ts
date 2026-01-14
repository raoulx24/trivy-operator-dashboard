import { Component, inject, OnInit } from '@angular/core';

import { SeverityDto } from '../../../api/models/severity-dto';
import { ClusterComplianceReportService } from '../../../api/services/cluster-compliance-report.service';

import { ClusterComplianceReportDenormalizedDto } from '../../../api/models';
import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { TrivyReportsDetailedBase } from '../abstracts/trivy-reports-detailed-base';
import { clusterComplianceReportDenormalizedColumns } from '../constants/cluster-compliance-reports.constants';

@Component({
  selector: 'app-cluster-compliance-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './cluster-compliance-reports-detailed.component.html',
  styleUrl: './cluster-compliance-reports-detailed.component.scss',
})
export class ClusterComplianceReportsDetailedComponent extends TrivyReportsDetailedBase implements OnInit {
  dataDtos?: ClusterComplianceReportDenormalizedDto[];
  severityDtos: SeverityDto[] = [];
  isLoading: boolean = false;

  csvFileName: string = 'Cluster.Compliance.Reports';

  trivyTableColumns: TrivyTableColumn[] = [...clusterComplianceReportDenormalizedColumns];

  private readonly dataDtoService = inject(ClusterComplianceReportService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  getTableDataDtos() {
    this.isLoading = true;
    this.dataDtoService.getClusterComplianceReportDenormalizedDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  private onGetDataDtos(dtos: ClusterComplianceReportDenormalizedDto[]) {
    this.dataDtos = dtos;
    this.isLoading = false;
  }
}
