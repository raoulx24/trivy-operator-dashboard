import { inject, Injectable, signal } from '@angular/core';
import { forkJoin } from 'rxjs';

import { AppVersion } from '../../api/models';
import { BackendSettingsDto } from '../../api/models/backend-settings-dto';
import { BackendSettingsService } from '../../api/services/backend-settings.service';
import { trivyMigrations } from '../constants/migration.constants';
import { DarkModeService } from './dark-mode.service';
import { MigrationService } from './migration.service';
import { SettingsService } from './settings.service';

@Injectable({
  providedIn: 'root',
})
export class MainAppInitService {
  defaultBackendSettingsDto: BackendSettingsDto | null = null;
  private readonly _backendSettingsDto = signal<BackendSettingsDto>({
    trivyReportConfigDtos: [],
  });
  readonly backendSettingsDto = this._backendSettingsDto.asReadonly();

  private readonly backendSettingsService = inject(BackendSettingsService);
  private readonly settingsService = inject(SettingsService);
  private readonly darkModeService = inject(DarkModeService);
  private readonly migrationService = inject(MigrationService);

  initializeApp(): Promise<void> {
    return new Promise((resolve, reject) => {
      forkJoin({
        backendSettings: this.backendSettingsService.getBackendSettings(),
        appVersion: this.settingsService.getAppVersion(),
      }).subscribe({
        next: ({ backendSettings, appVersion }) => {
          this.darkModeService.restoreMode();
          this.defaultBackendSettingsDto = backendSettings;
          this.mergeBackendSettingsDto(backendSettings);
          this.applyMigrations(appVersion);
          resolve();
        },
        error: (err) => {
          console.error('Error during app initialization:', err);
          reject(err);
        },
      });
    });
  }

  updateBackendSettingsTrivyReportConfigDto(newIds: string[]) {
    const newTrivyReportConfig = (
      this.defaultBackendSettingsDto ?? { trivyReportConfigDtos: [] }
    ).trivyReportConfigDtos?.map((dto) => {
      if (dto.enabled) {
        return { ...dto, enabled: newIds.includes(dto.id ?? '') };
      }
      return dto;
    });

    // const clone = JSON.parse(JSON.stringify(original)) as typeof original;
    this._backendSettingsDto.set({ trivyReportConfigDtos: newTrivyReportConfig });
    localStorage.setItem('backendSettings.trivyReportConfig', newIds.join(','));
    localStorage.setItem(
      'backendSettings.trivyReportConfig.defaultsPreviousSession',
      (this.defaultBackendSettingsDto?.trivyReportConfigDtos?.filter((x) => x.enabled).map((x) => x.id) ?? []).join(
        ',',
      ),
    );
  }

  private mergeBackendSettingsDto(backendSettingsDto: BackendSettingsDto) {
    const previousItems: string[] =
      localStorage.getItem('backendSettings.trivyReportConfig.defaultsPreviousSession')?.split(',') ?? [];
    const itemsToAdd = (
      backendSettingsDto.trivyReportConfigDtos?.filter((x) => x.enabled)?.map((x) => x.id ?? '') ?? []
    ).filter((x) => !previousItems.includes(x));
    const savedItems: string[] =
      localStorage.getItem('backendSettings.trivyReportConfig')?.split(',') ??
      backendSettingsDto.trivyReportConfigDtos?.filter((x) => x.enabled).map((x) => x.id ?? '') ??
      [];
    const mergedItems = [...savedItems, ...itemsToAdd.filter((item) => !savedItems.includes(item))];
    this.updateBackendSettingsTrivyReportConfigDto(mergedItems);
  }

  private applyMigrations(appVersion: AppVersion) {
    const appVersionKeyName = 'settings.appVersion';
    const savedAppVersion = localStorage.getItem(appVersionKeyName) ?? '1.0';

    this.migrationService.applyTableMigrations(savedAppVersion, appVersion.fileVersion ?? '1.0', trivyMigrations);
    //this.migrationService.applyTableMigrations("1.6", "1.7", trivyMigrations);

    localStorage.setItem(appVersionKeyName, appVersion.fileVersion ?? '1.0');
  }
}

export function initializeAppFactory(service: MainAppInitService): Promise<void> {
  return service.initializeApp(); // Return the actual Promise
}
