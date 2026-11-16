import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';

import { buildNetwork } from './network-fill.network';

// the drawing is at least this tall; a taller space makes it taller, in steps (so a small resize does not redraw it)
const MIN_HEIGHT = 600;
const HEIGHT_STEP = 200;

// a decoration for empty space: a faint network of nodes and links
// the host takes the size its parent gives it (e.g. flex-1) and shows the bottom right part of the drawing: the network
// keeps its density whatever the space is, and a space of 0 shows nothing
// the drawing is 1200 px wide and as tall as the space needs (600 px at least), so a tall space (e.g. a cell spanning
// two rows) has no blank part at the top
// the seed picks the network (the same seed, the same network on every render, for the same height)
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
  // the gap between a node and its ring
  protected readonly ringGap = 4;

  private readonly hostHeight = signal(0);

  protected readonly height = computed(() =>
    Math.max(MIN_HEIGHT, Math.ceil(this.hostHeight() / HEIGHT_STEP) * HEIGHT_STEP),
  );

  protected readonly layers = computed(() => buildNetwork(this.seed(), this.width, this.height()));

  constructor() {
    const host = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
    const destroyRef = inject(DestroyRef);

    afterNextRender(() => {
      const observer = new ResizeObserver(() => this.hostHeight.set(Math.floor(host.clientHeight)));
      observer.observe(host);
      destroyRef.onDestroy(() => observer.disconnect());
    });
  }
}
