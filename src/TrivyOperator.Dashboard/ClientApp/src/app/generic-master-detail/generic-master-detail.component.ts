import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';

import { SeverityDto } from '../../api/models/severity-dto';
import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import {
  TrivyExpandTableOptions,
  TrivyFilterData,
  TrivyTableCellCustomOptions,
  TrivyTableColumn,
  TrivyTableOptions,
} from '../trivy-table/trivy-table.types';

export interface IMasterDetail<TDetailDto> {
  details?: Array<TDetailDto> | null;
}

@Component({
  selector: 'app-generic-master-detail',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './generic-master-detail.component.html',
  styleUrl: './generic-master-detail.component.scss',
})
export class GenericMasterDetailComponent<TDataDto extends IMasterDetail<TDetailDto>, TDetailDto> {
  @Input() severityDtos: SeverityDto[] | null = [];
  @Input() activeNamespaces: string[] | null = [];
  @Input() mainTableColumns: TrivyTableColumn[] = [];
  @Input({ required: true }) mainTableOptions!: TrivyTableOptions;
  @Input() mainTableExpandTableOptions: TrivyExpandTableOptions<TDataDto> = new TrivyExpandTableOptions(false, 0, 0);
  @Input() isMainTableLoading: boolean = true;
  @ViewChild('mainTable', { static: true }) mainTable!: TrivyTableComponent<TDataDto>;
  @Input() detailsTableColumns: TrivyTableColumn[] = [];
  @Input({ required: true }) public detailsTableOptions!: TrivyTableOptions;
  @Output() refreshRequested = new EventEmitter<TrivyFilterData>();
  @Output() mainTableExpandCallback = new EventEmitter<TDataDto>();
  @Output() mainTableMultiHeaderActionRequested = new EventEmitter<string>();
  @Output() detailsTableMultiHeaderActionRequested = new EventEmitter<string>();
  @Output() mainTableSelectedRowChanged = new EventEmitter<TDataDto | null>();
  @Input() singleSelectDataDto?: TDataDto;
  selectedDataDto: TDataDto | null = null;

  private _dataDtos: TDataDto[] | null = [];

  get dataDtos(): TDataDto[] | null {
    return this._dataDtos;
  }

  /*@Input() dataDtos: TDataDto[] = [];*/
  @Input() set dataDtos(dataDtos: TDataDto[]) {
    this._dataDtos = dataDtos;
    this.onGetTDataDtos();
  }

  @Input() public mainTableExpandCellOptions: (
    dto: TDataDto,
    type: 'header' | 'row',
    column: number,
    row?: number,
  ) => TrivyTableCellCustomOptions = (_dto, _type, _column, _row) => ({
    value: '',
    style: '',
    buttonLink: undefined,
    badge: undefined,
    url: undefined,
  });

  onGetTDataDtos() {
    this.mainTable.onTableClearSelected();
    this.selectedDataDto = null;
    this.isMainTableLoading = false;
  }

  onMainTableSelectionChange(event: TDataDto[]) {
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

  onMainTableExpandCallback(event: TDataDto) {
    this.mainTableExpandCallback.emit(event);
  }

  onMainTableMultiHeaderActionRequested(event: string) {
    this.mainTableMultiHeaderActionRequested.emit(event);
  }

  onDetailsTableMultiHeaderActionRequested(event: string) {
    this.onDetailsTableMultiHeaderActionRequested(event);
  }
}
