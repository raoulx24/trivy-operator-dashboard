import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject, forkJoin } from 'rxjs';

import { BackendSettingsDto } from '../../api/models/backend-settings-dto';
import { BackendSettingsService } from '../../api/services/backend-settings.service';
import { SettingsService } from './settings.service';
import { AppVersion } from '../../api/models';
import { DarkModeService } from './dark-mode.service';

@Injectable({
  providedIn: 'root',
})
export class MainAppInitService {
  defaultBackendSettingsDto: BackendSettingsDto | null = null;
  private backendSettingsDtoSubject: BehaviorSubject<BackendSettingsDto> = new BehaviorSubject<BackendSettingsDto>({
    trivyReportConfigDtos: [],
  });
  backendSettingsDto$ = this.backendSettingsDtoSubject.asObservable();

  constructor(
    private backendSettingsService: BackendSettingsService,
    private settingsService: SettingsService,
    private darkModeService: DarkModeService) { }

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
          this.something(appVersion);
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
    this.backendSettingsDtoSubject.next({ trivyReportConfigDtos: newTrivyReportConfig });
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

  private something(appVersion: AppVersion) {
    const appVersionKeyName = 'settings.appVersion';
    const savedAppVersion = localStorage.getItem(appVersionKeyName);
    if (!savedAppVersion) {
      const keys: string[] = [];
      for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        if (key && key.startsWith('trivyTable')) {
          keys.push(key);
        }
      }
      keys.forEach(x => localStorage.removeItem(x));
    }
    localStorage.setItem(appVersionKeyName, appVersion.fileVersion ?? "1.0");
  }
}

export function initializeAppFactory(service: MainAppInitService): Promise<void> {
  return service.initializeApp(); // Return the actual Promise
}
