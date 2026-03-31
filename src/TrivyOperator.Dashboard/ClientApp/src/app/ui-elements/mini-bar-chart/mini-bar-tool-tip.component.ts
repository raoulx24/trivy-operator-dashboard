import { Component, model } from '@angular/core';
import { Tag } from 'primeng/tag';

import { SeverityDifCssStyleByIdPipe } from '../../pipes/severity-dif-css-style-by-id.pipe';
import { CounterIconPipe } from '../../pipes/counter-icon.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { MiniBarChartDataDto } from './mini-bar-chart.types';

@Component({
  selector: 'app-mini-bar-tooltip',
  template: `
    <div class="tod-tooltip tod-content-border-contrast-color tod-tooltip-shadow grid grid-cols-[auto_1fr] gap-4 items-center"
         [style.background]="background()"
         [style.color]="primaryColor()">

      @let currentData = data();
      @if (currentData) {

        <div class="mb-2">
          <small class="tod-text-contrast-color mr-2 my-0">{{ staticLabel() }}</small><br />
          <small class="tod-text-contrast-color font-bold mr-2 my-0">{{ currentData.label }}</small>
        </div>

        <!-- Tag Container: Use CSS Grid for Columns, 2 per row -->
        <div class="flex items-center justify-center">
          @for (severityCount of currentData.newCount; track $index) {
            <p-tag [rounded]="true"
                   [style]="$index | severityDifCssStyleById: severityCount"
                   class="mr-1"
                   [icon]="severityCount | counterIcon"
                   [value]="severityCount | vulnerabilityCount : 'hideZeroes'" />
          }
        </div>
        <hr class="tod-datatable-border-color" />
        <div class="flex items-center justify-center">
          @for (severityCount of currentData.removedCount; track $index) {
            <p-tag [rounded]="true"
                   [style]="$index | severityDifCssStyleById: (severityCount * (-1))"
                   class="mr-1"
                   [icon]="(severityCount * (-1)) | counterIcon"
                   [value]="(severityCount * (-1)) | vulnerabilityCount : 'hideZeroes'" />
          }
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
  staticLabel = model<string>("Date:");
  background = model<string | undefined>(undefined);
  primaryColor = model<string | undefined>(undefined);
}
