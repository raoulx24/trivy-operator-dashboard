import { Injectable, signal, effect, inject } from '@angular/core';
import { AppVersionService } from '../../api/services/app-version.service';
import { AppVersion } from '../../api/models';
import { Observable } from 'rxjs';

export type SeverityColorByNameOption =
  | 'all'
  | 'grayNulls'
  | 'grayBelowOne'
  | 'hideNonPositive';

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

  private readonly appVersionService = inject(AppVersionService);

  readonly severityCssStyleByIdOption = signal<SeverityColorByNameOption>(
    (localStorage.getItem('severityCssStyleByIdOption') as SeverityColorByNameOption) ??
    this.defaultOption
  );

  constructor() {
    effect(() => {
      localStorage.setItem(
        'severityCssStyleByIdOption',
        this.severityCssStyleByIdOption()
      );
    });
  }

  // API call stays Observable
  getAppVersion(): Observable<AppVersion> {
    return this.appVersionService.getCurrentVersion();
  }
}
