import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

import { ConfigAuditReportDto } from '../../../api/models/config-audit-report-dto';
import { ConfigAuditReportService } from '../../../api/services/config-audit-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import {
  configAuditReportColumns,
  configAuditReportComparedTableColumns,
  configAuditReportDetailColumns,
} from '../constants/config-audit-reports.constants';
import { namespacedColumns } from '../constants/generic.constants';

import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';

import { DialogModule } from 'primeng/dialog';
import { NamespacedResourceTrivyReportDataPageBase } from '../abstracts/pages/namespaced-trivy-report-data-page-base';
import { SecurityAssessmentReportDetailDto } from '../../../api/models/security-assessment-report-detail-dto';

@Component({
  selector: 'app-config-audit-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, GenericReportsCompareComponent],
  templateUrl: './config-audit-reports.component.html',
  styleUrl: './config-audit-reports.component.scss',
})
export class ConfigAuditReportsComponent
  extends NamespacedResourceTrivyReportDataPageBase<ConfigAuditReportDto, SecurityAssessmentReportDetailDto>
  implements OnInit
{
  mainTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...configAuditReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...configAuditReportDetailColumns];

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  comparedTableColumns: TrivyTableColumn[] = [...configAuditReportComparedTableColumns];

  private readonly dataDtoService = inject(ConfigAuditReportService);
  private readonly router = inject(Router);

  protected override dataDtosLoader = () => this.dataDtoService.getConfigAuditReportDtos();

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
    const url = this.router.serializeUrl(this.router.createUrlTree(['config-audit-reports-detailed']));
    window.open(url, '_blank');
  }

  private goToComparePage() {
    if (!this.dataDtos || !this.selectedTrivyReportDto) return;
    if (this.hasSeverities(this.selectedTrivyReportDto)) {
      this.messageService.pushSimple(
        'Nothing to compare',
        'Config Audit Reports',
        'info',
        'The selected item has no details, so there is nothing to compare...',
      );

      return;
    }

    this.compareNamespacedImageDtos = this.dataDtos
      .filter((car) => this.hasSeverities(car))
      .map((car) => ({
        uid: car.uid ?? '',
        resourceNamespace: car.resourceNamespace ?? '',
        mainLabel: car.resourceName,
        group: car.resourceKind,
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }
}
