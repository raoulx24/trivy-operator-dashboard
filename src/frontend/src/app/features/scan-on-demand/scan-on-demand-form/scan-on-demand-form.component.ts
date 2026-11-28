import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectButtonModule } from 'primeng/selectbutton';

import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';

import { LocalStorageUtils } from '@shared/utils/local-storage.utils';

import { reportTypeLabel, SUBMIT_FORM_STORAGE_KEY } from '../scan-on-demand.constants';

export interface ScanSubmitRequest {
  image: string;
  types: string[];
}

// the image and the report types to scan for; the server validates everything again (registries, allowed types)
@Component({
  selector: 'app-scan-on-demand-form',
  standalone: true,
  imports: [FormsModule, ButtonModule, InputTextModule, SelectButtonModule, TrivyBoxComponent],
  templateUrl: './scan-on-demand-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScanOnDemandFormComponent {
  // the ceiling from the backend settings; all of them are ticked by default
  allowedTypes = input<string[]>([]);
  // the server's answer to the last submit, shown under the form
  serverError = input<string | undefined>();
  isSubmitting = input<boolean>(false);

  submitted = output<ScanSubmitRequest>();

  image = (LocalStorageUtils.parseObject(LocalStorageUtils.getItem(SUBMIT_FORM_STORAGE_KEY) ?? '')?.image as string | undefined) ?? '';
  selectedTypes: string[] = [];
  readonly localError = signal<string | undefined>(undefined);
  readonly typeOptions = computed(() => this.allowedTypes().map((x) => ({ label: reportTypeLabel(x), value: x })));

  constructor() {
    effect(() => {
      // the remembered types that are still allowed; all of them when none
      const saved = LocalStorageUtils.parseObject(LocalStorageUtils.getItem(SUBMIT_FORM_STORAGE_KEY) ?? '')?.types;
      const remembered = Array.isArray(saved) ? this.allowedTypes().filter((x) => saved.includes(x)) : [];
      this.selectedTypes = remembered.length > 0 ? remembered : [...this.allowedTypes()];
    });
  }

  onInputChange() {
    this.localError.set(undefined);
    LocalStorageUtils.setItem(SUBMIT_FORM_STORAGE_KEY, JSON.stringify({ image: this.image, types: this.selectedTypes }));
  }

  onSubmit() {
    const image = this.image.trim();

    if (!image) {
      this.localError.set('Enter an image, e.g. nginx:1.29');
      return;
    }

    if (this.selectedTypes.length === 0) {
      this.localError.set('Select at least one report type');
      return;
    }

    this.localError.set(undefined);
    this.submitted.emit({ image, types: [...this.selectedTypes] });
  }
}
