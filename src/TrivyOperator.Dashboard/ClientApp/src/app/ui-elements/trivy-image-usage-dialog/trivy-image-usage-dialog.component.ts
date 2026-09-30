import { Component, input, model } from '@angular/core';

import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';

import { TrivyReportResourceInfoDto } from '../../../api/models/trivy-report-resource-info-dto';

@Component({
  selector: 'app-trivy-image-usage-dialog',
  standalone: true,
  imports: [DialogModule, TableModule],
  templateUrl: './trivy-image-usage-dialog.component.html',
})
export class TrivyImageUsageDialogComponent {
  readonly isVisible = model(false);

  readonly imageNameAndTag = input<string>('N/A');
  readonly resources = input<TrivyReportResourceInfoDto[]>([]);
}
