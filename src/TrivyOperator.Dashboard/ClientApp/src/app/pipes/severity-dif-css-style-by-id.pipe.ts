import { inject, Pipe, PipeTransform } from '@angular/core';
import { SeverityUtils } from '../utils/severity.utils';
import { DOCUMENT } from '@angular/common';

@Pipe({
  name: 'severityDifCssStyleById',
  standalone: true,
})
export class SeverityDifCssStyleByIdPipe implements PipeTransform {
  private document = inject(DOCUMENT);

  transform(severityId: number | string, severityCount: number = 0, showBorder: boolean = true): { [key: string]: string } {
    const rootElement = this.document.documentElement;
    const computedStyle = getComputedStyle(rootElement);
    const contrastColor = computedStyle.getPropertyValue('--p-text-color');

    let cssColor = '';
    let opacity = '';
    let border = '';
    const id = typeof severityId === 'string' ? (!isNaN(Number(severityId)) ? Number(severityId) : -1) : severityId;

    cssColor = severityCount === 0 ? 'gray' : SeverityUtils.getCssColor(id);
    opacity = severityCount < 1 ? '0.4' : '1';
    border = showBorder ? (severityCount !== 0 ? `2px solid ${contrastColor}` : '') : '';

    return {
      background: cssColor,
      opacity: opacity,
      border: border,
    };
  }
}
