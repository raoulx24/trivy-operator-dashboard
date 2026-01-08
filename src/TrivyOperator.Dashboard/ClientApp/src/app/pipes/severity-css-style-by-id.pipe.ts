import { inject, Pipe, PipeTransform } from '@angular/core';
import { SeverityUtils } from '../utils/severity.utils';
import { SettingsService, SeverityColorByNameOption } from '../services/settings.service';

@Pipe({
  name: 'severityCssStyleById',
  standalone: true,
})
export class SeverityCssStyleByIdPipe implements PipeTransform {
  private readonly settingsService = inject(SettingsService);

  transform(
    severityId: number | string,
    severityCount: number = 0,
    option: SeverityColorByNameOption = this.settingsService.severityCssStyleByIdOption(),
  ): { [key: string]: string } {
    let cssColor = '';
    let opacity = '';
    const id = typeof severityId === 'string' ? (!isNaN(Number(severityId)) ? Number(severityId) : -1) : severityId;
    switch (option) {
      case 'all':
        cssColor = SeverityUtils.getCssColor(id);
        opacity = '1';
        break;
      case 'grayNulls':
        cssColor = severityCount < 0 ? 'gray' : SeverityUtils.getCssColor(id);
        opacity = severityCount < 0 ? '0.2' : '1';
        break;
      case 'grayBelowOne':
        cssColor = severityCount < 1 ? 'gray' : SeverityUtils.getCssColor(id);
        opacity = severityCount < 1 ? '0.2' : '1';
        break;
      case 'hideNonPositive':
        cssColor = severityCount > 0 ? SeverityUtils.getCssColor(id) : 'transparent';
        opacity = severityCount > 0 ? '1' : '0';
        break;
    }

    return {
      background: cssColor,
      opacity: opacity,
    };
  }
}
