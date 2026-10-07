import { ClusteredScopedResourceTrivyReport, TrivyReportDetail } from '../types/trivy-report';
import { TrivyReportMasterDetailDataPageBase } from './trivy-report-data-page-base';
import { PairedOptionsDto } from '../../../ui-elements/paired-options-selector/paired-options-selector.types';

export abstract class ClusteredScopedTrivyReportDataPageBase<
  TTrivyReport extends ClusteredScopedResourceTrivyReport<TTrivyReportDetail>,
  TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReportMasterDetailDataPageBase<TTrivyReport, TTrivyReportDetail> {
  protected override prepareCompareDataDtos(): PairedOptionsDto[] {
    return this.dataDtos
      .filter((tr) => this.hasSeverities(tr))
      .map((tr) => ({
        uid: tr.uid ?? '',
        firstOption: 'N/A',
        secondOption: tr.resourceName,
      }));
  }
}
