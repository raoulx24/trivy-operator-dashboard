import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';

import { HomeImageHistoryService } from './home-image-history.service';
import { PivotImage } from './home-pivot.types';

// the Image and Registry fields of the Selected box (the image, with the buttons of its history and/or its report); n/a when no image is
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
      <!-- the buttons follow the name, with a small gap (not spread to the far edge) -->
      <span class="flex flex-wrap items-center gap-x-1">
        @if (image) {
          <span class="break-all">{{ image.name }}</span>
          <!-- the history: with both buttons it has its own icon, so the two can be told apart -->
          @if (target() !== 'report') {
            <p-button
              class="text-sm tod-button-compact inline-flex"
              [icon]="target() === 'both' ? 'pi pi-history' : 'pi pi-external-link'"
              size="small"
              severity="secondary"
              [text]="true"
              [rounded]="true"
              [ariaLabel]="'Open the vulnerability reports history of ' + image.name"
              [attr.title]="'Open the vulnerability reports history'"
              (onClick)="imageHistory.open(image.digest)"
            />
          }
          @if (target() !== 'history') {
            <p-button
              class="text-sm tod-button-compact inline-flex"
              icon="pi pi-external-link"
              size="small"
              severity="secondary"
              [text]="true"
              [rounded]="true"
              [ariaLabel]="'Open the vulnerability report of ' + image.name"
              [attr.title]="'Open the vulnerability report'"
              (onClick)="imageHistory.openReport(image.digest)"
            />
          }
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
  protected readonly imageHistory = inject(HomeImageHistoryService);

  value = input<PivotImage | undefined>();
  // what the buttons open: the history of the image, its vulnerability report, or both (a button each)
  target = input<'history' | 'report' | 'both'>('history');
}
