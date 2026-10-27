import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

import { NvdDialogService } from '@shared/ui/nvd-cve-dialog/nvd-dialog.service';

@Component({
  selector: 'app-tests',
  imports: [ButtonModule, FormsModule, InputTextModule],
  templateUrl: './tests.component.html',
  styleUrl: './tests.component.scss',
})
export class TestsComponent {
  protected readonly nvdDialog = inject(NvdDialogService);

  protected cveId = '';
}
