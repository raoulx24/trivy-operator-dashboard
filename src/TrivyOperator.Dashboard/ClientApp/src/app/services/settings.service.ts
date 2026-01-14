import { effect, inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { AppVersion } from '../../api/models';
import { AppVersionService } from '../../api/services/app-version.service';

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

  private readonly appVersionService = inject(AppVersionService);

  readonly severityCssStyleByIdOption = signal<SeverityColorByNameOption>(
    (localStorage.getItem('severityCssStyleByIdOption') as SeverityColorByNameOption) ?? this.defaultOption,
  );

  constructor() {
    effect(() => {
      localStorage.setItem('severityCssStyleByIdOption', this.severityCssStyleByIdOption());
    });
  }

  getAppVersion(): Observable<AppVersion> {
    return this.appVersionService.getCurrentVersion();
  }
}
