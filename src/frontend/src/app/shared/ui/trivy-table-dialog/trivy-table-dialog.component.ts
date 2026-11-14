import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, contentChild, input, model, output, TemplateRef } from '@angular/core';

import { DialogModule } from 'primeng/dialog';

import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import { MultiHeaderAction, TrivyTableColumn } from '../trivy-table/trivy-table.types';

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

  // commands: buttons in the table header; a click emits the action label (or the special action's event)
  readonly multiHeaderActions = input<MultiHeaderAction[]>([]);
  readonly multiHeaderActionRequested = output<string>();

  // pass-through to the table: set a unique key to persist sort/filters of this table separately
  readonly stateKey = input<string | undefined>(undefined);

  // options of the 'namespaces' / 'namespacesArrays' column filters; when not given, the distinct namespaces of the data
  readonly activeNamespaces = input<string[] | undefined>(undefined);

  // a click on an 'action' column button of the table
  readonly rowActionRequested = output<{ row: TData; col: string }>();

  protected readonly namespaceOptions = computed(() => {
    const given = this.activeNamespaces();
    if (given) {
      return given;
    }

    const fields = this.columns()
      .filter((c) => c.multiSelectType === 'namespaces' || c.multiSelectType === 'namespacesArrays')
      .map((c) => c.field);
    const values = this.dataDtos().flatMap((row) =>
      fields.flatMap((field) => String((row as Record<string, unknown>)[field] ?? '').split(',')),
    );

    return [...new Set(values.map((v) => v.trim()).filter(Boolean))].sort();
  });

  protected readonly beforeTemplate = contentChild<TemplateRef<unknown>>('before');
  protected readonly afterTemplate = contentChild<TemplateRef<unknown>>('after');
}
