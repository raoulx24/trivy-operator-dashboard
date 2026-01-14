import { DOCUMENT } from '@angular/common';
import { Pipe, PipeTransform, inject } from '@angular/core';

@Pipe({
  name: 'computedHeight',
  standalone: true,
})
export class ComputedHeightPipe implements PipeTransform {
  private document = inject(DOCUMENT);

  transform(): number {
    const rootElement = this.document.documentElement;
    const computedStyle = getComputedStyle(rootElement);

    // Retrieve both variables
    const lineHeight = parseFloat(computedStyle.getPropertyValue('--tod-host-line-height')) || 1.5;
    const paddingValues = computedStyle.getPropertyValue('--root-padding').split(' ');

    // Extract the first padding value and ensure it's a number
    const firstPadding = parseFloat(paddingValues[0]) || 0;

    // Calculate total height in REM
    const totalHeight = lineHeight + firstPadding;

    return totalHeight;
  }
}
