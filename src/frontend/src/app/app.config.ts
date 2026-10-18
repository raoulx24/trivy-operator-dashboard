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
import { provideApiConfiguration } from "../api/api-configuration";
import { environment } from '../environments/environment';
import { routes } from './app.routes';
import { kubernetesContextInterceptor } from './interceptors/kubernetes-context.interceptor';
import { DarkModeService } from './services/dark-mode.service';
import { provideLogging } from './services/logging/provide-logging';
import { MainAppInitService, initializeAppFactory } from './services/main-app-init.service';
import { trivyOperatorDashboardPreset } from './themes/trivy-operator-dashboard.preset';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([kubernetesContextInterceptor])),
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
