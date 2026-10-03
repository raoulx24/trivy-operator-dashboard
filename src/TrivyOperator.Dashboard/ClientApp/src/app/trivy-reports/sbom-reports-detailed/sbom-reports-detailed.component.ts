import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
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
  private readonly http = inject(HttpClient);

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
        console.error('sbom detailed - multi action call back - unknown: ' + event);
    }
  }

  exportSboms(exportType: 'all' | 'selected') {
    this.messageService.pushSimple('Download SBOMs', 'SBOMs', 'info', 'Download request sent. Please wait...');
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const params = new HttpParams().set('fileType', 'json');
    const apiUrl = `${this.service.rootUrl}api/sbom-reports/export`;

    let sbomsExport: { digest: string }[];

    if (exportType == 'all') {
      sbomsExport =
        this.dataDtos?.map((x) => {
          return { digest: x.digest ?? '' };
        }) ?? [];
    } else {
      sbomsExport =
        this.selectedDataDtos?.map((x) => {
          return { digest: x.digest ?? '' };
        }) ?? [];
    }

    // const httpReq = new HttpRequest("POST", apiUrl, sbomsExport, { headers: headers, params: params, responseType: 'blob' });

    this.http.post(apiUrl, sbomsExport, { headers, params, responseType: 'blob' as 'json' }).subscribe({
      next: (blob: any) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        const datetimeUtc = new Date().toISOString().replace(/:/g, '.').replace('T', '-').slice(0, 19);
        link.href = url;
        link.download = `sboms.exports.${datetimeUtc}.zip`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.onError(err);
      },
    });
  }
}
