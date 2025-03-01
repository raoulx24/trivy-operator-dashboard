import { provideHttpClient } from '@angular/common/http';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule, Title } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ApiModule } from '../api/api.module';
import { environment } from '../environments/environment';

import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { MenubarModule } from 'primeng/menubar';
import { PanelMenuModule } from 'primeng/panelmenu';
import { SidebarModule } from 'primeng/sidebar';
import { TagModule } from 'primeng/tag';

import { initializeAppFactory, MainAppInitService } from './services/main-app-init.service';
import { TrivyTableComponent } from './trivy-table/trivy-table.component';

import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot([
      {
        path: '',
        loadComponent: () => import('./home/home.component').then((m) => m.HomeComponent),
        data: { title: 'Home' },
      },
      {
        path: 'vulnerability-reports',
        loadComponent: () =>
          import('./vulnerability-reports/vulnerability-reports.component').then(
            (m) => m.VulnerabilityReportsComponent),
        data: { title: 'Vulnerability Reps' },
      },
      {
        path: 'vulnerability-reports-detailed',
        loadComponent: () =>
          import('./vulnerability-reports-detailed/vulnerability-reports-detailed.component').then(
            (m) => m.VulnerabilityReportsDetailedComponent),
        data: { title: 'Vulnerability Reps Detailed' },
      },
      {
        path: 'config-audit-reports',
        loadComponent: () =>
          import('./config-audit-reports/config-audit-reports.component').then(
            (m) => m.ConfigAuditReportsComponent),
        data: { title: 'Config Audit Reps' },
      },
      {
        path: 'config-audit-reports-detailed',
        loadComponent: () =>
          import('./config-audit-reports-detailed/config-audit-reports-detailed.component').then(
            (m) => m.ConfigAuditReportsDetailedComponent),
        data: { title: 'Config Audit Reps Detailed' },
      },
      {
        path: 'cluster-rbac-assessment-reports',
        loadComponent: () =>
          import('./cluster-rbac-assessment-reports/cluster-rbac-assessment-reports.component').then(
            (m) => m.ClusterRbacAssessmentReportsComponent),
        data: { title: 'Cluster RBAC Assessment Reps' },
      },
      {
        path: 'cluster-rbac-assessment-reports-detailed',
        loadComponent: () =>
          import('./cluster-rbac-assessment-reports-detailed/cluster-rbac-assessment-reports-detailed.component').then(
            (m) => m.ClusterRbacAssessmentReportsDetailedComponent),
        data: { title: 'Cluster RBAC Assessment Reps Detailed' },
      },
      {
        path: 'exposed-secret-reports',
        loadComponent: () =>
          import('./exposed-secret-reports/exposed-secret-reports.component').then(
            (m) => m.ExposedSecretReportsComponent),
        data: { title: 'Exposed Secret Reps' },
      },
      {
        path: 'exposed-secret-reports-detailed',
        loadComponent: () =>
          import('./exposed-secret-reports-detailed/exposed-secret-reports-detailed.component').then(
            (m) => m.ExposedSecretReportsDetailedComponent),
        data: { title: 'Exposed Secret Reps Detailed' },
      },
      {
        path: 'cluster-compliance-reports',
        loadComponent: () =>
          import('./cluster-compliance-reports/cluster-compliance-reports.component').then(
            (m) => m.ClusterComplianceReportsComponent),
        data: { title: 'Cluster Compliance Reps' },
      },
      {
        path: 'cluster-compliance-reports-detailed',
        loadComponent: () =>
          import('./cluster-compliance-reports-detailed/cluster-compliance-reports-detailed.component').then(
            (m) => m.ClusterComplianceReportsDetailedComponent),
        data: { title: 'Cluster Compliance Reps Detailed' },
      },

      {
        path: 'cluster-vulnerability-reports',
        loadComponent: () =>
          import('./cluster-vulnerability-reports/cluster-vulnerability-reports.component').then(
            (m) => m.ClusterVulnerabilityReportsComponent),
        data: { title: 'Cluster Vulnerability Reps' },
      },
      {
        path: 'cluster-vulnerability-reports-detailed',
        loadComponent: () =>
          import('./cluster-vulnerability-reports-detailed/cluster-vulnerability-reports-detailed.component').then(
            (m) => m.ClusterVulnerabilityReportsDetailedComponent),
        data: { title: 'Cluster Vulnerability Reps Detailed' },
      },
      {
        path: 'rbac-assessment-reports',
        loadComponent: () =>
          import('./rbac-assessment-reports/rbac-assessment-reports.component').then(
            (m) => m.RbacAssessmentReportsComponent),
        data: { title: 'RBAC Assessment Reps' },
      },
      {
        path: 'rbac-assessment-reports-detailed',
        loadComponent: () =>
          import('./rbac-assessment-reports-detailed/rbac-assessment-reports-detailed.component').then(
            (m) => m.RbacAssessmentReportsDetailedComponent),
        data: { title: 'RBAC Assessment Reps' },
      },
      {
        path: 'watcher-states',
        loadComponent: () => import('./watcher-state/watcher-state.component').then(
          (m) => m.WatcherStateComponent),
        data: { title: 'Watcher State' },
      },
      {
        path: 'settings',
        loadComponent: () => import('./settings/settings.component').then(
          (m) => m.SettingsComponent),
        data: { title: 'Settings' },
      },
      {
        path: 'about', loadComponent: () => import('./about/about.component').then(
          (m) => m.AboutComponent),
        data: { title: 'About' },
      },

      {
        path: 'sbom-reports',
        loadComponent: () => import('./sbom-reports/sbom-reports.component').then(
          (m) => m.SbomReportsComponent),
        data: { title: 'SBOM Reps' },
      },
    ]),
    ApiModule.forRoot({ rootUrl: environment.baseUrl }),
    BrowserAnimationsModule,
    BadgeModule,
    ButtonModule,
    MenubarModule,
    PanelMenuModule,
    SidebarModule,
    TagModule,
    TrivyTableComponent,
    FontAwesomeModule,
  ],
  providers: [
    provideHttpClient(),
    MainAppInitService,
    Title,
    { provide: APP_INITIALIZER, useFactory: initializeAppFactory, deps: [MainAppInitService], multi: true },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
