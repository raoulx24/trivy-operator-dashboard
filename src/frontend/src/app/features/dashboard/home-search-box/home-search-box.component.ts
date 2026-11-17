import { ChangeDetectionStrategy, Component, input, model, output } from '@angular/core';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';

// the search box of the "where is..." widgets: Enter searches (an empty box: the widget resets), the x clears the text
@Component({
  selector: 'app-home-search-box',
  standalone: true,
  imports: [IconFieldModule, InputIconModule, InputTextModule],
  templateUrl: './home-search-box.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeSearchBoxComponent {
  placeholder = input.required<string>();
  query = model<string>('');
  search = output<void>();

  protected onEnter(event: Event): void {
    this.search.emit();
    // on a touch device the Enter of the keyboard would go on to the next box (of the next widget): the keyboard closes
    if (window.matchMedia('(pointer: coarse)').matches) {
      (event.target as HTMLElement).blur();
    }
  }

  protected clear(box: HTMLInputElement): void {
    this.query.set('');
    box.focus();
  }
}
