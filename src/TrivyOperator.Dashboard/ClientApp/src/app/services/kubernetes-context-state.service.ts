import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, map, shareReplay } from 'rxjs';
import { KubernetesContextsService } from '../../api/services/kubernetes-contexts.service';
import { KubernetesContextsDto } from '../../api/models/kubernetes-contexts-dto';

@Injectable({
  providedIn: 'root',
})
export class KubernetesContextStateService {
  private readonly STORAGE_KEY = 'selectedKubernetesContext';

  private readonly k8sService = inject(KubernetesContextsService);

  // --- Backend-loaded values ---
  readonly contexts$ = this.k8sService.getKubernetesContexts().pipe(
    map((dto: KubernetesContextsDto[]) => dto[0]?.contexts ?? []),
    shareReplay(1)
  );

  readonly current$ = this.k8sService.getKubernetesContexts().pipe(
    map((dto: KubernetesContextsDto[]) => dto[0]?.current ?? null),
    shareReplay(1)
  );

  // --- User-selected context (bidirectional) ---
  private selectedContextSubject = new BehaviorSubject<string | null>(
    localStorage.getItem(this.STORAGE_KEY)
  );

  readonly selectedContext$ = this.selectedContextSubject.asObservable();

  /** Called by UI when user selects a context */
  setSelectedContext(context: string): void {
    this.selectedContextSubject.next(context);
    localStorage.setItem(this.STORAGE_KEY, context);
  }

  /** Used by interceptor to get the latest value synchronously */
  get selectedContext(): string | null {
    return this.selectedContextSubject.value;
  }
}
