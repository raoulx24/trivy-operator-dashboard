import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'genericObjectArraySummary',
  standalone: true, // 👈 This makes it standalone!
})
export class GenericObjectArraySummaryPipe implements PipeTransform {
  transform(value: any[]): string {
    if (value === null) return 'mama mia';
    if (!Array.isArray(value)) return 'none 1';
    if (!Array.isArray(value)) return 'none 1';
    if (value.length === 0) return 'none 2';

    return value
      .filter((obj) => this.isValidFormat(obj))
      .map((obj) => {
        const stringKey = Object.keys(obj).find((key) => typeof obj[key] === 'string')!;
        const numberKey = Object.keys(obj).find((key) => typeof obj[key] === 'number')!;
        return `"${obj[stringKey]}": ${obj[numberKey]}`;
      })
      .join(', ');
  }

  private isValidFormat(obj: any): boolean {
    if (typeof obj !== 'object' || !obj) return false;
    const keys = Object.keys(obj);
    const stringFields = keys.filter((k) => typeof obj[k] === 'string');
    const numberFields = keys.filter((k) => typeof obj[k] === 'number');
    return stringFields.length === 1 && numberFields.length === 1;
  }
}
