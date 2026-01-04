import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';

import { AlertsService } from '../../services/alerts.service';
import { DarkModeService } from '../../services/dark-mode.service';
import { MainAppInitService } from '../../services/main-app-init.service';
import { RouterEventEmitterService } from '../../services/router-event-emitter.service';
import { KubernetesContextStateService } from '../../services/kubernetes-context-state.service';

import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';
import { PanelMenuModule } from 'primeng/panelmenu';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';

import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-nav-menu',
  standalone: true,
  imports: [
    FormsModule,
    MenubarModule,
    DrawerModule,
    PanelMenuModule,
    ButtonModule,
    TagModule,
    BadgeModule,
    SelectModule,
    ToastModule,
    IconComponent,
  ],
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss'],
})
export class NavMenuComponent {
  protected router = inject(Router);
  private alertsService = inject(AlertsService);
  private darkModeService = inject(DarkModeService);
  private mainAppInitService = inject(MainAppInitService);
  private routerEventEmitterService = inject(RouterEventEmitterService);
  protected k8sContextState = inject(KubernetesContextStateService);

  isDrawerVisible = signal(false);

  // --- Signals from services ---
  alerts = toSignal(this.alertsService.alerts$, { initialValue: [] });
  backendSettings = toSignal(this.mainAppInitService.backendSettingsDto$);
  isDarkMode = toSignal(this.darkModeService.isDarkMode$);
  activePage = toSignal(this.routerEventEmitterService.title$);

  contexts = this.k8sContextState.contexts;
  selectedContext = this.k8sContextState.selectedContext;

  // --- Derived values ---
  alertsCount = computed(() => this.alerts().length);

  // This replaces the old enabledTrivyReports field
  enabledTrivyReports = computed<string[]>(() => {
    const dto = this.backendSettings();
    if (!dto || !dto.trivyReportConfigDtos) {
      // Fallback to your original default, if you still want that behavior
      return ['crar', 'car', 'esr', 'vr'];
    }

    return dto.trivyReportConfigDtos
      .filter(x => x.enabled)
      .map(x => x.id ?? '')
      .filter(id => !!id);
  });

  showContextDropdown = computed(() => this.contexts().length >= 2);

