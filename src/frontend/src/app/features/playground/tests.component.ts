import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

import { NvdCveDialogComponent } from '@shared/ui/nvd-cve-dialog/nvd-cve-dialog.component';

@Component({
  selector: 'app-tests',
  imports: [ButtonModule, FormsModule, InputTextModule, NvdCveDialogComponent],
  templateUrl: './tests.component.html',
  styleUrl: './tests.component.scss',
})
export class TestsComponent {
  protected cveIdInput = '';
  // set only on open - binding the input directly would reload an open dialog on every keystroke
  protected readonly nvdCveId = signal<string | undefined>(undefined);
  protected readonly isNvdDialogVisible = signal(false);

  protected openNvdDialog() {
    const cveId = this.cveIdInput.trim();

    if (cveId) {
      this.nvdCveId.set(cveId);
      this.isNvdDialogVisible.set(true);
    }
  }
}
