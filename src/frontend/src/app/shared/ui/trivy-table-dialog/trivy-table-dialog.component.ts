import { NgTemplateOutlet } from '@angular/common';
import { Component, contentChild, input, model, TemplateRef } from '@angular/core';

import { DialogModule } from 'primeng/dialog';

import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../trivy-table/trivy-table.types';

// Generic dialog showing one trivy table. Optional content around the table is given as templates:
//   <app-trivy-table-dialog ...>
//     <ng-template #before>...</ng-template>
//     <ng-template #after>...</ng-template>
//   </app-trivy-table-dialog>
@Component({
  selector: 'app-trivy-table-dialog',
  standalone: true,
  imports: [DialogModule, NgTemplateOutlet, TrivyTableComponent],
  templateUrl: './trivy-table-dialog.component.html',
})
export class TrivyTableDialogComponent<TData> {
  readonly isVisible = model(false);

  readonly title = input<string>('');
  readonly dataDtos = input<TData[]>([]);
  readonly columns = input.required<TrivyTableColumn[]>();

  // dialog size (any CSS length)
  readonly width = input<string>('60rem');
  readonly height = input<string>('60vh');

  // pass-through to the table: set a unique key to persist sort/filters of this table separately
  readonly stateKey = input<string | undefined>(undefined);

  protected readonly beforeTemplate = contentChild<TemplateRef<unknown>>('before');
  protected readonly afterTemplate = contentChild<TemplateRef<unknown>>('after');
}
