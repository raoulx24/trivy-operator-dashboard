import { Component, model } from '@angular/core';
import { Tag } from 'primeng/tag';

import { SeverityDifCssStyleByIdPipe } from '../../pipes/severity-dif-css-style-by-id.pipe';
import { CounterIconPipe } from '../../pipes/counter-icon.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { MiniBarChartDataDto } from './mini-bar-chart.types';

@Component({
  selector: 'app-mini-bar-tooltip',
  template: `
    <div class="tod-tooltip tod-content-border-contrast-color tod-tooltip-shadow
            grid grid-cols-[auto_repeat(5,max-content)]
            grid-rows-[auto_auto_auto] gap-x-4 items-center"
         [style.background]="background()"
         [style.color]="primaryColor()">

      @let currentData = data();
      @if (currentData) {

        <!-- Column 1 spans all 3 rows -->
        <div class="row-span-3 flex flex-col justify-center">
          <small class="tod-text-contrast-color mr-2 my-0">{{ staticLabel() }}</small>
          <small class="tod-text-contrast-color font-bold mr-2 my-0">{{ currentData.label }}</small>
        </div>

        <!-- Row 1: newCount -->
        @for (data of currentData.newCount; track $index) {
          <div class="flex justify-center">
            <p-tag [rounded]="true"
                   [style]="$index | severityDifCssStyleById: data"
                   [icon]="data | counterIcon"
                   [value]="data | vulnerabilityCount : 'hideZeroes'" />
          </div>
        }

        <!-- Row 2: hr (perfectly aligned across all columns) -->
        @for (i of [0,1,2,3,4]; track i) {
          <div class="flex justify-center w-full">
            <hr class="tod-datatable-border-color w-full my-2" />
          </div>
        }

        <!-- Row 3: removedCount -->
        @for (data of currentData.removedCount; track $index) {
          <div class="flex justify-center">
            <p-tag [rounded]="true"
                   [style]="$index | severityDifCssStyleById: (data * -1)"
                   [icon]="(data * -1) | counterIcon"
                   [value]="(data * -1) | vulnerabilityCount : 'hideZeroes'" />
          </div>
        }
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
  staticLabel = model<string>("Date:");
  background = model<string | undefined>(undefined);
  primaryColor = model<string | undefined>(undefined);
}
