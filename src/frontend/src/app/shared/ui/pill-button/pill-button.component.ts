import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

// the pill button (.tod-pill-button, the look of the CVSS / EPSS tag): outlined in the color of the text around it, small,
// raised at rest. One look for the small actions and toggles of the home page.
// - label, and optionally a dot (dotColor: any css color) before it and an icon (a pi class, e.g. 'pi-arrow-down') after it
// - toggle: a button that is on or off (active, aria-pressed); without it a plain action
// - activeColor (and activeTextColor): the fill while it is on, e.g. a severity color (the dot is then hidden, it would not show)
// - the click and the Enter and Space keys stop here, so they do not (de)select the row or list item the pill is in
@Component({
  selector: 'app-pill-button',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'inline-flex' },
  template: `
    <button
      type="button"
      class="tod-pill-button"
      [class.tod-pill-button-active]="active()"
      [class.tod-pill-button-disabled]="disabled()"
      [disabled]="disabled()"
      [attr.aria-pressed]="toggle() ? active() : null"
      [attr.title]="title() || null"
      [style.background]="filledColor()"
      [style.border-color]="filledColor()"
      [style.color]="filledColor() ? activeTextColor() : null"
      (click)="onClick($event)"
      (keydown.enter)="$event.stopPropagation()"
      (keydown.space)="$event.stopPropagation()"
    >
      @if (dotColor() && !filledColor()) {
        <span class="tod-state-dot" [style.background]="dotColor()"></span>
      }
      <span>{{ label() }}</span>
      @if (icon()) {
        <i class="pi" [class]="icon()" aria-hidden="true"></i>
      }
    </button>
  `,
})
export class PillButtonComponent {
  readonly label = input.required<string>();
  // a pi class: 'pi-arrow-down'
  readonly icon = input<string | undefined>(undefined);
  // any css color
  readonly dotColor = input<string | undefined>(undefined);
  readonly title = input<string>('');
  // a button that is on or off
  readonly toggle = input<boolean>(false);
  readonly active = input<boolean>(false);
  readonly disabled = input<boolean>(false);
  // the color of the pill while it is active (any css color, e.g. the severity), and the text on it; without it the active
  // pill has the tint of the theme
  readonly activeColor = input<string | undefined>(undefined);
  readonly activeTextColor = input<string | undefined>(undefined);

  readonly pressed = output<void>();

  // the fill, while the pill is on and has a color of its own
  protected readonly filledColor = computed(() => (this.active() ? this.activeColor() || null : null));

  protected onClick(event: Event): void {
    event.stopPropagation();
    this.pressed.emit();
  }
}
