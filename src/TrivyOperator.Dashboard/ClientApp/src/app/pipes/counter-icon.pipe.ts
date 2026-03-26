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
      // return 'pi pi-plus-circle';
      // return 'pi pi-arrow-circle-up';
    }
    if (data < 6) {
      return 'pi pi-arrow-down';
      // return 'pi pi-minus-circle';
      // return 'pi pi-arrow-circle-down';
    }

    return "";
  }
}
