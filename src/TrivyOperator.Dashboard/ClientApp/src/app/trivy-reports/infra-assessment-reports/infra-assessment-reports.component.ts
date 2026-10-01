import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { InfraAssessmentReportDto } from '../../../api/models/infra-assessment-report-dto';
import { InfraAssessmentReportService } from '../../../api/services/infra-assessment-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { namespacedColumns } from '../constants/generic.constants';
import {
  infraAssessmentReportColumns,
  infraAssessmentReportComparedTableColumns,
  infraAssessmentReportDetailColumns,
} from '../constants/infra-assessment-reports.constants';

import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { NamespacedImageDto } from '../../ui-elements/namespace-image-selector/namespace-image-selector.types';

import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { TrivyReportDataPageBase } from '../abstracts/trivy-report-data-page-base';

@Component({
  selector: 'app-infra-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, GenericReportsCompareComponent],
  templateUrl: './infra-assessment-reports.component.html',
  styleUrl: './infra-assessment-reports.component.scss',
})
export class InfraAssessmentReportsComponent extends TrivyReportDataPageBase implements OnInit {
  dataDtos: InfraAssessmentReportDto[] = [];
  activeNamespaces: string[] = [];

  mainTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...infraAssessmentReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...infraAssessmentReportDetailColumns];

  queryUid?: string;
  isSingleMode: boolean = false;
  selectedTrivyReportDto?: InfraAssessmentReportDto;

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  compareNamespacedImageDtos?: NamespacedImageDto[];
  comparedTableColumns: TrivyTableColumn[] = [...infraAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(InfraAssessmentReportService);
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
    this.dataDtoService.getInfraAssessmentReportDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  onGetDataDtos(dtos: InfraAssessmentReportDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace ?? 'N/A'))).sort();
    if (this.queryUid) {
      this.selectedTrivyReportDto = dtos.find((x) => x.uid == this.queryUid);
    }
    this.compareNamespacedImageDtos = undefined;
    this.isMainTableLoading = false;
  }

  public onRefreshRequested() {
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
        console.error('car - multi action call back - unknown: ' + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['infra-assessment-reports-detailed']));
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
      .filter((iar) => iar.criticalCount > 0 || iar.highCount > 0 || iar.mediumCount > 0 || iar.lowCount > 0)
      .map((iar) => ({
        uid: iar.uid ?? '',
        resourceNamespace: iar.resourceNamespace ?? '',
        mainLabel: iar.resourceName,
        group: iar.resourceKind,
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }

  onMainTableSelectedRowChanged(event: InfraAssessmentReportDto | null) {
    this.selectedTrivyReportDto = event ?? undefined;
  }
}
