import { Component, inject, OnInit } from '@angular/core';

import { ExposedSecretReportDenormalizedDto } from '../../../api/models/exposed-secret-report-denormalized-dto';
import { SeverityDto } from '../../../api/models/severity-dto';
import { ExposedSecretReportService } from '../../../api/services/exposed-secret-report.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { TrivyReportsDetailedBase } from '../abstracts/trivy-reports-detailed-base';
import { exposedSecretReportDenormalizedColumns } from '../constants/exposed-secret-reports.constants';
import { namespacedColumns } from '../constants/generic.constants';

@Component({
  selector: 'app-exposed-secret-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './exposed-secret-reports-detailed.component.html',
  styleUrl: './exposed-secret-reports-detailed.component.scss',
})
export class ExposedSecretReportsDetailedComponent extends TrivyReportsDetailedBase implements OnInit {
  public dataDtos?: ExposedSecretReportDenormalizedDto[];
  public severityDtos: SeverityDto[] = [];
  public activeNamespaces: string[] = [];
  public isLoading: boolean = false;

  public csvFileName: string = 'Exposed.Secret.Reports';

  public trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...exposedSecretReportDenormalizedColumns];

  private readonly dataDtoService = inject(ExposedSecretReportService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  public getTableDataDtos() {
    this.isLoading = true;
    this.dataDtoService.getExposedSecretReportDenormalizedDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  private onGetDataDtos(dtos: ExposedSecretReportDenormalizedDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace ?? 'N/A'))).sort();
    this.isLoading = false;
  }
}
