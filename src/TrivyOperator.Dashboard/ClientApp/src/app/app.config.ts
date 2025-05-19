import { provideHttpClient } from "@angular/common/http";
import { ApplicationConfig, importProvidersFrom, provideAppInitializer, inject } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { provideAnimationsAsync } from "@angular/platform-browser/animations/async";
import { provideRouter } from "@angular/router";

import { MessageService } from "primeng/api";
import { providePrimeNG } from "primeng/config";
import { ApiModule } from "../api/api.module";
import { environment } from "../environments/environment";
import { routes } from "./app.routes";
import { DarkModeService } from "./services/dark-mode.service";
import { MainAppInitService, initializeAppFactory } from "./services/main-app-init.service";
import { trivyOperatorDashboardPreset } from "./themes/trivy-operator-dashboard.preset";

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    provideAnimationsAsync(),
    importProvidersFrom(ApiModule.forRoot({ rootUrl: environment.baseUrl })),
    MainAppInitService,
    Title,
    MessageService,
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
        }
      }
    }),
  ],
};
