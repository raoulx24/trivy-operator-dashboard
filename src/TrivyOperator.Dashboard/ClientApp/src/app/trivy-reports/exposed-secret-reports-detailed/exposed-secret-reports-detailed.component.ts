import { Component, inject, OnInit } from '@angular/core';

import { ExposedSecretReportDenormalizedDto } from '../../../api/models/exposed-secret-report-denormalized-dto';
import { ExposedSecretReportsService } from '../../../api/services/exposed-secret-reports.service';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { exposedSecretReportDenormalizedColumns } from '../constants/exposed-secret-reports.constants';
import { namespacedColumns } from '../constants/generic.constants';
import { NamespacedDataPageBase } from '../abstracts/pages/namespaced-trivy-report-data-page-base';

@Component({
  selector: 'app-exposed-secret-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './exposed-secret-reports-detailed.component.html',
  styleUrl: './exposed-secret-reports-detailed.component.scss',
})
export class ExposedSecretReportsDetailedComponent
  extends NamespacedDataPageBase<ExposedSecretReportDenormalizedDto>
  implements OnInit
{
  public csvFileName: string = 'Exposed.Secret.Reports';

  public trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...exposedSecretReportDenormalizedColumns];

  private readonly dataDtoService = inject(ExposedSecretReportsService);
  protected readonly dataDtosLoader = () => this.dataDtoService.getExposedSecretReportDenormalizedDtos();

  ngOnInit() {
    this.initialize();
  }
}
