import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  HostListener,
  inject,
  signal,
  untracked,
} from '@angular/core';

import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

import { OnDemandScanDto } from '@api/models/on-demand-scan-dto';
import { OnDemandScanResultDto } from '@api/models/on-demand-scan-result-dto';
import { ScanOnDemandService } from '@api/services/scan-on-demand.service';
import { injectLogger } from '@core/logging/inject-logger';
import { MainAppInitService } from '@core/services/main-app-init.service';
import { ScanOnDemandHubService } from '@core/services/scan-on-demand-hub.service';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';
import { LocalStorageUtils } from '@shared/utils/local-storage.utils';

import { ScanOnDemandFormComponent, ScanSubmitRequest } from './scan-on-demand-form/scan-on-demand-form.component';
import { ScanOnDemandResultComponent } from './scan-on-demand-result/scan-on-demand-result.component';
import {
  CANCEL_FIELD,
  DELETE_FIELD,
  INSPECT_FIELD,
  INSPECTED_DIGEST_STORAGE_KEY,
  isFinalScanState,
  onDemandScanColumns,
  OnDemandScanRow,
  RESCAN_FIELD,
  SCAN_POLL_INTERVAL_MS,
  toScanRow,
} from './scan-on-demand.constants';

// one page, two views: the scans list with the submit form and, once a result is inspected, that result (a switcher in
// its header goes to another completed scan, a button back to the list)
// the list is read again whenever the hub says it changed (also while a result is inspected: the rail follows); while
// the hub is not connected, it is polled instead, as long as any scan is pending or running
@Component({
  selector: 'app-scan-on-demand',
  standalone: true,
  imports: [
    ConfirmDialogModule,
    ScanOnDemandFormComponent,
    ScanOnDemandResultComponent,
    TrivyTableComponent,
  ],
  providers: [ConfirmationService],
  templateUrl: './scan-on-demand.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScanOnDemandComponent {
  private readonly api = inject(ScanOnDemandService);
  private readonly hub = inject(ScanOnDemandHubService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly log = injectLogger('Scan on demand');
  private readonly destroyRef = inject(DestroyRef);

  private readonly settings = inject(MainAppInitService).backendSettingsDto;
  readonly isEnabled = computed(() => this.settings().isScanOnDemandEnabled);
  readonly unavailableReason = computed(() => this.settings().scanOnDemandUnavailableReason ?? undefined);
  readonly allowedTypes = computed(() => this.settings().scanOnDemandAllowedReportTypes ?? []);

  readonly scans = signal<OnDemandScanDto[]>([]);
  readonly rows = computed<OnDemandScanRow[]>(() => this.scans().map(toScanRow));
  readonly isScansLoading = signal<boolean>(false);
  readonly columns: TrivyTableColumn[] = [...onDemandScanColumns];

  readonly isSubmitting = signal<boolean>(false);
  readonly submitError = signal<string | undefined>(undefined);

  readonly inspectedDigest = signal<string | undefined>(LocalStorageUtils.getItem(INSPECTED_DIGEST_STORAGE_KEY) ?? undefined);
  readonly result = signal<OnDemandScanResultDto | undefined>(undefined);
  readonly isCollapsed = computed(() => !!this.inspectedDigest());
  // the scan the inspected result came from (its requested types)
  readonly resultScan = computed(() => {
    const scanId = this.result()?.lastScanId;
    return scanId ? this.scans().find((x) => x.id === scanId) : undefined;
  });

  private pollTimer: ReturnType<typeof setTimeout> | undefined;

  constructor() {
    this.destroyRef.onDestroy(() => clearTimeout(this.pollTimer));

    // the first load, every change pushed by the hub, and every switch between pushed and polled
    effect(() => {
      this.hub.changes();
      this.hub.isConnected();
      if (this.isEnabled()) {
        untracked(() => this.loadScans());
      }
    });

    effect(() => {
      const digest = this.inspectedDigest();
      this.result.set(undefined);
      if (digest) {
        this.loadResult(digest);
      }
    });
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    if (this.isCollapsed()) {
      this.expand();
    }
  }

  // #region list
  loadScans() {
    clearTimeout(this.pollTimer);
    this.isScansLoading.set(this.scans().length === 0);

    this.api.getOnDemandScans().subscribe({
      next: (scans) => {
        this.scans.set(scans);
        this.isScansLoading.set(false);
        // the fallback: no hub, so poll while anything can still change
        if (!this.hub.isConnected() && scans.some((x) => !isFinalScanState(x.state))) {
          this.pollTimer = setTimeout(() => this.loadScans(), SCAN_POLL_INTERVAL_MS);
        }
      },
      error: (error) => {
        this.isScansLoading.set(false);
        this.log.error('The scans could not be loaded.', { error });
      },
    });
  }

  onRowAction(event: { row: OnDemandScanRow; col: string }) {
    if (event.col === RESCAN_FIELD) {
      this.rescan(event.row);
    } else if (event.col === CANCEL_FIELD) {
      this.cancelScan(event.row);
    } else if (event.col === DELETE_FIELD) {
      this.deleteScan(event.row);
    } else if (event.col === INSPECT_FIELD) {
      if (event.row.state === 'Completed' && event.row.imageDigest) {
        this.inspect(event.row.imageDigest);
      } else {
        this.log.info(`Only a completed scan has a result (this one is ${event.row.state}).`, { toast: true });
      }
    }
  }

  // a pending or running scan; the backend cancels it right after (its pod deleted, nothing stored) and the hub pushes
  // the change; 409 when it finished meanwhile
  cancelScan(scan: OnDemandScanDto) {
    if (isFinalScanState(scan.state)) {
      this.log.info(`Only a pending or running scan can be cancelled (this one is ${scan.state}).`, { toast: true });
      return;
    }

    this.confirmation.confirm({
      header: 'Cancel the scan',
      message: `Cancel the scan of ${scan.image}? Its pod is deleted and nothing is stored.`,
      icon: 'pi pi-times',
      acceptButtonProps: { label: 'Cancel scan', severity: 'danger' },
      rejectButtonProps: { label: 'Keep it', text: true },
      accept: () =>
        this.api.cancelOnDemandScan({ id: scan.id }).subscribe({
          next: () => this.loadScans(),
          error: (error: unknown) => {
            const detail = this.problemDetail(error);
            this.log.error(detail ?? 'The scan could not be cancelled.', { error });
            this.loadScans();
          },
        }),
    });
  }

  // the scan record only (its result, if any, stays); the backend refuses a pending or running scan (409)
  deleteScan(scan: OnDemandScanDto) {
    if (!isFinalScanState(scan.state)) {
      this.log.info(`Only a finished scan can be deleted (this one is ${scan.state}).`, { toast: true });
      return;
    }

    this.confirmation.confirm({
      header: 'Delete the scan',
      message: `Delete the scan of ${scan.image} from the list? Its result, if any, stays.`,
      icon: 'pi pi-trash',
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', text: true },
      accept: () =>
        this.api.deleteOnDemandScan({ id: scan.id }).subscribe({
          next: () => this.loadScans(),
          error: (error: unknown) => {
            const detail = this.problemDetail(error);
            this.log.error(detail ?? 'The scan could not be deleted.', { error });
            this.loadScans();
          },
        }),
    });
  }
  // #endregion

  // #region submit
  submit(request: ScanSubmitRequest) {
    this.isSubmitting.set(true);
    this.submitError.set(undefined);

    this.api.submitOnDemandScan({ body: { image: request.image, reportTypes: request.types } }).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        if (response.isExisting) {
          this.log.info('The same scan is already waiting or running.', { toast: true });
        }
        this.loadScans();
      },
      error: (error: unknown) => {
        this.isSubmitting.set(false);
        this.submitError.set(this.problemDetail(error) ?? 'The scan could not be submitted.');
      },
    });
  }

  rescan(scan: OnDemandScanDto) {
    this.submit({ image: scan.image, types: scan.requestedReportTypes });
  }
  // #endregion

  // #region result
  inspect(digest: string) {
    LocalStorageUtils.setItem(INSPECTED_DIGEST_STORAGE_KEY, digest);
    this.inspectedDigest.set(digest);
  }

  expand() {
    LocalStorageUtils.removeItem(INSPECTED_DIGEST_STORAGE_KEY);
    this.inspectedDigest.set(undefined);
  }

  onRescanResult() {
    const scan = this.resultScan();
    const image = this.result()?.image;
    if (scan) {
      this.rescan(scan);
    } else if (image) {
      this.submit({ image, types: this.allowedTypes() });
    }
  }

  onDeleteResult() {
    const digest = this.inspectedDigest();
    if (!digest) {
      return;
    }

    this.confirmation.confirm({
      header: 'Delete the result',
      message: `Delete the result of ${this.result()?.image ?? digest} and its scans? This can't be undone.`,
      icon: 'pi pi-trash',
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', text: true },
      accept: () =>
        this.api.deleteOnDemandScanResult({ digest }).subscribe({
          next: () => {
            this.expand();
            this.loadScans();
          },
          error: (error) => this.log.error('The result could not be deleted.', { error }),
        }),
    });
  }

  private loadResult(digest: string) {
    this.api.getOnDemandScanResult({ digest }).subscribe({
      next: (result) => {
        if (this.inspectedDigest() === digest) {
          this.result.set(result);
        }
      },
      error: (error: unknown) => {
        // expired (TTL) or deleted elsewhere: back to the list
        if (error instanceof HttpErrorResponse && error.status === 404) {
          this.log.info('That result no longer exists (expired or deleted).', { toast: true });
        } else {
          this.log.error('The result could not be loaded.', { error });
        }
        this.expand();
      },
    });
  }
  // #endregion

  private problemDetail(error: unknown): string | undefined {
    if (error instanceof HttpErrorResponse) {
      const body = error.error as { detail?: string; title?: string } | null;
      return body?.detail ?? body?.title ?? undefined;
    }
    return undefined;
  }
}
