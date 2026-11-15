import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

import { CveDetailsDialogComponent } from '@shared/ui/cve-details-dialog/cve-details-dialog.component';

@Component({
  selector: 'app-tests',
  imports: [ButtonModule, FormsModule, InputTextModule, CveDetailsDialogComponent],
  templateUrl: './tests.component.html',
  styleUrl: './tests.component.scss',
})
export class TestsComponent {
  protected cveIdInput = '';
  // set only on open - binding the input directly would reload an open dialog on every keystroke
  protected readonly cveDetailsCveId = signal<string | undefined>(undefined);
  protected readonly isCveDetailsDialogVisible = signal(false);

  protected openCveDetailsDialog() {
    const cveId = this.cveIdInput.trim();

    if (cveId) {
      this.cveDetailsCveId.set(cveId);
      this.isCveDetailsDialogVisible.set(true);
    }
  }
}
