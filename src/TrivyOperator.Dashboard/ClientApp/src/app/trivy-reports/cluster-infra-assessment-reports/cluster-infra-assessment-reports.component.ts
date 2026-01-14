import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { ClusterInfraAssessmentReportDto } from '../../../api/models/cluster-infra-assessment-report-dto';
import { ClusterInfraAssessmentReportService } from '../../../api/services/cluster-infra-assessment-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyFilterData, TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import {
  infraAssessmentReportColumns,
  infraAssessmentReportComparedTableColumns,
  infraAssessmentReportDetailColumns,
} from '../constants/infra-assessment-reports.constants';

import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { nonExistingNamespace } from '../../ui-elements/namespace-image-selector/namespace-image-selector.component';
import { NamespacedImageDto } from '../../ui-elements/namespace-image-selector/namespace-image-selector.types';

import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { DataPageBase } from '../../abstracts/data-page-base';

@Component({
  selector: 'app-cluster-infra-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, GenericReportsCompareComponent],
  templateUrl: './cluster-infra-assessment-reports.component.html',
  styleUrl: './cluster-infra-assessment-reports.component.scss',
})
export class ClusterInfraAssessmentReportsComponent extends DataPageBase implements OnInit {
  dataDtos: ClusterInfraAssessmentReportDto[] = [];

  mainTableColumns: TrivyTableColumn[] = [...infraAssessmentReportColumns];
  isMainTableLoading: boolean = true;

  detailsTableColumns: TrivyTableColumn[] = [...infraAssessmentReportDetailColumns];

  queryUid?: string;
  isSingleMode: boolean = false;
  selectedTrivyReportDto?: ClusterInfraAssessmentReportDto;

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  compareNamespacedImageDtos?: NamespacedImageDto[];
  comparedTableColumns: TrivyTableColumn[] = [...infraAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(ClusterInfraAssessmentReportService);
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly messageService = inject(MessageService);

  ngOnInit() {
    this.activatedRoute.queryParamMap.subscribe((params) => {
      this.queryUid = params.get('uid') ?? undefined;
    });
    this.isSingleMode = !!this.queryUid;
    this.getDataDtos();
  }

  getDataDtos() {
    this.isMainTableLoading = true;
    this.dataDtoService.getClusterInfraAssessmentReportDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  onGetDataDtos(dtos: ClusterInfraAssessmentReportDto[]) {
    this.dataDtos = dtos;
    this.isMainTableLoading = false;
  }

  public onRefreshRequested(_event: TrivyFilterData) {
    this.getDataDtos();
  }

  onMainTableMultiHeaderActionRequested(event: string) {
    switch (event) {
      case 'goToDetailedPage':
        this.goToDetailedPage();
        break;
      case 'Compare with...':
        this.goToComparePage();
        break;
      default:
        console.error('ciar - multi action call back - unknown: ' + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['cluster-infra-assessment-reports-detailed']));
    window.open(url, '_blank');
  }

  private goToComparePage() {
    if (!this.dataDtos || !this.selectedTrivyReportDto) return;
    if (
      this.selectedTrivyReportDto.criticalCount < 1 &&
      this.selectedTrivyReportDto.highCount < 1 &&
      this.selectedTrivyReportDto.mediumCount < 1 &&
      this.selectedTrivyReportDto.lowCount < 1
    ) {
      this.messageService.add({
        severity: 'info',
        summary: 'Nothing to compare',
        detail: 'The selected item has no details, so there is nothing to compare...',
      });

      return;
    }

    this.compareNamespacedImageDtos = this.dataDtos
      .filter((car) => car.criticalCount > 0 || car.highCount > 0 || car.mediumCount > 0 || car.lowCount > 0)
      .map((car) => ({
        uid: car.uid ?? '',
        resourceNamespace: nonExistingNamespace,
        mainLabel: car.resourceName,
        group: car.resourceKind,
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }

  onMainTableSelectedRowChanged(event: ClusterInfraAssessmentReportDto | null) {
    this.selectedTrivyReportDto = event ?? undefined;
  }
}
