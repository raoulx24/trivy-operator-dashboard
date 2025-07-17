import { CommonModule } from '@angular/common';
import { Component, effect, input, OnInit, output, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { MenuItem } from 'primeng/api';
import { MultiSelectModule } from 'primeng/multiselect';
import { Popover, PopoverModule } from 'primeng/popover';
import { Select } from 'primeng/select';
import { SplitButton, SplitButtonModule } from 'primeng/splitbutton';
import { Table, TableModule, TableRowCollapseEvent, TableRowExpandEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';

import { LocalStorageUtils } from '../../utils/local-storage.utils';
import { PrimengTableStateUtil } from '../../utils/primeng-table-state.util';
import { SeverityDto } from '../../../api/models/severity-dto';
import { SeverityUtils } from '../../utils/severity.utils';
import { MultiHeaderAction, TrivyFilterData, TrivyTableColumn, TrivyTableExpandRowData, } from './trivy-table.types';

import { BooleanCssStylePipe } from '../../pipes/boolean-css-style.pipe';
import { CapitalizeFirstPipe } from '../../pipes/capitalize-first.pipe';
import { CronPipe } from '../../pipes/cron.pipe';
import { LocalTimePipe } from '../../pipes/local-time.pipe';
import { SemaphoreCssStyleByNamePipe } from '../../pipes/semaphore-css-style-by-name.pipe';
import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from "../../pipes/severity-name-by-id.pipe";
import { SeverityNamesMaxDisplayPipe } from "../../pipes/severity-names-max-display.pipe";
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';

import { ReactiveMap } from '../../abstracts/reactive-map'

@Component({
  selector: 'app-trivy-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CheckboxModule,
    InputTextModule,
    MultiSelectModule,
    PopoverModule,
    Select,
    SplitButtonModule,
    TableModule,
    TagModule,
    VulnerabilityCountPipe,
    BooleanCssStylePipe,
    CapitalizeFirstPipe,
    SeverityCssStyleByIdPipe,
    SemaphoreCssStyleByNamePipe,
    CronPipe,
    LocalTimePipe,
    SeverityNameByIdPipe,
    SeverityNamesMaxDisplayPipe,
  ],
  templateUrl: './trivy-table.component.html',
  styleUrl: './trivy-table.component.scss',
  encapsulation: ViewEncapsulation.None,
})
export class TrivyTableComponent<TData> implements OnInit {
  // data
  dataDtos = input<TData[] | null | undefined>([]);
  activeNamespaces = input<string[] | undefined>();
  // layout
  csvStorageKey = input<string>('default');
  csvFileName = input<string>('Default.csv.FileName');
  dataKey = input<string | undefined>(undefined);
  extraClasses = input<string | undefined>(undefined);
  isClearSelectionVisible = input<boolean | undefined>(false);
  isCollapseAllVisible = input<boolean | undefined>(false);
  isResetFiltersVisible = input<boolean | undefined>(false);
  isExportCsvVisible = input<boolean | undefined>(false);
  isRefreshVisible = input<boolean | undefined>(false);
  isRefreshFilterable = input<boolean | undefined>(false);
  isFooterVisible = input<boolean | undefined>(false);
  multiHeaderActions = input<MultiHeaderAction[]>([]);
  rowExpandData = input<TrivyTableExpandRowData<TData>>();
  rowExpansionRender = input<'messages' | 'table' | undefined>(undefined);
  selectionMode = input<'single' | 'multiple' | undefined>(undefined);
  style = input<{ [klass: string]: any } | undefined>({});
  stateKey = input<string | undefined>(undefined);

  trivyTableColumns = input.required<TrivyTableColumn[]>();
  //
  isLoading = input<boolean>(false);

  multiHeaderActionRequested = output<string>();
  refreshRequested = output<TrivyFilterData>();
  rowExpandActionCallback = output<TData>();
  rowExpandDataChange = output<TData>();
  rowActionRequested = output<TData>();
  selectedRowsChanged = output<TData[]>();

  @ViewChild('trivyTable') trivyTable!: Table;
  @ViewChild('serverFilterDataOp') serverFilterDataOp?: Popover;
  @ViewChild('csvExportOp') csvExportOp?: Popover;
  @ViewChild('refreshSplitButton') refreshSplitButton?: SplitButton;
  @ViewChild('filterNamespacesSelect') filterNamespacesSelect?: Select;

  // rows expand
  expandedRows = {};
  anyRowExpanded: boolean = false;
  // table custom filters
  filterSeverityOptions: number[] = [];
  filterSelectedSeverityIds: number[] = [];
  filterSelectedActiveNamespaces: string[] = [];
  filterRefreshActiveNamespace: string = '';
  filterRefreshSeverities?: SeverityDto[];
  severityDtos: SeverityDto[] = [...SeverityUtils.severityDtos];

  multiHeaderActionItems: (MenuItem & { initialData: MultiHeaderAction })[] = [];

  selectedDataDtos?: any;
  isTableRowsSelected = false;
  singleSelectDataDto = input<TData | undefined>();

  tableStateKey?: string;

  // custom back overlay
  overlayVisible: boolean = false;

  protected _dataDtos: TData[] = [];
  protected _csvFileName = this.csvFileName();
  protected _rowExpandMap = new ReactiveMap<TData, TrivyTableExpandRowData<TData>>();

  get trivyTableTotalRecords(): number {
    return this.dataDtos()?.length ?? 0;
  }
  get trivyTableSelectedRecords(): number {
    if (this.selectionMode() === 'single') {
      return this.selectedDataDtos ? 1 : 0;
    } else {
      return this.selectedDataDtos ? this.selectedDataDtos.length : 0;
    }
  }
  get trivyTableFilteredRecords(): number {
    return this.trivyTable?.filteredValue ? this.trivyTable.filteredValue.length : this.trivyTableTotalRecords;
  }

  constructor() {
    effect(() => {
      this._dataDtos = this.dataDtos() ?? [];
      this.updateMultiHeaderActionOnDataChanged();
      this.newData();
      const currentSingleSelectDataDto = this.singleSelectDataDto();
      if (currentSingleSelectDataDto) {
        this.scrollToDto(currentSingleSelectDataDto);
      }
    });
    effect(() => {
      const rowExpandDataResponse = this.rowExpandData();
      if (rowExpandDataResponse) {
        this._rowExpandMap.set(rowExpandDataResponse.rowKey, rowExpandDataResponse);
      }
    });
    effect(() => {
      this._csvFileName = this.csvFileName();
    });
    effect(() => {
      const value = this.singleSelectDataDto()
      if (this.selectedDataDtos === value) {
        return;  // avoid (re)selection
      }
      this.selectedDataDtos = value;
      this.updateMultiHeaderActionSelectionChanged();
      if (value) {
        this.scrollToDto(value);
        this.selectedRowsChanged.emit([value]);
      }
    })
  }

  ngOnInit() {
    const savedCsvFileName = localStorage.getItem(LocalStorageUtils.csvFileNameKeyPrefix + this.csvStorageKey());
    if (savedCsvFileName) {
      this._csvFileName = savedCsvFileName;
    }
    this.tableStateKey = LocalStorageUtils.trivyTableKeyPrefix + this.stateKey();
    this.filterSeverityOptions = this.severityDtos.map((x) => x.id);
    this.filterRefreshSeverities = [...this.severityDtos];

    this.multiHeaderActionInit();
  }

  public onTableClearSelected() {
    this.selectedDataDtos = undefined;
    this.isTableRowsSelected = false;
    this.updateMultiHeaderActionSelectionChanged();
  }



  onSelectionChange(event: any): void {
    this.isTableRowsSelected = this.selectedDataDtos ? this.selectedDataDtos.length > 0 : false;
    this.updateMultiHeaderActionSelectionChanged();
    if (!event) {
      this.selectedRowsChanged.emit([]);
      return;
    }
    if (this.selectionMode() === 'single') {
      this.selectedRowsChanged.emit([event]);
    } else {
      this.selectedRowsChanged.emit(event);
    }
  }

  onFilterRefresh(_event: MouseEvent) {
    this.onFilterData();
  }

  onFilterDropdownClick(_event: Event) {
    if (this.refreshSplitButton?.menu) {
      setTimeout(() => {
        this.refreshSplitButton?.menu?.hide();
      }, 0);
    }
    this.serverFilterDataOp?.toggle(_event);
  }

  onFilterData() {
    const event: TrivyFilterData = {
      namespaceName: this.filterRefreshActiveNamespace,
      selectedSeverityIds: this.filterRefreshSeverities?.map((x) => x.id) ?? [],
    };
    this.serverFilterDataOp?.hide();
    this.refreshRequested.emit(event);
  }

  onFilterReset() {
    this.filterRefreshSeverities = [...this.severityDtos];
    if (this.filterNamespacesSelect) {
      this.filterNamespacesSelect.clear();
    }
  }

  public onOverlayToggle() {
    this.overlayVisible = !this.overlayVisible;
  }

  onTrivyDetailsTableCallback(dto: TData) {
    this.rowExpandActionCallback.emit(dto);
  }

  onTableCollapseAll() {
    this.expandedRows = {};
    this.anyRowExpanded = false;
    this.updateMultiHeaderActionCollapsed();
    const stateKey = this.stateKey();
    if (stateKey) {
      const tableState = localStorage.getItem(stateKey);
      if (!tableState) {
        return;
      }
      const tableStateJson = JSON.parse(tableState);
      if (tableStateJson.hasOwnProperty('expandedRowKeys')) {
        delete tableStateJson.expandedRowKeys;
      }
      localStorage.setItem(stateKey, JSON.stringify(tableStateJson));
    }
  }

  onRowExpandCollapse(_event: any) {
    this.anyRowExpanded = JSON.stringify(this.expandedRows) != '{}';
    this.updateMultiHeaderActionCollapsed();
  }

  onExportToCsv(exportType: string) {
    localStorage.setItem(LocalStorageUtils.csvFileNameKeyPrefix + this.csvStorageKey(), this._csvFileName);
    switch (exportType) {
      case 'all':
        this.trivyTable.exportCSV({ allValues: true });
        break;
      case 'filtered':
        this.trivyTable.exportCSV();
        break;
    }
    if (this.csvExportOp) {
      this.csvExportOp.hide();
    }
  }

  public onTableStateSave() {
    if (!this.selectionMode()) {
      return;
    }
    if (!this.tableStateKey) {
      return;
    }
    const tableStateJson = localStorage.getItem(this.tableStateKey);
    if (!tableStateJson) {
      return;
    }
    const tableState = JSON.parse(tableStateJson);
    PrimengTableStateUtil.clearTableSelection(tableState);
    PrimengTableStateUtil.clearTableExpandedRows(tableState);
    localStorage.setItem(this.tableStateKey, JSON.stringify(tableState));
  }

  private multiHeaderActionInit() {
    const multiHeaderActions = this.multiHeaderActions()
    if (multiHeaderActions && (multiHeaderActions.length ?? 0) > 1) {
      this.multiHeaderActionItems = multiHeaderActions.slice(1).map(actionItem => ({
        label: actionItem.specialAction ?? actionItem.label,
        command: this.multiHeaderActionGetCommand(actionItem),
        icon: this.multiHeaderActionGetIcon(actionItem),
        disabled: this.isMultiHeaderActionDisabled(actionItem),
        initialData: actionItem,
      }));
    }
  }

  protected multiHeaderActionGetCommand(actionItem: MultiHeaderAction): () => void {
    if (actionItem.specialAction) {
      switch (actionItem.specialAction) {
        case "Go to Detailed ⧉":
          return () => this.multiHeaderActionRequested.emit("goToDetailedPage");
        case "Clear Selection":
          return () => this.onTableClearSelected();
        case "Clear Sort/Filters":
          return () => this.onClearSortFilters();
        case "Collapse All":
          return () => this.onTableCollapseAll();
        default:
          console.error(actionItem);
      }
    }
    return () => this.multiHeaderActionRequested.emit(actionItem.label);
  }

  private multiHeaderActionGetIcon(actionItem: MultiHeaderAction): string {
    if (actionItem.specialAction) {
      switch (actionItem.specialAction) {
        case "Go to Detailed \u29C9" :
          return 'pi pi-align-justify';
        case "Clear Selection":
          return 'pi pi-list';
        case "Clear Sort/Filters":
          return 'pi pi-filter';
        case "Collapse All":
          return 'pi pi-expand';
        default:
          console.error(actionItem);
      }
    }
    return actionItem.icon ?? '';
  }

  isMultiHeaderActionDisabled(actionItem: MultiHeaderAction): boolean {
    if (actionItem.enabledIfDataLoaded)
    {
      return this._dataDtos.length === 0;
    }
    if (actionItem.enabledIfRowSelected || actionItem.specialAction == 'Clear Selection')
    {
      return this.trivyTableSelectedRecords === 0;
    }
    if (actionItem.specialAction == 'Clear Sort/Filters')
    {
      return !this.isTableFilteredOrSorted();
    }
    if (actionItem.specialAction == 'Collapse All') {
      return !this.anyRowExpanded;
    }
    return false;
  }

  private updateMultiHeaderActionOnDataChanged() {
    this.multiHeaderActionItems
      .filter(actionItem => actionItem.initialData.enabledIfDataLoaded)
      .forEach(actionItem => {
        actionItem.disabled = (this._dataDtos.length ?? 0) === 0;
      });
  }

  private updateMultiHeaderActionClearSortFilters() {
    const menuItem = this.multiHeaderActionItems
      .find(x => x.initialData.specialAction == "Clear Sort/Filters");
    if (menuItem) {
      menuItem.disabled = !this.isTableFilteredOrSorted();
    }
  }

  private updateMultiHeaderActionSelectionChanged() {
    this.multiHeaderActionItems
      .filter(actionItem => actionItem.initialData.enabledIfRowSelected)
      .forEach(actionItem => {
        actionItem.disabled = this.trivyTableSelectedRecords === 0;
      });
    const menuItem = this.multiHeaderActionItems
      .find(x => x.initialData.specialAction == "Clear Selection");
    if (menuItem) {
      menuItem.disabled = this.trivyTableSelectedRecords === 0;
    }
  }

  private updateMultiHeaderActionCollapsed() {
    const menuItem = this.multiHeaderActionItems
      .find(x => x.initialData.specialAction == "Collapse All");
    if (menuItem) {
      menuItem.disabled = !this.anyRowExpanded;
    }
  }

  onSort() {
    this.updateMultiHeaderActionClearSortFilters();
  }

  onFilter() {
    this.updateMultiHeaderActionClearSortFilters();
  }

  onRowAction(event: TData) {
    this.rowActionRequested.emit(event);
  }

  onRowExpand(event: TableRowExpandEvent) {
    if (!this._rowExpandMap.hasKey(event.data)) {
      this.rowExpandDataChange.emit(event.data);
    }
    this.onRowExpandCollapse(event);
  }

  onRowCollapse(event: TableRowCollapseEvent) {
    this.onRowExpandCollapse(event);
  }

  public isTableFilteredOrSorted(): boolean {
    if (!this.trivyTable || this.isLoading()) {
      return false;
    }
    return (
        (!!this.trivyTable.filteredValue) ||
        (this.trivyTable.multiSortMeta == null ? false : this.trivyTable.multiSortMeta.length > 0)
    );
  }

  public onClearSortFilters() {
    PrimengTableStateUtil.clearFilters(this.trivyTable.filters);
    this.trivyTable.clear();
    this.filterSelectedActiveNamespaces = [];
    this.filterSelectedSeverityIds = [];
    this.updateMultiHeaderActionClearSortFilters();
    const stateKey = this.stateKey();
    if (stateKey) {
      const tableState = localStorage.getItem(stateKey);
      if (!tableState) {
        return;
      }
      const tableStateJson = JSON.parse(tableState);
      PrimengTableStateUtil.clearTableFilters(tableStateJson);
      PrimengTableStateUtil.clearTableMultiSort(tableStateJson);
      localStorage.setItem(stateKey, JSON.stringify(tableStateJson));
    }
    this.updateMultiHeaderActionClearSortFilters();
  }

  scrollToDto(value: TData) {
    setTimeout(() => {
      const index = this._dataDtos?.indexOf(value);
      if (index && this.trivyTable) {
        this.trivyTable.scrollToVirtualIndex(index);
      }
    }, 0)

  }

  // force resize event - bug as table is not properly sized and on row expand it looks not ok
  newData() {
      setTimeout(() => {
        window.dispatchEvent(new Event('resize'));
      }, 0);
  }

  // temp fix for https://github.com/primefaces/primeng/issues/16576
  // filter icon is not highlighted
  hasActiveFilter (columnKey: string): string {
    if (!this.trivyTable) {
      return '';
    }
    if (Object.hasOwn(this.trivyTable.filters, columnKey)) {
      let filter = this.trivyTable.filters[columnKey]
      if (!Array.isArray(filter)) {
        filter = [filter]
      }
      if (filter[0].value && filter[0].value !== 0) {
        return 'tod-active-filter'
      }
    }
    return '';
  }



}

// clear filters on reset table: https://stackoverflow.com/questions/51395624/reset-filter-value-on-primeng-table
