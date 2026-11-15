import { ChangeDetectionStrategy, Component } from '@angular/core';
import { SkeletonModule } from 'primeng/skeleton';

import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';

// the Selected box while the rows are asked
@Component({
  selector: 'app-home-pivot-selected-skeleton',
  standalone: true,
  imports: [TrivyBoxComponent, SkeletonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'flex flex-1 min-w-[24rem]' },
  template: `
    <app-trivy-box class="flex-1">
      <span boxTitle>Selected</span>
      <div class="grid grid-cols-[minmax(14rem,2fr)_minmax(0,3fr)] gap-x-8">
        <div class="flex flex-col gap-3">
          <p-skeleton width="70%" height="1.25rem" />
          <p-skeleton width="50%" height="1.25rem" />
        </div>
        <div class="flex flex-col gap-3">
          <p-skeleton width="60%" height="1.25rem" />
          <p-skeleton height="5rem" />
        </div>
      </div>
    </app-trivy-box>
  `,
})
export class HomePivotSelectedSkeletonComponent {}
