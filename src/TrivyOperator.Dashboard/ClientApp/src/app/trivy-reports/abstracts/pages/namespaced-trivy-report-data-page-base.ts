import {
  HasNamespace,
  NamespacedAggregateTrivyReport,
  NamespacedResourceTrivyReport,
  TrivyReport,
  TrivyReportDetail,
} from '../types/trivy-report';
import { TrivyReportMasterDetailDataPageBase } from './trivy-report-data-page-base';
import { signal } from '@angular/core';
import { DataPageBase } from './data-page-base';
import { PairedOptionsDto } from '../../../ui-elements/paired-options-selector/paired-options-selector.types';

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
> extends TrivyReportMasterDetailDataPageBase<TTrivyReport, TTrivyReportDetail> {
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

  protected override prepareCompareDataDtos(): PairedOptionsDto[] {
    return this.dataDtos
      .filter((tr) => this.hasSeverities(tr))
      .map((tr) => ({
        uid: tr.uid ?? '',
        firstOption: tr.resourceNamespace ?? '',
        secondOption: tr.resourceName,
      }));
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

    // we do not need to keep the original dataDtos since we have viewDataDtos, so we can clear it to save memory
    this.dataDtos = [];
  }

  protected override onMainTableSelectedRowChanged(event: TTrivyReport | null) {
    super.onMainTableSelectedRowChanged(event);

    this.singleSelectDataDto = event ?? undefined;
  }

  protected override onMainTableMultiHeaderActionRequested(event: string) {
    switch (event) {
      case 'Dependency tree':
        this.goToDependencyTree();
        break;

      default:
        super.onMainTableMultiHeaderActionRequested(event);
        break;
    }
  }

  protected goToDependencyTree() {
    if (this.selectedTrivyReportDto?.digest) {
      this.isDependencyTreeViewVisible.set(true);
    }
  }

  protected override prepareCompareDataDtos(): PairedOptionsDto[] {
    return this.viewDataDtos
      .filter((tr) => this.hasSeverities(tr))
      .map((tr) => ({
        uid: tr.uid ?? '',
        firstOption: 'N/A',
        secondOption: `${tr.lastImageNameAndTag}`,
      }));
  }

  protected abstract prepareViewDataDtos(dto: TTrivyReport): TExtendedDataDto;
}

