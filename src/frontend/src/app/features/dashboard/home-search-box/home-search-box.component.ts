import { ChangeDetectionStrategy, Component, input, model, output } from '@angular/core';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';

// the search box of the "where is..." widgets: Enter searches (an empty box: the widget resets), the x clears the text;
// when resettable, the x (and Esc) clear everything: the text, and the results and the selection of the widget
@Component({
  selector: 'app-home-search-box',
  standalone: true,
  imports: [IconFieldModule, InputIconModule, InputTextModule],
  templateUrl: './home-search-box.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeSearchBoxComponent {
  placeholder = input.required<string>();
  // the x and Esc reset the whole widget, not just the text
  resettable = input(false);
  query = model<string>('');
  search = output<void>();
  // everything was cleared (after the search that resets the widget): a widget can collapse, for one
  resetPressed = output<void>();

  protected onEnter(event: Event): void {
    this.search.emit();
    // on a touch device the Enter of the keyboard would go on to the next box (of the next widget): the keyboard closes
    if (window.matchMedia('(pointer: coarse)').matches) {
      (event.target as HTMLElement).blur();
    }
  }

  protected onEscape(): void {
    if (this.resettable() && this.query()) {
      this.reset();
    }
  }

  protected clear(box: HTMLInputElement): void {
    if (this.resettable()) {
      this.reset();
    } else {
      this.query.set('');
    }
    box.focus();
  }

  // an empty box and a search: the widget goes back to before the first search
  private reset(): void {
    this.query.set('');
    this.search.emit();
    this.resetPressed.emit();
  }
}
