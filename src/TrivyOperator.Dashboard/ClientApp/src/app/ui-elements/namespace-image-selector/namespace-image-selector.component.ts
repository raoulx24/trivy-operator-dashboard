import { Component, computed, effect, input, model, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';

import { IconComponent } from '../icon/icon.component';
import { NamespacedImageDto } from './namespace-image-selector.types';

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
    IconComponent
  ],
  templateUrl: './namespace-image-selector.component.html',
  styleUrl: './namespace-image-selector.component.scss'
})
export class NamespaceImageSelectorComponent {

  dataDtos = input.required<NamespacedImageDto[] | undefined>();
  disabled = input<boolean>(false);

  selectedImageId = model<string | undefined>();

  namespacePlaceholder = input<string>('Select namespace');
  imagePlaceholder = input<string>('Select image');

  constructor() {
    effect(() => {
      const namespaces = this.activeNamespaces();
      const selectedNs = this.selectedNamespace();

      // Auto-select namespace if only one exists and none selected yet
      if (namespaces.length === 1 && !selectedNs) {
        this.setNamespace(namespaces[0]);
      }

      const images = this.imageDtos();
      const selectedImg = this.selectedImageId();

      // Auto-select image if only one exists and none selected yet
      if (images.length === 1 && !selectedImg) {
        this.setImage(images[0].uid);
      }
    });
  }

  activeNamespaces = computed(() => {
    const dtos = this.dataDtos();
    if (!dtos || dtos.length === 0) return [];

    return Array.from(new Set(dtos.map(x => x.resourceNamespace)))
      .sort((a, b) => (a > b ? 1 : -1));
  });

  selectedNamespace = model<string | undefined>(undefined);

  imageDtos = computed(() => {
    const dtos = this.dataDtos();
    const ns = this.selectedNamespace();

    if (!dtos || !ns) return [];

    return dtos
      .filter(x => x.resourceNamespace === ns)
      .map(x => ({
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
    return list.find(x => x.uid === id);
  });

  setNamespace(ns: string | undefined) {
    this.selectedNamespace.set(ns);

    const images = this.imageDtos();
    const current = this.selectedImageId();

    if (current && images.some(img => img.uid === current)) {
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
