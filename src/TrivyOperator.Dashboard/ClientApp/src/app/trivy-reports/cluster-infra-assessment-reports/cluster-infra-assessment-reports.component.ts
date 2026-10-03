import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

import { ClusterInfraAssessmentReportDto } from '../../../api/models/cluster-infra-assessment-report-dto';
import { ClusterInfraAssessmentReportService } from '../../../api/services/cluster-infra-assessment-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import {
  infraAssessmentReportColumns,
  infraAssessmentReportComparedTableColumns,
  infraAssessmentReportDetailColumns,
} from '../constants/infra-assessment-reports.constants';

import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { nonExistingNamespace } from '../../ui-elements/namespace-image-selector/namespace-image-selector.component';

import { DialogModule } from 'primeng/dialog';
import { TrivyReportDataPageBase } from '../abstracts/pages/trivy-report-data-page-base';
import { SecurityAssessmentReportDetailDto } from '../../../api/models/security-assessment-report-detail-dto';

@Component({
  selector: 'app-cluster-infra-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, GenericReportsCompareComponent],
  templateUrl: './cluster-infra-assessment-reports.component.html',
  styleUrl: './cluster-infra-assessment-reports.component.scss',
})
export class ClusterInfraAssessmentReportsComponent
  extends TrivyReportDataPageBase<ClusterInfraAssessmentReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  mainTableColumns: TrivyTableColumn[] = [...infraAssessmentReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...infraAssessmentReportDetailColumns];

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  comparedTableColumns: TrivyTableColumn[] = [...infraAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(ClusterInfraAssessmentReportService);
  private readonly router = inject(Router);

  protected override dataDtosLoader = () => this.dataDtoService.getClusterInfraAssessmentReportDtos();

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
        console.error('ciar - multi action call back - unknown: ' + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['cluster-infra-assessment-reports-detailed']));
    window.open(url, '_blank');
  }

  private goToComparePage() {
    if (!this.dataDtos || !this.selectedTrivyReportDto) return;
    if (this.hasSeverities(this.selectedTrivyReportDto)) {
      this.messageService.pushSimple(
        'Nothing to compare',
        'Cluster Infra Assessment Reports',
        'info',
        'The selected item has no details, so there is nothing to compare...',
      );

      return;
    }

    this.compareNamespacedImageDtos = this.dataDtos
      .filter((car) => this.hasSeverities(car))
      .map((car) => ({
        uid: car.uid ?? '',
        resourceNamespace: nonExistingNamespace,
        mainLabel: car.resourceName,
        group: car.resourceKind,
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }
}
