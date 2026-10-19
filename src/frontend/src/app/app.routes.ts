import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('@features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
    data: { title: 'Home' },
  },
  {
    path: 'watcher-status',
    loadComponent: () => import('@features/watcher-state/watcher-state.component').then((m) => m.WatcherStateComponent),
    data: { title: 'Watcher Status' },
  },
  {
    path: 'logs',
    loadComponent: () => import('@features/log-messages/log-messages.component').then((m) => m.LogMessagesComponent),
    data: { title: 'Log Messages' },
  },
  {
    path: 'settings',
    loadComponent: () => import('@features/settings/settings.component').then((m) => m.SettingsComponent),
    data: { title: 'Settings' },
  },
  {
    path: 'about',
    loadComponent: () => import('@features/about/about.component').then((m) => m.AboutComponent),
    data: { title: 'About' },
  },
  {
    path: 'alerts',
    loadComponent: () => import('@features/alerts/alerts.component').then((m) => m.AlertsComponent),
    data: { title: 'Alerts' },
  },
  {
    path: 'test1',
    loadComponent: () => import('@features/playground/tests.component').then((m) => m.TestsComponent),
    data: { title: 'Test1' },
  },
  {
    path: '',
    loadChildren: () => import('@features/trivy-reports/trivy-reports.routes').then((m) => m.trivyReportsRoutes),
  },
  {
    path: '',
    loadChildren: () => import('@features/history-reports/history-reports.routes').then((m) => m.historyReportsRoutes),
  },
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full',
  },
];
