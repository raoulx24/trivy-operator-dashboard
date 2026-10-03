import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

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

import { DialogModule } from 'primeng/dialog';
import { NamespacedResourceTrivyReportDataPageBase } from '../abstracts/pages/namespaced-trivy-report-data-page-base';
import { SecurityAssessmentReportDetailDto } from '../../../api/models/security-assessment-report-detail-dto';

@Component({
  selector: 'app-infra-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, GenericReportsCompareComponent],
  templateUrl: './infra-assessment-reports.component.html',
  styleUrl: './infra-assessment-reports.component.scss',
})
export class InfraAssessmentReportsComponent
  extends NamespacedResourceTrivyReportDataPageBase<InfraAssessmentReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  mainTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...infraAssessmentReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...infraAssessmentReportDetailColumns];

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  comparedTableColumns: TrivyTableColumn[] = [...infraAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(InfraAssessmentReportService);
  private readonly router = inject(Router);

  protected override dataDtosLoader = () => this.dataDtoService.getInfraAssessmentReportDtos();

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
        console.error('car - multi action call back - unknown: ' + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['infra-assessment-reports-detailed']));
    window.open(url, '_blank');
  }

  private goToComparePage() {
    if (!this.dataDtos || !this.selectedTrivyReportDto) return;
    if (this.hasSeverities(this.selectedTrivyReportDto)) {
      this.messageService.pushSimple(
        'Nothing to compare',
        'Infrastructure Assessment Reports',
        'info',
        'The selected item has no details, so there is nothing to compare...',
      );

      return;
    }

    this.compareNamespacedImageDtos = this.dataDtos
      .filter((iar) => this.hasSeverities(iar))
      .map((iar) => ({
        uid: iar.uid ?? '',
        resourceNamespace: iar.resourceNamespace ?? '',
        mainLabel: iar.resourceName,
        group: iar.resourceKind,
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }
}
