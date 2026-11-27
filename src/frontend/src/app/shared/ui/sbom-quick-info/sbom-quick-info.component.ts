import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { TagModule } from 'primeng/tag';

import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { GenericSbomReportDetailDto, SbomIndex } from '../generic-sbom/generic-sbom.types';
import { TrivyBoxComponent } from '../trivy-box/trivy-box.component';

// the node hovered in an SBOM graph, in a box: always the same fields, so the box never changes its size (the graph
// under it would be laid out again); nothing hovered: n/a, the severities -1. It is hover only, so the mouse never reaches it:
// what is cut is cut (no tooltip, no copy)
@Component({
  selector: 'app-sbom-quick-info',
  standalone: true,
  imports: [TagModule, SeverityCssStyleByIdPipe, VulnerabilityCountPipe, TrivyBoxComponent],
  templateUrl: './sbom-quick-info.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // the box title sits on the top border, above the box: the room for it, so it does not overlap what is above
  host: { class: 'block pt-2' },
})
export class SbomQuickInfoComponent {
  // the hovered node; undefined: n/a, the severities -1
  dto = input<GenericSbomReportDetailDto | undefined>();
  // for the used by / depends on counts
  index = input.required<SbomIndex>();

  readonly severityIds = [0, 1, 2, 3, 4];

  readonly usedByCount = computed<number | undefined>(() => {
    const dto = this.dto();

    return dto ? (this.index().parentsByBomRef.get(dto.bomRef)?.length ?? 0) : undefined;
  });

  // direct dependencies, deduplicated, only those in the report (as the navigator counts them)
  readonly dependsOnCount = computed<number | undefined>(() => {
    const dto = this.dto();
    const index = this.index();

    return dto ? [...new Set(dto.dependsOn ?? [])].filter((id) => index.byBomRef.has(id)).length : undefined;
  });

  // -1: no count (as the tables show it)
  severityCount(dto: GenericSbomReportDetailDto, severityId: number): number {
    switch (severityId) {
      case 0:
        return dto.criticalCount ?? -1;
      case 1:
        return dto.highCount ?? -1;
      case 2:
        return dto.mediumCount ?? -1;
      case 3:
        return dto.lowCount ?? -1;
      default:
        return dto.unknownCount ?? -1;
    }
  }
}
