import { DataPageBase } from '../../abstracts/data-page-base';
import { effect, inject } from '@angular/core';
import { KubernetesContextStateService } from '../../services/kubernetes-context-state.service';

export abstract class TrivyReportsDetailedBase extends DataPageBase {
  private readonly kubernetesContextService = inject(KubernetesContextStateService);

  constructor() {
    super();
    effect(() => {
      const ctx = this.kubernetesContextService.selectedContext();
      this.getTableDataDtos();
    });
  }

  protected abstract getTableDataDtos(): void;
}
