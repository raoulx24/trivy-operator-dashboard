import {
  Component,
  computed,
  effect,
  HostListener,
  inject,
  input,
  model,
  output,
  signal,
  untracked,
} from '@angular/core';

import { NodeDataDto } from '../fcose/fcose.types';
import { genericSbomReportComparedColumns, genericSbomReportDetailColumns } from './generic-sbom.constants';
import { GenericSbomReportDetailDto, GenericSbomReportDto, GenericSbomReportMinimalDto } from './generic-sbom.types';

import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';

import { FcoseComponent } from '../fcose/fcose.component';
import {
  nonExistingNamespace, PairedOptionsSelectorComponent,
} from '../paired-options-selector/paired-options-selector.component';
import { PairedOptionsDto } from '../paired-options-selector/paired-options-selector.types';
import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import {
  MultiHeaderAction,
  SelectedDtosEvent,
  TrivyTableColumn,
  TrivyTableExpandRowData,
} from '../trivy-table/trivy-table.types';

import { DialogModule } from 'primeng/dialog';
import { SplitterModule } from 'primeng/splitter';
import { TagModule } from 'primeng/tag';

import { KubernetesContextStateService } from '@core/state/kubernetes-context-state.service';
import { GenericReportsCompareComponent } from '../generic-reports-compare/generic-reports-compare.component';

/** Lookups over the details of the current report, built once per report instead of scanning per node. */
interface SbomIndex {
  byBomRef: Map<string, GenericSbomReportDetailDto>;
  /** position of a detail in the report; keeps children in report order */
  order: Map<string, number>;
  /** bomRef -> details that depend on it (report order) */
  parentsByBomRef: Map<string, GenericSbomReportDetailDto[]>;
}

@Component({
  selector: 'app-generic-sbom',
  imports: [
    DialogModule,
    FcoseComponent,
    GenericReportsCompareComponent,
    SeverityCssStyleByIdPipe,
    SplitterModule,
    TagModule,
    TrivyTableComponent,
    VulnerabilityCountPipe,
    PairedOptionsSelectorComponent,
  ],
  templateUrl: './generic-sbom.component.html',
  styleUrl: './generic-sbom.component.scss',
})
export class GenericSbomComponent {
  private readonly kubernetesContextService = inject(KubernetesContextStateService);

  dataDtos = input<GenericSbomReportMinimalDto[]>([]);
  fullSbomDataDto = input<GenericSbomReportDto | undefined>();
  isStatic = input<boolean>(false);
  multiHeaderActions = input<MultiHeaderAction[]>([]);
  stateKey = input.required<string>();
  isCompareDependantOnExternalData = input<boolean>(false);
  compareFirstSelectedDto = input<GenericSbomReportDto | undefined>();
  compareSecondSelectedDto = input<GenericSbomReportDto | undefined>();

  isTableLoading = model<boolean>(false);
  selectedImageId = model<string | undefined>();

  multiActionEventChange = output<string>();
  refreshRequestedChange = output<void>();
  fullSbomDataDtoRequestedChange = output<void>();

  compareFirstDtoRequested = output<string>();
  compareSecondDtoRequested = output<string>();

  namespacedImageDtos?: PairedOptionsDto[];

  private readonly sbomIndex = computed<SbomIndex>(() => {
    const details = this.fullSbomDataDto()?.details ?? [];
    const byBomRef = new Map<string, GenericSbomReportDetailDto>();
    const order = new Map<string, number>();
    const parentsByBomRef = new Map<string, GenericSbomReportDetailDto[]>();

    details.forEach((detail, position) => {
      if (!byBomRef.has(detail.bomRef)) {
        byBomRef.set(detail.bomRef, detail);
        order.set(detail.bomRef, position);
      }
      for (const dependencyId of detail.dependsOn ?? []) {
        const parents = parentsByBomRef.get(dependencyId);
        if (!parents) {
          parentsByBomRef.set(dependencyId, [detail]);
        } else if (parents[parents.length - 1] !== detail) {
          parents.push(detail);
        }
      }
    });

    return { byBomRef, order, parentsByBomRef };
  });

