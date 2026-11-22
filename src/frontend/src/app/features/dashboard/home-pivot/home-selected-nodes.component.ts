import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TagModule } from 'primeng/tag';

import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';

// the package, the image and the namespace selected in the graph of a "where it runs" widget
export interface SelectedPackage {
  name: string;
  severityId: number;
  installed: string;
  fixedVersion: string;
  // the last field of the package, which is not the same in every widget (fix since, fix appeared, distro)
  extra: string;
}

export interface SelectedImage {
  name: string;
  packages: number;
  namespaces: number;
}

export interface SelectedNamespace {
  name: string;
  images: number;
}

// the body of the Selected box of the widgets with a graph of packages, images and namespaces (multi select: one node of
// each kind at most): always the same fields in two rows (the package; the image and the namespace), so the box never
// changes its size; each field is its muted name with the value under it, and a kind that is not selected shows n/a
@Component({
  selector: 'app-home-selected-nodes',
  standalone: true,
  imports: [TagModule, SeverityCssStyleByIdPipe, SeverityNameByIdPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block' },
  template: `
    @let p = package();
    @let i = image();
    @let n = namespace();
    <div class="grid grid-cols-[minmax(0,2fr)_repeat(4,minmax(0,1fr))] items-start gap-x-6 gap-y-2 text-sm">
      <div class="flex flex-col gap-0.5">
        <span class="text-xs tod-text-muted-color">Package</span>
        <span class="break-all" [class.tod-text-muted-color]="!p">{{ p?.name ?? 'n/a' }}</span>
      </div>
      <div class="flex flex-col gap-0.5">
        <span class="text-xs tod-text-muted-color">Severity</span>
        @if (p) {
          <span><p-tag class="text-xs" [rounded]="true" [style]="p.severityId | severityCssStyleById: 1" [value]="p.severityId | severityNameById" /></span>
        } @else {
          <span class="tod-text-muted-color">n/a</span>
        }
      </div>
      <div class="flex flex-col gap-0.5">
        <span class="text-xs tod-text-muted-color">Installed</span>
        <span class="break-all" [class.tod-text-muted-color]="!p">{{ p?.installed ?? 'n/a' }}</span>
      </div>
      <div class="flex flex-col gap-0.5">
        <span class="text-xs tod-text-muted-color">Fixed</span>
        <span class="break-all" [class.tod-text-muted-color]="!p">{{ p?.fixedVersion ?? 'n/a' }}</span>
      </div>
      <div class="flex flex-col gap-0.5">
        <span class="text-xs tod-text-muted-color">{{ extraLabel() }}</span>
        <span class="break-all" [class.tod-text-muted-color]="!p">{{ p?.extra ?? 'n/a' }}</span>
      </div>

      <div class="flex flex-col gap-0.5">
        <span class="text-xs tod-text-muted-color">Image</span>
        <span class="break-all" [class.tod-text-muted-color]="!i">{{ i?.name ?? 'n/a' }}</span>
      </div>
      <div class="flex flex-col gap-0.5">
        <span class="text-xs tod-text-muted-color">Packages</span>
        <span [class.tod-text-muted-color]="!i">{{ i?.packages ?? 'n/a' }}</span>
      </div>
      <div class="flex flex-col gap-0.5">
        <span class="text-xs tod-text-muted-color">Namespaces</span>
        <span [class.tod-text-muted-color]="!i">{{ i?.namespaces ?? 'n/a' }}</span>
      </div>
      <!-- a thin muted line, in the middle of the gap, between the image and the namespace -->
      <div class="flex flex-col gap-0.5 border-l border-[var(--p-content-border-color)] pl-3 -ml-3">
        <span class="text-xs tod-text-muted-color">Namespace</span>
        <span class="break-all" [class.tod-text-muted-color]="!n">{{ n?.name ?? 'n/a' }}</span>
      </div>
      <div class="flex flex-col gap-0.5">
        <span class="text-xs tod-text-muted-color">Images</span>
        <span [class.tod-text-muted-color]="!n">{{ n?.images ?? 'n/a' }}</span>
      </div>
    </div>
  `,
})
export class HomeSelectedNodesComponent {
  package = input<SelectedPackage | null>(null);
  image = input<SelectedImage | null>(null);
  namespace = input<SelectedNamespace | null>(null);
  // the caption of the last field of the package
  extraLabel = input.required<string>();
}
