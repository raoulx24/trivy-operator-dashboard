import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'tests',
  standalone: true,
})
export class TestsPipe implements PipeTransform {
  transform(data?: string | null): string {
    if (!data) {
      return '';
    }
    return `${data} - ${new Date().toISOString()}`;
  }
}
