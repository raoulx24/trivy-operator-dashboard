import { Component } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpRequest } from '@angular/common/http';

import { SbomReportService } from '../../api/services';
import { SbomReportImageDto } from '../../api/models';

import { TrivyTableComponent } from '../trivy-table/trivy-table.component'
import { ExportColumn, TrivyExpandTableOptions, TrivyTableCellCustomOptions, TrivyTableColumn, TrivyTableOptions } from '../trivy-table/trivy-table.types';
import { TrivyTableUtils } from '../utils/trivy-table.utils';

import { MessageService } from 'primeng/api';


@Component({
  selector: 'app-sbom-reports-detailed',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './sbom-reports-detailed.component.html',
  styleUrl: './sbom-reports-detailed.component.scss'
})
export class SbomReportsDetailedComponent {
  dataDtos: SbomReportImageDto[] | null = null;
  activeNamespaces: string[] | undefined = [];
  selectedDataDtos: SbomReportImageDto[] | null = null;
  isTableLoading: boolean = false;

  trivyTableColumns: TrivyTableColumn[] = [];
  trivyTableOptions: TrivyTableOptions;
  //dependsOnTableExpandTableOptions: TrivyExpandTableOptions<SbomReportImageDto> = new TrivyExpandTableOptions(false, 2, 0, this.getPropertiesCount);
  public exportColumns: ExportColumn[];

  constructor(private service: SbomReportService, private http: HttpClient, private messageService: MessageService) {
    this.getTableDataDtos();

    this.trivyTableColumns = [
      {
        field: 'resourceNamespace',
        header: 'Namespace',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'namespaces',
        style: 'width: 130px; max-width: 130px;',
        renderType: 'standard',
      },
      {
        field: 'imageName',
        header: 'Image Name',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 380px; max-width: 380px;',
        renderType: 'standard',
      },
      {
        field: 'imageTag',
        header: 'Image Tag',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 210px; max-width: 210px;',
        renderType: 'standard',
      },
      {
        field: 'imageDigest',
        header: 'Image Digest',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 550px; max-width: 550px;',
        renderType: 'standard',
      },
      {
        field: 'repository',
        header: 'Repository',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 270px; max-width: 270px;',
        renderType: 'standard',
      },
      {
        field: 'hasVulnerabilities',
        header: 'Has VRs',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 130px; max-width: 130px;',
        renderType: 'boolean',
      },
      {
        field: 'criticalCount',
        header: 'C',
        isFiltrable: false,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 50px; max-width: 50px;',
        renderType: 'severityValue',
        extraFields: ['0'],
      },
      {
        field: 'highCount',
        header: 'H',
        isFiltrable: false,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 50px;',
        renderType: 'severityValue',
        extraFields: ['1'],
      },
      {
        field: 'mediumCount',
        header: 'M',
        isFiltrable: false,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 50px; max-width: 50px;',
        renderType: 'severityValue',
        extraFields: ['2'],
      },
      {
        field: 'lowCount',
        header: 'L',
        isFiltrable: false,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 50px; max-width: 50px;',
        renderType: 'severityValue',
        extraFields: ['3'],
      },
      {
        field: 'unknownCount',
        header: 'U',
        isFiltrable: false,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 50px; max-width: 50px;',
        renderType: 'severityValue',
        extraFields: ['4'],
      },
    ];
    this.trivyTableOptions = {
      isClearSelectionVisible: true,
      isExportCsvVisible: true,
      isResetFiltersVisible: true,
      isRefreshVisible: true,
      isRefreshFiltrable: false,
      isFooterVisible: true,
      tableSelectionMode: 'multiple',
      tableStyle: { width: '1920px' },
      stateKey: 'SBOM Reports - Detailed',
      //dataKey: 'uid',
      dataKey: null,
      rowExpansionRender: null,
      extraClasses: '',
      multiHeaderAction: ["Export All", "Export Selected"],
    };
    this.exportColumns = TrivyTableUtils.convertFromTableColumnToExportColumn(this.trivyTableColumns);
  }

  getTableDataDtos() {
    this.isTableLoading = true;
    this.service.getSbomReportImageDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
    this.service.getSbomReportActiveNamespaces().subscribe({
      next: (res) => (this.activeNamespaces = res.sort()),
      error: (err) => console.error(err),
    });
  }

  private onGetDataDtos(dtos: SbomReportImageDto[]) {
    this.dataDtos = dtos;
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

  exportSboms(exporType: "all" | "selected") {
    this.messageService.add({
      severity: "info",
      summary: "Download SBOMs",
      detail: "Download request sent. Please wait...",
    })
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const params = new HttpParams().set('fileType', 'json');
    const apiUrl = `${this.service.rootUrl}/api/sbom-reports/export`;

    let sbomsExport: { namespaceName: string, digest: string }[] = []

    if (exporType == "all") {
      sbomsExport = this.dataDtos?.map(x => {
        return { namespaceName: x.resourceNamespace ?? "", digest: x.imageDigest ?? "" }
      }) ?? [];
    }
    else {
      sbomsExport = this.selectedDataDtos?.map(x => {
        return { namespaceName: x.resourceNamespace ?? "", digest: x.imageDigest ?? "" }
      }) ?? [];
    }

    const httpReq = new HttpRequest("POST", apiUrl, sbomsExport, { headers: headers, params: params, responseType: 'blob' });

    this.http.post(apiUrl, sbomsExport, { headers, params, responseType: 'blob' as 'json' }).subscribe({
      next: (blob: any) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        const datetimeutc = new Date().toISOString().replace(/:/g, '.').replace('T', '-').slice(0, 19);;
        link.href = url;
        link.download = `sboms.exports.${datetimeutc}.zip`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error(`Error during the POST request:`, err);
      }
    });
  }

  // # region table expand row
  //getPropertiesCount(data: SbomReportImageDto): number {
  //  return data.details?.[0]?.properties?.length ?? 0;
  //}

  //dependsOnTableExpandCellOptions(
  //  dto: SbomReportImageDto,
  //  type: 'header' | 'row',
  //  colIndex: number,
  //  rowIndex?: number,
  //): TrivyTableCellCustomOptions {
  //  rowIndex ?? 0;
  //  let celValue: string = '';
  //  let celStyle: string = '';
  //  let celBadge: string | undefined;
  //  let celButtonLink: string | undefined;
  //  let celUrl: string | undefined;

  //  switch (colIndex) {
  //    case 0:
  //      celStyle = 'width: 70px; min-width: 70px; height: 50px';
  //      celValue = dto.details?.[0]?.properties?.[rowIndex ?? 0]?.[0] ?? "";
  //      break;
  //    case 1:
  //      celStyle = 'white-space: normal; display: flex; align-items: center; height: 50px;';
  //      celValue = dto.details?.[0]?.properties?.[rowIndex ?? 0]?.[1] ?? "";
  //      break;
  //  }

  //  return {
  //    value: celValue,
  //    style: celStyle,
  //    badge: celBadge,
  //    buttonLink: celButtonLink,
  //    url: celUrl,
  //  };
  //}
  // #endregion
}
