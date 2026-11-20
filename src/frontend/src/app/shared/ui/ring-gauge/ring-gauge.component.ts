import { ChangeDetectionStrategy, Component, computed, input, model } from '@angular/core';

import { RingGaugeData } from './ring-gauge.types';

// a ring that shows a percent in the middle, with a legend under it; drawn in svg
// - everything comes from the input: data (id, value, color, label, optional centerText), size
// - selectable: a click (or Enter / Space) selects the ring, a second one clears it; the selection is the id of the data,
//   in the selected model, so it is two-way: [(selected)]="id" - with more rings on the same variable only one is selected
// - selected: the ring grows (1.2) and its color is highlighted; hovered: the same, a little less (1.1)
// - the control does not know what the value means
@Component({
  selector: 'app-ring-gauge',
  standalone: true,
  templateUrl: './ring-gauge.component.html',
  styleUrl: './ring-gauge.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RingGaugeComponent {
  data = input.required<RingGaugeData>();
  // px, the side of the ring (without the growth of the selection)
  size = input<number>(80);
  // the thickness of the ring, in percent of the size
  thickness = input<number>(10);

  // the id of the selected ring (the data id), undefined = none; two-way
  selected = model<string | undefined>(undefined);

  protected readonly isSelected = computed(() => this.selected() === this.data().id);

  // viewBox 0 0 100 100
  protected readonly strokeWidth = computed(() => Math.min(Math.max(this.thickness(), 1), 40));
  protected readonly radius = computed(() => 50 - this.strokeWidth() / 2);

  // the circle has pathLength 100, so the dash is the percent itself
  protected readonly percent = computed(() => Math.min(Math.max(this.data().value, 0), 100));
  protected readonly centerText = computed(() => this.data().centerText ?? `${Math.round(this.percent())}%`);

  protected toggle(): void {
    this.selected.set(this.isSelected() ? undefined : this.data().id);
  }
}
