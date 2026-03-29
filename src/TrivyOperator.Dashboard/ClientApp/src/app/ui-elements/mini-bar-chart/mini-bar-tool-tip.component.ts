import { Component, model } from '@angular/core';
import { Tag } from 'primeng/tag';
import { SeverityDifCssStyleByIdPipe } from '../../pipes/severity-dif-css-style-by-id.pipe';
import { CounterIconPipe } from '../../pipes/counter-icon.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { MiniBarChartDataDto } from './mini-bar-chart.types';

@Component({
  selector: 'app-mini-bar-tooltip',
  template: `
    <div class="tod-tooltip" [style.background]="background" [style.color]="primaryColor">
      @if (data()) {
        @let criticalCount = data()?.criticalCount ?? 0;
        @let highCount = data()?.highCount ?? 0;
        @let mediumCount = data()?.mediumCount ?? 0;
        @let lowCount = data()?.lowCount ?? 0;
        @let unknownCount = data()?.unknownCount ?? 0;
        <p-tag [rounded]="true"
               [style]="0 | severityDifCssStyleById: criticalCount"
               class="mr-1"
               [icon]="criticalCount | counterIcon"
               [value]="criticalCount | vulnerabilityCount : false" />
        <p-tag [rounded]="true"
               [style]="1 | severityDifCssStyleById: highCount"
               class="mr-1"
               [icon]="highCount | counterIcon"
               [value]="highCount | vulnerabilityCount : false" />
        <p-tag [rounded]="true"
               [style]="2 | severityDifCssStyleById: mediumCount"
               class="mr-1"
               [icon]="mediumCount | counterIcon"
               [value]="mediumCount | vulnerabilityCount : false" />
        <p-tag [rounded]="true"
               [style]="3 | severityDifCssStyleById: lowCount"
               class="mr-1"
               [icon]="lowCount | counterIcon"
               [value]="lowCount | vulnerabilityCount : false" />
        <p-tag [rounded]="true"
               [style]="4 | severityDifCssStyleById: unknownCount"
               class="mr-1"
               [icon]="unknownCount | counterIcon"
               [value]="unknownCount | vulnerabilityCount : false" />
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
    `,
  ],
  standalone: true,
  imports: [Tag, SeverityDifCssStyleByIdPipe, CounterIconPipe, VulnerabilityCountPipe],
})
export class MiniBarTooltipComponent {
  data = model<MiniBarChartDataDto | undefined>(undefined);
  background = model<string | undefined>(undefined);
  primaryColor = model<string | undefined>(undefined);
}
