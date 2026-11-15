import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { PopoverService } from '@shared/ui/popover/popover.service';

import { NamespacesFooter } from './home-pivot.types';

// the footer of a box: the first namespaces, then a pill with how many more; its popover lists the ones that were sent
// one line (cut with an ellipsis): the boxes of a row must have the same height (the virtual row is as tall as its
// tallest rendered box)
@Component({
  selector: 'app-home-namespaces-footer',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block' },
  template: `
    @let box = footer();
    <div class="flex items-center text-sm">
      <span class="truncate min-w-0" [class.tod-text-muted-color]="box.namespaceNames.length === 0">{{
        box.namespaceNames.length === 0 ? 'n/a' : shown().join(', ')
      }}</span>
      @if (box.namespaces > shown().length) {
        <span
          class="tod-counter-pill"
          (mouseenter)="popover.show(namespacesTip, $event, tipData())"
          (mousemove)="popover.move($event)"
          (mouseleave)="popover.hide()"
        >
          +{{ box.namespaces - shown().length }}
        </span>
      }
    </div>

    <ng-template #namespacesTip let-tip>
      <div class="tod-text-contrast-color">
        @for (name of tip.names; track name) {
          <div>{{ name }}</div>
        }
        @if (tip.more > 0) {
          <div class="tod-text-inverted-muted-color">+{{ tip.more }} more</div>
        }
      </div>
    </ng-template>
  `,
})
export class HomeNamespacesFooterComponent {
  // the namespaces named in the footer; the pill counts the others
  private static readonly shownNamespaces = 2;

  protected readonly popover = inject(PopoverService);

  footer = input.required<NamespacesFooter>();

  protected shown(): string[] {
    return this.footer().namespaceNames.slice(0, HomeNamespacesFooterComponent.shownNamespaces);
  }

  // what the popover of the "+N" pill lists: the namespaces that were sent and are not in the footer, and how many
  // were not sent (the server caps the list)
  protected tipData(): { names: string[]; more: number } {
    const box = this.footer();

    return {
      names: box.namespaceNames.slice(HomeNamespacesFooterComponent.shownNamespaces),
      more: Math.max(0, box.namespaces - box.namespaceNames.length),
    };
  }
}
