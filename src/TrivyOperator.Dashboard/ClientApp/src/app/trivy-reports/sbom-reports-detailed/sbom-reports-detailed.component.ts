import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';

import { SbomReportImageMinimalDto } from '../../../api/models/sbom-report-image-minimal-dto';
import { SbomReportService } from '../../../api/services/sbom-report.service';
import { sbomReportDenormalizedColumns } from '../constants/sbom-reports.constans';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component';
import { SelectedDtosEvent, TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

import { MessageService } from 'primeng/api';
import { TrivyReportsDetailedBase } from '../abstracts/trivy-reports-detailed-base';
import { namespacedColumns } from '../constants/generic.constants';

@Component({
  selector: 'app-sbom-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './sbom-reports-detailed.component.html',
  styleUrl: './sbom-reports-detailed.component.scss',
})
export class SbomReportsDetailedComponent extends TrivyReportsDetailedBase implements OnInit {
  dataDtos: SbomReportImageMinimalDto[] = [];
  activeNamespaces: string[] = [];
  selectedDataDtos: SbomReportImageMinimalDto[] | null = null;

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...sbomReportDenormalizedColumns];

  private readonly service = inject(SbomReportService);
  private readonly http = inject(HttpClient);
  private readonly messageService = inject(MessageService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  getTableDataDtos() {
    this.isMainTableLoading = true;
    this.service.getSbomReportImageMinimalDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  private onGetDataDtos(dtos: SbomReportImageMinimalDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array.from(new Set(dtos.map((dto) => dto.resourceNamespace))).sort();
    this.isMainTableLoading = false;
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
    this.messageService.add({
      severity: 'info',
      summary: 'Download SBOMs',
      detail: 'Download request sent. Please wait...',
    });
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
