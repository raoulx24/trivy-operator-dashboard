import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

import { OnDemandScanDto } from '@api/models/on-demand-scan-dto';

import { scanStateColor, shortImageName } from '../scan-on-demand.constants';

// the scans list folded into a narrow strip: a state dot and a short image name per scan
// only completed scans can be selected (they are the ones with a result)
@Component({
  selector: 'app-scan-on-demand-rail',
  standalone: true,
  imports: [ButtonModule, TooltipModule],
  templateUrl: './scan-on-demand-rail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScanOnDemandRailComponent {
  scans = input<OnDemandScanDto[]>([]);
  // the scan the shown result came from; by scan, not by digest: the same image scanned twice is two rows, one result
  selectedScanId = input<string | undefined>();

  // the digest of the selected completed scan
  selected = output<string>();
  expandRequested = output<void>();

  readonly stateColor = scanStateColor;
  readonly shortName = shortImageName;

  onClick(scan: OnDemandScanDto) {
    if (scan.state === 'Completed' && scan.imageDigest) {
      this.selected.emit(scan.imageDigest);
    }
  }
}
