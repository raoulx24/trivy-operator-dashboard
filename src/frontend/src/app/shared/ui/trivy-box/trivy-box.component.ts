import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// a small rectangle in sections; every section is projected, and one that gets nothing takes no room
// - [boxTitle]: on the top border, like the legend of a fieldset (text, and e.g. a tag)
// - [boxTitleEnd]: the same, on the right of the top border
// - [boxHeader]: above the body
// - hasBackground: the box is filled with the content background
// - isCompact: less padding
// - the rest (no attribute): the body
// - [boxFooter]: below the body, after a thin line
@Component({
  selector: 'app-trivy-box',
  standalone: true,
  templateUrl: './trivy-box.component.html',
  styleUrl: './trivy-box.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must wrap inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class TrivyBoxComponent {
  // fills the box with the content background (a header box that stands out of the page)
  hasBackground = input<boolean>(false);
  // less padding (a header strip)
  isCompact = input<boolean>(false);
}
