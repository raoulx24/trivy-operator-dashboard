import { Component, inject, OnInit } from '@angular/core';

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

import { TrivyImageUsageDialogComponent } from '../../ui-elements/trivy-image-usage-dialog/trivy-image-usage-dialog.component';

import { TrivyReportResourceInfoDto } from '../../../api/models/trivy-report-resource-info-dto';
import { TrivyDependencyDialogComponent } from '../../ui-elements/trivy-dependency-dialog/trivy-dependency-dialog.component';
import { NamespacedAggregateTrivyReportDataPageBase } from '../abstracts/pages/namespaced-trivy-report-data-page-base';
import { ExposedSecretReportDetailDto } from '../../../api/models/exposed-secret-report-detail-dto';
import {
  TrivyReportsCompareDialogComponent
} from '../../ui-elements/trivy-reports-compare-dialog/trivy-reports-compare-dialog.component';

// for sorting in trivy table
type ExposedSecretReportImageTableDto = ExposedSecretReportImageDto & {
  __namespaceNamesSort: string;
};

@Component({
  selector: 'app-exposed-secret-reports',
  standalone: true,
  imports: [
    GenericMasterDetailComponent,
    TrivyImageUsageDialogComponent,
    TrivyDependencyDialogComponent,
    TrivyReportsCompareDialogComponent,
  ],
  templateUrl: './exposed-secret-reports.component.html',
  styleUrl: './exposed-secret-reports.component.scss',
})
export class ExposedSecretReportsComponent
  extends NamespacedAggregateTrivyReportDataPageBase<ExposedSecretReportImageDto, ExposedSecretReportDetailDto, ExposedSecretReportImageTableDto> implements OnInit {
  protected override detailedPageRoute: string = 'exposed-secret-reports-detailed';
  protected override friendlyReportName: string = 'Exposed Secret Reports';

  mainTableColumns: TrivyTableColumn[] = [...namespacedArrayColumns, ...exposedSecretReportColumns];

  detailsTableColumns: TrivyTableColumn[] = [...exposedSecretReportDetailColumns];

  imageUsageResources: TrivyReportResourceInfoDto[] = [];
  imageUsageImageNameAndTag = '';
  isImageUsageDialogVisible = false;

  comparedTableColumns: TrivyTableColumn[] = [...exposedSecretReportComparedTableColumns];

  private readonly dataDtoService = inject(ExposedSecretReportsService);

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
}
