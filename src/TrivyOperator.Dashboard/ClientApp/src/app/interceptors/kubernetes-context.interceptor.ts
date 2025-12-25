import { inject, Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { KubernetesContextStateService } from '../services/kubernetes-context-state.service';

@Injectable()
export class KubernetesContextInterceptor implements HttpInterceptor {
  private readonly state = inject(KubernetesContextStateService);

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const context = this.state.selectedContext;

    if (!context) {
      return next.handle(req);
    }

    const modified = req.clone({
      setHeaders: {
        'X-Kubernetes-Context': context,
      },
    });

    return next.handle(modified);
  }
}