  items = computed<MenuItem[]>(() => {
    const dto = this.backendSettings();
    if (!dto) return [];

    const enabled = this.enabledTrivyReports();

    const items: MenuItem[] = [
      {
        label: 'Home',
        icon: 'home',
        command: () => {
          this.router.navigate(['']);
          this.isDrawerVisible.set(false);
        },
      },
      {
        label: 'Namespaced',
        icon: 'dynamic_feed',
        expanded: true,
        items: [
          {
            label: 'Vulnerabilities',
            icon: 'security',
            disabled: !this.enabledTrivyReports().includes('vr'),
            command: () => {
              this.router.navigate(['vulnerability-reports']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'SBOMs',
            icon: 'graph_3',
            disabled: !this.enabledTrivyReports().includes('sr'),
            command: () => {
              this.router.navigate(['sbom-reports']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Config Audits',
            icon: 'assignment',
            disabled: !this.enabledTrivyReports().includes('car'),
            command: () => {
              this.router.navigate(['config-audit-reports']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Exposed Secrets',
            icon: 'key_off',
            disabled: !this.enabledTrivyReports().includes('esr'),
            command: () => {
              this.router.navigate(['exposed-secret-reports']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'RBAC Assessments',
            icon: 'admin_panel_settings',
            disabled: !this.enabledTrivyReports().includes('rar'),
            command: () => {
              this.router.navigate(['rbac-assessment-reports']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Infra Assessments',
            icon: 'host',
            disabled: !this.enabledTrivyReports().includes('iar'),
            command: () => {
              this.router.navigate(['infra-assessment-reports']);
              this.isDrawerVisible.set(false);
            },
          },
        ]
      },
      {
        label: 'Cluster Level',
        icon: 'storage',
        expanded: true,
        items: [
          {
            label: 'Vulnerabilities',
            icon: 'security',
            disabled: !this.enabledTrivyReports().includes('cvr'),
            command: () => {
              this.router.navigate(['cluster-vulnerability-reports']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'SBOMs',
            icon: 'graph_3',
            disabled: !this.enabledTrivyReports().includes('csr'),
            command: () => {
              this.router.navigate(['cluster-sbom-reports']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'RBAC Assessments',
            icon: 'admin_panel_settings',
            disabled: !this.enabledTrivyReports().includes('crar'),
            command: () => {
              this.router.navigate(['cluster-rbac-assessment-reports']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Compliance',
            icon: 'policy',
            disabled: !this.enabledTrivyReports().includes('ccr'),
            command: () => {
              this.router.navigate(['cluster-compliance-reports']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Infra Assessments',
            icon: 'host',
            disabled: !this.enabledTrivyReports().includes('ciar'),
            command: () => {
              this.router.navigate(['cluster-infra-assessment-reports']);
              this.isDrawerVisible.set(false);
            },
          },
        ]
      },
      {
        label: 'Namespaced - Detailed',
        icon: 'dynamic_feed',
        expanded: false,
        items: [
          {
            label: 'Vulnerabilities',
            icon: 'security',
            disabled: !this.enabledTrivyReports().includes('vr'),
            command: () => {
              this.router.navigate(['vulnerability-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'SBOMs',
            icon: 'graph_3',
            disabled: !this.enabledTrivyReports().includes('sr'),
            command: () => {
              this.router.navigate(['sbom-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Config Audits',
            icon: 'assignment',
            disabled: !this.enabledTrivyReports().includes('car'),
            command: () => {
              this.router.navigate(['config-audit-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Exposed Secrets',
            icon: 'key_off',
            disabled: !this.enabledTrivyReports().includes('esr'),
            command: () => {
              this.router.navigate(['exposed-secret-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'RBAC Assessments',
            icon: 'admin_panel_settings',
            disabled: !this.enabledTrivyReports().includes('rar'),
            command: () => {
              this.router.navigate(['rbac-assessment-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Infra Assessments',
            icon: 'host',
            disabled: !this.enabledTrivyReports().includes('iar'),
            command: () => {
              this.router.navigate(['infra-assessment-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
        ]
      },
      {
        label: 'Cluster Level - Detailed',
        icon: 'storage',
        expanded: false,
        items: [
          {
            label: 'Vulnerabilities',
            icon: 'security',
            disabled: !this.enabledTrivyReports().includes('cvr'),
            command: () => {
              this.router.navigate(['cluster-vulnerability-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'RBAC Assessments',
            icon: 'admin_panel_settings',
            disabled: !this.enabledTrivyReports().includes('crar'),
            command: () => {
              this.router.navigate(['cluster-rbac-assessment-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Compliance',
            icon: 'policy',
            disabled: !this.enabledTrivyReports().includes('ccr'),
            command: () => {
              this.router.navigate(['cluster-compliance-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Infra Assessments',
            icon: 'host',
            disabled: !this.enabledTrivyReports().includes('ciar'),
            command: () => {
              this.router.navigate(['cluster-infra-assessment-reports-detailed']);
              this.isDrawerVisible.set(false);
            },
          },
        ]
      },
      {
        label: 'System',
        icon: 'settings',
        expanded: true,
        items: [
          {
            label: 'Watcher Status',
            icon: 'mystery',
            command: () => {
              this.router.navigate(['watcher-status']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'Settings',
            icon: 'settings_applications',
            command: () => {
              this.router.navigate(['settings']);
              this.isDrawerVisible.set(false);
            },
          },
          {
            label: 'About',
            icon: 'chat_info',
            command: () => {
              this.router.navigate(['about']);
              this.isDrawerVisible.set(false);
            },
          },
        ],
      },
    ];

    return items;
  });

  // --- UI actions ---
  switchLightDarkMode() {
    this.darkModeService.toggleDarkMode();
  }

  onAlertsClick() {
    if (this.router.url === '/alerts') {
      this.alertsService.triggerRefresh();
    } else {
      this.router.navigate(['alerts']);
    }
  }

  openDrawer() {
    this.isDrawerVisible.set(true);
  }

  onContextChange(context: string) {
    this.k8sContextState.setSelectedContext(context);
    this.isDrawerVisible.set(false);
  }
}
