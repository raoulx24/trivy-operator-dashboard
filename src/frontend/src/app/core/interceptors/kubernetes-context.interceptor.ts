import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { KubernetesContextsService } from '@api/services/kubernetes-contexts.service';
import { KubernetesContextStateService } from '../state/kubernetes-context-state.service';

export const kubernetesContextInterceptor: HttpInterceptorFn = (req, next) => {
  // the contexts list is what the selected context is chosen from, so it never carries one. Checking it before the
  // inject also avoids a circular injection: KubernetesContextStateService sends this request from its constructor.
  // endsWith because the generated client builds the URL as rootUrl + path; a query string on this call would break
  // the match (it has none today).
  if (req.url.endsWith(KubernetesContextsService.GetKubernetesContextsPath)) {
    return next(req);
  }

  const context = inject(KubernetesContextStateService).selectedContextSync;
  if (!context) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { 'X-Kubernetes-Context': context } }));
};
