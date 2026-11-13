import { Directive, computed, input } from '@angular/core';

// marks something in a widget that should move on its own when the widget changes with HomeWidgetBase.animateItems
// (a box, a row): the key names it for the view transition, so the browser matches it before and after the change
// - the key must be unique on the page, so start it with the widget, e.g. "base-os-box-debian-12"
// - the name is applied only while an items transition runs (styles.scss, .tod-vt-item), so it costs nothing otherwise
// - tod-vt-enter, on something that appears (animate.enter), is its fade-in when no transition runs
@Directive({
  selector: '[appHomeVtItem]',
  standalone: true,
  host: {
    class: 'tod-vt-item',
    '[style.--tod-vt-name]': 'name()',
  },
})
export class HomeVtItemDirective {
  appHomeVtItem = input.required<string>();

  protected readonly name = computed(() => `vt-${this.appHomeVtItem().replace(/[^a-zA-Z0-9_-]/g, '-')}`);
}
