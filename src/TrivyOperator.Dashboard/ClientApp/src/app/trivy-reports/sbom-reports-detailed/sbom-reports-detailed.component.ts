import { Component, inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

import { SbomReportService } from '../../../api/services';
import { SbomReportImageDto } from '../../../api/models';
import { sbomReportDenormalizedColumns } from '../constants/sbom-reports.constans';

import { TrivyTableComponent } from '../../ui-elements/trivy-table/trivy-table.component'
import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

import { MessageService } from 'primeng/api';
import { namespacedColumns } from '../constants/generic.constants';
import { TrivyReportsDetailedBase } from '../abstracts/trivy-reports-detailed-base';

@Component({
  selector: 'app-sbom-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './sbom-reports-detailed.component.html',
  styleUrl: './sbom-reports-detailed.component.scss'
})
export class SbomReportsDetailedComponent  extends TrivyReportsDetailedBase implements OnInit {
  dataDtos: SbomReportImageDto[] | null = null;
  activeNamespaces: string[] = [];
  selectedDataDtos: SbomReportImageDto[] | null = null;
  isTableLoading: boolean = false;

  trivyTableColumns: TrivyTableColumn[] = [...namespacedColumns, ...sbomReportDenormalizedColumns];

  private readonly service = inject(SbomReportService);
  private readonly http = inject(HttpClient);
  private readonly messageService = inject(MessageService);

  ngOnInit() {
    this.getTableDataDtos();
  }

  getTableDataDtos() {
    this.isTableLoading = true;
    this.service.getSbomReportImageDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  private onGetDataDtos(dtos: SbomReportImageDto[]) {
    this.dataDtos = dtos;
    this.activeNamespaces = Array
      .from(new Set(dtos.map(dto => dto.resourceNamespace ?? "N/A")))
      .sort();
    this.isTableLoading = false;
  }

  onTableSelectedRowChange(event: SbomReportImageDto[]) {
    this.selectedDataDtos = event;
  }

  onMultiHeaderActionRequested(event: string) {
    switch (event) {
      case "Export All":
        this.exportSboms("all");
        break;
      case "Export Selected":
        this.exportSboms("selected");
        break;
      default:
        console.error("sbom detailed - multi action call back - unknown: " + event);
    }
  }

  exportSboms(exportType: "all" | "selected") {
    this.messageService.add({
      severity: "info",
      summary: "Download SBOMs",
      detail: "Download request sent. Please wait...",
    })
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const params = new HttpParams().set('fileType', 'json');
    const apiUrl = `${this.service.rootUrl}api/sbom-reports/export`;

    let sbomsExport: { namespaceName: string, digest: string }[];

    if (exportType == "all") {
      sbomsExport = this.dataDtos?.map(x => {
        return { namespaceName: x.resourceNamespace ?? "", digest: x.imageDigest ?? "" }
      }) ?? [];
    }
    else {
      sbomsExport = this.selectedDataDtos?.map(x => {
        return { namespaceName: x.resourceNamespace ?? "", digest: x.imageDigest ?? "" }
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
        console.error(`Error during the POST request:`, err);
      }
    });
  }
}
