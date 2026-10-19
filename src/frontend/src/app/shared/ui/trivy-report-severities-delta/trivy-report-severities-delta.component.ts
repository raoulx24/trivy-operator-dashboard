import { Component, model } from '@angular/core';
import { Tag } from 'primeng/tag';

import { CounterIconPipe } from '../../pipes/counter-icon.pipe';
import { SeverityDifCssStyleByIdPipe } from '../../pipes/severity-dif-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { MiniBarChartDataDto } from '../mini-bar-chart/mini-bar-chart.types';

@Component({
  selector: 'app-trivy-report-severities-delta',
  imports: [Tag, SeverityDifCssStyleByIdPipe, CounterIconPipe, VulnerabilityCountPipe],
  templateUrl: `./trivy-report-severities-delta.component.html`,
  styleUrl: 'trivy-report-severities-delta.component.css',
})
export class TrivyReportSeveritiesDeltaComponent {
  data = model<MiniBarChartDataDto | undefined>(undefined);
  staticLabel = model<string>('Date:');
  background = model<string | undefined>(undefined);
  primaryColor = model<string | undefined>(undefined);
}
