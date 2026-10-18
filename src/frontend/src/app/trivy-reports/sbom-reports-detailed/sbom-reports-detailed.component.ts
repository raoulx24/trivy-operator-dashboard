import { Component, inject, OnInit } from '@angular/core';

import { SbomReportImageMinimalDto } from '../../../api/models/sbom-report-image-minimal-dto';
import { SbomReportService } from '../../../api/services/sbom-report.service';
import { sbomReportDenormalizedColumns } from '../constants/sbom-reports.constans';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { SelectedDtosEvent, TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

import { namespacedColumns } from '../constants/generic.constants';
import { NamespacedDataPageBase } from '../abstracts/pages/namespaced-trivy-report-data-page-base';

@Component({
  selector: 'app-sbom-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './sbom-reports-detailed.component.html',
  styleUrl: './sbom-reports-detailed.component.scss',
})
export class SbomReportsDetailedComponent extends NamespacedDataPageBase<SbomReportImageMinimalDto> implements OnInit {
  selectedDataDtos: SbomReportImageMinimalDto[] | null = null;

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...sbomReportDenormalizedColumns];

  private readonly service = inject(SbomReportService);

  protected override dataDtosLoader = () => this.service.getSbomReportImageMinimalDtos();

  private readonly sbomReportsService = inject(SbomReportService);

  ngOnInit() {
    this.initialize();
  }

  onTableSelectedRowChange(event: SelectedDtosEvent<SbomReportImageMinimalDto>) {
    this.selectedDataDtos = event.selectedDtos;
  }

  onMultiHeaderActionRequested(event: string) {
    switch (event) {
      case 'Export All':
        this.exportSboms('all');
        break;
      case 'Export Selected':
        this.exportSboms('selected');
        break;
      default:
        console.error('sbom detailed - multi action call back - unknown: ' + event);
    }
  }

  exportSboms(exportType: 'all' | 'selected') {
    this.log.info('Download request sent. Please wait...', { source: 'SBOMs', toast: true });

    const sbomsExport: { digest: string }[] =
      exportType === 'all'
        ? this.dataDtos?.map((x) => ({ digest: x.digest ?? '' })) ?? []
        : this.selectedDataDtos?.map((x) => ({ digest: x.digest ?? '' })) ?? [];

    this.sbomReportsService
      .exportSbomReport({ body: sbomsExport, fileType: 'json', })
      .subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          const datetimeUtc =
            new Date().toISOString().replace(/:/g, '.').replace('T', '-').slice(0, 19);

          link.href = url;
          link.download = `sboms.exports.${datetimeUtc}.zip`;

          link.click();
          link.remove();

          setTimeout(() => window.URL.revokeObjectURL(url), 1000);
        },
        error: (err) => { this.onError(err); },
      });
  }
}
