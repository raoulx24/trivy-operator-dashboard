import { Pipe, PipeTransform } from '@angular/core';
import { SeverityUtils } from '../utils/severity.utils';

@Pipe({
  name: 'severityNamesMaxDisplay',
  standalone: true,
})
export class SeverityNamesMaxDisplayPipe implements PipeTransform {
  transform(severityIds: number[], maxDisplay: number = 2): string {
    return SeverityUtils.getNames(severityIds, maxDisplay);
  }
}
