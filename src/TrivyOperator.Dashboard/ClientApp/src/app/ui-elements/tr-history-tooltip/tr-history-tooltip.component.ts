import { Component, model } from '@angular/core';
import { Tag } from 'primeng/tag';

import { CounterIconPipe } from '../../pipes/counter-icon.pipe';
import { SeverityDifCssStyleByIdPipe } from '../../pipes/severity-dif-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { MiniBarChartDataDto } from '../mini-bar-chart/mini-bar-chart.types';

@Component({
  selector: 'app-tr-history-tooltip',
  imports: [Tag, SeverityDifCssStyleByIdPipe, CounterIconPipe, VulnerabilityCountPipe],
  templateUrl: `./tr-history-tooltip.component.html`,
  styleUrl: 'tr-history-tooltip.component.css',
})
export class TrHistoryTooltipComponent {
  data = model<MiniBarChartDataDto | undefined>(undefined);
  staticLabel = model<string>('Date:');
  background = model<string | undefined>(undefined);
  primaryColor = model<string | undefined>(undefined);
}
