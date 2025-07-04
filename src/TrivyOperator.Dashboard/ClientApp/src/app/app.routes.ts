import { Routes } from '@angular/router';

export const routes: Routes = [
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
    data: { title: 'Vulnerability Reports' },
  },
  {
    path: 'vulnerability-reports-detailed',
    loadComponent: () =>
      import('./vulnerability-reports-detailed/vulnerability-reports-detailed.component').then(
        (m) => m.VulnerabilityReportsDetailedComponent),
    data: { title: 'Vulnerability Reports Detailed' },
  },
  {
    path: 'vulnerability-reports-compare',
    loadComponent: () =>
      import('./vulnerability-reports-compare/vulnerability-reports-compare.component').then(
        (m) => m.VulnerabilityReportsCompareComponent),
    data: { title: 'Vulnerability Reports Compare' },
  },
  {
    path: 'config-audit-reports',
    loadComponent: () =>
      import('./config-audit-reports/config-audit-reports.component').then(
        (m) => m.ConfigAuditReportsComponent),
    data: { title: 'Config Audit Reports' },
  },
  {
    path: 'config-audit-reports-detailed',
    loadComponent: () =>
      import('./config-audit-reports-detailed/config-audit-reports-detailed.component').then(
        (m) => m.ConfigAuditReportsDetailedComponent),
    data: { title: 'Config Audit Reports Detailed' },
  },
  {
    path: 'cluster-rbac-assessment-reports',
    loadComponent: () =>
      import('./cluster-rbac-assessment-reports/cluster-rbac-assessment-reports.component').then(
        (m) => m.ClusterRbacAssessmentReportsComponent),
    data: { title: 'Cluster RBAC Assessment Reports' },
  },
  {
    path: 'cluster-rbac-assessment-reports-detailed',
    loadComponent: () =>
      import('./cluster-rbac-assessment-reports-detailed/cluster-rbac-assessment-reports-detailed.component').then(
        (m) => m.ClusterRbacAssessmentReportsDetailedComponent),
    data: { title: 'Cluster RBAC Assessment Reports Detailed' },
  },
  {
    path: 'exposed-secret-reports',
    loadComponent: () =>
      import('./exposed-secret-reports/exposed-secret-reports.component').then(
        (m) => m.ExposedSecretReportsComponent),
    data: { title: 'Exposed Secret Reports' },
  },
  {
    path: 'exposed-secret-reports-detailed',
    loadComponent: () =>
      import('./exposed-secret-reports-detailed/exposed-secret-reports-detailed.component').then(
        (m) => m.ExposedSecretReportsDetailedComponent),
    data: { title: 'Exposed Secret Reports Detailed' },
  },
  {
    path: 'cluster-compliance-reports',
    loadComponent: () =>
      import('./cluster-compliance-reports/cluster-compliance-reports.component').then(
        (m) => m.ClusterComplianceReportsComponent),
    data: { title: 'Cluster Compliance Reports' },
  },
  {
    path: 'cluster-compliance-reports-detailed',
    loadComponent: () =>
      import('./cluster-compliance-reports-detailed/cluster-compliance-reports-detailed.component').then(
        (m) => m.ClusterComplianceReportsDetailedComponent),
    data: { title: 'Cluster Compliance Reports Detailed' },
  },

  {
    path: 'cluster-vulnerability-reports',
    loadComponent: () =>
      import('./cluster-vulnerability-reports/cluster-vulnerability-reports.component').then(
        (m) => m.ClusterVulnerabilityReportsComponent),
    data: { title: 'Cluster Vulnerability Reports' },
  },
  {
    path: 'cluster-vulnerability-reports-detailed',
    loadComponent: () =>
      import('./cluster-vulnerability-reports-detailed/cluster-vulnerability-reports-detailed.component').then(
        (m) => m.ClusterVulnerabilityReportsDetailedComponent),
    data: { title: 'Cluster Vulnerability Reports Detailed' },
  },
  {
    path: 'rbac-assessment-reports',
    loadComponent: () =>
      import('./rbac-assessment-reports/rbac-assessment-reports.component').then(
        (m) => m.RbacAssessmentReportsComponent),
    data: { title: 'RBAC Assessment Reports' },
  },
  {
    path: 'rbac-assessment-reports-detailed',
    loadComponent: () =>
      import('./rbac-assessment-reports-detailed/rbac-assessment-reports-detailed.component').then(
        (m) => m.RbacAssessmentReportsDetailedComponent),
    data: { title: 'RBAC Assessment Reports Detailed' },
  },
  {
    path: 'watcher-status',
    loadComponent: () => import('./watcher-state/watcher-state.component').then(
      (m) => m.WatcherStateComponent),
    data: { title: 'Watcher Status' },
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
    data: { title: 'SBOM Reports' },
  },
  {
    path: 'sbom-reports-detailed',
    loadComponent: () => import('./sbom-reports-detailed/sbom-reports-detailed.component').then(
      (m) => m.SbomReportsDetailedComponent),
    data: { title: 'SBOM Reports Detailed' },
  },
  {
    path: 'test1',
    loadComponent: () =>
      import('./fcose-help/fcose-help.component').then(
        (m) => m.FcoseHelpComponent),
    data: { title: 'Test' },
  },
  {
    path: 'test2',
    loadComponent: () =>
      import('./sbom-reports-compare/sbom-reports-compare.component').then(
        (m) => m.SbomReportsCompareComponent),
    data: { title: 'Test' },
  },
  {
    path: 'test3',
    loadComponent: () =>
      import('./tests/tests.component').then(
        (m) => m.TestsComponent),
    data: { title: 'Test' },
  },
];
