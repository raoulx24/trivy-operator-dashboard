import { ChangeDetectionStrategy, Component, computed, effect, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';

import { IconComponent } from '../icon/icon.component';
import { PairedOptionsDto } from './paired-options-selector.types';
import { NgClass } from '@angular/common';

export const nonExistingNamespace = 'N/A';

@Component({
  selector: 'app-paired-options-selector',
  imports: [
    FormsModule,
    SelectModule,
    TagModule,
    IconComponent,
    NgClass,
  ],
  templateUrl: './paired-options-selector.component.html',
  styleUrl: './paired-options-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PairedOptionsSelectorComponent {
  dataDtos = input.required<PairedOptionsDto[] | undefined>();
  disabled = input<boolean>(false);

  selectedUid = model<string | undefined>();

  firstOptionPlaceholder = input<string>('Select namespace');
  secondOptionPlaceholder = input<string>('Select image');
  firstOptionWider = input<boolean>(false);

  // private initialImageIdHandled = false;
  private internallySelectedUid?: string;

  constructor() {
    effect(() => {
      const dtos = this.dataDtos();
      const firstUniqueOptions = this.firstUniqueOptions();
      const selectedFirstOption = this.selectedFirstOption();
      const filteredPairedOptions = this.filteredPairedOptions();
      const selectedUid = this.selectedUid();

      // --- RULE 1: Reset when datasource is cleared ---
      if (!dtos || dtos.length === 0) {
        this.internallySelectedUid = undefined;
        this.selectedFirstOption.set(undefined);
        this.setSelectedUid(undefined);
        return;
      }

      // --- RULE 2: Parent-provided selectedUid wins ---
      if (this.internallySelectedUid !== selectedUid && selectedUid) {
        const dto = dtos.find((x) => x.uid === selectedUid);

        if (dto) {
          this.selectedFirstOption.set(dto.firstOption);
          return;
        }

        // Selected UID no longer exists in the datasource
        this.internallySelectedUid = undefined;
        this.setSelectedUid(undefined);
        return;
      }

      // --- RULE 3: Auto-select first option if only one exists ---
      if (firstUniqueOptions.length === 1 && !selectedFirstOption) {
        this.selectedFirstOption.set(firstUniqueOptions[0]);
        return;
      }

      // --- RULE 4: Auto-select second option if only one exists ---
      if (filteredPairedOptions.length === 1 && !selectedUid) {
        this.setSelectedUid(filteredPairedOptions[0].uid);
        return;
      }
    });
  }


  firstUniqueOptions = computed(() => {
    const dtos = this.dataDtos();
    if (!dtos || dtos.length === 0) return [];

    return Array.from(new Set(dtos.map((x) => x.firstOption))).sort((a, b) => (a > b ? 1 : -1));
  });

  selectedFirstOption = model<string | undefined>(undefined);

  filteredPairedOptions = computed(() => {
    const dtos = this.dataDtos();
    const selectedFirstOption = this.selectedFirstOption();

    if (!dtos || !selectedFirstOption) return [];

    return dtos
      .filter((x) => x.firstOption === selectedFirstOption)
      .sort((a, b) => {
        const gA = a.group ?? '';
        const gB = b.group ?? '';
        if (gA !== gB) return gA < gB ? -1 : 1;
        return a.secondOption < b.secondOption ? -1 : 1;
      });
  });

  selectedDto = computed(() => {
    const uid = this.selectedUid();
    if (!uid) return undefined;

    return this.dataDtos()?.find((x) => x.uid === uid);
  });

  setFirstOption(value: string | undefined) {
    this.selectedFirstOption.set(value);

    const options = this.filteredPairedOptions();
    const currentUid = this.selectedUid();

    if (currentUid && options.some((x) => x.uid === currentUid)) {
      return;
    }

    if (options.length === 1) {
      this.setSelectedUid(options[0].uid);
      return;
    }

    this.setSelectedUid(undefined);
  }

  setUid(uid: string | undefined) {
    this.setSelectedUid(uid);
  }

  private setSelectedUid(uid: string | undefined) {
    this.internallySelectedUid = uid;
    this.selectedUid.set(uid);
  }
}
