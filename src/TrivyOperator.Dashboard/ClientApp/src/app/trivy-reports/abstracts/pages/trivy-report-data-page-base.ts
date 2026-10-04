import { DataPageBase } from './data-page-base';
import { TrivyReport, TrivyReportDetail } from '../types/trivy-report';
import { PairedOptionDto } from '../../../ui-elements/paired-options-selector/paired-options-selector.types';
import { inject, signal } from '@angular/core';
import { Router } from '@angular/router';

export abstract class TrivyReportDataPageBase<
  TTrivyReport extends TrivyReport<TTrivyReportDetail>,
  TTrivyReportDetail extends TrivyReportDetail,
> extends DataPageBase<TTrivyReport> { }

export abstract class TrivyReportMasterDetailDataPageBase<
  TTrivyReport extends TrivyReport<TTrivyReportDetail>,
  TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReportDataPageBase<TTrivyReport, TTrivyReportDetail> {
  selectedTrivyReportDto: TTrivyReport | null = null;

  protected compareNamespacedImageDtos?: PairedOptionDto[];
  protected isTrivyReportsCompareVisible = signal<boolean>(false);
  protected compareFirstSelectedId?: string;

  protected abstract detailedPageRoute: string;

  protected abstract friendlyReportName: string;

  protected readonly router = inject(Router);

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

  protected onMainTableMultiHeaderActionRequested(event: string) {
    switch (event) {
      case 'goToDetailedPage':
        this.goToDetailedPage();
        break;

      case 'Compare with...':
        this.goToComparePage();
        break;

      default:
        console.error(`Unhandled action: ${event}`);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree([this.detailedPageRoute]));
    window.open(url, '_blank');
  }

  private goToComparePage() {
    if (!this.selectedTrivyReportDto) return;
    if (!this.hasSeverities(this.selectedTrivyReportDto)) {
      this.messageService.pushSimple(
        'Nothing to compare',
        this.friendlyReportName,
        'info',
        'The selected item has no details, so there is nothing to compare...',
      );

      return;
    }

    this.compareNamespacedImageDtos = this.prepareCompareDataDtos();
    this.compareFirstSelectedId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }

  protected abstract prepareCompareDataDtos(): PairedOptionDto[];
}
