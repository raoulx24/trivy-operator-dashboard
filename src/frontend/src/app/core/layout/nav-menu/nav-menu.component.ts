import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { AlertsService } from '../../services/alerts.service';
import { DarkModeService } from '../../services/dark-mode.service';
import { KubernetesContextStateService } from '../../state/kubernetes-context-state.service';
import { MainAppInitService } from '../../services/main-app-init.service';
import { RouterEventEmitterService } from '../../services/router-event-emitter.service';

import { MenuItem } from 'primeng/api';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { MenubarModule } from 'primeng/menubar';
import { PanelMenuModule } from 'primeng/panelmenu';
import { Ripple } from 'primeng/ripple';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';

import { IconComponent } from '@shared/ui/icon/icon.component';
import { buildNavMenuItems, createDrawerNavigator } from './nav-menu.items';

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
    Ripple,
  ],
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavMenuComponent {
  protected router = inject(Router);
  private alertsService = inject(AlertsService);
  private darkModeService = inject(DarkModeService);
  private mainAppInitService = inject(MainAppInitService);
  private routerEventEmitterService = inject(RouterEventEmitterService);
  protected k8sContextState = inject(KubernetesContextStateService);

  isDrawerVisible = signal(false);
  private navigateAndClose = createDrawerNavigator(this.router, this.isDrawerVisible);

  // --- Signals from services ---
  alerts = this.alertsService.alerts;
  backendSettings = this.mainAppInitService.backendSettingsDto;
  isDarkMode = this.darkModeService.isDarkMode;
  activePageTitle = this.routerEventEmitterService.title;

  contexts = this.k8sContextState.contexts;
  selectedContext = this.k8sContextState.selectedContext;

  // --- Derived values ---
  alertsCount = computed(() => this.alerts().length);

  // This replaces the old enabledTrivyReports field
  enabledTrivyReports = computed<string[]>(() => {
    const dto = this.backendSettings();
    if (!dto || !dto.trivyReportConfigDtos) {
      // Backend settings not loaded yet: assume all reports are enabled
      return ['vr', 'vrh', 'sr', 'car', 'esr', 'rar', 'iar', 'cvr', 'csr', 'crar', 'ccr', 'ciar'];
    }

    return dto.trivyReportConfigDtos
      .filter((x) => x.enabled)
      .map((x) => x.id ?? '')
      .filter((id) => !!id);
  });

  items = computed<MenuItem[]>(() => {
    if (!this.backendSettings()) return [];

    return buildNavMenuItems(this.enabledTrivyReports(), this.navigateAndClose);
  });

  // --- UI actions ---
  switchLightDarkMode() {
    this.darkModeService.toggleDarkMode();
  }

  onAlertsClick() {
    if (this.router.url === '/alerts') {
      this.alertsService.triggerRefresh();
    } else {
      this.navigateAndClose('alerts');
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
