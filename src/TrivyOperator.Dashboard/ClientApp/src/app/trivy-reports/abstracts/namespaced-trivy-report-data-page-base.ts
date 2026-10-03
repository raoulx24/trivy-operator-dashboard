import { TrivyReportDataPageBase } from './trivy-report-data-page-base';
import {HasNamespace, NamespacedAggregateTrivyReport, NamespacedResourceTrivyReport, TrivyReportDetail } from './trivy-report';

export abstract class NamespacedTrivyReportDataPageBase<TData> extends TrivyReportDataPageBase<TData> {
  protected activeNamespaces: string[] = [];
}

export abstract class NamespacedDataTrivyReportDataPageBase<
  TDataHasNamespace extends HasNamespace,
> extends NamespacedTrivyReportDataPageBase<TDataHasNamespace> {
  protected override onGetDataDtos(dtos: TDataHasNamespace[]): void {
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace))).sort();

    super.onGetDataDtos(dtos);
  }
}

export abstract class NamespacedResourceTrivyReportDataPageBase<
  TTrivyReportDetail extends TrivyReportDetail,
> extends NamespacedDataTrivyReportDataPageBase<NamespacedResourceTrivyReport<TTrivyReportDetail>> {}

// export class RbacAssessmentReportsComponent extends NamespacedResourceTrivyReportDataPageBase<RbacAssessmentReportDetail> {}

export abstract class NamespacedAggregateTrivyReportDataPageBase<
  TTrivyReportDetail extends TrivyReportDetail,
> extends NamespacedTrivyReportDataPageBase<NamespacedAggregateTrivyReport<TTrivyReportDetail>> {

  protected override onGetDataDtos(dtos: NamespacedAggregateTrivyReport<TTrivyReportDetail>[]): void {
    this.activeNamespaces = Array.from(new Set(dtos.flatMap((dto) => dto.namespaceNames))).sort();

    super.onGetDataDtos(dtos);
  }
}
