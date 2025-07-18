import {
  Component,
  effect,
  HostListener,
  input,
  output,
  ViewChild,
} from '@angular/core';

import { SeverityDto } from '../../../api/models/severity-dto';
import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import {
  MultiHeaderAction,
  TrivyFilterData,
  TrivyTableColumn,
  TrivyTableExpandRowData,
} from '../trivy-table/trivy-table.types';
import { TrivyReport, TrivyReportDetail } from '../../trivy-reports/abstracts/trivy-report';

import { SplitterModule } from 'primeng/splitter';


@Component({
  selector: 'app-generic-master-detail',
  standalone: true,
  imports: [TrivyTableComponent, SplitterModule],
  templateUrl: './generic-master-detail.component.html',
  styleUrl: './generic-master-detail.component.scss',
})
export class GenericMasterDetailComponent<TTrivyReport extends TrivyReport<TTrivyReportDetail>, TTrivyReportDetail extends TrivyReportDetail> {
  severityDtos= input<SeverityDto[]>([]);
  activeNamespaces = input<string[] | undefined>([]);
  mainTableColumns = input.required<TrivyTableColumn[]>();
  mainTableRowExpandResponse = input<TrivyTableExpandRowData<TTrivyReport>>();
  isMainTableLoading = input<boolean>(true);
  detailsTableColumns = input.required<TrivyTableColumn[]>();
  singleSelectDataDto = input<TTrivyReport | undefined>();
  splitterStorageKey = input<string | undefined>();

  refreshRequested = output<TrivyFilterData>();

  mainTableRowExpandChange = output<TTrivyReport>();
  mainTableExpandCallback = output<TTrivyReport>();
  mainTableMultiHeaderActionRequested = output<string>();
  mainTableSelectedRowChanged = output<TTrivyReport | null>();

  mainTableIsClearSelectionVisible = input<boolean | undefined>(false);
  mainTableIsCollapseAllVisible = input<boolean | undefined>(false);
  mainTableIsResetFiltersVisible = input<boolean | undefined>(false);
  mainTableIsExportCsvVisible = input<boolean | undefined>(false);
  mainTableIsRefreshVisible = input<boolean | undefined>(false);
  mainTableIsRefreshFilterable = input<boolean | undefined>(false);
  mainTableIsFooterVisible = input<boolean | undefined>(false);
  mainTableSelectionMode = input<'single' | 'multiple' | undefined>(undefined);
  mainTableStyle = input<{ [klass: string]: any } | undefined>({});
  mainTableStateKey = input<string | undefined>(undefined);
  mainTableDataKey = input<string | undefined>(undefined);
  mainTableRowExpansionRender = input<'messages' | 'table' | undefined>(undefined);
  mainTableExtraClasses = input<string | undefined>(undefined);
  mainTableMultiHeaderActions = input<MultiHeaderAction[]>([]);


  detailsIsClearSelectionVisible = input<boolean | undefined>(false);
  detailsIsCollapseAllVisible = input<boolean | undefined>(false);
  detailsIsResetFiltersVisible = input<boolean | undefined>(false);
  detailsIsExportCsvVisible = input<boolean | undefined>(false);
  detailsIsRefreshVisible = input<boolean | undefined>(false);
  detailsIsRefreshFilterable = input<boolean | undefined>(false);
  detailsIsFooterVisible = input<boolean | undefined>(false);
  detailsSelectionMode = input<'single' | 'multiple' | undefined>(undefined);
  detailsStyle = input<{ [klass: string]: any } | undefined>({});
  detailsStateKey = input<string | undefined>(undefined);
  detailsDataKey = input<string | undefined>(undefined);
  detailsRowExpansionRender = input<'messages' | 'table' | undefined>(undefined);
  detailsExtraClasses = input<string | undefined>(undefined);
  detailsMultiHeaderActions = input<MultiHeaderAction[]>([]);


  detailsTableMultiHeaderActionRequested = output<string>();


  @ViewChild('mainTable', { static: true }) mainTable?: TrivyTableComponent<TTrivyReport>;

  dataDtos = input<TTrivyReport[]>([]);
  selectedDataDto: TTrivyReport | null = null;

  screenSize: string = this.getScreenSize();

  protected _dataDtos: TTrivyReport[] = [];
  protected _isMainTableLoading: boolean = this.isMainTableLoading();

  constructor() {
    effect(() => {
      this._isMainTableLoading = this.isMainTableLoading();
    })
    effect(() => {
      this._dataDtos = this.dataDtos();
      this.onGetTDataDtos();
    });

  }

  onGetTDataDtos() {
    if (this.mainTable) {
      this.mainTable.onTableClearSelected();
    }
    this.selectedDataDto = null;
    this._isMainTableLoading = false;
  }

  onMainTableSelectionChange(event: TTrivyReport[]) {
    if (event == null || event.length == 0) {
      this.selectedDataDto = null;
      this.mainTableSelectedRowChanged.emit(null);
      return;
    } else {
      this.selectedDataDto = event[0];
      this.mainTableSelectedRowChanged.emit(event[0]);
    }
  }

  onRefreshRequested(event: TrivyFilterData) {
    this.refreshRequested.emit(event);
  }

  onMainTableExpandCallback(event: TTrivyReport) {
    this.mainTableExpandCallback.emit(event);
  }

  onMainTableMultiHeaderActionRequested(event: string) {
    this.mainTableMultiHeaderActionRequested.emit(event);
  }

  onDetailsTableMultiHeaderActionRequested(event: string) {
    this.detailsTableMultiHeaderActionRequested.emit(event);
  }

  onMainTableRowExpandChange(event: TTrivyReport) {
    this.mainTableRowExpandChange.emit(event);
  }

  // screen size
  @HostListener('window:resize', [])
  onResize() {
    this.screenSize = this.getScreenSize();
  }

  getScreenSize(): string {
    const cssVarValue = getComputedStyle(document.documentElement)
      .getPropertyValue('--tod-screen-width-sm')
      .trim(); // Get and clean the CSS variable value

    const threshold = parseInt(cssVarValue, 10); // Convert it to a number

    return window.innerWidth < threshold ? 'sm' : 'lg';
  }
}
