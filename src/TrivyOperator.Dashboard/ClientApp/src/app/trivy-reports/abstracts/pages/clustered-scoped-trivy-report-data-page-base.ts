import { ClusteredScopedResourceTrivyReport, TrivyReportDetail } from '../types/trivy-report';
import { TrivyReportMasterDetailDataPageBase } from './trivy-report-data-page-base';
import { PairedOptionDto } from '../../../ui-elements/paired-options-selector/paired-options-selector.types';

export abstract class ClusteredScopedTrivyReportDataPageBase<
  TTrivyReport extends ClusteredScopedResourceTrivyReport<TTrivyReportDetail>,
  TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReportMasterDetailDataPageBase<TTrivyReport, TTrivyReportDetail> {
  protected override prepareCompareDataDtos(): PairedOptionDto[] {
    return this.dataDtos
      .filter((tr) => this.hasSeverities(tr))
      .map((tr) => ({
        uid: tr.uid ?? '',
        resourceNamespace: 'N/A',
        mainLabel: tr.resourceName,
      }));
  }
}
