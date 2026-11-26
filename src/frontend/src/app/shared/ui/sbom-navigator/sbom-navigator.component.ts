import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';

import {
  sbomNavigatorDependsOnColumns,
  sbomNavigatorDiveInField,
  sbomNavigatorDiveOutField,
  sbomNavigatorUsedByColumns,
} from '../generic-sbom/generic-sbom.constants';
import { GenericSbomReportDetailDto, SbomIndex } from '../generic-sbom/generic-sbom.types';
import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import { TrivyTableColumn, TrivyTableExpandRowData } from '../trivy-table/trivy-table.types';

// "used by" and "depends on" of one SBOM package; an SBOM is a graph, so a package can be used by several others
// the tables are static: the only action is the dive button of each row (up: dive out, down: dive in)
@Component({
  selector: 'app-sbom-navigator',
  standalone: true,
  imports: [TrivyTableComponent],
  templateUrl: './sbom-navigator.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SbomNavigatorComponent {
  index = input.required<SbomIndex>();
  // the package shown
  bomRef = input<string | undefined>();
  stateKey = input.required<string>();

  // the bomRef to refocus generic-sbom on
  diveRequested = output<string>();

  readonly usedByColumns: TrivyTableColumn[] = [...sbomNavigatorUsedByColumns];
  readonly dependsOnColumns: TrivyTableColumn[] = [...sbomNavigatorDependsOnColumns];

  readonly usedBy = computed<GenericSbomReportDetailDto[]>(() => {
    const bomRef = this.bomRef();

    return bomRef ? [...(this.index().parentsByBomRef.get(bomRef) ?? [])] : [];
  });

  // direct dependencies only, deduplicated, in report order
  readonly dependsOn = computed<GenericSbomReportDetailDto[]>(() => {
    const bomRef = this.bomRef();
    const index = this.index();
    const dto = bomRef ? index.byBomRef.get(bomRef) : undefined;

    return [...new Set(dto?.dependsOn ?? [])]
      .map((id) => index.byBomRef.get(id))
      .filter((x): x is GenericSbomReportDetailDto => !!x)
      .sort((a, b) => (index.order.get(a.bomRef) ?? 0) - (index.order.get(b.bomRef) ?? 0));
  });

  // the expanded row shows the package's properties, as in generic-sbom's package list
  readonly usedByRowExpand = signal<TrivyTableExpandRowData<GenericSbomReportDetailDto> | undefined>(undefined);
  readonly dependsOnRowExpand = signal<TrivyTableExpandRowData<GenericSbomReportDetailDto> | undefined>(undefined);

  onUsedByRowExpand(dto: GenericSbomReportDetailDto) {
    this.usedByRowExpand.set(this.buildRowExpand(dto));
  }

  onDependsOnRowExpand(dto: GenericSbomReportDetailDto) {
    this.dependsOnRowExpand.set(this.buildRowExpand(dto));
  }

  private buildRowExpand(dto: GenericSbomReportDetailDto): TrivyTableExpandRowData<GenericSbomReportDetailDto> {
    return {
      rowKey: dto,
      colStyles: [
        { width: '70px', 'min-width': '70px', height: '50px' },
        { 'white-space': 'normal', display: 'flex', 'align-items': 'center', height: '50px' },
      ],
      details: (dto.properties ?? []).map(({ name, value }) => [{ label: name }, { label: value }]),
    };
  }

  onRowAction(event: { row: GenericSbomReportDetailDto; col: string }) {
    if ((event.col === sbomNavigatorDiveInField || event.col === sbomNavigatorDiveOutField) && event.row.bomRef) {
      this.diveRequested.emit(event.row.bomRef);
    }
  }
}
