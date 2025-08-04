import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { GetExposedSecretReportImageDtos$Params } from '../../../api/fn/exposed-secret-report/get-exposed-secret-report-image-dtos';
import { ExposedSecretReportImageDto } from '../../../api/models/exposed-secret-report-image-dto';
import { ExposedSecretReportService } from '../../../api/services/exposed-secret-report.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import { TrivyFilterData, TrivyTableColumn, TrivyTableExpandRowData } from '../../ui-elements/trivy-table/trivy-table.types';
import { SeverityUtils } from '../../utils/severity.utils';

import { ReportHelper, TrivyReportImageDto } from '../abstracts/trivy-report-image';
import { VulnerabilityReportImageResourceDto } from '../../../api/models/vulnerability-report-image-resource-dto';
import { namespacedColumns } from '../constants/generic.constants';
import {
  exposedSecretReportColumns,
  exposedSecretReportComparedTableColumns,
  exposedSecretReportDetailColumns,
} from '../constants/exposed-secret-reports.constants';

import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { TrivyDependencyComponent, ImageInfo } from '../../trivy-dependency/trivy-dependency.component';
import { NamespacedImageDto } from '../../ui-elements/namespace-image-selector/namespace-image-selector.types';

import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-exposed-secret-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent, GenericReportsCompareComponent, TrivyDependencyComponent, DialogModule, TableModule],
  templateUrl: './exposed-secret-reports.component.html',
  styleUrl: './exposed-secret-reports.component.scss',
})
export class ExposedSecretReportsComponent implements OnInit {
  dataDtos: ExposedSecretReportImageDto[] = [];
  activeNamespaces?: string[] = [];

  mainTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...exposedSecretReportColumns];
  mainTableExpandCallbackDto?: ExposedSecretReportImageDto;
  isMainTableLoading: boolean = true;

  detailsTableColumns: TrivyTableColumn[] = [...exposedSecretReportDetailColumns];

  isImageUsageDialogVisible: boolean = false;

  queryNamespaceName?: string;
  queryDigest?: string;
  isSingleMode: boolean = false;
  selectedTrivyReportDto?: ExposedSecretReportImageDto;

  isTrivyReportsCompareVisible: boolean = false;
  compareFirstSelectedIdId?: string;
  compareNamespacedImageDtos?: NamespacedImageDto[];
  comparedTableColumns: TrivyTableColumn[] = [... exposedSecretReportComparedTableColumns];

  isDependencyTreeViewVisible: boolean = false;
  trivyImage?: ImageInfo;
  trivyDependencyDialogTitle: string = "";

  private readonly dataDtoService = inject(ExposedSecretReportService);
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly messageService = inject(MessageService);

  ngOnInit() {
    this.activatedRoute.queryParamMap.subscribe(params => {
      this.queryNamespaceName = params.get('namespaceName') ?? undefined;
      this.queryDigest = params.get('digest') ?? undefined;
    });
    this.isSingleMode = !!(this.queryNamespaceName && this.queryDigest);
    this.getDataDtos();
  }

  private getDataDtos() {
    this.isMainTableLoading = true;
    this.dataDtoService.getExposedSecretReportImageDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  private onGetDataDtos(dtos: ExposedSecretReportImageDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array
      .from(new Set(dtos.map(dto => dto.resourceNamespace ?? "N/A")))
      .sort();
    if (this.isSingleMode) {
      this.selectedTrivyReportDto = dtos
        .find(x => x.imageDigest == this.queryDigest && x.resourceNamespace == this.queryNamespaceName);
    }
    this.isMainTableLoading = false;
  }

  onMainTableExpandCallback(dto: ExposedSecretReportImageDto) {
    this.mainTableExpandCallbackDto = dto;
    this.isImageUsageDialogVisible = true;
  }

  onRefreshRequested(event: TrivyFilterData) {
    const excludedSeverities =
      SeverityUtils.getSeverityIds().filter((severityId) => !event.selectedSeverityIds.includes(severityId)) || [];

    const params: GetExposedSecretReportImageDtos$Params = {
      namespaceName: event.namespaceName ?? undefined,
      excludedSeverities: excludedSeverities.length > 0 ? excludedSeverities.join(',') : undefined,
    };
    this.isMainTableLoading = true;
    this.dataDtoService.getExposedSecretReportImageDtos(params).subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  getPanelHeaderText() {
    return `Image Usage for ${this.mainTableExpandCallbackDto?.imageName}:${this.mainTableExpandCallbackDto?.imageTag} in namespace ${this.mainTableExpandCallbackDto?.resourceNamespace}`;
  }

  rowExpandResponse?: TrivyTableExpandRowData<ExposedSecretReportImageDto>;
  onRowExpandChange(dto: ExposedSecretReportImageDto) {
    this.rowExpandResponse = {
      rowKey: dto,
      colStyles: [
        { 'width': '70px', 'min-width': '70px', 'height': '50px' },
        { 'white-space': 'normal', 'display': 'flex', 'align-items': 'center', 'height': '50px' }
      ],
      details: [
        // [
        //   { label: 'Image Digest' },
        //   { label: dto.imageDigest ?? '' },
        // ],
        [
          { label: 'Repository' },
          { label: dto.imageRepository ?? ''},
        ],
        // [
        //   { label: 'Update Moment' },
        //   { label: '', localTime: dto.updateTimestamp },
        // ],
        [
          { label: 'Used By' },
          ReportHelper.getNarrowedResourceNames(
            dto as TrivyReportImageDto<VulnerabilityReportImageResourceDto>),
        ],
      ]
    }
  }

  onMainTableMultiHeaderActionRequested(event: string) {
    switch (event) {
      case "goToDetailedPage":
        this.goToDetailedPage();
        break;
      case "Compare with...":
        this.goToComparePage();
        break;
      case "Dependency tree":
        this.goToDependencyTree();
        break;
      default:
        console.error("esr - multi action call back - unknown: " + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(
      this.router.createUrlTree(['/exposed-secret-reports-detailed'])
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
      .filter(esr => esr.criticalCount > 0 || esr.highCount > 0 || esr.mediumCount > 0 || esr.lowCount > 0)
      .map(esr => ({
        uid: esr.uid ?? '', resourceNamespace: esr.resourceNamespace ?? '',
        mainLabel: `${esr.imageName ?? ''}:${esr.imageTag ?? '' }`}));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible = true;
  }

  private goToDependencyTree() {
    const digest = this.selectedTrivyReportDto?.imageDigest;
    const namespace = this.selectedTrivyReportDto?.resourceNamespace;
    if (digest && namespace) {
      const imageRepository = this.selectedTrivyReportDto?.imageRepository ?? 'n/a';
      const imageName = this.selectedTrivyReportDto?.imageName ?? 'n/a';
      const imageTag = this.selectedTrivyReportDto?.imageTag ?? 'n/a';
      const imageNamespace = this.selectedTrivyReportDto?.resourceNamespace ?? 'n/a';
      this.trivyDependencyDialogTitle = `Dependency Tree for Image ${imageRepository}/${imageName}:${imageTag} in ${imageNamespace}`;
      this.trivyImage = { digest: digest, namespaceName: namespace };
      this.isDependencyTreeViewVisible = true;
    }
  }

  onMainTableSelectedRowChanged(event: ExposedSecretReportImageDto | null) {
    this.selectedTrivyReportDto = event ?? undefined;
  }
}
