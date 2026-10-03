import { effect, inject, OnInit } from '@angular/core';
import { RouterEventEmitterService } from '../../services/router-event-emitter.service';
import { TrivyMessageService } from '../../services/trivy-message.service';
import { KubernetesContextStateService } from '../../services/kubernetes-context-state.service';
import { Observable } from 'rxjs';

export abstract class TrivyReportDataPageBase<TData> implements OnInit {
  // data retrieved from the API
  protected dataDtos: TData[] = [];
  protected abstract readonly dataDtosLoader: () => Observable<TData[]>;

  protected isMainTableLoading: boolean = false;

  // injected services for error handling
  private readonly trivyMessageService = inject(TrivyMessageService);
  private readonly routerEventEmitterService = inject(RouterEventEmitterService);
  // injected service for kubernetes context
  private readonly kubernetesContextService = inject(KubernetesContextStateService);

  constructor() {
    // watch for changes in the selected Kubernetes context and refresh the table data accordingly
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

  // data retrieval on component initialization
  ngOnInit(): void {
    this.getTableDataDtos();
  }

  protected getTableDataDtos(): void {
    this.isMainTableLoading = true;

    this.dataDtosLoader().subscribe({
      next: (dtos) => this.onGetDataDtos(dtos),
      error: (err) => this.onError(err),
    });
  }

  protected onGetDataDtos(dtos: TData[]): void {
    this.dataDtos = dtos;
    this.isMainTableLoading = false;
  }

  public onRefreshRequested() {
    this.getTableDataDtos();
  }

  // Handle errors when fetching data
  protected onError(err: any) {
    this.trivyMessageService.pushSimple('Error on getting data.', this.routerEventEmitterService.title(), 'error', err);
    this.isMainTableLoading = false;
  }

  protected showErrorToast(message: string, title: string) {
    this.trivyMessageService.pushSimple(message, title, 'error');
    this.isMainTableLoading = false;
  }
}
