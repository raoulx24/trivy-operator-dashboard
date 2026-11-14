import { Component, input, model } from '@angular/core';

import { TrivyReportResourceInfoDto } from '@api/models/trivy-report-resource-info-dto';
import { TrivyTableColumn } from '../trivy-table/trivy-table.types';
import { TrivyTableDialogComponent } from '../trivy-table-dialog/trivy-table-dialog.component';

@Component({
  selector: 'app-trivy-image-usage-dialog',
  standalone: true,
  imports: [TrivyTableDialogComponent],
  templateUrl: './trivy-image-usage-dialog.component.html',
})
export class TrivyImageUsageDialogComponent {
  readonly isVisible = model(false);

  readonly imageNameAndTag = input<string>('N/A');
  readonly resources = input<TrivyReportResourceInfoDto[]>([]);

  protected readonly columns: TrivyTableColumn[] = [
    { field: 'namespaceName', header: 'Resource Namespace', isFilterable: false, isSortable: true, style: 'width: 25%', renderType: 'standard' },
    { field: 'name', header: 'Resource Name', isFilterable: false, isSortable: true, style: 'width: 30%', renderType: 'standard' },
    { field: 'kind', header: 'Kind', isFilterable: false, isSortable: true, style: 'width: 20%', renderType: 'standard' },
    { field: 'containerName', header: 'Container', isFilterable: false, isSortable: true, style: 'width: 25%', renderType: 'standard' },
  ];
}
