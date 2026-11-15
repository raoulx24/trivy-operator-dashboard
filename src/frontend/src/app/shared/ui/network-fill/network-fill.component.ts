import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { buildNetwork } from './network-fill.network';

// a decoration for empty space: a faint network of nodes and links
// the host takes the size its parent gives it (e.g. flex-1) and shows the bottom right part of a fixed size drawing:
// the network keeps its size whatever the space is, and a space of 0 shows nothing
// the seed picks the network (the same seed, the same network on every render)
@Component({
  selector: 'app-network-fill',
  standalone: true,
  templateUrl: './network-fill.component.html',
  styleUrl: './network-fill.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true' },
})
export class NetworkFillComponent {
  seed = input.required<string>();

  protected readonly width = 1200;
  protected readonly height = 600;
  // the gap between a node and its ring
  protected readonly ringGap = 4;

  protected readonly layers = computed(() => buildNetwork(this.seed(), this.width, this.height));
}
