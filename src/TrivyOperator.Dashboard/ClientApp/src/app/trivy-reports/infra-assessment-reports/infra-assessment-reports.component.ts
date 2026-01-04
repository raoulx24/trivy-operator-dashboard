import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { GetInfraAssessmentReportDtos$Params } from '../../../api/fn/infra-assessment-report/get-infra-assessment-report-dtos';
import { InfraAssessmentReportDto } from '../../../api/models/infra-assessment-report-dto';
import { InfraAssessmentReportService } from '../../../api/services/infra-assessment-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyFilterData, TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { SeverityUtils } from '../../utils/severity.utils';
import { namespacedColumns } from '../constants/generic.constants';
import {
  infraAssessmentReportColumns,
  infraAssessmentReportComparedTableColumns,
  infraAssessmentReportDetailColumns,
} from '../constants/infra-assessment-reports.constants';

import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { NamespacedImageDto } from '../../ui-elements/namespace-image-selector/namespace-image-selector.types';

import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-infra-assessment-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, GenericReportsCompareComponent],
  templateUrl: './infra-assessment-reports.component.html',
  styleUrl: './infra-assessment-reports.component.scss',
})
export class InfraAssessmentReportsComponent implements OnInit {
  dataDtos: InfraAssessmentReportDto[] = [];
  activeNamespaces: string[] = [];

  mainTableColumns: TrivyTableColumn[] = [... namespacedColumns, ...infraAssessmentReportColumns];
  isMainTableLoading: boolean = true;

  detailsTableColumns: TrivyTableColumn[] = [... infraAssessmentReportDetailColumns ];

  queryUid?: string;
  isSingleMode: boolean = false;
  selectedTrivyReportDto?: InfraAssessmentReportDto;

  isTrivyReportsCompareVisible: boolean = false;
  compareFirstSelectedIdId?: string;
  compareNamespacedImageDtos?: NamespacedImageDto[];
  comparedTableColumns: TrivyTableColumn[] = [... infraAssessmentReportComparedTableColumns];

  private readonly dataDtoService = inject(InfraAssessmentReportService);
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly messageService = inject(MessageService);

  ngOnInit() {
    this.activatedRoute.queryParamMap.subscribe(params => {
      this.queryUid = params.get('uid') ?? undefined;
    });
    this.isSingleMode = !!(this.queryUid);
    this.getDataDtos();
  }

  getDataDtos(params?: GetInfraAssessmentReportDtos$Params) {
    this.isMainTableLoading = true;
    this.dataDtoService.getInfraAssessmentReportDtos(params).subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  onGetDataDtos(dtos: InfraAssessmentReportDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array
      .from(new Set(dtos.map(dto => dto.resourceNamespace ?? "N/A")))
      .sort();
    if (this.queryUid) {
      this.selectedTrivyReportDto = dtos.find(x => x.uid == this.queryUid);
    }
    this.compareNamespacedImageDtos = undefined;
    this.isMainTableLoading = false;
  }

  public onRefreshRequested(event: TrivyFilterData) {
    const excludedSeverities =
      SeverityUtils.getSeverityIds().filter((severityId) => !event.selectedSeverityIds.includes(severityId)) || [];

    const params: GetInfraAssessmentReportDtos$Params = {
      namespaceName: event.namespaceName ?? undefined,
      excludedSeverities: excludedSeverities.length > 0 ? excludedSeverities.join(',') : undefined,
    };
    this.getDataDtos(params);
  }

  onMainTableMultiHeaderActionRequested(event: string) {
    switch (event) {
      case "goToDetailedPage":
        this.goToDetailedPage();
        break;
      case "Compare with...":
        this.goToComparePage();
        break;
      default:
        console.error("car - multi action call back - unknown: " + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(
      this.router.createUrlTree(['infra-assessment-reports-detailed'])
    );
    window.open(url, '_blank');
  }

  private goToComparePage() {
    if (!this.dataDtos || !this.selectedTrivyReportDto) return;
    if (this.selectedTrivyReportDto.criticalCount < 1 && this.selectedTrivyReportDto.highCount < 1 &&
      this.selectedTrivyReportDto.mediumCount < 1 && this.selectedTrivyReportDto.lowCount < 1) {
      this.messageService.add({
        severity: "info",
        summary: "Nothing to compare",
        detail: "The selected item has no details, so there is nothing to compare...",
      });

      return;
    }

    this.compareNamespacedImageDtos = this.dataDtos
      .filter(car => car.criticalCount > 0 || car.highCount > 0 || car.mediumCount > 0 || car.lowCount > 0)
      .map(car => ({
        uid: car.uid ?? '', resourceNamespace: car.resourceNamespace ?? '',
        mainLabel: car.resourceName, group: car.resourceKind }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible = true;
  }

  onMainTableSelectedRowChanged(event: InfraAssessmentReportDto | null) {
    this.selectedTrivyReportDto = event ?? undefined;
  }
}
