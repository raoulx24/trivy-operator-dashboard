import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { KubernetesContextsDto } from '../../api/models/kubernetes-contexts-dto';
import { KubernetesContextsService } from '../../api/services/kubernetes-contexts.service';

@Injectable({
  providedIn: 'root',
})
export class KubernetesContextStateService {
  private readonly kubernetesContextKey = 'settings.selectedKubernetesContext';
  private readonly k8sService = inject(KubernetesContextsService);

  // --- Backend DTO as a signal ---
  private readonly dto = toSignal(this.k8sService.getKubernetesContexts(), { initialValue: null });

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
    const stored = this.safeGetFromLocalStorage(this.kubernetesContextKey);

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
      this.safeRemoveFromLocalStorage(this.kubernetesContextKey);
    } else {
      this.safeSetToLocalStorage(this.kubernetesContextKey, value);
    }
  }

  private safeGetFromLocalStorage(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  private safeSetToLocalStorage(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {}
  }

  private safeRemoveFromLocalStorage(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch {}
  }
}
