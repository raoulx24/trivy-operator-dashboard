import { ChangeDetectionStrategy, Component } from '@angular/core';

// a small rectangle in sections; every section is projected, and one that gets nothing takes no room
// - [boxTitle]: on the top border, like the legend of a fieldset (text, and e.g. a tag)
// - [boxHeader]: above the body
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
export class TrivyBoxComponent {}
