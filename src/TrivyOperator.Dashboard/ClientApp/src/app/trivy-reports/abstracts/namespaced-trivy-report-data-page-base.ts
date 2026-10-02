import { TrivyReportDataPageBase } from './trivy-report-data-page-base';
import { NamespacedAggregateTrivyReport, NamespacedResourceTrivyReport, TrivyReportDetail } from './trivy-report';

export abstract class NamespacedTrivyReportDataPageBase<TData> extends TrivyReportDataPageBase<TData> {
  protected activeNamespaces: string[] = [];
}

export abstract class NamespacedResourceTrivyReportDataPageBase<
  TTrivyReportDetail extends TrivyReportDetail,
> extends NamespacedTrivyReportDataPageBase<NamespacedResourceTrivyReport<TTrivyReportDetail>> {

  protected override onGetDataDtos(dtos: NamespacedResourceTrivyReport<TTrivyReportDetail>[]): void {
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace))).sort();

    super.onGetDataDtos(dtos);
  }
}

// export class RbacAssessmentReportsComponent extends NamespacedResourceTrivyReportDataPageBase<RbacAssessmentReportDetail> {}

export abstract class NamespacedAggregateTrivyReportDataPageBase<
  TTrivyReportDetail extends TrivyReportDetail,
> extends NamespacedTrivyReportDataPageBase<NamespacedAggregateTrivyReport<TTrivyReportDetail>> {

  protected override onGetDataDtos(dtos: NamespacedAggregateTrivyReport<TTrivyReportDetail>[]): void {
    this.activeNamespaces = Array.from(new Set(dtos.flatMap((dto) => dto.namespaceNames))).sort();

    super.onGetDataDtos(dtos);
  }
}
