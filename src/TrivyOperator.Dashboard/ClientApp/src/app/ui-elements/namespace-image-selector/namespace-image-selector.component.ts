import { ChangeDetectionStrategy, Component, computed, effect, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';

import { IconComponent } from '../icon/icon.component';
import { NamespacedImageDto } from './namespace-image-selector.types';
import { NgClass } from '@angular/common';

interface ImageDto {
  uid: string;
  group?: string;
  mainLabel: string;
  icon?: string;
}

export const nonExistingNamespace = 'N/A';

@Component({
  selector: 'app-namespace-image-selector',
  imports: [
    FormsModule,
    SelectModule,
    TagModule,
    IconComponent,
    NgClass,
  ],
  templateUrl: './namespace-image-selector.component.html',
  styleUrl: './namespace-image-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NamespaceImageSelectorComponent {
  dataDtos = input.required<NamespacedImageDto[] | undefined>();
  disabled = input<boolean>(false);

  selectedImageId = model<string | undefined>();

  namespacePlaceholder = input<string>('Select namespace');
  imagePlaceholder = input<string>('Select image');
  firstLonger = input<boolean>(false);

  private initialImageIdHandled = false;

  constructor() {
    effect(() => {
      const dtos = this.dataDtos();
      const namespaces = this.activeNamespaces();
      const selectedNs = this.selectedNamespace();
      const images = this.imageDtos();
      const selectedImg = this.selectedImageId();

      // --- RULE 4: Reset when datasource is cleared ---
      if (!dtos || dtos.length === 0) {
        this.initialImageIdHandled = false;
        this.selectedNamespace.set(undefined);
        this.selectedImageId.set(undefined);
        return;
      }

      // --- RULE 1: Parent-provided selectedImageId wins (only once) ---
      if (!this.initialImageIdHandled && selectedImg) {
        this.initialImageIdHandled = true;

        const dto = dtos.find((x) => x.uid === selectedImg);
        if (dto) {
          this.selectedNamespace.set(dto.resourceNamespace);
        }
        return;
      }

      // --- RULE 2: Auto-select namespace if only one and none selected ---
      if (namespaces.length === 1 && !selectedNs) {
        this.selectedNamespace.set(namespaces[0]);
        return;
      }

      // --- RULE 3: Auto-select image if only one and none selected ---
      if (images.length === 1 && !selectedImg) {
        this.selectedImageId.set(images[0].uid);
        return;
      }
    });
  }

  activeNamespaces = computed(() => {
    const dtos = this.dataDtos();
    if (!dtos || dtos.length === 0) return [];

    return Array.from(new Set(dtos.map((x) => x.resourceNamespace))).sort((a, b) => (a > b ? 1 : -1));
  });

  selectedNamespace = model<string | undefined>(undefined);

  imageDtos = computed(() => {
    const dtos = this.dataDtos();
    const ns = this.selectedNamespace();

    if (!dtos || !ns) return [];

    return dtos
      .filter((x) => x.resourceNamespace === ns)
      .map((x) => ({
        uid: x.uid ?? '',
        mainLabel: x.mainLabel,
        group: x.group,
        icon: x.icon,
      }))
      .sort((a, b) => {
        const gA = a.group ?? '';
        const gB = b.group ?? '';
        if (gA !== gB) return gA < gB ? -1 : 1;
        return a.mainLabel < b.mainLabel ? -1 : 1;
      });
  });

  selectedImageDto = computed(() => {
    const id = this.selectedImageId();
    const list = this.imageDtos();
    if (!id || !list) return undefined;
    return list.find((x) => x.uid === id);
  });

  setNamespace(ns: string | undefined) {
    this.selectedNamespace.set(ns);

    const images = this.imageDtos();
    const current = this.selectedImageId();

    if (current && images.some((img) => img.uid === current)) {
      return;
    }

    if (images.length === 1) {
      this.selectedImageId.set(images[0].uid);
      return;
    }

    this.selectedImageId.set(undefined);
  }

  setImage(id: string | undefined) {
    this.selectedImageId.set(id);
  }
}
