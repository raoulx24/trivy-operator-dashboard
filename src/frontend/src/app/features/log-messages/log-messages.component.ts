import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';

import { StoredLogEntry } from '@core/logging/log.models';
import { MemorySink } from '@core/logging/sinks/memory.sink';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import {
  MultiHeaderAction,
  TrivyTableColumn,
  TrivyTableExpandRowData,
} from '@shared/ui/trivy-table/trivy-table.types';
import { logMessageColumns } from './log-messages.constants';

const clearLogAction = 'Clear Log';

@Component({
  selector: 'app-log-messages',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './log-messages.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogMessagesComponent {
  private readonly memorySink = inject(MemorySink);

  // newest first; the entries keep their identity, so expanded rows and their details survive new arrivals
  protected readonly entries = computed(() => [...this.memorySink.entries()].reverse());
  protected readonly trivyTableColumns: TrivyTableColumn[] = [...logMessageColumns];
  protected readonly multiHeaderActions: MultiHeaderAction[] = [{ label: clearLogAction, icon: 'pi pi-trash' }];
  protected readonly rowExpandResponse = signal<TrivyTableExpandRowData<StoredLogEntry> | undefined>(undefined);


  protected onMultiHeaderActionRequested(action: string) {
    if (action === clearLogAction) {
      this.memorySink.clear();
    }
  }

  protected onRowExpandChange(entry: StoredLogEntry) {
    const mono = 'font-mono text-xs whitespace-pre-wrap';
    const rows: { label: string; value: string; class?: string }[] = [];

    if (entry.suppressed) {
      rows.push({ label: 'Repeated', value: `${entry.suppressed} identical entries dropped before this one` });
    }
    if (entry.error) {
      rows.push({ label: 'Error', value: `${entry.error.name}: ${entry.error.message}` });
      if (entry.error.status !== undefined) {
        rows.push({ label: 'HTTP status', value: String(entry.error.status) });
      }
      if (entry.error.stack) {
        rows.push({ label: 'Stack', value: entry.error.stack, class: mono });
      }
    }
    if (entry.data !== undefined) {
      rows.push({ label: 'Data', value: this.stringify(entry.data), class: mono });
    }
    if (rows.length === 0) {
      rows.push({ label: 'Details', value: 'No additional details.' });
    }

    this.rowExpandResponse.set({
      rowKey: entry,
      colStyles: [
        { width: '110px', 'min-width': '110px', 'vertical-align': 'top' },
        { 'vertical-align': 'top' },
      ],
      details: rows.map((row) => [
        { label: row.label },
        { label: row.value, class: row.class ?? 'whitespace-normal' },
      ]),
    });
  }

  private stringify(data: unknown): string {
    try {
      return JSON.stringify(data, null, 2) ?? String(data);
    } catch {
      return String(data); // e.g. circular references
    }
  }
}
