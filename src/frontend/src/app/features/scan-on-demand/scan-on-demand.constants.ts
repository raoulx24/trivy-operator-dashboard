import { OnDemandScanDto } from '@api/models/on-demand-scan-dto';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';
import { SemaphoreStatusUtils } from '@shared/utils/semaphore-status.utils';

// the report types, as the backend names them (ScanReportType), and how the page shows them
export const SCAN_REPORT_TYPE_LABELS: Record<string, string> = {
  VulnerabilityReport: 'Vulnerabilities',
  ExposedSecretReport: 'Exposed secrets',
  SbomReport: 'SBOM',
};

const SCAN_REPORT_TYPE_SHORT: Record<string, string> = {
  VulnerabilityReport: 'VR',
  ExposedSecretReport: 'ES',
  SbomReport: 'SBOM',
};

export const FINAL_SCAN_STATES: ReadonlySet<string> = new Set(['Completed', 'Failed', 'TimedOut', 'Cancelled']);

export const SCAN_POLL_INTERVAL_MS = 5000;

// the inspected result survives a reload (the page has no query params)
export const INSPECTED_DIGEST_STORAGE_KEY = 'scanOnDemand.inspectedDigest';

// a scan as a row of the scans table: the dto plus the derived columns and the fields of the action buttons
export interface OnDemandScanRow extends OnDemandScanDto {
  duration: string;
  reports: string;
  inspect: string;
  rescan: string;
  cancel: string;
  delete: string;
}

export const INSPECT_FIELD = 'inspect';
export const RESCAN_FIELD = 'rescan';
export const CANCEL_FIELD = 'cancel';
export const DELETE_FIELD = 'delete';

export const isFinalScanState = (state: string | undefined): boolean => !!state && FINAL_SCAN_STATES.has(state);

export const reportTypeLabel = (type: string): string => SCAN_REPORT_TYPE_LABELS[type] ?? type;

// the color of the rail dot, the same as the state tag of the table
export const scanStateColor = (state: string): string =>
  SemaphoreStatusUtils.getCssColorByName(
    { Running: 'Neutral', Completed: 'Green', Failed: 'Red', TimedOut: 'Yellow' }[state] ?? 'Unknown',
  );

// e.g. docker.io/library/nginx:latest -> nginx:latest
export const shortImageName = (image: string): string => image.split('/').pop() ?? image;

// from the start of the scan (or its creation, while pending) to its end (or now, while not final)
export const scanDuration = (scan: OnDemandScanDto, now: number = Date.now()): string => {
  if (scan.state === 'Pending') {
    return '–';
  }

  const start = Date.parse(scan.startedAt ?? scan.createdAt);
  const end = scan.finishedAt ? Date.parse(scan.finishedAt) : now;
  const seconds = Math.max(0, Math.round((end - start) / 1000));

  return `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}`;
};

// requested types; the ones requested but not produced (completed scans only) are marked with a '-'
export const scanReports = (scan: OnDemandScanDto): string =>
  scan.requestedReportTypes
    .map((x) =>
      scan.state === 'Completed' && !scan.producedReportTypes.includes(x)
        ? `-${SCAN_REPORT_TYPE_SHORT[x] ?? x}`
        : (SCAN_REPORT_TYPE_SHORT[x] ?? x),
    )
    .join(' ');

export const toScanRow = (scan: OnDemandScanDto): OnDemandScanRow => ({
  ...scan,
  duration: scanDuration(scan),
  reports: scanReports(scan),
  inspect: '',
  rescan: '',
  cancel: '',
  delete: '',
});

export const onDemandScanColumns: readonly TrivyTableColumn[] = [
  {
    field: 'image',
    header: 'Image',
    isFilterable: true,
    isSortable: true,
    style: 'white-space: nowrap; text-overflow: ellipsis; overflow: hidden; min-width: 16rem;',
    renderType: 'standard',
  },
  {
    field: 'state',
    header: 'State',
    isFilterable: true,
    isSortable: true,
    tagSet: 'scanState',
    style: 'width: 8rem; max-width: 8rem;',
    renderType: 'tag',
  },
  {
    field: 'createdAt',
    header: 'Created',
    isFilterable: false,
    isSortable: true,
    style: 'width: 140px; max-width: 140px;',
    renderType: 'dateTime',
  },
  {
    field: 'duration',
    header: 'Took',
    isFilterable: false,
    isSortable: false,
    style: 'width: 5rem; max-width: 5rem;',
    renderType: 'standard',
  },
  {
    field: 'reports',
    header: 'Reports',
    isFilterable: false,
    isSortable: false,
    style: 'width: 9rem; max-width: 9rem;',
    renderType: 'standard',
  },
  {
    field: 'failureMessage',
    header: 'Message',
    isFilterable: true,
    isSortable: false,
    style: 'white-space: nowrap; text-overflow: ellipsis; overflow: hidden; min-width: 12rem;',
    renderType: 'standard',
  },
  {
    field: INSPECT_FIELD,
    header: 'Inspect',
    isFilterable: false,
    isSortable: false,
    actionIcon: 'pi pi-eye',
    style: 'width: 5.5rem; max-width: 5.5rem; text-align: center;',
    renderType: 'action',
  },
  {
    field: RESCAN_FIELD,
    header: 'Re-scan',
    isFilterable: false,
    isSortable: false,
    actionIcon: 'pi pi-refresh',
    style: 'width: 5.5rem; max-width: 5.5rem; text-align: center;',
    renderType: 'action',
  },
  {
    // a pending or running scan (the backend refuses a finished one)
    field: CANCEL_FIELD,
    header: 'Cancel',
    isFilterable: false,
    isSortable: false,
    actionIcon: 'pi pi-times',
    style: 'width: 5.5rem; max-width: 5.5rem; text-align: center;',
    renderType: 'action',
  },
  {
    // the scan record only; a finished scan (the backend refuses a pending or running one)
    field: DELETE_FIELD,
    header: 'Delete',
    isFilterable: false,
    isSortable: false,
    actionIcon: 'pi pi-trash',
    style: 'width: 5.5rem; max-width: 5.5rem; text-align: center;',
    renderType: 'action',
  },
];