  // #region dependsOnTable data
  selectedSbomDetail: SelectedDtosEvent<GenericSbomReportDetailDto> = { source: 'programmatic', selectedDtos: [] };
  selectedSbomDetailDto?: GenericSbomReportDetailDto;
  dependsOnBoms: GenericSbomReportDetailDto[] = [];
  deletedDependsOnBom: GenericSbomReportDetailDto[] = [];

  dependsOnTableColumns: TrivyTableColumn[] = [...genericSbomReportDetailColumns];
  // #endregion

  hoveredSbomDetailDto?: GenericSbomReportDetailDto;
  nodeDataDtos: NodeDataDto[] = [];
  selectedSbomDetailBomRef?: string;

  isTrivyReportsCompareVisible = signal<boolean>(false);
  comparedTableColumns: TrivyTableColumn[] = [...genericSbomReportComparedColumns];

  screenSize: string = this.getScreenSize();

  constructor() {
    let initialized = false;

    effect(() => {
      // also runs for an empty list, so the selector is cleared when there is nothing to show
      this.getNamespacedImageDtos();
    });

    effect(() => {
      const fullSbomDataDto = this.fullSbomDataDto();
      if (fullSbomDataDto) {
        this.showNode(fullSbomDataDto.rootNodeBomRef ?? '');
      }
    });

    effect(() => {
      const selectedImageId = this.selectedImageId();
      // only a change of the selected image triggers this, not the arrival of the report
      const currentDto = untracked(() => this.fullSbomDataDto());
      // avoid re-selecting the same image
      if (selectedImageId === currentDto?.uid) return;

      if (currentDto) {
        this.fullSbomDataDtoRequestedChange.emit();
        this.resetAllRelatedData();
      }
    });

    effect(() => {
      const ctx = this.kubernetesContextService.selectedContext();
      if (!ctx) return;

      if (!initialized) {
        initialized = true;
        return; // skip initial run
      }

      this.onRefreshRequested();
    });
  }

  onRefreshRequested() {
    this.namespacedImageDtos = undefined;
    this.resetAllRelatedData();

    this.refreshRequestedChange.emit();
  }

  private resetAllRelatedData() {
    this.dependsOnBoms = [];
    this.deletedDependsOnBom = [];
    this.nodeDataDtos = [];
    this.selectedSbomDetailDto = undefined;
    this.selectedSbomDetailBomRef = undefined;
    this.selectedSbomDetail = { source: 'programmatic', selectedDtos: [] };
    this.hoveredSbomDetailDto = undefined;
    this.rowExpandResponse = undefined;
  }

  // #region Image selector data
  getNamespacedImageDtos() {
    this.namespacedImageDtos =
      this.dataDtos()?.map(
        (x) =>
          ({
            uid: x.uid ?? '',
            firstOption: x.resourceNamespace ?? nonExistingNamespace,
            secondOption: `${x.imageName ?? ''}:${x.imageTag ?? ''}`,
            icon: x.hasVulnerabilityReport ? 'security' : undefined,
          }) as PairedOptionsDto,
      ) ?? [];
  }
  // #endregion

  // #region Get Parent and Children Nodes
  /** Builds the table and graph data around a node and selects that node. */
  private showNode(nodeId: string) {
    this.getDataDtosByNodeId(nodeId);
    const baseSbomDetailDto = this.dependsOnBoms.find((x) => x.level == 'Base');
    if (!baseSbomDetailDto) {
      return;
    }
    this.selectedSbomDetailDto = baseSbomDetailDto;
    this.selectedSbomDetailBomRef = baseSbomDetailDto.bomRef ?? undefined;
    this.selectedSbomDetail = { source: 'programmatic', selectedDtos: [baseSbomDetailDto] };
  }

