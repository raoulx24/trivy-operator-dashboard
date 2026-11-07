import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { AlertsService } from '../../services/alerts.service';
import { KubernetesContextStateService } from '../../state/kubernetes-context-state.service';
import { MainAppInitService } from '../../services/main-app-init.service';
import { CurrentPageService } from '../../services/current-page.service';

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
import { ThemeSwitcherComponent } from '@shared/ui/theme-switcher/theme-switcher.component';
import { buildNavMenuItems, createDrawerNavigator, findSectionIdForRoute } from './nav-menu.items';

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
    ThemeSwitcherComponent,
    Ripple,
  ],
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavMenuComponent {
  protected router = inject(Router);
  private alertsService = inject(AlertsService);
  private mainAppInitService = inject(MainAppInitService);
  private currentPageService = inject(CurrentPageService);
  protected k8sContextState = inject(KubernetesContextStateService);

  isDrawerVisible = signal(false);
  private navigateAndClose = createDrawerNavigator(this.router, this.isDrawerVisible);

  // --- Signals from services ---
  alerts = this.alertsService.alerts;
  backendSettings = this.mainAppInitService.backendSettingsDto;
  activePageTitle = this.currentPageService.title;
  // matched against MenuItem.id (= NavEntry.route) in the template
  activeRoute = this.currentPageService.route;

  contexts = this.k8sContextState.contexts;
  selectedContext = this.k8sContextState.selectedContext;

  // --- Derived values ---
  alertsCount = computed(() => this.alerts().length);

  // This replaces the old enabledTrivyReports field
  enabledTrivyReports = computed<string[]>(() => {
    const dto = this.backendSettings();
    if (!dto || !dto.trivyReportConfigDtos) {
      // Backend settings not loaded yet: assume all reports are enabled
      return ['vr', 'vrh', 'sr', 'car', 'esr', 'rar', 'iar', 'cvr', 'csr', 'crar', 'ccr', 'ccar', 'ciar'];
    }

    const enabledIds = dto.trivyReportConfigDtos
      .filter((x) => x.enabled)
      .map((x) => x.id ?? '')
      .filter((id) => !!id);

    // history is not a Trivy report; the backend reports it as a separate flag
    return dto.isVrHistoryEnabled ? [...enabledIds, 'vrh'] : enabledIds;
  });

  private menuItems = computed<MenuItem[]>(() => {
    if (!this.backendSettings()) return [];

    return buildNavMenuItems(this.enabledTrivyReports(), this.navigateAndClose);
  });

  // p-panelMenu is OnPush and only re-reads `expanded` when its model input changes. Programmatic changes to
  // `expanded` (auto-expand) bump this, which hands the same item objects over in a new array.
  private modelVersion = signal(0);
  items = computed<MenuItem[]>(() => {
    this.modelVersion();
    return [...this.menuItems()];
  });

  // --- UI actions ---
  onAlertsClick() {
    if (this.router.url === '/alerts') {
      this.alertsService.triggerRefresh();
    } else {
      this.navigateAndClose('alerts');
    }
  }

  openDrawer() {
    this.syncSectionExpansion();
    this.isDrawerVisible.set(true);
  }

  toggleSection(section: MenuItem) {
    section.expanded = !section.expanded;
    this.releaseAutoExpanded(section);
  }

  // clicking a section row is handled by p-panelMenu itself; we only note that the user took over
  onMenuItemClick(item: MenuItem) {
    if (item.items) {
      this.releaseAutoExpanded(item);
    }
  }

  // --- Auto-expand of the section holding the active page ---
  // Applied when the drawer opens (not on navigation), so the user never sees it reshuffle while it slides away.
  // A section we expanded is collapsed again once the active page moves elsewhere, unless the user toggled it meanwhile.
  private autoExpandedSectionId: string | null = null;
  private lastSyncedRoute: string | null = null;

  private syncSectionExpansion() {
    const route = this.activeRoute();
    // once per page, so a section the user collapsed stays collapsed until the next navigation
    if (route === this.lastSyncedRoute) return;
    this.lastSyncedRoute = route;

    const sections = this.menuItems();
    const targetId = findSectionIdForRoute(route);
    let changed = false;

    if (this.autoExpandedSectionId && this.autoExpandedSectionId !== targetId) {
      const previous = sections.find((s) => s.id === this.autoExpandedSectionId);
      if (previous?.expanded) {
        previous.expanded = false;
        changed = true;
      }
      this.autoExpandedSectionId = null;
    }

    const target = targetId ? sections.find((s) => s.id === targetId) : undefined;
    if (target && !target.expanded) {
      target.expanded = true;
      this.autoExpandedSectionId = targetId ?? null;
      changed = true;
    }

    if (changed) {
      this.modelVersion.update((v) => v + 1);
    }
  }

  private releaseAutoExpanded(section: MenuItem) {
    if (section.id === this.autoExpandedSectionId) {
      this.autoExpandedSectionId = null;
    }
  }

  onContextChange(context: string) {
    this.k8sContextState.setSelectedContext(context);
    this.isDrawerVisible.set(false);
  }
}
