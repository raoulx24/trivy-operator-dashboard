import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'capitalizeFirst',
  standalone: true,
})
export class CapitalizeFirstPipe implements PipeTransform {
  transform(data: string): string {
    if (!data) {
      return data;
    }
    return data.charAt(0).toUpperCase() + data.slice(1);
  }
}
