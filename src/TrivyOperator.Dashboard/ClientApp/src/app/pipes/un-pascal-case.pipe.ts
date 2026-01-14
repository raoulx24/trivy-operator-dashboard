import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'unPascalCase',
  standalone: true,
})
export class UnPascalCasePipe implements PipeTransform {
  transform(str?: string | null, exclude: string[] = []): string {
    if (!str) return '';

    // Step 1: Add spaces between lowercase-uppercase transitions
    let spaced = str
      .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2') // handles acronyms like XMLHTTPRequest
      .replace(/([a-z0-9])([A-Z])/g, '$1 $2');

    // Step 2: Remove excluded words
    if (exclude.length) {
      spaced = spaced
        .split(' ')
        .filter((word) => !exclude.includes(word))
        .join(' ');
    }

    return spaced;
  }
}
