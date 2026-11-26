import { ChangeDetectionStrategy, Component, effect, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';

import { reportTypeLabel } from '../scan-on-demand.constants';

export interface ScanSubmitRequest {
  image: string;
  types: string[];
}

// the image and the report types to scan for; the server validates everything again (registries, allowed types)
@Component({
  selector: 'app-scan-on-demand-form',
  standalone: true,
  imports: [FormsModule, ButtonModule, CheckboxModule, InputTextModule],
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

  image = '';
  selectedTypes: string[] = [];
  readonly localError = signal<string | undefined>(undefined);
  readonly typeLabel = reportTypeLabel;

  constructor() {
    effect(() => {
      this.selectedTypes = [...this.allowedTypes()];
    });
  }

  onInputChange() {
    this.localError.set(undefined);
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
