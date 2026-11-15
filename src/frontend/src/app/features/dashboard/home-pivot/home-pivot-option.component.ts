import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TagModule } from 'primeng/tag';

import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityUtils } from '@shared/utils/severity.utils';

import { PivotOption } from './home-pivot.types';

// an entry of the selects: the name (and the severity of a pair), the exploited tag on the right
@Component({
  selector: 'app-home-pivot-option',
  standalone: true,
  imports: [TagModule, SeverityCssStyleByIdPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block w-full' },
  template: `
    @let o = option();
    <div class="flex w-full items-center justify-between gap-2">
      <span class="inline-flex items-center gap-1 break-all">
        {{ o.label }}
        @if (o.note) {
          <span class="text-xs tod-text-muted-color">{{ o.note }}</span>
        }
        @if (o.severityId !== undefined) {
          <p-tag
            class="text-xs"
            [rounded]="true"
            [style]="o.severityId | severityCssStyleById: 1"
            [value]="severityLabel(o.severityId)"
          />
        }
      </span>
      @if (o.isKnownExploited) {
        <p-tag class="text-xs" severity="secondary" [rounded]="true" value="EXPLOIT" />
      }
    </div>
  `,
})
export class HomePivotOptionComponent {
  option = input.required<PivotOption>();

  protected severityLabel(severityId: number): string {
    return SeverityUtils.severityDtos.find((x) => x.id === severityId)?.name ?? String(severityId);
  }
}
