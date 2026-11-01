import { DialogModule } from 'primeng/dialog';
import { GenericReportsCompareComponent } from '../generic-reports-compare/generic-reports-compare.component';
import { TrivyTableColumn } from '../trivy-table/trivy-table.types';
import { Component, input, model, output } from '@angular/core';
import { CveInfoDto } from '@api/models/cve-info-dto';
import { TrivyReportComparable, TrivyReportComparableDetail } from '@shared/abstracts/types/trivy-report';
import { PairedOptionsDto } from '../paired-options-selector/paired-options-selector.types';

@Component({
  selector: 'app-trivy-reports-compare-dialog',
  standalone: true,
  imports: [
    DialogModule,
    GenericReportsCompareComponent,
  ],
  templateUrl: './trivy-reports-compare-dialog.component.html',
})
export class TrivyReportsCompareDialogComponent {
  isVisible = model(false);

  readonly comparedTableColumns = input<TrivyTableColumn[]>([]);
  readonly reportsName = input<string>('');
  readonly namespacePlaceholder = input<string>('');
  readonly imagePlaceholder = input<string>('');
  readonly dataDtos = input<TrivyReportComparable<TrivyReportComparableDetail>[]>([]);
  readonly firstSelectedTrivyReportId = input<string | undefined>();
  readonly secondSelectedTrivyReportId = input<string | undefined>();
  readonly namespacedImageDtos = input<PairedOptionsDto[] | undefined>();
  readonly firstSelectorLonger = input<boolean>(false);
  readonly walkingIsEnabled = input<boolean>(false);
  // vulnerability reports only - the tags of the 'vulnerabilityId' column; the host opens their dialogs
  readonly isNvdEnabled = input<boolean>(true);
  readonly cveInfoById = input<ReadonlyMap<string, CveInfoDto> | undefined>(undefined);
  readonly rowActionRequested = output<{ row: TrivyReportComparableDetail; col: string; action?: string }>();
}
