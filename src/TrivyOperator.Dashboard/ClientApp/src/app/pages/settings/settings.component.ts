import { Component, effect, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { PanelModule } from 'primeng/panel';
import { SelectButtonModule, SelectButtonOptionClickEvent } from 'primeng/selectbutton';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';

import { BackendSettingsDto } from '../../../api/models/backend-settings-dto';
import { MainAppInitService } from '../../services/main-app-init.service';
import { LocalStorageUtils } from '../../utils/local-storage.utils';
import { PrimengTableStateUtil } from '../../utils/primeng-table-state.util';
import { ClearTablesOptions, SavedCsvFileName, TrivyReportConfig } from './settings.types';
import { SettingsService, SeverityColorByNameOption } from '../../services/settings.service';

import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { sample } from 'rxjs';


@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    FormsModule,
    ButtonModule,
    CardModule,
    CheckboxModule,
    InputTextModule,
    PanelModule,
    SelectButtonModule,
    TableModule,
    TagModule,
    VulnerabilityCountPipe,
    SeverityCssStyleByIdPipe,
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent implements OnInit {
  public clearTablesOptions: ClearTablesOptions[] = [];
  public csvFileNames: SavedCsvFileName[] = [];
  public trivyReportConfigs: TrivyReportConfig[] = [];

  severityCssStyleByIdOptions: { id: SeverityColorByNameOption, label: string }[] = [];
  severityCssStyleByIdOptionIndex: number = 0;
  severityCssStyleByIdOptionValue: SeverityColorByNameOption = 'grayBelowOne';
  severityCssStyleByIdOptionDescription: string = "";

  private readonly mainAppInitService = inject(MainAppInitService);
  private readonly settingsService = inject(SettingsService);

  constructor() {
    effect(() => {
      const updatedBackendSettingsDto = this.mainAppInitService.backendSettingsDto();
      this.loadTrivyReportsStates(updatedBackendSettingsDto);
    });
  }

  ngOnInit() {
    this.loadTableOptions();
    this.loadCsvFileNames();
    this.loadSeverityColorByName();
  }

  onClearTableStatesSelected(_event: MouseEvent) {
    this.clearTablesOptions.forEach((option) => {
      const tableStateJson = localStorage.getItem(option.dataKey);
      if (tableStateJson) {
        if (option.all) {
          localStorage.removeItem(option.dataKey);
        } else {
          const tableState = JSON.parse(tableStateJson);
          if (option.filters) {
            PrimengTableStateUtil.clearTableFilters(tableState);
          }
          if (option.sort) {
            PrimengTableStateUtil.clearTableMultiSort(tableState);
          }
          if (option.columnWidths) {
            PrimengTableStateUtil.clearTableColumnWidths(tableState);
          }
          if (option.columnOrder) {
            PrimengTableStateUtil.clearTableColumnOrder(tableState);
          }
          localStorage.setItem(option.dataKey, JSON.stringify(tableState));
        }
      }
    });
    this.loadTableOptions();
  }

  onClearTableStatesAll(_event: MouseEvent) {
    this.clearTablesOptions.forEach((option) => {
      const tableStateJson = localStorage.getItem(option.dataKey);
      if (tableStateJson) {
        localStorage.removeItem(option.dataKey);
      }
    });
    this.loadTableOptions();
  }

  onUpdateCsvFileNames(_event: MouseEvent) {
    this.csvFileNames.forEach((x) => {
      localStorage.setItem(x.dataKey, x.savedCsvName);
    });
  }

  onUpdateTrivyReportsStates(_event: MouseEvent) {
    this.mainAppInitService.updateBackendSettingsTrivyReportConfigDto(
      this.trivyReportConfigs.filter((x) => x.frontendEnabled).map((x) => x.id),
    );
  }

  private loadTableOptions() {
    this.clearTablesOptions = LocalStorageUtils.getKeysWithPrefix(LocalStorageUtils.trivyTableKeyPrefix)
      .sort((x, y) => (x > y ? 1 : -1))
      .map((x) => {
        return new ClearTablesOptions(x, x.slice(LocalStorageUtils.trivyTableKeyPrefix.length));
      });
  }

  private loadCsvFileNames() {
    this.csvFileNames = LocalStorageUtils.getKeysWithPrefix(LocalStorageUtils.csvFileNameKeyPrefix)
      .sort((x, y) => (x > y ? 1 : -1))
      .map((x) => {
        return {
          dataKey: x,
          description: x.slice(LocalStorageUtils.csvFileNameKeyPrefix.length),
          savedCsvName: localStorage.getItem(x) ?? '',
        };
      });
  }

  private loadTrivyReportsStates(backendSettingsDto: BackendSettingsDto) {
    const defaultBackedSettings = this.mainAppInitService.defaultBackendSettingsDto ?? { trivyReportConfigDtos: [] };
    this.trivyReportConfigs =
      backendSettingsDto.trivyReportConfigDtos?.map((x) => ({
        id: x.id ?? '',
        name: x.name ?? '',
        backendEnabled: defaultBackedSettings.trivyReportConfigDtos?.find((def) => def.id === x.id)?.enabled ?? false,
        frontendEnabled: x.enabled ?? false,
      })) ?? [];
  }

  private loadSeverityColorByName() {
    this.setSeverityColorByNameOptionIndex(this.settingsService.severityCssStyleByIdOptions.indexOf(this.settingsService.severityCssStyleByIdOption()));

    this.severityCssStyleByIdOptions = this.settingsService.severityCssStyleByIdOptions.map(x => {
      let label = "";
      switch (x) {
        case 'all':
          label = "All";
          break;
        case 'grayBelowOne':
          label = 'Non Zero';
          break;
        case 'grayNulls':
          label = "Non Null";
          break;
        case 'hideNonPositive':
          label = "Only Non Zero";
          break;
      }
      return { id: x, label: label };
    });
    this.severityCssStyleByIdOptionValue = this.settingsService.severityCssStyleByIdOption();
  }

  onSeverityCssStyleByIdOptionsClick(event: SelectButtonOptionClickEvent) {
    if (event.index !== undefined) {
      this.setSeverityColorByNameOptionIndex(event.index);
    }
  }

  private setSeverityColorByNameOptionIndex(index: number) {
    this.severityCssStyleByIdOptionIndex = index;
    this.settingsService.severityCssStyleByIdOption.set(this.settingsService.severityCssStyleByIdOptions[index] ?? this.settingsService.severityCssStyleByIdOption());
    switch (index) {
      case 0:
        this.severityCssStyleByIdOptionDescription = "All are visible";
        break;
      case 1:
        this.severityCssStyleByIdOptionDescription = "All not null (0 and above) are visible; rest are faint";
        break;
      case 2:
        this.severityCssStyleByIdOptionDescription = "All above 0 are visible; rest are faint";
        break;
      case 3:
        this.severityCssStyleByIdOptionDescription = "All above 0 are visible; rest are not";
        break;
    }
  }

  protected readonly sample = sample;
}
