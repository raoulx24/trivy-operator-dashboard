import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

import { GetExposedSecretReportImageDtos$Params } from '../../../api/fn/exposed-secret-reports/get-exposed-secret-report-image-dtos';
import { ExposedSecretReportImageDto } from '../../../api/models/exposed-secret-report-image-dto';
import { ExposedSecretReportsService } from '../../../api/services/exposed-secret-reports.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import {
  TrivyFilterData,
  TrivyTableColumn,
  TrivyTableExpandRowData,
} from '../../ui-elements/trivy-table/trivy-table.types';
import { SeverityUtils } from '../../utils/severity.utils';

import { ReportHelper } from '../abstracts/trivy-report-image';
import {
  exposedSecretReportColumns,
  exposedSecretReportComparedTableColumns,
  exposedSecretReportDetailColumns,
} from '../constants/exposed-secret-reports.constants';
import { namespacedArrayColumns } from '../constants/generic.constants';

import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { NamespacedImageDto } from '../../ui-elements/namespace-image-selector/namespace-image-selector.types';
import { ImageInfo, TrivyDependencyComponent } from '../../ui-elements/trivy-dependency/trivy-dependency.component';

import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { DataPageBase } from '../../abstracts/data-page-base';

// for sorting in trivy table
type ExposedSecretReportImageTableDto = ExposedSecretReportImageDto & {
  __namespaceNamesSort: string;
};

@Component({
  selector: 'app-exposed-secret-reports',
  standalone: true,
  imports: [
    GenericMasterDetailComponent,
    GenericReportsCompareComponent,
    TrivyDependencyComponent,
    DialogModule,
    TableModule,
  ],
  templateUrl: './exposed-secret-reports.component.html',
  styleUrl: './exposed-secret-reports.component.scss',
})
export class ExposedSecretReportsComponent extends DataPageBase implements OnInit {
  dataDtos: ExposedSecretReportImageTableDto[] = [];
  activeNamespaces: string[] = [];

  mainTableColumns: TrivyTableColumn[] = [...namespacedArrayColumns, ...exposedSecretReportColumns];
  mainTableExpandCallbackDto?: ExposedSecretReportImageDto;

  detailsTableColumns: TrivyTableColumn[] = [...exposedSecretReportDetailColumns];

  isImageUsageDialogVisible = signal<boolean>(false);

  queryNamespaceName?: string;
  queryDigest?: string;
  isPreselected: boolean = false;
  selectedTrivyReportDto?: ExposedSecretReportImageDto;

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  compareNamespacedImageDtos?: NamespacedImageDto[];
  comparedTableColumns: TrivyTableColumn[] = [...exposedSecretReportComparedTableColumns];

  isDependencyTreeViewVisible = signal<boolean>(false);
  trivyImage?: ImageInfo;
  trivyDependencyDialogTitle: string = '';

  private readonly dataDtoService = inject(ExposedSecretReportsService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  ngOnInit() {
    const state = history.state;

    this.queryNamespaceName = state.namespaceName;
    this.queryDigest = state.digest;

    this.isPreselected = !!(this.queryNamespaceName && this.queryDigest);
    this.getDataDtos();
  }

  private getDataDtos() {
    this.isMainTableLoading = true;
    this.dataDtoService.getExposedSecretReportImageDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  private onGetDataDtos(dtos: ExposedSecretReportImageDto[]) {
    this.dataDtos = dtos.map((dto) => ({
      ...dto,
      __namespaceNamesSort: [...dto.namespaceNames].sort().join(', '),
    }));
    this.activeNamespaces = Array.from(new Set(dtos.flatMap(dto => dto.namespaceNames))).sort();
    if (this.isPreselected) {
      this.selectedTrivyReportDto = dtos.find(
        (x) => x.digest == this.queryDigest,
      );
    }
    this.isMainTableLoading = false;
  }

  onMainTableExpandCallback(dto: ExposedSecretReportImageDto) {
    this.mainTableExpandCallbackDto = dto;
    this.isImageUsageDialogVisible.set(true);
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
    return `Image Usage for ${this.mainTableExpandCallbackDto?.lastImageNameAndTag ?? 'N/A'}`;
  }

  rowExpandResponse?: TrivyTableExpandRowData<ExposedSecretReportImageDto>;
  onRowExpandChange(dto: ExposedSecretReportImageDto) {
    this.rowExpandResponse = {
      rowKey: dto,
      colStyles: [
        { width: '70px', 'min-width': '70px', height: '50px' },
        { 'white-space': 'normal', display: 'flex', 'align-items': 'center', height: '50px' },
      ],
      details: [
        // [
        //   { label: 'Image Digest' },
        //   { label: dto.imageDigest ?? '' },
        // ],
        [
          { label: 'Registry' },
          { label: dto.lastImageRegistry ?? '' },
        ],
        // [
        //   { label: 'Update Moment' },
        //   { label: '', localTime: dto.updateTimestamp },
        // ],
        [
          { label: 'Used By' },
          ReportHelper.getNarrowedResourceNames(dto),
        ],
      ],
    };
  }

  onMainTableMultiHeaderActionRequested(event: string) {
    switch (event) {
      case 'goToDetailedPage':
        this.goToDetailedPage();
        break;
      case 'Compare with...':
        this.goToComparePage();
        break;
      case 'Dependency tree':
        this.goToDependencyTree();
        break;
      default:
        console.error('esr - multi action call back - unknown: ' + event);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['exposed-secret-reports-detailed']));
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
      .filter((esr) => esr.criticalCount > 0 || esr.highCount > 0 || esr.mediumCount > 0 || esr.lowCount > 0)
      .map((esr) => ({
        uid: esr.uid ?? '',
        resourceNamespace: 'N/A',
        mainLabel: `${esr.lastImageNameAndTag}`,
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }

  private goToDependencyTree() {
    const digest = this.selectedTrivyReportDto?.digest;
    if (digest) {
      const imageNameAndTag = this.selectedTrivyReportDto?.lastImageNameAndTag ?? 'n/a';
      this.trivyDependencyDialogTitle = `Dependency Tree for Image ${imageNameAndTag}`;
      this.trivyImage = { digest: digest };
      this.isDependencyTreeViewVisible.set(true);
    }
  }

  onMainTableSelectedRowChanged(event: ExposedSecretReportImageDto | null) {
    this.selectedTrivyReportDto = event ?? undefined;
  }
}
