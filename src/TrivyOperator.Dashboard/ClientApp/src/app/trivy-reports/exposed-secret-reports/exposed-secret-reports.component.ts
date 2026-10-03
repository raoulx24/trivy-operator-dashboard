import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

import { ExposedSecretReportImageDto } from '../../../api/models/exposed-secret-report-image-dto';
import { ExposedSecretReportsService } from '../../../api/services/exposed-secret-reports.service';
import { GenericMasterDetailComponent } from '../../ui-elements/generic-master-detail/generic-master-detail.component';
import {
  TrivyTableColumn,
  TrivyTableExpandRowData,
} from '../../ui-elements/trivy-table/trivy-table.types';

import { ReportHelper } from '../abstracts/report-helper';
import {
  exposedSecretReportColumns,
  exposedSecretReportComparedTableColumns,
  exposedSecretReportDetailColumns,
} from '../constants/exposed-secret-reports.constants';
import { namespacedArrayColumns } from '../constants/generic.constants';

import { GenericReportsCompareComponent } from '../../ui-elements/generic-reports-compare/generic-reports-compare.component';
import { TrivyImageUsageDialogComponent } from '../../ui-elements/trivy-image-usage-dialog/trivy-image-usage-dialog.component';

import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { TrivyReportResourceInfoDto } from '../../../api/models/trivy-report-resource-info-dto';
import { TrivyDependencyDialogComponent } from '../../ui-elements/trivy-dependency-dialog/trivy-dependency-dialog.component';
import { NamespacedAggregateTrivyReportDataPageBase } from '../abstracts/pages/namespaced-trivy-report-data-page-base';
import { ExposedSecretReportDetailDto } from '../../../api/models/exposed-secret-report-detail-dto';

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
    DialogModule,
    TableModule,
    TrivyImageUsageDialogComponent,
    TrivyDependencyDialogComponent,
  ],
  templateUrl: './exposed-secret-reports.component.html',
  styleUrl: './exposed-secret-reports.component.scss',
})
export class ExposedSecretReportsComponent
  extends NamespacedAggregateTrivyReportDataPageBase<ExposedSecretReportImageDto, ExposedSecretReportDetailDto, ExposedSecretReportImageTableDto> implements OnInit {

  mainTableColumns: TrivyTableColumn[] = [...namespacedArrayColumns, ...exposedSecretReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...exposedSecretReportDetailColumns];

  imageUsageResources: TrivyReportResourceInfoDto[] = [];
  imageUsageImageNameAndTag = '';
  isImageUsageDialogVisible = false;

  isTrivyReportsCompareVisible = signal<boolean>(false);
  compareFirstSelectedIdId?: string;
  comparedTableColumns: TrivyTableColumn[] = [...exposedSecretReportComparedTableColumns];

  private readonly dataDtoService = inject(ExposedSecretReportsService);
  private readonly router = inject(Router);

  protected override dataDtosLoader =
    () => this.dataDtoService.getExposedSecretReportImageDtos();

  ngOnInit() {
    this.initialize();
  }

  protected override prepareViewDataDtos(dto: ExposedSecretReportImageDto): ExposedSecretReportImageTableDto {
    return {
      ...dto,
      __namespaceNamesSort: [...dto.namespaceNames].sort().join(', '),
    };
  }

  onMainTableExpandCallback(dto: ExposedSecretReportImageDto) {
    this.imageUsageResources = dto.resources ?? [];
    this.imageUsageImageNameAndTag = dto.lastImageNameAndTag ?? 'N/A';
    this.isImageUsageDialogVisible = true;
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
    if (!this.hasSeverities(this.selectedTrivyReportDto)) {
      this.messageService.pushSimple(
        'Nothing to compare',
        'Exposed Secret Reports',
        'info',
        'The selected item has no details, so there is nothing to compare...',
      );

      return;
    }

    this.compareNamespacedImageDtos = this.dataDtos
      .filter((esr) => this.hasSeverities(esr))
      .map((esr) => ({
        uid: esr.uid ?? '',
        resourceNamespace: 'N/A',
        mainLabel: `${esr.lastImageNameAndTag}`,
      }));
    this.compareFirstSelectedIdId = this.selectedTrivyReportDto.uid;
    this.isTrivyReportsCompareVisible.set(true);
  }
}
