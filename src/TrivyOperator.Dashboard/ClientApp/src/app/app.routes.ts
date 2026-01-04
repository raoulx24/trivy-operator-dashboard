import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home.component').then((m) => m.HomeComponent),
    data: { title: 'Home' },
  },
  {
    path: 'vulnerability-reports',
    loadComponent: () =>
      import('./trivy-reports/vulnerability-reports/vulnerability-reports.component').then(
        (m) => m.VulnerabilityReportsComponent),
    data: { title: 'Vulnerability Reports' },
  },
  {
    path: 'vulnerability-reports-detailed',
    loadComponent: () =>
      import('./trivy-reports/vulnerability-reports-detailed/vulnerability-reports-detailed.component').then(
        (m) => m.VulnerabilityReportsDetailedComponent),
    data: { title: 'Vulnerability Reports Detailed' },
  },
  {
    path: 'config-audit-reports',
    loadComponent: () =>
      import('./trivy-reports/config-audit-reports/config-audit-reports.component').then(
        (m) => m.ConfigAuditReportsComponent),
    data: { title: 'Config Audit Reports' },
  },

  {
    path: 'config-audit-reports-detailed',
    loadComponent: () =>
      import('./trivy-reports/config-audit-reports-detailed/config-audit-reports-detailed.component').then(
        (m) => m.ConfigAuditReportsDetailedComponent),
    data: { title: 'Config Audit Reports Detailed' },
  },
  {
    path: 'cluster-rbac-assessment-reports',
    loadComponent: () =>
      import('./trivy-reports/cluster-rbac-assessment-reports/cluster-rbac-assessment-reports.component').then(
        (m) => m.ClusterRbacAssessmentReportsComponent),
    data: { title: 'Cluster RBAC Assessment Reports' },
  },
  {
    path: 'cluster-rbac-assessment-reports-detailed',
    loadComponent: () =>
      import('./trivy-reports/cluster-rbac-assessment-reports-detailed/cluster-rbac-assessment-reports-detailed.component').then(
        (m) => m.ClusterRbacAssessmentReportsDetailedComponent),
    data: { title: 'Cluster RBAC Assessment Reports Detailed' },
  },
  {
    path: 'exposed-secret-reports',
    loadComponent: () =>
      import('./trivy-reports/exposed-secret-reports/exposed-secret-reports.component').then(
        (m) => m.ExposedSecretReportsComponent),
    data: { title: 'Exposed Secret Reports' },
  },
  {
    path: 'exposed-secret-reports-detailed',
    loadComponent: () =>
      import('./trivy-reports/exposed-secret-reports-detailed/exposed-secret-reports-detailed.component').then(
        (m) => m.ExposedSecretReportsDetailedComponent),
    data: { title: 'Exposed Secret Reports Detailed' },
  },
  {
    path: 'cluster-compliance-reports',
    loadComponent: () =>
      import('./trivy-reports/cluster-compliance-reports/cluster-compliance-reports.component').then(
        (m) => m.ClusterComplianceReportsComponent),
    data: { title: 'Cluster Compliance Reports' },
  },
  {
    path: 'cluster-compliance-reports-detailed',
    loadComponent: () =>
      import('./trivy-reports/cluster-compliance-reports-detailed/cluster-compliance-reports-detailed.component').then(
        (m) => m.ClusterComplianceReportsDetailedComponent),
    data: { title: 'Cluster Compliance Reports Detailed' },
  },

  {
    path: 'cluster-vulnerability-reports',
    loadComponent: () =>
      import('./trivy-reports/cluster-vulnerability-reports/cluster-vulnerability-reports.component').then(
        (m) => m.ClusterVulnerabilityReportsComponent),
    data: { title: 'Cluster Vulnerability Reports' },
  },
  {
    path: 'cluster-vulnerability-reports-detailed',
    loadComponent: () =>
      import('./trivy-reports/cluster-vulnerability-reports-detailed/cluster-vulnerability-reports-detailed.component').then(
        (m) => m.ClusterVulnerabilityReportsDetailedComponent),
    data: { title: 'Cluster Vulnerability Reports Detailed' },
  },
  {
    path: 'rbac-assessment-reports',
    loadComponent: () =>
      import('./trivy-reports/rbac-assessment-reports/rbac-assessment-reports.component').then(
        (m) => m.RbacAssessmentReportsComponent),
    data: { title: 'RBAC Assessment Reports' },
  },
  {
    path: 'rbac-assessment-reports-detailed',
    loadComponent: () =>
      import('./trivy-reports/rbac-assessment-reports-detailed/rbac-assessment-reports-detailed.component').then(
        (m) => m.RbacAssessmentReportsDetailedComponent),
    data: { title: 'RBAC Assessment Reports Detailed' },
  },
  {
    path: 'watcher-status',
    loadComponent: () => import('./pages/watcher-state/watcher-state.component').then(
      (m) => m.WatcherStateComponent),
    data: { title: 'Watcher Status' },
  },
  {
    path: 'alerts',
    loadComponent: () => import('./pages/alerts/alerts.component').then(
      (m) => m.AlertsComponent),
    data: { title: 'Alerts' },
  },
  {
    path: 'settings',
    loadComponent: () => import('./pages/settings/settings.component').then(
      (m) => m.SettingsComponent),
    data: { title: 'Settings' },
  },
  {
    path: 'about', loadComponent: () => import('./pages/about/about.component').then(
      (m) => m.AboutComponent),
    data: { title: 'About' },
  },
  {
    path: 'sbom-reports',
    loadComponent: () => import('./trivy-reports/sbom-reports/sbom-reports.component').then(
      (m) => m.SbomReportsComponent),
    data: { title: 'SBOM Reports' },
  },
  {
    path: 'sbom-reports-detailed',
    loadComponent: () => import('./trivy-reports/sbom-reports-detailed/sbom-reports-detailed.component').then(
      (m) => m.SbomReportsDetailedComponent),
    data: { title: 'SBOM Reports Detailed' },
  },
  {
    path: 'cluster-sbom-reports',
    loadComponent: () => import('./trivy-reports/cluster-sbom-reports/cluster-sbom-reports.component').then(
      (m) => m.ClusterSbomReportsComponent),
    data: { title: 'Cluster SBOM Reports' },
  },
  {
    path: 'infra-assessment-reports',
    loadComponent: () =>
      import('./trivy-reports/infra-assessment-reports/infra-assessment-reports.component').then(
        (m) => m.InfraAssessmentReportsComponent),
    data: { title: 'Infra Assessment Reports' },
  },
  {
    path: 'infra-assessment-reports-detailed',
    loadComponent: () =>
      import('./trivy-reports/infra-assessment-reports-detailed/infra-assessment-reports-detailed.component').then(
        (m) => m.InfraAssessmentReportsDetailedComponent),
    data: { title: 'Infra Assessment Reports Detailed' },
  },
  {
    path: 'cluster-infra-assessment-reports',
    loadComponent: () =>
      import('./trivy-reports/cluster-infra-assessment-reports/cluster-infra-assessment-reports.component').then(
        (m) => m.ClusterInfraAssessmentReportsComponent),
    data: { title: 'Cluster Infra Assessment Reports' },
  },
  {
    path: 'cluster-infra-assessment-reports-detailed',
    loadComponent: () =>
      import('./trivy-reports/cluster-infra-assessment-reports-detailed/cluster-infra-assessment-reports-detailed.component').then(
        (m) => m.ClusterInfraAssessmentReportsDetailedComponent),
    data: { title: 'Cluster Infra Assessment Reports Detailed' },
  },
  {
    path: 'test1',
    loadComponent: () =>
      import('./tests/tests.component').then(
        (m) => m.TestsComponent),
    data: { title: 'Test' },
  },
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full',
  }
];
