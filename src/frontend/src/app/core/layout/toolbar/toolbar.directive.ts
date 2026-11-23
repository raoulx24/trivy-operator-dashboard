import { afterNextRender, DestroyRef, Directive, inject, TemplateRef } from '@angular/core';

import { ToolbarService } from './toolbar.service';

// <ng-template appToolbar> ... </ng-template> in a page: its content is shown in the nav bar while the page is alive.
// Registered after the render (the nav bar was already checked in this pass), removed when the page is destroyed.
@Directive({
  selector: 'ng-template[appToolbar]',
  standalone: true,
})
export class ToolbarDirective {
  constructor() {
    const template = inject<TemplateRef<unknown>>(TemplateRef);
    const toolbar = inject(ToolbarService);

    afterNextRender(() => toolbar.register(template));
    inject(DestroyRef).onDestroy(() => toolbar.unregister(template));
  }
}
