import { ClusteredScopedResourceTrivyReport, TrivyReportDetail } from '../types/trivy-report';
import { TrivyReportMasterDetailDataPageBase } from './trivy-report-data-page-base';
import { NamespacedImageDto } from '../../../ui-elements/namespace-image-selector/namespace-image-selector.types';

export abstract class ClusteredScopedTrivyReportDataPageBase<
  TTrivyReport extends ClusteredScopedResourceTrivyReport<TTrivyReportDetail>,
  TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReportMasterDetailDataPageBase<TTrivyReport, TTrivyReportDetail> {
  protected override prepareCompareDataDtos(): NamespacedImageDto[] {
    return this.dataDtos
      .filter((tr) => this.hasSeverities(tr))
      .map((tr) => ({
        uid: tr.uid ?? '',
        resourceNamespace: 'N/A',
        mainLabel: tr.resourceName,
      }));
  }
}
