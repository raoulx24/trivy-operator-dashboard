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

import { NodeDataDto } from '../cytoscape-graph/cytoscape-graph.types';
import { GraphNavigation } from '../cytoscape-graph/cytoscape-graph.navigation';
import { genericSbomReportComparedColumns, genericSbomReportSlimDetailColumns } from './generic-sbom.constants';
import {
  GenericSbomReportDetailDto,
  GenericSbomReportDto,
  GenericSbomReportMinimalDto,
  GenericSbomViewMode,
  SbomIndex,
} from './generic-sbom.types';

import { CytoscapeGraphComponent } from '../cytoscape-graph/cytoscape-graph.component';
import { SbomNavigatorComponent } from '../sbom-navigator/sbom-navigator.component';
import { SbomNeighbourLevel } from '../sbom-navigator/sbom-navigator.types';
import { SbomQuickInfoComponent } from '../sbom-quick-info/sbom-quick-info.component';
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
import { PopoverModule } from 'primeng/popover';
import { SelectButtonModule } from 'primeng/selectbutton';
import { FormsModule } from '@angular/forms';
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
    PopoverModule,
    CytoscapeGraphComponent,
    GenericReportsCompareComponent,
    SbomNavigatorComponent,
    SbomQuickInfoComponent,
    SelectButtonModule,
    FormsModule,
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
  // the chosen view (navigator / graph) is remembered per stateKey; a host that does not want that turns it off
  isViewModePersisted = input<boolean>(true);

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

  // all the images (the compare dialog gets them all)
  namespacedImageDtos?: PairedOptionsDto[];
  // the images of the selector: all, or only the active ones (with a vulnerability report); the selected image is always
  // kept, as the selector drops a selection that is not in its list
  selectorImageDtos?: PairedOptionsDto[];
  // true: only the active images in the selector; saved per page
  readonly isActiveOnly = signal<boolean>(true);
  // the namespace picked in the selector (its own model, bound both ways); the counts are for it when there is one
  readonly selectedNamespace = signal<string | undefined>(undefined);
  private readonly countedImages = computed(() => {
    const namespace = this.selectedNamespace();
    const images = this.dataDtos();

    return namespace
      ? images.filter((x) => (x.resourceNamespace ?? nonExistingNamespace) === namespace)
      : images;
  });
  readonly imagesFilterOptions = computed(() => {
    const images = this.countedImages();

    return [
      { value: true, label: `Active (${images.filter((x) => x.hasVulnerabilityReport).length})` },
      { value: false, label: `All (${images.length})` },
    ];
  });

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

  // #region right side: navigator (neighbours table, quick info, small graph) or the full graph, and the dive breadcrumb
  readonly viewMode = signal<GenericSbomViewMode>(DEFAULT_VIEW_MODE);
  // the view switch: icons only, the label is the tooltip and the aria label
  readonly viewModeOptions: { value: GenericSbomViewMode; icon: string; label: string }[] = [
    { value: 'graph', icon: 'pi pi-share-alt', label: 'Full graph view' },
    { value: 'navigator', icon: 'pi pi-list', label: 'Neighbours view' },
  ];

  // the breadcrumb of the dives, shared by both views (the graph's own is hidden); every dive goes through showNode
  readonly navigation = new GraphNavigation({
    getNodeName: (nodeId) => this.sbomIndex().byBomRef.get(nodeId)?.name ?? undefined,
    onNavigate: (nodeId) => this.onActiveNodeIdChange(nodeId),
  });

  // the level of each package of the dive set (the package list), for the navigator's table; set per dive,
  // so hiding nodes in the full graph does not change it
  levelByBomRef = new Map<string, SbomNeighbourLevel>();
  // #endregion

  // the full graph view's quick info: the hovered node only
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
      // the filter and the selection decide what the selector lists (the selected image always stays in)
      this.isActiveOnly();
      this.selectedImageId();
      untracked(() => this.updateSelectorImageDtos());
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
    const saved = this.isViewModePersisted() ? LocalStorageUtils.getItem(this.viewModeStorageKey()) : null;
    if (saved === 'navigator' || saved === 'graph') {
      this.viewMode.set(saved);
    }
    this.isActiveOnly.set(LocalStorageUtils.getItem(this.activeOnlyStorageKey()) !== 'false');
  }

  setActiveOnly(isActiveOnly: boolean | null) {
    // a select button can be unselected by a click on the selected option: the filter stays
    if (isActiveOnly === null) {
      return;
    }
    this.isActiveOnly.set(isActiveOnly);
    LocalStorageUtils.setItem(this.activeOnlyStorageKey(), String(isActiveOnly));
  }

  private activeOnlyStorageKey(): string {
    return `genericSbom.activeOnly.${this.stateKey()}`;
  }

  setViewMode(viewMode: GenericSbomViewMode | null) {
    // a select button can be unselected by a click on the selected option: the view stays
    if (!viewMode) {
      return;
    }
    this.viewMode.set(viewMode);
    if (this.isViewModePersisted()) {
      LocalStorageUtils.setItem(this.viewModeStorageKey(), viewMode);
    }
  }

  private viewModeStorageKey(): string {
    return `genericSbom.viewMode.${this.stateKey()}`;
  }

  onBreadcrumbItemClick(event: BreadcrumbItemClickEvent) {
    this.navigation.onItemClick(event);
  }

  onDiveRequested(bomRef: string) {
    // already the dive root: a dive would only reset the selection
    if (this.levelByBomRef.get(bomRef) === 'Base') {
      return;
    }
    this.onActiveNodeIdChange(bomRef);
  }

  // the Select button of a navigator row: that package becomes the selected row of the list (the navigator then shows
  // it); a package not in the list (outside the dive, or hidden in the full graph) is not selectable
  onSelectRequested(bomRef: string) {
    const dto = this.dependsOnBoms.find((x) => x.bomRef === bomRef);
    if (!dto) {
      return;
    }
    this.selectedSbomDetailDto = dto;
    this.selectedSbomDetailBomRef = dto.bomRef;
    this.selectedSbomDetail = { source: 'programmatic', selectedDtos: [dto] };
  }

  onRefreshRequested() {
    this.namespacedImageDtos = undefined;
    this.selectorImageDtos = undefined;
    this.resetAllRelatedData();

    this.refreshRequestedChange.emit();
  }

  private resetAllRelatedData() {
    this.dependsOnBoms = [];
    this.deletedDependsOnBom = [];
    this.nodeDataDtos = [];
    this.levelByBomRef = new Map();
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
    this.updateSelectorImageDtos();
  }

  private updateSelectorImageDtos() {
    const all = this.namespacedImageDtos;
    if (!all || !this.isActiveOnly()) {
      this.selectorImageDtos = all;
      return;
    }
    const activeUids = new Set(this.dataDtos().filter((x) => x.hasVulnerabilityReport).map((x) => x.uid));
    const selectedImageId = this.selectedImageId();
    this.selectorImageDtos = all.filter((x) => activeUids.has(x.uid) || x.uid === selectedImageId);
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
    this.levelByBomRef = new Map(sbomDetailDtos.filter((x) => x.level).map((x) => [x.bomRef, x.level!]));
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
