import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

import { RbacAssessmentReportDto } from '../../../api/models/rbac-assessment-report-dto';
import { RbacAssessmentReportService } from '../../../api/services/rbac-assessment-report.service';
import { namespacedColumns } from '../constants/generic.constants';
import {
  rbacAssessmentReportColumns,
  rbacAssessmentReportComparedTableColumns,
  rbacAssessmentReportDetailColumns,
} from '../constants/rbac-assessment-reports.constants';

import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { NamespacedImageDto } from '../../ui-elements/namespace-image-selector/namespace-image-selector.types';
import { TrivyFilterData, TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { DataPageBase } from '../../abstracts/data-page-base';

@Component({
  selector: 'app-rbac-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, GenericReportsCompareComponent, DialogModule],
  templateUrl: './rbac-assessment-reports.component.html',
  styleUrl: './rbac-assessment-reports.component.scss',
})
export class RbacAssessmentReportsComponent extends DataPageBase implements OnInit {
  dataDtos: RbacAssessmentReportDto[] = [];
  activeNamespaces: string[] = [];

  mainTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...rbacAssessmentReportColumns];
  isMainTableLoading: boolean = true;

  detailsTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportDetailColumns];

  selectedTrivyReportDto?: RbacAssessmentReportDto;

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  compareNamespacedImageDtos?: NamespacedImageDto[];
  comparedTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(RbacAssessmentReportService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  private getTableDataDtos() {
    this.isMainTableLoading = true;
    this.dataDtoService.getRbacAssessmentReportDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  private onGetDataDtos(dtos: RbacAssessmentReportDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace ?? 'N/A'))).sort();
    this.isMainTableLoading = false;
  }

  onRefreshRequested(_event: TrivyFilterData) {
    this.getTableDataDtos();
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
        console.error('rbac - multi action call back - unknown: ' + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['rbac-assessment-reports-detailed']));
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
      .filter((rar) => rar.criticalCount > 0 || rar.highCount > 0 || rar.mediumCount > 0 || rar.lowCount > 0)
      .map((rar) => ({
        uid: rar.uid ?? '',
        resourceNamespace: rar.resourceNamespace ?? '',
        mainLabel: rar.resourceName,
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }

  onMainTableSelectedRowChanged(event: RbacAssessmentReportDto | null) {
    this.selectedTrivyReportDto = event ?? undefined;
  }
}
