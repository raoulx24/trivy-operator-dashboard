import { effect, inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { AppVersion } from '@api/models';
import { AppVersionsService } from '@api/services/app-versions.service';
import { LocalStorageUtils } from '@shared/utils/local-storage.utils';

export type SeverityColorByNameOption = 'all' | 'grayNulls' | 'grayBelowOne' | 'hideNonPositive';

@Injectable({
  providedIn: 'root',
})
export class SettingsService {
  readonly severityCssStyleByIdOptions: ReadonlyArray<SeverityColorByNameOption> = [
    'all',
    'grayNulls',
    'grayBelowOne',
    'hideNonPositive',
  ];

  private readonly defaultOption: SeverityColorByNameOption = 'grayBelowOne';

  private readonly appVersionsService = inject(AppVersionsService);

  readonly severityCssStyleByIdOption = signal<SeverityColorByNameOption>(this.restoreSeverityCssStyleByIdOption());

  constructor() {
    effect(() => {
      LocalStorageUtils.setItem('severityCssStyleByIdOption', this.severityCssStyleByIdOption());
    });
  }

  private restoreSeverityCssStyleByIdOption(): SeverityColorByNameOption {
    const saved = LocalStorageUtils.getItem('severityCssStyleByIdOption');
    if (saved === null) {
      return this.defaultOption;
    }
    if (this.severityCssStyleByIdOptions.includes(saved as SeverityColorByNameOption)) {
      return saved as SeverityColorByNameOption;
    }
    return this.defaultOption;
  }

  getAppVersion(): Observable<AppVersion> {
    return this.appVersionsService.getCurrentVersion();
  }
}
