import {
  HasNamespace,
  NamespacedAggregateTrivyReport,
  NamespacedResourceTrivyReport,
  TrivyReport,
  TrivyReportDetail,
} from '../types/trivy-report';
import { TrivyReportDataPageBase } from './trivy-report-data-page-base';
import { signal } from '@angular/core';
import { DataPageBase } from './data-page-base';

export abstract class NamespacedDataPageBase<TData extends HasNamespace> extends DataPageBase<TData> {
  protected activeNamespaces: string[] = [];

  protected override onGetDataDtos(dtos: TData[]): void {
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace))).sort();

    super.onGetDataDtos(dtos);
  }
}

export abstract class NamespacedTrivyReportDataPageBase<
  TTrivyReport extends TrivyReport<TTrivyReportDetail>,
  TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReportDataPageBase<TTrivyReport, TTrivyReportDetail> {
  protected activeNamespaces: string[] = [];
}

export abstract class NamespacedResourceTrivyReportDataPageBase<
  TTrivyReport extends NamespacedResourceTrivyReport<TTrivyReportDetail>,
  TTrivyReportDetail extends TrivyReportDetail,
> extends NamespacedTrivyReportDataPageBase<TTrivyReport, TTrivyReportDetail> {

  protected override onGetDataDtos(dtos: TTrivyReport[]): void {
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace))).sort();

    super.onGetDataDtos(dtos);
  }
}


export abstract class NamespacedAggregateTrivyReportDataPageBase<
  TTrivyReport extends NamespacedAggregateTrivyReport<TTrivyReportDetail>,
  TTrivyReportDetail extends TrivyReportDetail,
  TExtendedDataDto extends TTrivyReport = TTrivyReport,
> extends NamespacedTrivyReportDataPageBase<TTrivyReport, TTrivyReportDetail> {
  protected viewDataDtos: TExtendedDataDto[] = [];
  protected singleSelectDataDto?: TTrivyReport;

  protected isDependencyTreeViewVisible = signal<boolean>(false);

  private queryDigest?: string;

  protected override initialize(): void {
    const state = history.state;
    this.queryDigest = state.digest;

    super.initialize();
  }

  protected override onGetDataDtos(dtos: TTrivyReport[]): void {
    this.activeNamespaces = Array.from(new Set(dtos.flatMap((dto) => dto.namespaceNames))).sort();

    this.viewDataDtos = dtos.map((dto) => this.prepareViewDataDtos(dto));

    super.onGetDataDtos(dtos);

    if (this.queryDigest) {
      this.singleSelectDataDto = dtos.find((x) => x.digest == this.queryDigest);
    }
  }

  protected override onMainTableSelectedRowChanged(event: TTrivyReport | null) {
    super.onMainTableSelectedRowChanged(event);

    this.singleSelectDataDto = event ?? undefined;
  }

  protected goToDependencyTree() {
    if (this.selectedTrivyReportDto?.digest) {
      this.isDependencyTreeViewVisible.set(true);
    }
  }

  protected abstract prepareViewDataDtos(dto: TTrivyReport): TExtendedDataDto;
}

