import { effect, inject } from '@angular/core';
import { KubernetesContextStateService } from '../../services/kubernetes-context-state.service';

export abstract class TrivyReportsDetailedBase {
  private readonly kubernetesContextService = inject(KubernetesContextStateService);

  constructor() {
    effect(() => {
      const ctx = this.kubernetesContextService.selectedContext();
      this.getTableDataDtos();
    });
  }

  protected abstract getTableDataDtos(): void;
}
