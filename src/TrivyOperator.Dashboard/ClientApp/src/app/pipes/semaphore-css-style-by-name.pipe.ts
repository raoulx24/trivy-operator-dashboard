import { Pipe, PipeTransform } from '@angular/core';
import { SemaphoreStatusUtils } from '../utils/semaphore-status.utils';

@Pipe({
  name: 'semaphoreCssStyleByName',
  standalone: true,
})
export class SemaphoreCssStyleByNamePipe implements PipeTransform {
  transform(semaphoreStatusNameStr: string): { [key: string]: string } {
    return {
      background: SemaphoreStatusUtils.getCssColorByName(semaphoreStatusNameStr),
    };
  }
}
