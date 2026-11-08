import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { KubernetesContextsDto } from '@api/models/kubernetes-contexts-dto';
import { KubernetesContextsService } from '@api/services/kubernetes-contexts.service';
import { LocalStorageUtils } from '@shared/utils/local-storage.utils';
import { RetryPolicy, retryWithBackoff } from '@shared/utils/retry-policy.utils';

@Injectable({
  providedIn: 'root',
})
export class KubernetesContextStateService {
  private readonly kubernetesContextKey = 'settings.selectedKubernetesContext';
  private readonly k8sService = inject(KubernetesContextsService);

  // --- Backend DTO as a signal ---
  // This service is created by kubernetesContextInterceptor on the very first HTTP call, i.e. during app init, when
  // the backend may still be down: retry like the init does. A toSignal over an errored observable throws on every
  // read (and breaks the nav menu), so a non-transient error ends as "no contexts" instead.
  private readonly dto = toSignal(
    this.k8sService.getKubernetesContexts().pipe(
      retryWithBackoff(new RetryPolicy({ initialDelayMs: 500, maxDelayMs: 10_000 })),
      catchError(() => of(null)),
    ),
    { initialValue: null },
  );

  // --- Derived backend values ---
  readonly contexts = computed(() => this.dto()?.contexts ?? []);
  readonly backendCurrent = computed(() => this.dto()?.current ?? null);

  // --- Primary in-memory state ---
  private readonly _selectedContext = signal<string | null>(null);
  readonly selectedContext = computed(() => this._selectedContext());

  constructor() {
    // Initialize once DTO arrives
    effect(() => {
      const dto = this.dto();
      if (!dto) return;

      this.initializeSelectedContext(dto);
    });
  }

  private initializeSelectedContext(dto: KubernetesContextsDto) {
    const contexts = dto.contexts ?? [];
    const backendCurrent = dto.current ?? null;

    // Try localStorage
    const stored = LocalStorageUtils.getItem(this.kubernetesContextKey);

    let chosen: string | null = null;

    if (stored) {
      if (contexts.includes(stored)) {
        chosen = stored;
      } else if (backendCurrent && contexts.includes(backendCurrent)) {
        chosen = backendCurrent;
      }
    } else {
      if (backendCurrent && contexts.includes(backendCurrent)) {
        chosen = backendCurrent;
      }
    }

    this.setSelectedContextInternal(chosen);
  }

  // --- Public API ---
  setSelectedContext(context: string): void {
    const contexts = this.contexts();
    if (!contexts.includes(context)) return;

    this.setSelectedContextInternal(context);
  }

  get selectedContextSync(): string | null {
    return this._selectedContext();
  }

  // --- Internal helpers ---
  private setSelectedContextInternal(value: string | null) {
    this._selectedContext.set(value);

    if (value === null) {
      LocalStorageUtils.removeItem(this.kubernetesContextKey);
    } else {
      LocalStorageUtils.setItem(this.kubernetesContextKey, value);
    }
  }
}
