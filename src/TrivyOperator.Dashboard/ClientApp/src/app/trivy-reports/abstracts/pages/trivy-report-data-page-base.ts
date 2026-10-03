import { DataPageBase } from './data-page-base';
import { TrivyReport, TrivyReportDetail } from '../types/trivy-report';
import { NamespacedImageDto } from '../../../ui-elements/namespace-image-selector/namespace-image-selector.types';

export abstract class TrivyReportDataPageBase<
  TTrivyReport extends TrivyReport<TTrivyReportDetail>,
  TTrivyReportDetail extends TrivyReportDetail,
> extends DataPageBase<TTrivyReport> {
  selectedTrivyReportDto: TTrivyReport | null = null;

  compareNamespacedImageDtos?: NamespacedImageDto[];

  protected override onGetDataDtos(dtos: TTrivyReport[]) {
    this.compareNamespacedImageDtos = [];

    super.onGetDataDtos(dtos);
  }

  protected onMainTableSelectedRowChanged(event: TTrivyReport | null) {
    this.selectedTrivyReportDto = event;
  }

  protected hasSeverities(report: TTrivyReport): boolean {
    return report.criticalCount > 0 || report.highCount > 0 || report.mediumCount > 0 || report.lowCount > 0;
  }
}
