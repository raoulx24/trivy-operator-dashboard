import { effect, inject } from '@angular/core';
import { DataPageBase } from '../../abstracts/data-page-base';
import { KubernetesContextStateService } from '../../services/kubernetes-context-state.service';

export abstract class TrivyReportsDetailedBase extends DataPageBase {
  private readonly kubernetesContextService = inject(KubernetesContextStateService);

  constructor() {
    super();

    let initialized = false;

    effect(() => {
      const ctx = this.kubernetesContextService.selectedContext();

      if (!initialized) {
        initialized = true;
        return; // skip initial run
      }
      this.getTableDataDtos();
    });
  }

  protected abstract getTableDataDtos(): void;
}
