import { Component } from '@angular/core';
// import { ChangeDetectionStrategy, Component, effect, inject, OnInit, signal, ViewChild } from '@angular/core';
// import { FormsModule } from '@angular/forms';

// import { MainAppInitService } from '@core/services/main-app-init.service';
// import { LocalStorageUtils } from '@shared/utils/local-storage.utils';

// import { DashboardClusterRbacAssessmentReportsComponent } from './widgets/cluster-rbac/cluster-rbac.component';
// import { DashboardConfigAuditReportsComponent } from './widgets/config-audit/config-audit.component';
// import { DashboardExposedSecretReportsComponent } from './widgets/exposed-secrets/exposed-secrets.component';
// import { DashboardVulnerabilityReportsComponent } from './widgets/vulnerabilities/vulnerabilities.component';

// import { ButtonModule } from 'primeng/button';
// import { TabsModule } from 'primeng/tabs';
// import { ToggleSwitchModule } from 'primeng/toggleswitch';
// import { KubernetesContextStateService } from '@core/state/kubernetes-context-state.service';
// import { NgClass } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    // FormsModule,
    // DashboardVulnerabilityReportsComponent,
    // DashboardConfigAuditReportsComponent,
    // DashboardClusterRbacAssessmentReportsComponent,
    // DashboardExposedSecretReportsComponent,
    // TabsModule,
    // ToggleSwitchModule,
    // ButtonModule,
    // NgClass,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  // changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
// export class DashboardComponent implements OnInit {
  // enabledTrivyReports: string[] = ['crar', 'car', 'esr', 'vr'];
  // tabPageActiveIndex: string = '0';
  //
  // @ViewChild(DashboardVulnerabilityReportsComponent) homeVr?: DashboardVulnerabilityReportsComponent;
  // @ViewChild(DashboardConfigAuditReportsComponent) homeCar?: DashboardConfigAuditReportsComponent;
  // @ViewChild(DashboardClusterRbacAssessmentReportsComponent) homeCrar?: DashboardClusterRbacAssessmentReportsComponent;
  // @ViewChild(DashboardExposedSecretReportsComponent) homeEsr?: DashboardExposedSecretReportsComponent;
  //
  // private readonly kubernetesContextService = inject(KubernetesContextStateService);
  //
  // private readonly mainAppInitService = inject(MainAppInitService);

  constructor() {
    // let initialized = false;
    //
    // effect(() => {
    //   const showDistinctValues = this.showDistinctValues();
    //   localStorage.setItem('home.showDistinctValues', showDistinctValues.toString());
    // });
    // effect(() => {
    //   const ctx = this.kubernetesContextService.selectedContext();
    //
    //   if (!initialized) {
    //     initialized = true;
    //     return; // skip initial run
    //   }
    //
    //   this.onRefreshData();
    // });
    // effect(() => {
    //   const updatedBackendSettingsDto = this.mainAppInitService.backendSettingsDto();
    //   this.enabledTrivyReports =
    //     updatedBackendSettingsDto.trivyReportConfigDtos?.filter((x) => x.enabled).map((x) => x.id ?? '') ??
    //     this.enabledTrivyReports;
    // });
  }

  // showDistinctValues = signal<boolean>(true);
  //
  // ngOnInit() {
  //   this.showDistinctValues.set(LocalStorageUtils.getBoolKeyValue('home.showDistinctValues') ?? true);
  //   this.tabPageActiveIndex = localStorage.getItem('home.tabPageActiveIndex') ?? '0';
  // }
  //
  // onTabPageChange(event: string | number | undefined) {
  //   if (event) {
  //     localStorage.setItem('home.tabPageActiveIndex', event.toString());
  //   }
  // }
  //
  // onRefreshData() {
  //   if (this.homeVr) {
  //     this.homeVr.loadData();
  //   }
  //   if (this.homeCar) {
  //     this.homeCar.loadData();
  //   }
  //   if (this.homeEsr) {
  //     this.homeEsr.loadData();
  //   }
  // }
}
