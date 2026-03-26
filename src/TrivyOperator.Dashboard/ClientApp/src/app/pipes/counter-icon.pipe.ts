import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'counterIcon',
  standalone: true,
})
export class CounterIconPipe implements PipeTransform {
  transform(data: number | undefined): string {
    if (!data) {
      return '';
    }
    if (data > 6) {
      return 'pi pi-arrow-up';
    }
    if (data < 6) {
      return 'pi pi-arrow-down';
    }

    return "";
  }
}
