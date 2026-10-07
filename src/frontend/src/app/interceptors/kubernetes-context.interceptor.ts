import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { KubernetesContextStateService } from '../services/kubernetes-context-state.service';

export const kubernetesContextInterceptor: HttpInterceptorFn = (req, next) => {
  try {
    // Optional injection prevents bootstrap failures
    const state = inject(KubernetesContextStateService, { optional: true });
    const context = state?.selectedContextSync;

    // const isApiRequest = req.url.startsWith('api/');
    const isApiRequest = true;

    if (!isApiRequest || !context) {
      return next(req);
    }

    const modified = req.clone({
      setHeaders: {
        'X-Kubernetes-Context': context,
      },
    });

    return next(modified);
  } catch {
    // Prevent bootstrap from breaking
    return next(req);
  }
};
