import { Pipe, PipeTransform } from '@angular/core';
import { SeverityUtils } from '../utils/severity.utils';

@Pipe({
  name: 'severityNameById',
  standalone: true,
})
export class SeverityNameByIdPipe implements PipeTransform {
  transform(severityId: number | string, shortName: boolean = false): string {
    const id = typeof severityId === 'string' ? (!isNaN(Number(severityId)) ? Number(severityId) : -1) : severityId;
    return shortName ? SeverityUtils.getCapitalizedShortName(id) : SeverityUtils.getCapitalizedName(id);
  }
}
