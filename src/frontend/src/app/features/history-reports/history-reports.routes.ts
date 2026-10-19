import { Routes } from '@angular/router';

export const historyReportsRoutes: Routes = [
  {
    path: 'vulnerability-reports-history',
    loadComponent: () => import('./vulnerability-reports-history/vulnerability-reports-history.component').then((m) => m.VulnerabilityReportsHistoryComponent),
    data: { title: 'Vulnerability Reports History' },
  },
  {
    path: 'vulnerability-reports-history-detailed',
    loadComponent: () => import('./vulnerability-reports-history-detailed/vulnerability-reports-history-detailed.component').then((m) => m.VulnerabilityReportsHistoryDetailedComponent),
    data: { title: 'Vulnerability Reports History Detailed' },
  },
];
