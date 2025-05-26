import { Component } from '@angular/core';

import { GetConfigAuditReportDtos$Params } from '../../api/fn/config-audit-report/get-config-audit-report-dtos';
import { ConfigAuditReportDto } from '../../api/models/config-audit-report-dto';
import { ConfigAuditReportService } from '../../api/services/config-audit-report.service';
import { GenericMasterDetailComponent } from '../generic-master-detail/generic-master-detail.component';
import { TrivyFilterData, TrivyTableColumn, TrivyTableOptions } from '../trivy-table/trivy-table.types';
import { SeverityUtils } from '../utils/severity.utils';

@Component({
  selector: 'app-config-audit-reports',
  standalone: true,
  imports: [GenericMasterDetailComponent],
  templateUrl: './config-audit-reports.component.html',
  styleUrl: './config-audit-reports.component.scss',
})
export class ConfigAuditReportsComponent {
  public dataDtos: ConfigAuditReportDto[] = [];
  public activeNamespaces?: string[] = [];

  public mainTableColumns: TrivyTableColumn[] = [];
  public mainTableOptions: TrivyTableOptions;
  public mainTableExpandCallbackDto: ConfigAuditReportDto | null = null;
  public isMainTableLoading: boolean = true;

  public detailsTableColumns: TrivyTableColumn[] = [];
  public detailsTableOptions: TrivyTableOptions;

  constructor(private dataDtoService: ConfigAuditReportService) {
    dataDtoService.getConfigAuditReportDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
    dataDtoService.getConfigAuditReportActiveNamespaces().subscribe({
      next: (res) => this.onGetActiveNamespaces(res),
      error: (err) => console.error(err),
    });
    this.mainTableColumns = [
      {
        field: 'resourceNamespace',
        header: 'NS',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'namespaces',
        style: 'width: 130px; max-width: 130px;',
        renderType: 'standard',
      },
      {
        field: 'resourceName',
        header: 'Name',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 265px; max-width: 265px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'resourceKind',
        header: 'Kind',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 100px; max-width: 100px;',
        renderType: 'standard',
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
    ];
    this.mainTableOptions = {
      isClearSelectionVisible: false,
      isExportCsvVisible: false,
      isResetFiltersVisible: true,
      isRefreshVisible: true,
      isRefreshFiltrable: true,
      isFooterVisible: true,
      tableSelectionMode: 'single',
      tableStyle: { width: '775px' },
      stateKey: 'Config Audit Reports - Main',
      dataKey: null,
      rowExpansionRender: null,
      extraClasses: 'trivy-half',
    };
    this.detailsTableColumns = [
      {
        field: 'severityId',
        header: 'Sev',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'severities',
        style: 'width: 90px; max-width: 90px;',
        renderType: 'severityBadge',
      },
      {
        field: 'category',
        header: 'Category',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 140px; max-width: 140px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'checkId',
        header: 'Id',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 95px; max-width: 95px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'title',
        header: 'Title',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 180px; max-width: 180px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'description',
        header: 'Description',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 360px; max-width: 360px; white-space: normal;',
        renderType: 'standard',
      },
      {
        field: 'remediation',
        header: 'Remediation',
        isFiltrable: true,
        isSortable: true,
        multiSelectType: 'none',
        style: 'width: 360px; max-width: 360px; white-space: normal;',
        renderType: 'standard',
      },
    ];
    this.detailsTableOptions = {
      isClearSelectionVisible: false,
      isExportCsvVisible: false,
      isResetFiltersVisible: true,
      isRefreshVisible: false,
      isRefreshFiltrable: false,
      isFooterVisible: false,
      tableSelectionMode: null,
      tableStyle: {},
      stateKey: 'Config Audit Reports - Details',
      dataKey: 'uid',
      rowExpansionRender: 'messages',
      extraClasses: 'trivy-half',
    };
  }

  onGetDataDtos(dtos: ConfigAuditReportDto[]) {
    this.dataDtos = dtos;
  }

  onGetActiveNamespaces(activeNamespaces: string[]) {
    this.activeNamespaces = activeNamespaces.sort((x, y) => (x > y ? 1 : -1));
  }

  public onRefreshRequested(event: TrivyFilterData) {
    const excludedSeverities =
      SeverityUtils.getSeverityIds().filter((severityId) => !event.selectedSeverityIds.includes(severityId)) || [];

    const params: GetConfigAuditReportDtos$Params = {
      namespaceName: event.namespaceName ?? undefined,
      excludedSeverities: excludedSeverities.length > 0 ? excludedSeverities.join(',') : undefined,
    };
    this.isMainTableLoading = true;
    this.dataDtoService.getConfigAuditReportDtos(params).subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }
}
