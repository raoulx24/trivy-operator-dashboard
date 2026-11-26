import {
  Component,
  computed,
  effect,
  HostListener,
  inject,
  input,
  model,
  OnInit,
  output,
  signal,
  untracked,
} from '@angular/core';

import { NodeDataDto } from '../fcose/fcose.types';
import { FcoseNavigation } from '../fcose/fcose.navigation';
import { genericSbomReportComparedColumns, genericSbomReportSlimDetailColumns } from './generic-sbom.constants';
import {
  GenericSbomReportDetailDto,
  GenericSbomReportDto,
  GenericSbomReportMinimalDto,
  GenericSbomViewMode,
  SbomIndex,
} from './generic-sbom.types';

import { FcoseComponent } from '../fcose/fcose.component';
import { SbomNavigatorComponent } from '../sbom-navigator/sbom-navigator.component';
import { SbomPackageBoxComponent } from '../sbom-package-box/sbom-package-box.component';
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

import { BreadcrumbItemClickEvent, BreadcrumbModule } from 'primeng/breadcrumb';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { Splitter, SplitterModule } from 'primeng/splitter';
import { resetSplitterSizes } from '../splitter/splitter.utils';

import { KubernetesContextStateService } from '@core/state/kubernetes-context-state.service';
import { LocalStorageUtils } from '@shared/utils/local-storage.utils';
import { GenericReportsCompareComponent } from '../generic-reports-compare/generic-reports-compare.component';

const DEFAULT_VIEW_MODE: GenericSbomViewMode = 'navigator';

@Component({
  selector: 'app-generic-sbom',
  imports: [
    BreadcrumbModule,
    ButtonModule,
    DialogModule,
    FcoseComponent,
    GenericReportsCompareComponent,
    SbomNavigatorComponent,
    SbomPackageBoxComponent,
    SplitterModule,
    TrivyTableComponent,
    PairedOptionsSelectorComponent,
  ],
  templateUrl: './generic-sbom.component.html',
  styleUrl: './generic-sbom.component.scss',
})
export class GenericSbomComponent implements OnInit {
  private readonly kubernetesContextService = inject(KubernetesContextStateService);

  // false where the report is not one of many (e.g. a scan on demand result)
  isImageSelectorVisible = input<boolean>(true);
  // true: the graph view also hides the package list, so the graph gets the full width
  hideListInGraphView = input<boolean>(false);

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

  // the view of the right side: once at start (the saved one), then on every switch
  viewModeChange = output<GenericSbomViewMode>();

  compareFirstDtoRequested = output<string>();
  compareSecondDtoRequested = output<string>();

  namespacedImageDtos?: PairedOptionsDto[];

  protected readonly sbomIndex = computed<SbomIndex>(() => {
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

  dependsOnTableColumns: TrivyTableColumn[] = [...genericSbomReportSlimDetailColumns];
  // #endregion

  // #region right side: navigator or graph, the package box and the dive breadcrumb
  readonly viewMode = signal<GenericSbomViewMode>(DEFAULT_VIEW_MODE);
  readonly isListCollapsed = computed(() => this.hideListInGraphView() && this.viewMode() === 'graph');

  // the breadcrumb of the dives, shared by both views (fcose's own is hidden); every dive goes through showNode
  readonly navigation = new FcoseNavigation({
    getNodeName: (nodeId) => this.sbomIndex().byBomRef.get(nodeId)?.name ?? undefined,
    onNavigate: (nodeId) => this.onActiveNodeIdChange(nodeId),
  });

  // the box: in graph view the hovered node while hovering, otherwise the selected package
  get packageBoxDto(): GenericSbomReportDetailDto | undefined {
    return this.viewMode() === 'graph'
      ? (this.hoveredSbomDetailDto ?? this.selectedSbomDetailDto)
      : this.selectedSbomDetailDto;
  }
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
        untracked(() => {
          this.navigation.init(fullSbomDataDto.rootNodeBomRef ?? undefined);
          this.showNode(fullSbomDataDto.rootNodeBomRef ?? '');
        });
      }
    });

    effect(() => {
      const selectedImageId = this.selectedImageId();
      // without the image selector there is no selection: the report is given as is (e.g. a scan on demand result),
      // and an undefined selectedImageId must not reset it
      if (!this.isImageSelectorVisible()) return;
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

  ngOnInit() {
    const saved = LocalStorageUtils.getItem(this.viewModeStorageKey());
    if (saved === 'navigator' || saved === 'graph') {
      this.viewMode.set(saved);
    }
    this.viewModeChange.emit(this.viewMode());
  }

  setViewMode(viewMode: GenericSbomViewMode) {
    this.viewMode.set(viewMode);
    LocalStorageUtils.setItem(this.viewModeStorageKey(), viewMode);
    this.viewModeChange.emit(viewMode);
  }

  private viewModeStorageKey(): string {
    return `genericSbom.viewMode.${this.stateKey()}`;
  }

  onBreadcrumbItemClick(event: BreadcrumbItemClickEvent) {
    this.navigation.onItemClick(event);
  }

  onDiveRequested(bomRef: string) {
    this.onActiveNodeIdChange(bomRef);
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
    this.navigation.clear();
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
    this.navigation.update(nodeId);
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
    const properties = dto.properties ?? [];
    const values = properties.map((p) => p.value);

    if (values.includes('nuget')) {
      return `${dto.name?.split('.')[0] ?? ''} (nuget)`;
    }

    if (values.includes('dotnet-core')) {
      return `${dto.name?.split('.')[0] ?? ''} (dotnet-core)`;
    }

    if (values.includes('gobinary')) {
      return `${dto.name?.split('/')[0] ?? ''} (gobinary)`;
    }

    const todGroup = properties.find((p) => p.name === 'tod.group')?.value;
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
      details: (dto.properties ?? []).map(({ name, value }) => [
        { label: name },
        { label: value },
      ]),
    };
  }

  /** Double click on the gutter restores the default panel sizes (and the saved ones). */
  onSplitterDoubleClick(event: MouseEvent, splitter: Splitter) {
    resetSplitterSizes(event, splitter, [35, 65]);
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
