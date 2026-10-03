import { Component, inject, OnInit, signal } from '@angular/core';

import { Router } from '@angular/router';
import { Dialog } from 'primeng/dialog';
import { ClusterRbacAssessmentReportDto } from '../../../api/models/cluster-rbac-assessment-report-dto';
import { ClusterRbacAssessmentReportService } from '../../../api/services/cluster-rbac-assessment-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { nonExistingNamespace } from '../../ui-elements/namespace-image-selector/namespace-image-selector.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import {
  rbacAssessmentReportColumns,
  rbacAssessmentReportComparedTableColumns,
  rbacAssessmentReportDetailColumns,
} from '../constants/rbac-assessment-reports.constants';
import { TrivyReportDataPageBase } from '../abstracts/pages/trivy-report-data-page-base';
import { SecurityAssessmentReportDetailDto } from '../../../api/models/security-assessment-report-detail-dto';

@Component({
  selector: 'app-cluster-rbac-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, Dialog, GenericReportsCompareComponent],
  templateUrl: './cluster-rbac-assessment-reports.component.html',
  styleUrl: './cluster-rbac-assessment-reports.component.scss',
})
export class ClusterRbacAssessmentReportsComponent
  extends TrivyReportDataPageBase<ClusterRbacAssessmentReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  mainTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportDetailColumns];

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  comparedTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(ClusterRbacAssessmentReportService);
  private readonly router = inject(Router);

  protected override dataDtosLoader = () => this.dataDtoService.getClusterRbacAssessmentReportDtos();

  ngOnInit() {
    this.initialize();
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
        console.error('cvr - multi action call back - unknown: ' + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['cluster-rbac-assessment-reports-detailed']));
    window.open(url, '_blank');
  }

  private goToComparePage() {
    if (!this.dataDtos || !this.selectedTrivyReportDto) return;
    if (!this.hasSeverities(this.selectedTrivyReportDto)) {
      this.messageService.pushSimple(
        'Nothing to compare',
        'Cluster RBAC Assessment Reports',
        'info',
        'The selected item has no details, so there is nothing to compare...',
      );

      return;
    }

    this.compareNamespacedImageDtos = this.dataDtos
      .filter((crar) => this.hasSeverities(crar))
      .map((crar) => ({
        uid: crar.uid ?? '',
        resourceNamespace: nonExistingNamespace,
        mainLabel: crar.resourceName ?? '',
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }
}
