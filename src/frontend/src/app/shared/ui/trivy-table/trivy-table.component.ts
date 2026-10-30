import { CommonModule } from '@angular/common';
import { injectLogger } from '@core/logging/inject-logger';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect, inject,
  input,
  model,
  OnInit,
  output,
  signal,
  ViewChild,
  ViewEncapsulation,
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { FilterService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { Popover, PopoverModule } from 'primeng/popover';
import { SplitButtonModule } from 'primeng/splitbutton';
import { Table, TableModule, TableRowCollapseEvent, TableRowExpandEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';

import { CveInfoDto } from '@api/models/cve-info-dto';
import { SeverityDto } from '@api/models/severity-dto';
import { LocalStorageUtils } from '../../utils/local-storage.utils';
import { PrimengTableStateUtil } from '../../utils/primeng-table-state.util';
import { SeverityUtils } from '../../utils/severity.utils';
import { TagOptionPipe } from './tag-option.pipe';
import { TagOption, TagSetName, TrivyTableTagSets } from './trivy-table-tag-sets';
import { ExploitedLabelPipe, VulnerabilityTagsPipe } from './vulnerability-tags.pipe';
import {
  MultiHeaderAction,
  SelectedDtosEvent,
  TrivyTableColumn,
  TrivyTableExpandRowData,
} from './trivy-table.types';

import { BooleanCssStylePipe } from '../../pipes/boolean-css-style.pipe';
import { CapitalizeFirstPipe } from '../../pipes/capitalize-first.pipe';
import { CounterIconPipe } from '../../pipes/counter-icon.pipe';
import { CronPipe } from '../../pipes/cron.pipe';
import { FriendlyTimePipe } from '../../pipes/local-time.pipe';
import { SemaphoreCssStyleByNamePipe } from '../../pipes/semaphore-css-style-by-name.pipe';
import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '../../pipes/severity-name-by-id.pipe';
import { SeverityNamesMaxDisplayPipe } from '../../pipes/severity-names-max-display.pipe';
import { UnPascalCasePipe } from '../../pipes/un-pascal-case.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';

import { ReactiveMap } from '../../utils/reactive-map';
import { MiniBarChartComponent } from '../mini-bar-chart/mini-bar-chart.component';
import {SeverityDifCssStyleByIdPipe} from "../../pipes/severity-dif-css-style-by-id.pipe";


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
    SplitButtonModule,
    TableModule,
    TagModule,
    TooltipModule,
    VulnerabilityCountPipe,
    BooleanCssStylePipe,
    CapitalizeFirstPipe,
    SeverityCssStyleByIdPipe,
    SemaphoreCssStyleByNamePipe,
    CounterIconPipe,
    CronPipe,
    FriendlyTimePipe,
    SeverityNameByIdPipe,
    SeverityNamesMaxDisplayPipe,
    UnPascalCasePipe,
    MiniBarChartComponent,
    SeverityDifCssStyleByIdPipe,
    TagOptionPipe,
    ExploitedLabelPipe,
    VulnerabilityTagsPipe,
  ],
  templateUrl: './trivy-table.component.html',
  styleUrl: './trivy-table.component.scss',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrivyTableComponent<TData> implements OnInit {
  private readonly log = injectLogger('Trivy table');
  // data
  dataDtos = model<TData[]>([]);
  activeNamespaces = input<string[]>([]);

  // browser storage keys
  stateKey = input<string | undefined>(undefined);
  csvStorageKey = input<string>('default');
  csvFileName = input<string>('Default.csv.FileName');

  // layout
  extraClasses = input<string | undefined>(undefined);
  style = input<{ [klass: string]: any } | undefined>({});
  rowHeight = input<number>(39);
  isFlex = input<boolean>(true);
  isLoading = input<boolean>(false);

  // columns
  trivyTableColumns = input.required<TrivyTableColumn[]>();

  // buttons & footer visibility
  isClearSelectionVisible = input<boolean | undefined>(false);
  isCollapseAllVisible = input<boolean | undefined>(false);
  isResetFiltersVisible = input<boolean | undefined>(false);
  isExportCsvVisible = input<boolean | undefined>(false);
  isRefreshVisible = input<boolean | undefined>(false);
  isFooterVisible = input<boolean | undefined>(false);

  // settings for multi actions button
  multiHeaderActions = input<MultiHeaderAction[]>([]);

  // row expand
  dataKey = input<string | undefined>(undefined);
  rowExpandData = input<TrivyTableExpandRowData<TData>>();
  rowExpansionRender = input<'messages' | 'table' | undefined>(undefined);

  // row dimmer
  rowDimmer = input<((row: TData) => boolean) | undefined>();
  readonly defaultRowDimmer = () => false;

  selectionMode = input<'single' | 'multiple' | undefined>(undefined);

  // 'vulnerabilityId' render type - the NVD tag is shown only when enabled; KEV/EPSS/EUVD tags come from the map
  isNvdEnabled = input<boolean>(true);
  cveInfoById = input<ReadonlyMap<string, CveInfoDto> | undefined>(undefined);

  refreshValue = input<number | undefined>(undefined);
  private previousRefreshValue?: number;

  // output signals
  multiHeaderActionRequested = output<string>();
  refreshRequested = output<void>();
  rowExpandActionCallback = output<TData>();
  rowExpandDataChange = output<TData>();
  // action - set when a column has more than one action (e.g. the VulnerabilityTagSource of a 'vulnerabilityId' tag)
  rowActionRequested = output<{ row: TData; col: string; action?: string }>();

  // models
  selectedData = model<SelectedDtosEvent<TData>>({selectedDtos: [], source: 'user'});

  // view child
  @ViewChild('trivyTable') trivyTable!: Table;
  @ViewChild('csvExportOp') csvExportOp?: Popover;

  // rows expand
  protected expandedRows = signal<Record<string, boolean>>({});
  protected anyRowExpanded = computed(() => {
    return JSON.stringify(this.expandedRows()) !== '{}';
  });

  // table custom filters
  protected filterSeverityOptions: number[] = [];
  protected severityDtos: SeverityDto[] = [...SeverityUtils.severityDtos.filter((x) => x.isTrivySeverity)];

  protected multiHeaderActionItems = computed(() => {
    const actions = this.multiHeaderActions();
    if (!actions || actions.length <= 1) return [];

    const dataLoaded = (this.dataDtos().length ?? 0) > 0;
    const rowSelected = this.isTableRowsSelected();
    const filteredOrSorted = this.isTableFilteredSorted();
    const anyExpanded = this.anyRowExpanded();

    return actions.slice(1).map(actionItem => {
      const disabled =
        (actionItem.enabledIfDataLoaded && !dataLoaded) ||
        (actionItem.enabledIfRowSelected && !rowSelected) ||
        (actionItem.specialAction === 'Clear Selection' && !rowSelected) ||
        (actionItem.specialAction === 'Clear Sort/Filters' && !filteredOrSorted) ||
        (actionItem.specialAction === 'Collapse All' && !anyExpanded);

      return {
        label: actionItem.specialAction ?? actionItem.label,
        command: this.multiHeaderActionGetCommand(actionItem),
        icon: this.multiHeaderActionGetIcon(actionItem),
        disabled,
        initialData: actionItem,
      };
    });
  });

  protected isTableFilteredSorted = signal<boolean>(this.checkIfTableIsFilteredOrSorted());
  protected isTableRowsSelected = computed(() => this.selectedData().selectedDtos.length > 0);

  // custom back overlay
  protected overlayVisible = signal(false);

  // table state key for browser local storage
  protected tableStateKey?: string;

  protected internalCsvFileName = signal(this.csvFileName());
  protected flexStyles = {'display': 'flex', 'flex-direction': 'column', 'flex-grow': '1' };
  protected _rowExpandMap = new ReactiveMap<TData, TrivyTableExpandRowData<TData>>();

  protected trivyTableTotalRecords = computed(() => this.dataDtos().length);
  protected trivyTableFilteredRecords = signal<number>(this.trivyTableTotalRecords())

  protected rowHeightPx = computed(() => { return `${this.rowHeight()}px` });
  // distinct values per 'arrayValues' column (comma-joined cells), for its multiselect filter
  protected arrayValueOptions = computed(() => {
    const options = new Map<string, string[]>();

    for (const col of this.trivyTableColumns().filter((c) => c.multiSelectType === 'arrayValues')) {
      const values = this.dataDtos().flatMap((row) => String((row as Record<string, unknown>)[col.field] ?? '').split(','));

      options.set(col.field, [...new Set(values.map((v) => v.trim()).filter(Boolean))].sort());
    }

    return options;
  });
  protected fullRowHeight =computed(() => { return this.rowHeight() + 8 });
  protected fullRowHeightPx = computed(() => { return `${this.rowHeight() + 8}px` });

  private readonly filterService = inject(FilterService);
  private readonly tagOptionsByColumn = new WeakMap<TrivyTableColumn, TagOption[]>();

  constructor() {
    // new dataDtos()
    effect(() => {
      const value = this.dataDtos();

      this.newData();
    });
    // new rowExpandData (callback)
    effect(() => {
      const rowExpandDataResponse = this.rowExpandData();
      if (rowExpandDataResponse) {
        this._rowExpandMap.set(rowExpandDataResponse.rowKey, rowExpandDataResponse);
      }
    });
    // csvFileName
    effect(() => {
      this.initCsvFileName();
    });
    // selectedDataDtos
    effect(() => {
      const value = this.selectedData().selectedDtos;

      this.scrollToDto();
    });
    effect(() => {
      const value = this.refreshValue();
      // avoid changing of dataDtos()
      if (value === this.previousRefreshValue) {
        return;
      }
      if (this.isTableSorted()) {
        this.dataDtos.set([...this.dataDtos()]);
      }
      this.previousRefreshValue = value;
    });
    // filter service
    this.filterService.register('namespaceArrayIn', (value: string | null | undefined, filter: string[]) => {
      if (!filter?.length) {
        return true;
      }

      const namespaces = value?.split(',').map((ns) => ns.trim()) ?? [];

      return filter.some((selected) => namespaces.includes(selected));
    });
  }

  ngOnInit() {
    this.tableStateKey = LocalStorageUtils.trivyTableKeyPrefix + this.stateKey();
    this.filterSeverityOptions = this.severityDtos.map((x) => x.id);
  }

  // tag sets: 'booleans' multiSelectType stays supported as an alias of the boolean tag set
  protected tagOptions(col: TrivyTableColumn): TagOption[] {
    const tagSet: TagSetName | undefined = col.tagSet ?? (col.multiSelectType === 'booleans' ? 'boolean' : undefined);
    if (!tagSet) {
      return [];
    }
    const cached = this.tagOptionsByColumn.get(col);
    if (cached) {
      return cached;
    }
    const options = TrivyTableTagSets.getOptions(tagSet);
    // boolean columns can carry their own true|false labels in extraFields[1]
    const labels = tagSet === 'boolean' ? col.extraFields?.[1]?.split('|') : undefined;
    const result =
      labels?.length === 2 ? options.map((x) => ({ ...x, label: x.value ? labels[0] : labels[1] })) : options;
    // stable reference per column, the multiselect would otherwise reset on every change detection
    this.tagOptionsByColumn.set(col, result);
    return result;
  }

  // table main actions
  onTableClearSelected() {
    this.trivyTable.selection = [];
    this.trivyTable.selectionKeys = {};
    this.selectedData.set({selectedDtos: [], source: 'user'});
  }

  protected onTableCollapseAll() {
    this.expandedRows.set({});
    // this.updateMultiHeaderActionCollapsed();
    const stateKey = this.stateKey();
    if (stateKey) {
      const tableState = LocalStorageUtils.getItem(stateKey);
      if (!tableState) {
        return;
      }
      const tableStateJson = LocalStorageUtils.parseObject(tableState);
      if (!tableStateJson) {
        return;
      }
      if (tableStateJson.hasOwnProperty('expandedRowKeys')) {
        delete tableStateJson.expandedRowKeys;
      }
      LocalStorageUtils.setItem(stateKey, JSON.stringify(tableStateJson));
    }
  }

  protected onRefreshData() {
    this.onTableCollapseAll();
    this.refreshRequested.emit();
  }

  protected onSelectionChange(event: any): void {
    let value: TData[] = [];
    if (event) {
      value = this.selectionMode() === 'single' ? [event] : event;
    }
    this.selectedData.set({selectedDtos: value, source: 'user'});
  }

  scrollToDto() {
    if (this.selectedData().source === 'programmatic' && this.selectedData().selectedDtos.length > 0) {
      const value = this.selectedData().selectedDtos[0];

      setTimeout(() => {
        const index = this.dataDtos()?.indexOf(value);
        if (index !== -1 && this.trivyTable) {
          this.trivyTable.scrollToVirtualIndex(index);
        }
      }, 100);
    }
  }

  protected onOverlayToggle() {
    this.overlayVisible.update((value) => !value);
  }

  protected onTrivyDetailsTableCallback(dto: TData) {
    this.rowExpandActionCallback.emit(dto);
  }

  // csv export
  protected onExportToCsv(exportType: string) {
    LocalStorageUtils.setItem(LocalStorageUtils.csvFileNameKeyPrefix + this.csvStorageKey(), this.internalCsvFileName());
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
  private initCsvFileName() {
    const savedCsvFileName =
      LocalStorageUtils.getItem(LocalStorageUtils.csvFileNameKeyPrefix + this.csvStorageKey()) || this.csvFileName();
    this.internalCsvFileName.set(savedCsvFileName);
  }

  protected onTableStateSave() {
    if (!this.selectionMode()) {
      return;
    }
    if (!this.tableStateKey) {
      return;
    }
    const tableStateJson = LocalStorageUtils.getItem(this.tableStateKey);
    if (!tableStateJson) {
      return;
    }
    const tableState = LocalStorageUtils.parseObject(tableStateJson);
    if (!tableState) {
      return;
    }
    PrimengTableStateUtil.clearTableSelection(tableState);
    PrimengTableStateUtil.clearTableExpandedRows(tableState);
    LocalStorageUtils.setItem(this.tableStateKey, JSON.stringify(tableState));
  }

  // multiHeader actions - helpers
  protected multiHeaderActionGetCommand(actionItem: MultiHeaderAction): () => void {
    if (actionItem.specialAction) {
      switch (actionItem.specialAction) {
        case 'Go to Detailed ⧉':
          return () => this.multiHeaderActionRequested.emit('goToDetailedPage');
        case 'Clear Selection':
          return () => this.onTableClearSelected();
        case 'Clear Sort/Filters':
          return () => this.onClearSortFilters();
        case 'Collapse All':
          return () => this.onTableCollapseAll();
        default:
          this.log.warn('Unknown special action.', { data: actionItem, toast: false });
      }
    }
    return () => this.multiHeaderActionRequested.emit(actionItem.label);
  }

  private multiHeaderActionGetIcon(actionItem: MultiHeaderAction): string {
    if (actionItem.specialAction) {
      switch (actionItem.specialAction) {
        case 'Go to Detailed \u29C9':
          return 'pi pi-align-justify';
        case 'Clear Selection':
          return 'pi pi-list';
        case 'Clear Sort/Filters':
          return 'pi pi-filter';
        case 'Collapse All':
          return 'pi pi-expand';
        default:
          this.log.warn('Unknown special action.', { data: actionItem, toast: false });
      }
    }
    return actionItem.icon ?? '';
  }

  // various table event handlers
  protected onRowAction(rowDto: TData, columnName: string, action?: string, event?: Event) {
    // a tag click inside a selectable row must not (de)select the row as well
    event?.stopPropagation();
    this.rowActionRequested.emit({ row: rowDto, col: columnName, action });
  }

  protected onRowExpand(event: TableRowExpandEvent) {
    if (!this._rowExpandMap.hasKey(event.data)) {
      this.rowExpandDataChange.emit(event.data);
    }
    this.expandedRows.set({ ...this.expandedRows() });
  }

  protected onRowCollapse(_event: TableRowCollapseEvent) {
    this.expandedRows.set({ ...this.expandedRows() });
  }

  protected onClearSortFilters() {
    PrimengTableStateUtil.clearFilters(this.trivyTable.filters);
    this.trivyTable.clear();
    this.isTableFilteredSorted.set(this.checkIfTableIsFilteredOrSorted());
    // this.updateMultiHeaderActionClearSortFilters();
    const stateKey = this.stateKey();
    if (stateKey) {
      const tableState = LocalStorageUtils.getItem(stateKey);
      if (!tableState) {
        return;
      }
      const tableStateJson = LocalStorageUtils.parseObject(tableState);
      if (!tableStateJson) {
        return;
      }
      PrimengTableStateUtil.clearTableFilters(tableStateJson);
      PrimengTableStateUtil.clearTableMultiSort(tableStateJson);
      LocalStorageUtils.setItem(stateKey, JSON.stringify(tableStateJson));
    }
  }

  protected onSort() {
    this.isTableFilteredSorted.set(this.checkIfTableIsFilteredOrSorted());
  }

  protected onFilter() {
    const table = this.trivyTable;
    this.trivyTableFilteredRecords.set(table?.filteredValue?.length ?? this.trivyTableTotalRecords());
    this.isTableFilteredSorted.set(this.checkIfTableIsFilteredOrSorted());
    this.newData();
  }

  // is sorted or filtered helper
  protected checkIfTableIsFilteredOrSorted(): boolean {
    if (!this.trivyTable || this.isLoading()) {
      return false;
    }
    return (
      !!this.trivyTable.filteredValue ||
      (this.trivyTable.multiSortMeta == null ? false : this.trivyTable.multiSortMeta.length > 0)
    );
  }

  protected isTableSorted(): boolean {
    if (this.trivyTable){
      return this.trivyTable.multiSortMeta == null ? false : this.trivyTable.multiSortMeta.length > 0;
    }
    return false;
  }

  // force resize event - bug as table is not properly sized and, on row expand, it doesn't look ok
  newData() {
    setTimeout(() => {
      window.dispatchEvent(new Event('resize'));
    }, 0);
  }

  protected readonly isNaN = isNaN;
}

// clear filters on reset table: https://stackoverflow.com/questions/51395624/reset-filter-value-on-primeng-table
