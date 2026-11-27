import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';

import { CytoscapeGraphComponent } from '../cytoscape-graph/cytoscape-graph.component';
import { NodeDataDto } from '../cytoscape-graph/cytoscape-graph.types';
import {
  sbomNavigatorColumns,
  sbomNavigatorDiveField,
  sbomNavigatorSelectField,
} from '../generic-sbom/generic-sbom.constants';
import { GenericSbomReportDetailDto, SbomIndex } from '../generic-sbom/generic-sbom.types';
import { SbomQuickInfoComponent } from '../sbom-quick-info/sbom-quick-info.component';
import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import { TrivyTableColumn, TrivyTableExpandRowData } from '../trivy-table/trivy-table.types';
import { SbomNeighbourLevel, SbomNeighbourRow } from './sbom-navigator.types';

// the direct neighbours of one SBOM package (an SBOM is a graph, so a package can be used by several others): one
// table (used by, then depends on), the quick info of the node hovered in the graph, and a small layered graph
// the table is static: the only actions are the dive and the select buttons of each row; in the graph a click only selects (in the graph)
// and a double click dives
@Component({
  selector: 'app-sbom-navigator',
  standalone: true,
  imports: [CytoscapeGraphComponent, SbomQuickInfoComponent, TrivyTableComponent],
  templateUrl: './sbom-navigator.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a flex column of the height of the host's cell: the table and the graph share what the quick info leaves
  host: { class: 'flex flex-col gap-1 min-h-0' },
})
export class SbomNavigatorComponent {
  index = input.required<SbomIndex>();
  // the package shown
  bomRef = input<string | undefined>();
  // the level of each package of the dive set; a neighbour that is not in it cannot be selected on the left
  levelByBomRef = input<Map<string, SbomNeighbourLevel>>(new Map());
  stateKey = input.required<string>();

  // the bomRef to refocus generic-sbom on
  diveRequested = output<string>();
  // the bomRef to select in generic-sbom's list (the Select button of a row; the navigator then shows that package)
  selectRequested = output<string>();

  readonly columns: TrivyTableColumn[] = [...sbomNavigatorColumns];

  readonly rows = computed<SbomNeighbourRow[]>(() => {
    const bomRef = this.bomRef();
    const index = this.index();
    if (!bomRef) {
      return [];
    }
    const levelByBomRef = this.levelByBomRef();
    const toRow = (dto: GenericSbomReportDetailDto, direction: SbomNeighbourRow['direction']): SbomNeighbourRow => ({
      ...dto,
      rowKey: `${direction}|${dto.bomRef}`,
      direction,
      directionIcon: direction === 'Used by' ? 'pi pi-arrow-up' : 'pi pi-arrow-down',
      isSelectDisabled: !levelByBomRef.has(dto.bomRef),
    });

    return [
      ...this.usedBy(bomRef, index).map((x) => toRow(x, 'Used by')),
      ...this.dependsOn(bomRef, index).map((x) => toRow(x, 'Depends on')),
    ];
  });

  // the small graph: the shown package (the main node), its parents (edges to it) and its children (edges from it)
  // only; whether a node has children of its own comes from the report, so they stay divable
  readonly nodeDataDtos = computed<NodeDataDto[]>(() => {
    const bomRef = this.bomRef();
    const index = this.index();
    const dto = bomRef ? index.byBomRef.get(bomRef) : undefined;
    if (!bomRef || !dto) {
      return [];
    }
    const hasChildren = (x: GenericSbomReportDetailDto) => (x.dependsOn?.length ?? 0) > 0;
    const children = this.dependsOn(bomRef, index);
    const childIds = new Set(children.map((x) => x.bomRef));
    // a parent that is also a child (a cycle) is drawn once, with the children
    const parents = this.usedBy(bomRef, index).filter((x) => !childIds.has(x.bomRef) && x.bomRef !== bomRef);

    return [
      { id: bomRef, name: dto.name, isMain: true, dependsOn: children.map((x) => x.bomRef), isLeaf: !hasChildren(dto) },
      ...parents.map((x) => ({ id: x.bomRef, name: x.name, dependsOn: [bomRef], isLeaf: !hasChildren(x) })),
      ...children
        .filter((x) => x.bomRef !== bomRef)
        .map((x) => ({ id: x.bomRef, name: x.name, dependsOn: [], isLeaf: !hasChildren(x) })),
    ];
  });

  // the quick info: the node hovered in the graph only (the table does not feed it)
  readonly hoveredDto = signal<GenericSbomReportDetailDto | undefined>(undefined);

  // the expanded row shows the package's properties, as in generic-sbom's package list
  readonly rowExpand = signal<TrivyTableExpandRowData<SbomNeighbourRow> | undefined>(undefined);

  onRowExpand(row: SbomNeighbourRow) {
    this.rowExpand.set({
      rowKey: row,
      colStyles: [
        { width: '70px', 'min-width': '70px', height: '50px' },
        { 'white-space': 'normal', display: 'flex', 'align-items': 'center', height: '50px' },
      ],
      details: (row.properties ?? []).map(({ name, value }) => [{ label: name }, { label: value }]),
    });
  }

  onRowAction(event: { row: SbomNeighbourRow; col: string }) {
    if (!event.row.bomRef) {
      return;
    }
    if (event.col === sbomNavigatorDiveField) {
      this.diveRequested.emit(event.row.bomRef);
    } else if (event.col === sbomNavigatorSelectField && !event.row.isSelectDisabled) {
      this.selectRequested.emit(event.row.bomRef);
    }
  }

  onHoveredNodeIdChange(nodeId: string | undefined) {
    this.hoveredDto.set(nodeId ? this.index().byBomRef.get(nodeId) : undefined);
  }

  // a double click on a node of the small graph: a full dive; also on the main node (the shown package), which is
  // usually not the dive root (generic-sbom ignores a dive into the current root)
  onActiveNodeIdChange(nodeId: string) {
    this.diveRequested.emit(nodeId);
  }

  private usedBy(bomRef: string, index: SbomIndex): GenericSbomReportDetailDto[] {
    return [...(index.parentsByBomRef.get(bomRef) ?? [])];
  }

  // direct dependencies only, deduplicated, in report order
  private dependsOn(bomRef: string, index: SbomIndex): GenericSbomReportDetailDto[] {
    const dto = index.byBomRef.get(bomRef);

    return [...new Set(dto?.dependsOn ?? [])]
      .map((id) => index.byBomRef.get(id))
      .filter((x): x is GenericSbomReportDetailDto => !!x)
      .sort((a, b) => (index.order.get(a.bomRef) ?? 0) - (index.order.get(b.bomRef) ?? 0));
  }
}
