import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { TagModule } from 'primeng/tag';

import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { GenericSbomReportDetailDto } from '../generic-sbom/generic-sbom.types';

// one line about one SBOM package: name, version, the severity counts and the purl
@Component({
  selector: 'app-sbom-package-box',
  standalone: true,
  imports: [TagModule, SeverityCssStyleByIdPipe, VulnerabilityCountPipe],
  templateUrl: './sbom-package-box.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SbomPackageBoxComponent {
  // the hovered node in graph view, otherwise the selected package; nothing is shown when undefined
  dto = input<GenericSbomReportDetailDto | undefined>();
}
