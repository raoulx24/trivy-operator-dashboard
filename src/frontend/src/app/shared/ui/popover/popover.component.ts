import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, signal, TemplateRef } from '@angular/core';

export type PopoverContent = string | TemplateRef<{ $implicit: unknown }>;

@Component({
  selector: 'app-popover',
  imports: [NgTemplateOutlet],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="tod-popover tod-content-border-contrast-color tod-tooltip-shadow"
         [style.width]="width()" [style.max-width]="maxWidth()">
      @let c = content();
      @if (isTemplate(c)) {
        <ng-container [ngTemplateOutlet]="c" [ngTemplateOutletContext]="{ $implicit: context() }" />
      } @else {
        {{ c }}
      }
    </div>
  `,
  styleUrl: './popover.component.css',
})
export class PopoverComponent {
  readonly content = signal<PopoverContent>('');
  readonly context = signal<unknown>(undefined);
  /** Any CSS length (e.g. '20rem'); undefined = fit content. */
  readonly width = signal<string | undefined>(undefined);
  /** Any CSS length; undefined = default cap from the stylesheet (24rem). */
  readonly maxWidth = signal<string | undefined>(undefined);

  protected isTemplate(c: PopoverContent): c is TemplateRef<{ $implicit: unknown }> {
    return typeof c !== 'string';
  }
}