  private getDataDtosByNodeId(nodeId: string) {
    this.dependsOnBoms = [];
    this.deletedDependsOnBom = [];
    const index = this.sbomIndex();
    const sbomDetailDtos: GenericSbomReportDetailDto[] = [];
    const rootSbomDetailDto = index.byBomRef.get(nodeId);
    if (rootSbomDetailDto) {
      const rootSbomExtended: GenericSbomReportDetailDto = {
        ...rootSbomDetailDto,
        level: 'Base',
        group: this.getGroupFromSbomReportDetail(rootSbomDetailDto),
      };
      sbomDetailDtos.push(rootSbomExtended);
      this.getDirectParentsSbomDtos(rootSbomExtended, sbomDetailDtos, index);
      const seenBomRefs = new Set(sbomDetailDtos.map((x) => x.bomRef));
      this.getChildrenSbomDtos(rootSbomExtended, nodeId, sbomDetailDtos, seenBomRefs, index);
    }

    this.dependsOnBoms = sbomDetailDtos;
    this.nodeDataDtos = sbomDetailDtos.map((x) => ({
      id: x.bomRef ?? undefined,
      dependsOn: x.dependsOn,
      name: x.name,
      groupName: x.group,
      isMain: x.bomRef == nodeId,
    }));
  }

  private getDirectParentsSbomDtos(
    sded: GenericSbomReportDetailDto,
    sdeds: GenericSbomReportDetailDto[],
    index: SbomIndex,
  ) {
    const parents = (index.parentsByBomRef.get(sded.bomRef ?? '') ?? []).map((y) => {
      const parentSbom: GenericSbomReportDetailDto = {
        ...y,
        level: 'Ancestor',
        group: this.getGroupFromSbomReportDetail(y),
        dependsOn: [sded.bomRef ?? ''],
      };
      return parentSbom;
    });

    sdeds.push(...parents);
  }

  private getChildrenSbomDtos(
    sded: GenericSbomReportDetailDto,
    baseBomRef: string,
    sdeds: GenericSbomReportDetailDto[],
    seenBomRefs: Set<string>,
    index: SbomIndex,
  ) {
    if (!sded?.dependsOn) {
      return;
    }
    const newSbomDetailDtos = [...new Set(sded.dependsOn)]
      .filter((id) => !seenBomRefs.has(id))
      .map((id) => index.byBomRef.get(id))
      .filter((x): x is GenericSbomReportDetailDto => !!x)
      .sort((a, b) => (index.order.get(a.bomRef) ?? 0) - (index.order.get(b.bomRef) ?? 0))
      .map((y) => {
        const childSbom: GenericSbomReportDetailDto = {
          ...y,
          level: sded.bomRef == baseBomRef ? 'Child' : 'Descendant',
          group: this.getGroupFromSbomReportDetail(y),
        };
        return childSbom;
      });
    sdeds.push(...newSbomDetailDtos);
    newSbomDetailDtos.forEach((x) => seenBomRefs.add(x.bomRef));
    newSbomDetailDtos.forEach((sbomDetailDto) =>
      this.getChildrenSbomDtos(sbomDetailDto, baseBomRef, sdeds, seenBomRefs, index),
    );
  }
  // #endregion

  onHoveredNodeDtoChange(nodeId: string | undefined) {
    this.hoveredSbomDetailDto = nodeId ? this.sbomIndex().byBomRef.get(nodeId) : undefined;
  }

  onActiveNodeIdChange(event: string) {
    if (this.sbomIndex().byBomRef.has(event)) {
      this.showNode(event);
    }
  }

  private getGroupFromSbomReportDetail(
    dto: GenericSbomReportDetailDto,
  ): string {
    const properties = dto.properties ?? {};

    if (Object.values(properties).includes('nuget')) {
      return `${dto.name?.split('.')[0] ?? ''} (nuget)`;
    }

    if (Object.values(properties).includes('dotnet-core')) {
      return `${dto.name?.split('.')[0] ?? ''} (dotnet-core)`;
    }

    if (Object.values(properties).includes('gobinary')) {
      return `${dto.name?.split('/')[0] ?? ''} (gobinary)`;
    }

    const todGroup = properties['tod.group'];
    if (todGroup) {
      return todGroup;
    }

    return '';
  }

