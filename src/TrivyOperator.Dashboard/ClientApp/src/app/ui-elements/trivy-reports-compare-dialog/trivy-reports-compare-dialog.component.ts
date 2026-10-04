import { DialogModule } from 'primeng/dialog';
import { GenericReportsCompareComponent } from '../generic-reports-compare/generic-reports-compare.component';
import { TrivyTableColumn } from '../trivy-table/trivy-table.types';
import { Component, input, model } from '@angular/core';
import { NamespacedImageDto } from '../namespace-image-selector/namespace-image-selector.types';
import { TrivyReportComparable, TrivyReportComparableDetail } from '../../trivy-reports/abstracts/types/trivy-report';

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
  readonly namespacedImageDtos = input<NamespacedImageDto[] | undefined>();
  readonly firstSelectorLonger = input<boolean>(false);
  readonly walkingIsEnabled = input<boolean>(false);
}
