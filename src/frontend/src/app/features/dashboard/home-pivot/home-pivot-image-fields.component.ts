import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';

import { HomeImageHistoryService } from './home-image-history.service';
import { PivotImage } from './home-pivot.types';

// the Image and Registry fields of the Selected box (the image, with the button of its history); n/a when no image is
// selected (the box keeps its size)
// the host adds no box of its own: the fields are laid out by the column they are in
@Component({
  selector: 'app-home-pivot-image-fields',
  standalone: true,
  imports: [ButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'contents' },
  template: `
    @let image = value();
    <div class="flex flex-col gap-0.5">
      <span class="text-xs tod-text-muted-color">Image</span>
      <!-- the button follows the name, with a small gap (not spread to the far edge) -->
      <span class="flex flex-wrap items-center gap-x-1">
        @if (image) {
          <span class="break-all">{{ image.name }}</span>
          <p-button
            class="text-sm tod-button-compact inline-flex"
            icon="pi pi-external-link"
            size="small"
            severity="secondary"
            [text]="true"
            [rounded]="true"
            [ariaLabel]="(target() === 'report' ? 'Open the vulnerability report of ' : 'Open the vulnerability reports history of ') + image.name"
            (onClick)="open(image.digest)"
          />
        } @else {
          <span class="tod-text-muted-color">n/a</span>
        }
      </span>
    </div>
    <div class="flex flex-col gap-0.5">
      <span class="text-xs tod-text-muted-color">Registry</span>
      <span class="break-all" [class.tod-text-muted-color]="!image">{{ image?.registry ?? 'n/a' }}</span>
    </div>
  `,
})
export class HomePivotImageFieldsComponent {
  private readonly imageHistory = inject(HomeImageHistoryService);

  value = input<PivotImage | undefined>();
  // what the button opens: the history of the image, or its vulnerability report
  target = input<'history' | 'report'>('history');

  protected open(digest: string): void {
    if (this.target() === 'report') {
      this.imageHistory.openReport(digest);
    } else {
      this.imageHistory.open(digest);
    }
  }
}
