import { DialogModule } from 'primeng/dialog';
import { TrivyDependencyComponent } from '../trivy-dependency/trivy-dependency.component';
import { Component, computed, input, model } from '@angular/core';

@Component({
  selector: 'app-trivy-dependency-dialog',
  standalone: true,
  imports: [DialogModule, TrivyDependencyComponent],
  templateUrl: './trivy-dependency-dialog.component.html',
})
export class TrivyDependencyDialogComponent {
  readonly isVisible = model(false);

  readonly digest = input<string | undefined>();
  readonly imageNameAndTag = input<string>('N/A');

  readonly title = computed(() => `Dependency Tree for Image ${this.imageNameAndTag()}`);
}
