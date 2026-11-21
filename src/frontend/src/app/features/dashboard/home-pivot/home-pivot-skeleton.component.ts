import { ChangeDetectionStrategy, Component } from '@angular/core';
import { SkeletonModule } from 'primeng/skeleton';

import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';

// the rows of a pivot widget while they are asked: two rows of boxes, with the size of the real ones
@Component({
  selector: 'app-home-pivot-skeleton',
  standalone: true,
  imports: [TrivyBoxComponent, SkeletonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'contents' },
  template: `
    @for (row of rows; track row) {
      <section class="flex flex-col">
        <div class="flex h-4 items-center">
          <p-skeleton width="24rem" height="0.75rem" />
        </div>
        <!-- a plain row, clipped at the right edge: the skeleton does not scroll -->
        <div class="flex gap-4 overflow-hidden pt-3">
          @for (box of boxes; track box) {
            <app-trivy-box class="w-80 shrink-0 mt-2">
              <p-skeleton boxTitle width="9rem" height="1rem" />

              <div class="flex h-[12.8rem] flex-col gap-3 overflow-hidden">
                <p-skeleton width="45%" height="0.75rem" />
                @for (line of lines; track line) {
                  <p-skeleton height="1.25rem" />
                }
              </div>

              <div boxFooter><p-skeleton width="55%" height="1.25rem" /></div>
            </app-trivy-box>
          }
        </div>
      </section>
    }
  `,
})
export class HomePivotSkeletonComponent {
  protected readonly rows = [0, 1];
  protected readonly boxes = [0, 1, 2, 3];
  protected readonly lines = [0, 1, 2, 3, 4, 5];
}
