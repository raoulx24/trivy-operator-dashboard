import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';
import { Title } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';

import { MessageService } from 'primeng/api';
import { providePrimeNG } from 'primeng/config';
import { provideApiConfiguration } from "@api/api-configuration";
import { environment } from '../environments/environment';
import { routes } from './app.routes';
import { httpErrorLoggingInterceptor } from '@core/interceptors/http-error-logging.interceptor';
import { kubernetesContextInterceptor } from '@core/interceptors/kubernetes-context.interceptor';
import { DarkModeService } from '@core/services/dark-mode.service';
import { provideLogging } from '@core/logging/provide-logging';
import { MainAppInitService, initializeAppFactory } from '@core/services/main-app-init.service';
import { trivyOperatorDashboardPreset } from './themes/trivy-operator-dashboard.preset';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([httpErrorLoggingInterceptor, kubernetesContextInterceptor])),
    provideRouter(routes),
    provideApiConfiguration(environment.baseUrl),
    MainAppInitService,
    Title,
    MessageService,
    provideLogging(),
    provideAppInitializer(() => initializeAppFactory(inject(MainAppInitService))),
    providePrimeNG({
      ripple: true,
      theme: {
        preset: trivyOperatorDashboardPreset,
        options: {
          darkModeSelector: `.${DarkModeService.DARK_MODE_SELECTOR}`,
          cssLayer: {
            name: 'primeng',
            order: 'tailwind, primeng, app-layer',
          },
        },
      },
    }),
  ],
};
