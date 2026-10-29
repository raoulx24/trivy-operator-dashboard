import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom, forkJoin, Observable, timeout } from 'rxjs';

import { AppVersion } from '@api/models';
import { BackendSettingsDto } from '@api/models/backend-settings-dto';
import { BackendSettingsService } from '@api/services/backend-settings.service';
import { trivyMigrations } from '../migration/migration.constants';
import { DarkModeService } from './dark-mode.service';
import { injectLogger } from '../logging/inject-logger';
import { MigrationService } from '../migration/migration.service';
import { SettingsService } from './settings.service';
import { RetryPolicy, retryWithBackoff } from '@shared/utils/retry-policy.utils';
import { LocalStorageUtils } from '@shared/utils/local-storage.utils';

@Injectable({
  providedIn: 'root',
})
export class MainAppInitService {
  defaultBackendSettingsDto: BackendSettingsDto | null = null;
  private readonly _backendSettingsDto = signal<BackendSettingsDto>({
    trivyReportConfigDtos: [],
    isDefaultContextUsed: true,
    isFileRepositoryUsed: false,
    isKubeConfigUsed: false,
    isNamespaceListUsed: false,
    vrHistoryMaxAgeDays: 14,
    // off until the backend says otherwise, so the NVD tags do not flash before the settings arrive
    isNvdEnabled: false,
    isCveEnrichmentEnabled: false,
  });
  readonly backendSettingsDto = this._backendSettingsDto.asReadonly();

  private readonly initRetryPolicy = new RetryPolicy({
    initialDelayMs: 500,
    maxDelayMs: 8_000,
    maxRetries: 6, // about 30s in the worst case, then give up
  });

  private readonly log = injectLogger('App init');
  private readonly backendSettingsService = inject(BackendSettingsService);
  private readonly settingsService = inject(SettingsService);
  private readonly darkModeService = inject(DarkModeService);
  private readonly migrationService = inject(MigrationService);

  async initializeApp(): Promise<void> {
    this.darkModeService.restoreMode(); // no network needed; running it first avoids a light-mode flash

    const withResilience = <T>(source$: Observable<T>) =>
      source$.pipe(timeout(10_000), retryWithBackoff<T>(this.initRetryPolicy));

    try {
      const { backendSettings, appVersion } = await firstValueFrom(
        forkJoin({
          backendSettings: withResilience(this.backendSettingsService.getBackendSettings()),
          appVersion: withResilience(this.settingsService.getAppVersion()),
        }),
      );

      this.defaultBackendSettingsDto = backendSettings;
      this.mergeBackendSettingsDto(backendSettings);
      this.applyMigrations(appVersion);
    } catch (err) {
      // no toast: a failing initializer aborts bootstrap, so no toast host exists yet
      this.log.error('Error during app initialization.', { error: err, toast: false });
      throw err;
    }
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
    this._backendSettingsDto.update((item) => {
      const newItem: BackendSettingsDto = {
        isDefaultContextUsed: this.defaultBackendSettingsDto?.isDefaultContextUsed ?? false,
        isFileRepositoryUsed: this.defaultBackendSettingsDto?.isFileRepositoryUsed ?? false,
        isNamespaceListUsed: this.defaultBackendSettingsDto?.isNamespaceListUsed ?? false,
        isKubeConfigUsed: this.defaultBackendSettingsDto?.isKubeConfigUsed ?? false,
        trivyReportConfigDtos: newTrivyReportConfig,
        vrHistoryMaxAgeDays: this.defaultBackendSettingsDto?.vrHistoryMaxAgeDays ?? 14,
        isNvdEnabled: this.defaultBackendSettingsDto?.isNvdEnabled ?? false,
        isCveEnrichmentEnabled: this.defaultBackendSettingsDto?.isCveEnrichmentEnabled ?? false,
      };
      return newItem;
    });

    LocalStorageUtils.setItem('backendSettings.trivyReportConfig', newIds.join(','));
    LocalStorageUtils.setItem(
      'backendSettings.trivyReportConfig.defaultsPreviousSession',
      (this.defaultBackendSettingsDto?.trivyReportConfigDtos?.filter((x) => x.enabled).map((x) => x.id) ?? []).join(
        ',',
      ),
    );
  }

  private mergeBackendSettingsDto(backendSettingsDto: BackendSettingsDto) {
    const previousItems = this.parseIds(
      LocalStorageUtils.getItem('backendSettings.trivyReportConfig.defaultsPreviousSession'),
    );
    const itemsToAdd = (
      backendSettingsDto.trivyReportConfigDtos?.filter((x) => x.enabled)?.map((x) => x.id ?? '') ?? []
    ).filter((x) => !previousItems.includes(x));
    // an empty saved value means "no reports enabled", not "no saved value"
    const savedRaw = LocalStorageUtils.getItem('backendSettings.trivyReportConfig');
    const savedItems: string[] =
      savedRaw !== null
        ? this.parseIds(savedRaw)
        : (backendSettingsDto.trivyReportConfigDtos?.filter((x) => x.enabled).map((x) => x.id ?? '') ?? []);
    const mergedItems = [...savedItems, ...itemsToAdd.filter((item) => !savedItems.includes(item))];
    this.updateBackendSettingsTrivyReportConfigDto(mergedItems);
  }

  private parseIds(raw: string | null): string[] {
    return raw ? raw.split(',').filter((id) => id !== '') : [];
  }

  private applyMigrations(appVersion: AppVersion) {
    const appVersionKeyName = 'settings.appVersion';
    const savedAppVersion = LocalStorageUtils.getItem(appVersionKeyName) ?? '1.0';

    this.migrationService.applyTableMigrations(savedAppVersion, appVersion.fileVersion ?? '1.0', trivyMigrations);
    //this.migrationService.applyTableMigrations("1.6", "1.7", trivyMigrations);

    LocalStorageUtils.setItem(appVersionKeyName, appVersion.fileVersion ?? '1.0');
  }
}

export function initializeAppFactory(service: MainAppInitService): Promise<void> {
  return service.initializeApp(); // Return the actual Promise
}
