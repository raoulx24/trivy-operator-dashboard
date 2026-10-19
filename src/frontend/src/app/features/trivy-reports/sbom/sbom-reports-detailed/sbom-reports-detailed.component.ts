import { Component, inject, OnInit } from '@angular/core';

import { SbomReportImageMinimalDto } from '@api/models/sbom-report-image-minimal-dto';
import { SbomReportService } from '@api/services/sbom-report.service';
import { sbomReportDenormalizedColumns } from '../sbom-reports.constants';

import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { SelectedDtosEvent, TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { namespacedColumns } from '@shared/constants/generic.constants';
import { NamespacedDataPageBase } from '@shared/abstracts/pages/namespaced-trivy-report-data-page-base';

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
        this.log.warn('Unknown multi action callback.', { data: event, toast: false });
    }
  }

  exportSboms(exportType: 'all' | 'selected') {
    this.log.info('Download request sent. Please wait...', { toast: true });

    const sbomsExport: { digest: string }[] =
      exportType === 'all'
        ? (this.dataDtos?.map((x) => ({ digest: x.digest ?? '' })) ?? [])
        : (this.selectedDataDtos?.map((x) => ({ digest: x.digest ?? '' })) ?? []);

    this.service.exportSbomReport({ body: sbomsExport, fileType: 'json' }).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        const datetimeUtc = new Date().toISOString().replace(/:/g, '.').replace('T', '-').slice(0, 19);

        link.href = url;
        link.download = `sboms.exports.${datetimeUtc}.zip`;

        link.click();
        link.remove();

        setTimeout(() => window.URL.revokeObjectURL(url), 1000);
      },
      error: (err) => {
        this.onError(err);
      },
    });
  }
}
