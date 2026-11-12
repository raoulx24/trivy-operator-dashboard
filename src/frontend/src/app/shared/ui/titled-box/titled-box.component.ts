import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// a small rectangle with its title on the top border (like the legend of a fieldset); the content is projected
@Component({
  selector: 'app-titled-box',
  standalone: true,
  templateUrl: './titled-box.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must wrap inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class TitledBoxComponent {
  // without a title it is a plain box
  title = input<string>('');
}
