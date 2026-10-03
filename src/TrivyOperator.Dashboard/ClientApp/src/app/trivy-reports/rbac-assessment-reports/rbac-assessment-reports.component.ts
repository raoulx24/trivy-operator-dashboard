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
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

import { DialogModule } from 'primeng/dialog';
import {NamespacedResourceTrivyReportDataPageBase} from "../abstracts/pages/namespaced-trivy-report-data-page-base";
import {SecurityAssessmentReportDetailDto} from "../../../api/models/security-assessment-report-detail-dto";

@Component({
  selector: 'app-rbac-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, GenericReportsCompareComponent, DialogModule],
  templateUrl: './rbac-assessment-reports.component.html',
  styleUrl: './rbac-assessment-reports.component.scss',
})
export class RbacAssessmentReportsComponent extends NamespacedResourceTrivyReportDataPageBase<
  RbacAssessmentReportDto,
  SecurityAssessmentReportDetailDto
> implements OnInit {
  mainTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...rbacAssessmentReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportDetailColumns];

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  comparedTableColumns: TrivyTableColumn[] = [...rbacAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(RbacAssessmentReportService);
  private readonly router = inject(Router);

  protected override dataDtosLoader = () => this.dataDtoService.getRbacAssessmentReportDtos();

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
        console.error('rbac - multi action call back - unknown: ' + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['rbac-assessment-reports-detailed']));
    window.open(url, '_blank');
  }

  private goToComparePage() {
    if (!this.dataDtos || !this.selectedTrivyReportDto) return;
    if (this.hasSeverities(this.selectedTrivyReportDto)) {
      this.messageService.pushSimple(
        'Nothing to compare',
        'RBAC Assessment Reports',
        'info',
        'The selected item has no details, so there is nothing to compare...',
      );

      return;
    }

    this.compareNamespacedImageDtos = this.dataDtos
      .filter((rar) => this.hasSeverities(rar))
      .map((rar) => ({
        uid: rar.uid ?? '',
        resourceNamespace: rar.resourceNamespace ?? '',
        mainLabel: rar.resourceName,
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }
}
