import { Component, model } from '@angular/core';
import { Tag } from 'primeng/tag';

import { SeverityDifCssStyleByIdPipe } from '../../pipes/severity-dif-css-style-by-id.pipe';
import { CounterIconPipe } from '../../pipes/counter-icon.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { MiniBarChartDataDto } from './mini-bar-chart.types';

@Component({
  selector: 'app-mini-bar-tooltip',
  template: `
    <div class="tod-tooltip grid grid-cols-[auto_1fr] gap-4 items-center"
         [style.background]="background()"
         [style.color]="primaryColor()">

      @let currentData = data();
      @if (currentData) {

        <div class="mb-2">
          <small class="tod-text-contrast-color mr-2 my-0">Date:</small><br />
          <small class="tod-text-contrast-color font-bold mr-2 my-0">
            {{ currentData.label }}
          </small>
        </div>

        <!-- Tag Container: Use CSS Grid for Columns, 2 per row -->
        <div class="flex items-center justify-center">

          <!-- Tag 1 -->
          <p-tag [rounded]="true"
                 [style]="0 | severityDifCssStyleById: currentData.criticalCount"
                 class="mr-1"
                 [icon]="currentData.criticalCount | counterIcon"
                 [value]="currentData.criticalCount | vulnerabilityCount : false" />

          <!-- Tag 2 -->
          <p-tag [rounded]="true"
                 [style]="1 | severityDifCssStyleById: currentData.highCount"
                 class="mr-1"
                 [icon]="currentData.highCount | counterIcon"
                 [value]="currentData.highCount | vulnerabilityCount : false" />

          <!-- Tag 3 -->
          <p-tag [rounded]="true"
                 [style]="2 | severityDifCssStyleById: currentData.mediumCount"
                 class="mr-1"
                 [icon]="currentData.mediumCount | counterIcon"
                 [value]="currentData.mediumCount | vulnerabilityCount : false" />

          <!-- Tag 4 -->
          <p-tag [rounded]="true"
                 [style]="3 | severityDifCssStyleById: currentData.lowCount"
                 class="mr-1"
                 [icon]="currentData.lowCount | counterIcon"
                 [value]="currentData.lowCount | vulnerabilityCount : false" />

          <!-- Tag 5 -->
          <p-tag [rounded]="true"
                 [style]="4 | severityDifCssStyleById: currentData.unknownCount"
                 class="mr-1"
                 [icon]="currentData.unknownCount | counterIcon"
                 [value]="currentData.unknownCount | vulnerabilityCount : false" />
        </div>
      }
    </div>
  `,
  styles: [
    `
      .tod-tooltip {
        pointer-events: none;
        color: var(--tod-text-primary-color);
        background: var(--p-highlight-background);
        padding: 5px;
        box-shadow: 0 0 5px rgba(0, 0, 0, 0.2);
      }

      .tod-tooltip,
      .tod-tooltip * {
        pointer-events: none !important;
      }

    `,
  ],
  standalone: true,
  imports: [Tag, SeverityDifCssStyleByIdPipe, CounterIconPipe, VulnerabilityCountPipe],
})
export class MiniBarTooltipComponent {
  data = model<MiniBarChartDataDto | undefined>(undefined);
  label = model
  background = model<string | undefined>(undefined);
  primaryColor = model<string | undefined>(undefined);
}
