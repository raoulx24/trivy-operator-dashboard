import { ChangeDetectionStrategy, Component, input, model, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
// the search box of the "where is..." widgets: Enter searches (an empty box: the widget resets), the x clears the text
@Component({
  selector: 'app-home-search-box',
  standalone: true,
  imports: [ButtonModule, IconFieldModule, InputIconModule, InputTextModule],
  templateUrl: './home-search-box.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeSearchBoxComponent {
  placeholder = input.required<string>();
  // shows a raised "Clear" button next to the box that resets the whole widget
  resettable = input(false);
  query = model<string>('');
  search = output<void>();
  // the reset button was pressed (after the search that resets the widget): a widget can collapse, for one
  resetPressed = output<void>();

  protected onEnter(event: Event): void {
    this.search.emit();
    // on a touch device the Enter of the keyboard would go on to the next box (of the next widget): the keyboard closes
    if (window.matchMedia('(pointer: coarse)').matches) {
      (event.target as HTMLElement).blur();
    }
  }

  // the button next to the box: clears the text and the search (an empty search resets the widget)
  protected reset(): void {
    this.query.set('');
    this.search.emit();
    this.resetPressed.emit();
  }

  protected clear(box: HTMLInputElement): void {
    this.query.set('');
    box.focus();
  }
}
