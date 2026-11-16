import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';

import { NetworkFillComponent } from '../../../shared/ui/network-fill/network-fill.component';

import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// the frame of a home widget: border, heading with its hint, the status line and the "..." button that expands the
// widget (shown when canExpand); the widget itself is the projected content
// the widget class extends HomeWidgetBase and binds canExpand / expanded / expandToggle to it
// the same rule for every widget:
// - collapsed: the top row is the heading (left) and the hint (right), the bottom row the status (left) and the button
// the top row ends with the refresh button (when refreshable), after the hint
// - expanded: the top row is the heading (left), the status (center, muted) and the hint (right); the bottom row the
//   controls of the widget (the projected content with the attribute frameActions, left) and the button
@Component({
  selector: 'app-home-widget-frame',
  standalone: true,
  imports: [ButtonModule, DialogModule, NetworkFillComponent, NgTemplateOutlet],
  templateUrl: './home-widget-frame.component.html',
  styleUrl: './home-widget-frame.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeWidgetFrameComponent {
  heading = input.required<string>();
  hint = input<string>('');
  state = input.required<HomeWidgetState>();
  canExpand = input(false);
  expanded = input(false);

  // the widget can refresh itself: a button after the hint (in the top row); the widget reloads only its own data
  refreshable = input(false);

  // the explanation of the widget, as HTML (Angular sanitizes it: no scripts, no style attributes); a "?" button after the
  // heading opens it in a modal, movable, resizable dialog (a click outside closes it); no help text, no button
  help = input<string>('');
  protected readonly isHelpVisible = signal(false);

  // the empty space under the content (when the row is taller than the widget) gets a faint network drawing; false for a widget
  // whose content already grows (flex-1): the two would share the space
  fillGap = input(true);

  // a new drawing every time the widget is created (e.g. on every visit of the home page); taken once, not in the
  // template: there it would change on every change detection, and the drawing with it
  private readonly createdAt = new Date().toISOString();
  protected readonly fillSeed = computed(() => `${this.heading()} ${this.createdAt}`);

  expandToggle = output<void>();
  refreshRequested = output<void>();

  // no answer yet (the status line says "Loading..."): the refresh icon spins
  protected readonly isLoading = computed(() => {
    const state = this.state();
    return state.isAvailable === undefined && !state.error && !state.idleText;
  });
}
