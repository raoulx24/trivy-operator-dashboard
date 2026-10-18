import { WritableSignal } from '@angular/core';
import { Router } from '@angular/router';

import { MenuItem } from 'primeng/api';
import { timeout } from 'rxjs';

interface NavEntry {
  label: string;
  icon: string;
  route: string;
  // Trivy report id that must be enabled for the entry to be active; entries without one are always active
  reportId?: string;
}

interface NavSection {
  label: string;
  icon: string;
  expanded: boolean;
  entries: NavEntry[];
}

const HOME_ENTRY: NavEntry = { label: 'Home', icon: 'home', route: '' };

const NAV_SECTIONS: NavSection[] = [
  {
    label: 'Namespaced',
    icon: 'dynamic_feed',
    expanded: true,
    entries: [
      { label: 'Vulnerabilities', icon: 'security', route: 'vulnerability-reports', reportId: 'vr' },
      { label: 'Vulnerabilities History', icon: 'safety_check', route: 'vulnerability-reports-history', reportId: 'vrh' },
      { label: 'SBOMs', icon: 'graph_3', route: 'sbom-reports', reportId: 'sr' },
      { label: 'Config Audits', icon: 'assignment', route: 'config-audit-reports', reportId: 'car' },
      { label: 'Exposed Secrets', icon: 'key_off', route: 'exposed-secret-reports', reportId: 'esr' },
      { label: 'RBAC Assessments', icon: 'admin_panel_settings', route: 'rbac-assessment-reports', reportId: 'rar' },
      { label: 'Infra Assessments', icon: 'host', route: 'infra-assessment-reports', reportId: 'iar' },
    ],
  },
  {
    label: 'Cluster Level',
    icon: 'storage',
    expanded: true,
    entries: [
      { label: 'Vulnerabilities', icon: 'security', route: 'cluster-vulnerability-reports', reportId: 'cvr' },
      { label: 'SBOMs', icon: 'graph_3', route: 'cluster-sbom-reports', reportId: 'csr' },
      { label: 'RBAC Assessments', icon: 'admin_panel_settings', route: 'cluster-rbac-assessment-reports', reportId: 'crar' },
      { label: 'Compliance', icon: 'policy', route: 'cluster-compliance-reports', reportId: 'ccr' },
      { label: 'Infra Assessments', icon: 'host', route: 'cluster-infra-assessment-reports', reportId: 'ciar' },
    ],
  },
  {
    label: 'Namespaced - Detailed',
    icon: 'dynamic_feed',
    expanded: false,
    entries: [
      { label: 'Vulnerabilities', icon: 'security', route: 'vulnerability-reports-detailed', reportId: 'vr' },
      { label: 'Vulnerabilities History', icon: 'safety_check', route: 'vulnerability-reports-history-detailed', reportId: 'vrh' },
      { label: 'SBOMs', icon: 'graph_3', route: 'sbom-reports-detailed', reportId: 'sr' },
      { label: 'Config Audits', icon: 'assignment', route: 'config-audit-reports-detailed', reportId: 'car' },
      { label: 'Exposed Secrets', icon: 'key_off', route: 'exposed-secret-reports-detailed', reportId: 'esr' },
      { label: 'RBAC Assessments', icon: 'admin_panel_settings', route: 'rbac-assessment-reports-detailed', reportId: 'rar' },
      { label: 'Infra Assessments', icon: 'host', route: 'infra-assessment-reports-detailed', reportId: 'iar' },
    ],
  },
  {
    label: 'Cluster Level - Detailed',
    icon: 'storage',
    expanded: false,
    entries: [
      { label: 'Vulnerabilities', icon: 'security', route: 'cluster-vulnerability-reports-detailed', reportId: 'cvr' },
      { label: 'RBAC Assessments', icon: 'admin_panel_settings', route: 'cluster-rbac-assessment-reports-detailed', reportId: 'crar' },
      { label: 'Compliance', icon: 'policy', route: 'cluster-compliance-reports-detailed', reportId: 'ccr' },
      { label: 'Infra Assessments', icon: 'host', route: 'cluster-infra-assessment-reports-detailed', reportId: 'ciar' },
    ],
  },
  {
    label: 'System',
    icon: 'settings',
    expanded: true,
    entries: [
      { label: 'Watcher Status', icon: 'mystery', route: 'watcher-status' },
      { label: 'Settings', icon: 'settings_applications', route: 'settings' },
      { label: 'About', icon: 'chat_info', route: 'about' },
    ],
  },
];

/**
 * Creates a navigate function that first closes the drawer, then navigates on the next
 * macrotask, so the drawer state change is not competing with the route change.
 */
export const createDrawerNavigator =
  (router: Router, isDrawerVisible: WritableSignal<boolean>) =>
  (route: string): void => {

    router.navigate([route]);

    setTimeout((() => isDrawerVisible.set(false)), 300 );
  };

export const buildNavMenuItems = (
  enabledReports: string[],
  navigate: (route: string) => void,
): MenuItem[] => {
  const toMenuItem = (entry: NavEntry): MenuItem => ({
    label: entry.label,
    icon: entry.icon,
    disabled: !!entry.reportId && !enabledReports.includes(entry.reportId),
    command: () => navigate(entry.route),
  });

  return [
    toMenuItem(HOME_ENTRY),
    ...NAV_SECTIONS.map(
      (section): MenuItem => ({
        label: section.label,
        icon: section.icon,
        expanded: section.expanded,
        items: section.entries.map(toMenuItem),
      }),
    ),
  ];
};
