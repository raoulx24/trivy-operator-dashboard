import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal } from '@angular/core';

import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { SelectButtonModule } from 'primeng/selectbutton';
import { TagModule } from 'primeng/tag';

import { OnDemandScanDto } from '@api/models/on-demand-scan-dto';
import { OnDemandScanResultDto } from '@api/models/on-demand-scan-result-dto';
import { exposedSecretReportDetailColumns } from '@features/trivy-reports/exposed-secrets/exposed-secret-reports.constants';
import { vulnerabilityReportDetailColumns } from '@features/trivy-reports/vulnerability/vulnerability-reports.constants';
import { FriendlyTimePipe } from '@shared/pipes/local-time.pipe';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '@shared/pipes/vulnerability-count.pipe';
import { ScrollerComponent } from '@shared/ui/scroller/scroller.component';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { GenericSbomComponent } from '@shared/ui/generic-sbom/generic-sbom.component';
import { GenericSbomReportDto } from '@shared/ui/generic-sbom/generic-sbom.types';
import { MultiHeaderAction, TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';

import { reportTypeLabel, scanReports, scanStateColor, shortImageName } from '../scan-on-demand.constants';

interface ResultTab {
  type: string;
  label: string;
  // requested but not produced by trivy-operator (or not requested at all, for an older result): not selectable
  isDisabled: boolean;
}

const TAB_ORDER = ['VulnerabilityReport', 'ExposedSecretReport', 'SbomReport'];

// one scan result: a two-line header strip and one select button option per requested report type
// the content of a report exists only while it is selected: the graph needs a visible container to lay out
@Component({
  selector: 'app-scan-on-demand-result',
  standalone: true,
  imports: [
    FormsModule,
    ButtonModule,
    SelectButtonModule,
    SelectModule,
    TagModule,
    ScrollerComponent,
    TrivyBoxComponent,
    FriendlyTimePipe,
    SeverityCssStyleByIdPipe,
    VulnerabilityCountPipe,
    GenericSbomComponent,
    TrivyTableComponent,
  ],
  templateUrl: './scan-on-demand-result.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block h-full' },
})
export class ScanOnDemandResultComponent {
  result = input.required<OnDemandScanResultDto>();
  // the scan the result came from (its requested types); undefined once that scan record expired
  scan = input<OnDemandScanDto | undefined>();

  // the scans list, for the switcher: its completed scans
  scans = input<OnDemandScanDto[]>([]);

  // the digest of the completed scan picked in the switcher
  scanSelected = output<string>();
  allScansRequested = output<void>();
  rescanRequested = output<void>();
  deleteRequested = output<void>();

  readonly vulnerabilityColumns: TrivyTableColumn[] = [...vulnerabilityReportDetailColumns];
  readonly exposedSecretColumns: TrivyTableColumn[] = [...exposedSecretReportDetailColumns];
  readonly sbomActions: MultiHeaderAction[] = [
    { label: 'Dive In', icon: 'pi pi-arrow-down-right', enabledIfRowSelected: true },
    { label: '', specialAction: 'Clear Selection' },
    { label: '', specialAction: 'Clear Sort/Filters' },
    { label: '', specialAction: 'Collapse All' },
  ];

  // the generated contract marks the reports as always present; the backend sends null for the missing ones
  readonly vulnerabilityReport = computed(() => this.result().vulnerabilityReport ?? undefined);
  readonly exposedSecretReport = computed(() => this.result().exposedSecretReport ?? undefined);
  readonly sbomReport = computed<GenericSbomReportDto | undefined>(() => this.result().sbomReport ?? undefined);

  readonly tabs = computed<ResultTab[]>(() => {
    const present: Record<string, boolean> = {
      VulnerabilityReport: !!this.vulnerabilityReport(),
      ExposedSecretReport: !!this.exposedSecretReport(),
      SbomReport: !!this.sbomReport(),
    };
    const requested = this.scan()?.requestedReportTypes ?? TAB_ORDER.filter((x) => present[x]);

    return TAB_ORDER.filter((x) => requested.includes(x) || present[x]).map((type) => ({
      type,
      label: this.tabLabel(type),
      isDisabled: !present[type],
    }));
  });

  readonly scanOptions = computed(() =>
    this.scans()
      .filter((x) => x.state === 'Completed' && !!x.imageDigest)
      .map((x) => ({
        id: x.id,
        digest: x.imageDigest as string,
        label: x.image,
        shortLabel: shortImageName(x.image),
        createdAt: x.createdAt,
        reports: scanReports(x),
        color: scanStateColor(x.state),
      })),
  );

  readonly activeTab = signal<string>(TAB_ORDER[0]);

  constructor() {
    // a new result: the first tab that has a report
    effect(() => {
      const first = this.tabs().find((x) => !x.isDisabled)?.type ?? TAB_ORDER[0];
      this.activeTab.set(first);
    });
  }

  onScanChange(id: string | undefined) {
    const option = this.scanOptions().find((x) => x.id === id);
    if (option) {
      this.scanSelected.emit(option.digest);
    }
  }

  onTabChange(value: string | number | undefined) {
    if (typeof value === 'string') {
      this.activeTab.set(value);
    }
  }

  private tabLabel(type: string): string {
    const count =
      type === 'VulnerabilityReport'
        ? this.vulnerabilityReport()?.details.length
        : type === 'ExposedSecretReport'
          ? this.exposedSecretReport()?.details.length
          : this.sbomReport()?.details.length;

    return count === undefined ? reportTypeLabel(type) : `${reportTypeLabel(type)} ${count}`;
  }
}