  onTableSelectedRowChange(event: SelectedDtosEvent<GenericSbomReportDetailDto>) {
    if (event.selectedDtos.length == 0) {
      this.selectedSbomDetailDto = undefined;
      this.selectedSbomDetailBomRef = undefined;
    } else {
      const dataDto = event.selectedDtos[0];
      this.selectedSbomDetailDto = dataDto;
      this.selectedSbomDetailBomRef = dataDto.bomRef ?? undefined;
    }
  }

  onNodeIdChange(nodeId: string | undefined) {
    if (nodeId) {
      const sbomReportDetailDto = this.dependsOnBoms?.find((x) => x.bomRef == nodeId);
      const prevSelectedDto = this.selectedSbomDetailBomRef;
      const source = prevSelectedDto === sbomReportDetailDto?.bomRef ? 'user' : 'programmatic';
      if (sbomReportDetailDto) {
        this.selectedSbomDetailDto = sbomReportDetailDto;
        this.selectedSbomDetail = { source: source, selectedDtos: [sbomReportDetailDto] };
      }
    } else {
      this.selectedSbomDetailDto = undefined;
      this.selectedSbomDetail = { source: 'user', selectedDtos: [] };
    }
    this.selectedSbomDetailBomRef = nodeId;
  }

  onDeletedNodeIds(nodeIds: string[] | undefined) {
    this.deletedDependsOnBom.push(...(this.dependsOnBoms?.filter((x) => nodeIds?.includes(x.bomRef ?? '')) ?? []));
    this.dependsOnBoms = this.dependsOnBoms?.filter((x) => !nodeIds?.includes(x.bomRef ?? ''));
  }

  onUndeletedNodeIds(nodeIds: string[] | undefined) {
    const undeletedSboms = this.deletedDependsOnBom.filter((x) => nodeIds?.includes(x.bomRef ?? ''));
    this.deletedDependsOnBom = this.deletedDependsOnBom.filter((x) => !nodeIds?.includes(x.bomRef ?? ''));
    this.dependsOnBoms = [...(this.dependsOnBoms || []), ...undeletedSboms];
  }

  onCompareFirstDtoRequested(uid: string) {
    this.compareFirstDtoRequested.emit(uid);
  }

  onCompareSecondDtoRequested(uid: string) {
    this.compareSecondDtoRequested.emit(uid);
  }

  onMultiHeaderActionRequested(event: string) {
    switch (event) {
      case 'Dive In': {
        const bomRefId = this.selectedSbomDetailDto?.bomRef ?? undefined;
        if (bomRefId) {
          this.onActiveNodeIdChange(bomRefId);
        }
        break;
      }
      case 'Compare with...':
        this.goToComparePage();
        break;
      default:
        // let the parent handle it
        this.multiActionEventChange.emit(event);
    }
  }

  private goToComparePage() {
    if (!this.fullSbomDataDto() || !this.namespacedImageDtos) return;

    this.isTrivyReportsCompareVisible.set(true);
  }

  rowExpandResponse?: TrivyTableExpandRowData<GenericSbomReportDetailDto>;
  onRowExpandChange(dto: GenericSbomReportDetailDto) {
    this.rowExpandResponse = {
      rowKey: dto,
      colStyles: [
        { width: '70px', 'min-width': '70px', height: '50px' },
        {
          'white-space': 'normal',
          display: 'flex',
          'align-items': 'center',
          height: '50px',
        },
      ],
      details: Object.entries(dto.properties ?? {}).map(([key, value]) => [
        { label: key },
        { label: value },
      ]),
    };
  }

  // screen size
  @HostListener('window:resize', [])
  onResize() {
    this.screenSize = this.getScreenSize();
  }

  getScreenSize(): string {
    const cssVarValue = getComputedStyle(document.documentElement).getPropertyValue('--tod-screen-width-sm').trim(); // Get and clean the CSS variable value

    const threshold = parseInt(cssVarValue, 10); // Convert it to a number

    return window.innerWidth < threshold ? 'sm' : 'lg';
  }
}
