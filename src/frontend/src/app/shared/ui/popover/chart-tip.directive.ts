import { DestroyRef, Directive, inject, input, OnInit, TemplateRef } from '@angular/core';

import { CHART_TIP_SOURCE } from './chart-tip';
import { PopoverOptions, PopoverService } from './popover.service';

/**
 * Shows the data popover of a chart, with a template of the owner:
 *   <app-stacked-bar-chart [todChartTip]="columnTip" ... />
 *   <ng-template #columnTip let-ctx> ... </ng-template>
 * The template context is what the chart emits for the hovered item (e.g. StackedBarTipContext). The popover follows the
 * mouse and is hidden when the chart goes away.
 */
@Directive({ selector: '[todChartTip]' })
export class ChartTipDirective implements OnInit {
  readonly template = input.required<TemplateRef<any>>({ alias: 'todChartTip' });
  readonly options = input<PopoverOptions | undefined>(undefined, { alias: 'todChartTipOptions' });

  private readonly popover = inject(PopoverService);
  private readonly source = inject(CHART_TIP_SOURCE);

  constructor() {
    inject(DestroyRef).onDestroy(() => this.popover.hide());
  }

  ngOnInit(): void {
    this.source.tipShow.subscribe((tip) => this.popover.show(this.template(), tip.event, tip.context, this.options()));
    this.source.tipMove.subscribe((event) => this.popover.move(event));
    this.source.tipHide.subscribe(() => this.popover.hide());
  }
}
