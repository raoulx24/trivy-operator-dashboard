import { Component, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { MainAppInitService } from '../../services/main-app-init.service';
import { LocalStorageUtils } from '../../utils/local-storage.utils';

import { DashboardClusterRbacAssessmentReportsComponent } from './dashboard-cluster-rbac-assessment-reports/dashboard-cluster-rbac-assessment-reports.component';
import { DashboardConfigAuditReportsComponent } from './dashboard-config-audit-reports/dashboard-config-audit-reports.component';
import { DashboardExposedSecretReportsComponent } from './dashboard-exposed-secret-reports/dashboard-exposed-secret-reports.component';
import { DashboardVulnerabilityReportsComponent } from './dashboard-vulnerability-reports/dashboard-vulnerability-reports.component';

import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { ToggleSwitchModule } from 'primeng/toggleswitch';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    FormsModule,
    DashboardVulnerabilityReportsComponent,
    DashboardConfigAuditReportsComponent,
    DashboardClusterRbacAssessmentReportsComponent,
    DashboardExposedSecretReportsComponent,
    TabsModule,
    ToggleSwitchModule,
    ButtonModule,
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  enabledTrivyReports: string[] = ['crar', 'car', 'esr', 'vr'];
  tabPageActiveIndex: string = "0";

  @ViewChild(DashboardVulnerabilityReportsComponent) homeVr?: DashboardVulnerabilityReportsComponent;
  @ViewChild(DashboardConfigAuditReportsComponent) homeCar?: DashboardConfigAuditReportsComponent;
  @ViewChild(DashboardClusterRbacAssessmentReportsComponent) homeCrar?: DashboardClusterRbacAssessmentReportsComponent;
  @ViewChild(DashboardExposedSecretReportsComponent) homeEsr?: DashboardExposedSecretReportsComponent;

  constructor(private mainAppInitService: MainAppInitService) {}

  private _showDistinctValues: boolean = true;

  get showDistinctValues() {
    return this._showDistinctValues;
  }

  set showDistinctValues(value: boolean) {
    this._showDistinctValues = value;
    localStorage.setItem('home.showDistinctValues', value.toString());
  }

  ngOnInit() {
    this.mainAppInitService.backendSettingsDto$.subscribe((updatedBackendSettingsDto) => {
      this.enabledTrivyReports =
        updatedBackendSettingsDto.trivyReportConfigDtos?.filter((x) => x.enabled).map((x) => x.id ?? '') ??
        this.enabledTrivyReports;
    });

    this.showDistinctValues = LocalStorageUtils.getBoolKeyValue('home.showDistinctValues') ?? true;
    this.tabPageActiveIndex = localStorage.getItem('home.tabPageActiveIndex') ?? "0";
  }

  onTabPageChange(event: string | number | undefined) {
    if (event) {
        localStorage.setItem('home.tabPageActiveIndex', event.toString());
    }
  }

  onRefreshData() {
    if (this.homeVr) {
      this.homeVr.loadData();
    }
    if (this.homeCar) {
      this.homeCar.loadData();
    }
    if (this.homeEsr) {
      this.homeEsr.loadData();
    }
  }
}
