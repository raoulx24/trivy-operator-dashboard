import { ChangeDetectionStrategy, Component, computed, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';

// what the filters need of the detail of an id (the details of the three widgets have more)
export interface NodeFiltersDetail {
  packages: { packageName: string; imageIndexes: number[] }[];
  imageNames: string[];
  // per image, the indexes of its namespaces
  imageNamespaceIndexes: number[][];
  namespaceNames: string[];
}

interface NodeOption {
  label: string;
  // the id of the node in the graph: 'p:2', 'i:0', 'n:1'
  value: string;
}

// the three selects (package, image, namespace) of a "where it runs" widget, interconnected: each one offers only what
// fits the other two. They are the selection of the graph (selectedNodes, the ids of its nodes), so a pick in a select
// selects the node and a click on a node sets the select
@Component({
  selector: 'app-home-node-filters',
  standalone: true,
  imports: [FormsModule, SelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'contents' },
  template: `
    <p-select
      size="small"
      class="tod-home-filter-select w-56"
      placeholder="Select a package"
      optionLabel="label"
      optionValue="value"
      filterBy="label"
      appendTo="body"
      panelStyleClass="tod-home-filter-panel"
      [filter]="true"
      [showClear]="true"
      [options]="packageOptions()"
      [ngModel]="selectedOf('p:')"
      (onChange)="pick('p:', $event.value)"
    />
    <p-select
      size="small"
      class="tod-home-filter-select w-72"
      placeholder="Select an image"
      optionLabel="label"
      optionValue="value"
      filterBy="label"
      appendTo="body"
      panelStyleClass="tod-home-filter-panel"
      [filter]="true"
      [showClear]="true"
      [options]="imageOptions()"
      [ngModel]="selectedOf('i:')"
      (onChange)="pick('i:', $event.value)"
    />
    <p-select
      size="small"
      class="tod-home-filter-select w-56"
      placeholder="Select a namespace"
      optionLabel="label"
      optionValue="value"
      filterBy="label"
      appendTo="body"
      panelStyleClass="tod-home-filter-panel"
      [filter]="true"
      [showClear]="true"
      [options]="namespaceOptions()"
      [ngModel]="selectedOf('n:')"
      (onChange)="pick('n:', $event.value)"
    />
  `,
})
export class HomeNodeFiltersComponent {
  detail = input.required<NodeFiltersDetail>();
  selectedNodes = model<string[]>([]);

  // the index of the selected node of a kind, or null
  private index(prefix: string): number | null {
    const id = this.selectedNodes().find((x) => x.startsWith(prefix));

    return id === undefined ? null : Number(id.substring(2));
  }

  protected selectedOf(prefix: string): string | null {
    return this.selectedNodes().find((x) => x.startsWith(prefix)) ?? null;
  }

  protected readonly packageOptions = computed<NodeOption[]>(() => {
    const d = this.detail();
    const image = this.index('i:');
    const namespace = this.index('n:');

    return d.packages
      .map((p, i) => ({ p, i }))
      .filter(({ p }) => {
        const images = image === null ? p.imageIndexes : p.imageIndexes.filter((x) => x === image);

        return images.some((x) => namespace === null || (d.imageNamespaceIndexes[x]?.includes(namespace) ?? false));
      })
      .map(({ p, i }) => ({ label: p.packageName, value: `p:${i}` }))
      .sort((a, b) => a.label.localeCompare(b.label));
  });

  protected readonly imageOptions = computed<NodeOption[]>(() => {
    const d = this.detail();
    const pack = this.index('p:');
    const namespace = this.index('n:');

    return d.imageNames
      .map((name, i) => ({ name, i }))
      .filter(
        ({ i }) =>
          (pack === null || (d.packages[pack]?.imageIndexes.includes(i) ?? false)) &&
          (namespace === null || (d.imageNamespaceIndexes[i]?.includes(namespace) ?? false)),
      )
      .map(({ name, i }) => ({ label: name, value: `i:${i}` }))
      .sort((a, b) => a.label.localeCompare(b.label));
  });

  protected readonly namespaceOptions = computed<NodeOption[]>(() => {
    const d = this.detail();
    const pack = this.index('p:');
    const image = this.index('i:');
    const packageImages = pack === null ? null : (d.packages[pack]?.imageIndexes ?? []);

    return d.namespaceNames
      .map((name, n) => ({ name, n }))
      .filter(({ n }) =>
        d.imageNamespaceIndexes.some(
          (list, i) =>
            list.includes(n) && (image === null || image === i) && (packageImages === null || packageImages.includes(i)),
        ),
      )
      .map(({ name, n }) => ({ label: name, value: `n:${n}` }))
      .sort((a, b) => a.label.localeCompare(b.label));
  });

  // a pick (or a clear, null) sets the node of its kind and keeps the other two
  protected pick(prefix: string, id: string | null): void {
    const others = this.selectedNodes().filter((x) => !x.startsWith(prefix));
    this.selectedNodes.set(id ? [...others, id] : others);
  }
}
