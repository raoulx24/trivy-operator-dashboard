import { Pipe, PipeTransform } from '@angular/core';
import { SeverityUtils } from '../utils/severity.utils';

@Pipe({
  name: 'severityDifCssStyleById',
  standalone: true,
})
export class SeverityDifCssStyleByIdPipe implements PipeTransform {

  transform(
    severityId: number | string,
    severityCount: number = 0,
  ): { [key: string]: string } {
    let cssColor = '';
    let opacity = '';
    let border = '';
    const id = typeof severityId === 'string' ? (!isNaN(Number(severityId)) ? Number(severityId) : -1) : severityId;

    cssColor = severityCount === 0 ? 'gray' : SeverityUtils.getCssColor(id);
    opacity = severityCount < 1 ? '0.4' : '1';
    border = severityCount > 0 ? '2px solid #ffffff' : ''

    return {
      background: cssColor,
      opacity: opacity,
      border: border,
    };
  }
}
