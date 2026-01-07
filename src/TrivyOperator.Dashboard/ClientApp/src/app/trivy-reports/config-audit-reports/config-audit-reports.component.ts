import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { GetConfigAuditReportDtos$Params } from '../../../api/fn/config-audit-report/get-config-audit-report-dtos';
import { ConfigAuditReportDto } from '../../../api/models/config-audit-report-dto';
import { ConfigAuditReportService } from '../../../api/services/config-audit-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyFilterData, TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';
import { SeverityUtils } from '../../utils/severity.utils';
import { namespacedColumns } from '../constants/generic.constants';
import {
  configAuditReportColumns,
  configAuditReportComparedTableColumns,
  configAuditReportDetailColumns,
} from '../constants/config-audit-reports.constants';

import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { NamespacedImageDto } from '../../ui-elements/namespace-image-selector/namespace-image-selector.types';

import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-config-audit-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, DialogModule, GenericReportsCompareComponent],
  templateUrl: './config-audit-reports.component.html',
  styleUrl: './config-audit-reports.component.scss',
})
export class ConfigAuditReportsComponent implements OnInit {
  dataDtos: ConfigAuditReportDto[] = [];
  activeNamespaces: string[] = [];

  mainTableColumns: TrivyTableColumn[] = [... namespacedColumns, ...configAuditReportColumns];
  isMainTableLoading: boolean = true;

  detailsTableColumns: TrivyTableColumn[] = [... configAuditReportDetailColumns ];

  queryUid?: string;
  isSingleMode: boolean = false;
  selectedTrivyReportDto?: ConfigAuditReportDto;

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  compareNamespacedImageDtos?: NamespacedImageDto[];
  comparedTableColumns: TrivyTableColumn[] = [... configAuditReportComparedTableColumns];

  private readonly dataDtoService = inject(ConfigAuditReportService);
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

  getDataDtos(params?: GetConfigAuditReportDtos$Params) {
    this.isMainTableLoading = true;
    this.dataDtoService.getConfigAuditReportDtos(params).subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  onGetDataDtos(dtos: ConfigAuditReportDto[]) {
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

    const params: GetConfigAuditReportDtos$Params = {
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
      this.router.createUrlTree(['config-audit-reports-detailed'])
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
    this.isTrivyReportsCompareVisible.set(true);
  }

  onMainTableSelectedRowChanged(event: ConfigAuditReportDto | null) {
    this.selectedTrivyReportDto = event ?? undefined;
  }
}
