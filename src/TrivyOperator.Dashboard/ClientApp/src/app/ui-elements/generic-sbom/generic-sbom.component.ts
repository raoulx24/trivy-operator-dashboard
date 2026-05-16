import { Component, effect, HostListener, inject, input, model, output, signal } from '@angular/core';

import { NodeDataDto } from '../fcose/fcose.types';
import { genericSbomReportComparedColumns, genericSbomReportDetailColumns } from './generic-sbom.constans';
import { GenericSbomReportDetailDto, GenericSbomReportDto, GenericSbomReportMinimalDto } from './generic-sbom.types';

import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';

import { FcoseComponent } from '../fcose/fcose.component';
import {
  NamespaceImageSelectorComponent,
  nonExistingNamespace,
} from '../namespace-image-selector/namespace-image-selector.component';
import { NamespacedImageDto } from '../namespace-image-selector/namespace-image-selector.types';
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

import { KubernetesContextStateService } from '../../services/kubernetes-context-state.service';
import { GenericReportsCompareComponent } from '../generic-reports-compare/generic-reports-compare.component';

@Component({
  selector: 'app-generic-sbom',
  imports: [
    DialogModule,
    FcoseComponent,
    GenericReportsCompareComponent,
    NamespaceImageSelectorComponent,
    SeverityCssStyleByIdPipe,
    SplitterModule,
    TagModule,
    TrivyTableComponent,
    VulnerabilityCountPipe,
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

  protected _fullSbomDataDto?: GenericSbomReportDto;

  namespacedImageDtos?: NamespacedImageDto[];

  // region dependsOnTable data
  selectedSbomDetail: SelectedDtosEvent<GenericSbomReportDetailDto> = { source: 'programmatic', selectedDtos: [] };
  selectedSbomDetailDto?: GenericSbomReportDetailDto;
  dependsOnBoms: GenericSbomReportDetailDto[] = [];
  deletedDependsOnBom: GenericSbomReportDetailDto[] = [];

  dependsOnTableColumns: TrivyTableColumn[] = [...genericSbomReportDetailColumns];
  // endregion

  hoveredSbomDetailDto?: GenericSbomReportDetailDto;
  nodeDataDtos: NodeDataDto[] = [];
  selectedSbomDetailBomRef?: string;
  private readonly _rootNodeId: string = '00000000-0000-0000-0000-000000000000';

  isTrivyReportsCompareVisible = signal<boolean>(false);
  comparedTableColumns: TrivyTableColumn[] = [...genericSbomReportComparedColumns];

  screenSize: string = this.getScreenSize();

  constructor() {
    let initialized = false;

    effect(() => {
      const dataDtos = this.dataDtos();
      if (dataDtos && dataDtos.length > 0) {
        this.getNamespacedImageDtos();
      }
    });

    effect(() => {
      const fullSbomDataDto = this.fullSbomDataDto();
      this._fullSbomDataDto = fullSbomDataDto;
      if (fullSbomDataDto) {
        this.getDataDtosByNodeId(fullSbomDataDto.rootNodeBomRef ?? '');
        this.onActiveNodeIdChange(fullSbomDataDto.rootNodeBomRef ?? '');
      }
    });

    effect(() => {
      const selectedImageId = this.selectedImageId();
      // avoid re-selecting the same image
      if (selectedImageId === this._fullSbomDataDto?.uid) return;

      if (this._fullSbomDataDto) {
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
    // this.fullSbomDataDto = undefined;
    this.dependsOnBoms = [];
    this.deletedDependsOnBom = [];
    this.nodeDataDtos = [];
  }

  getNamespacedImageDtos() {
    this.namespacedImageDtos =
      this.dataDtos()?.map(
        (x) =>
          ({
            uid: x.uid ?? '',
            resourceNamespace: x.resourceNamespace ?? nonExistingNamespace,
            mainLabel: `${x.imageName ?? ''}:${x.imageTag ?? ''}`,
            icon: x.hasVulnerabilities ? 'security' : undefined,
          }) as NamespacedImageDto,
      ) ?? [];
  }
  // #endregion

  // #region Get Parent and Children Nodes
  private getDataDtosByNodeId(nodeId: string) {
    this.isTableLoading.set(true);
    this.dependsOnBoms = [];
    this.deletedDependsOnBom = [];
    const sbomDetailDtos: GenericSbomReportDetailDto[] = [];
    const rootSbomDetailDto = this.fullSbomDataDto()?.details?.find((x) => x.bomRef == nodeId);
    if (rootSbomDetailDto) {
      const rootSbomExtended: GenericSbomReportDetailDto = {
        ...rootSbomDetailDto,
        level: 'Base',
        group: this.getGroupFromSbomReportDetail(rootSbomDetailDto),
      };
      sbomDetailDtos.push(rootSbomExtended);
      this.getDirectParentsSbomDtos(rootSbomExtended, sbomDetailDtos);
      this.getChildrenSbomDtos(rootSbomExtended, nodeId, sbomDetailDtos);
    }

    this.dependsOnBoms = sbomDetailDtos;
    this.nodeDataDtos =
      sbomDetailDtos.map((x) => ({
        id: x.bomRef ?? undefined,
        dependsOn: x.dependsOn,
        name: x.name,
        groupName: x.group,
        isMain: x.bomRef == nodeId,
      })) ?? [];
    this.isTableLoading.set(false);
  }

  private getDirectParentsSbomDtos(sded: GenericSbomReportDetailDto, sdeds: GenericSbomReportDetailDto[]) {
    const parents =
      this.fullSbomDataDto()
        ?.details?.filter((x) => x.dependsOn?.includes(sded.bomRef ?? ''))
        .map((y) => {
          const parentSbom: GenericSbomReportDetailDto = {
            ...y,
            level: 'Ancestor',
            group: this.getGroupFromSbomReportDetail(y),
            dependsOn: [sded.bomRef ?? ''],
          };
          return parentSbom;
        }) ?? [];

    sdeds.push(...parents);
  }

  private getChildrenSbomDtos(
    sded: GenericSbomReportDetailDto,
    baseBomRef: string,
    sdeds: GenericSbomReportDetailDto[],
  ) {
    if (!sded) {
      return;
    }
    const detailIds = sded.dependsOn;
    if (!detailIds) {
      return;
    }
    const newDetailIds: string[] = [];
    detailIds.forEach((id) => {
      if (!sdeds.find((x) => x.bomRef === id)) {
        newDetailIds.push(id);
      }
    });
    const newSbomDetailDtos =
      this.fullSbomDataDto()
        ?.details?.filter((x) => newDetailIds.includes(x.bomRef ?? ''))
        .map((y) => {
          const childSbom: GenericSbomReportDetailDto = {
            ...y,
            level: sded.bomRef == baseBomRef ? 'Child' : 'Descendant',
            group: this.getGroupFromSbomReportDetail(y),
          };
          return childSbom;
        }) ?? [];
    sdeds.push(...newSbomDetailDtos);
    newSbomDetailDtos.forEach((sbomDetailDto) => this.getChildrenSbomDtos(sbomDetailDto, baseBomRef, sdeds));
  }
  // #endregion

  // #region sanitize property name and value
  public sanitizePropertyName(value: string | null | undefined): string | null | undefined {
    return value?.replaceAll(':', ' ').replaceAll('.', '-').replaceAll('@', '-');
  }

  public sanitizePropertyValue(value: string | null | undefined): string | null | undefined {
    return value?.replaceAll('@', ' [@] ').replaceAll('sha256:', ' [sha256:] ');
  }
  // #endregion

  onHoveredNodeDtoChange(nodeId: string | undefined) {
    this.hoveredSbomDetailDto = this.getSbomDetailDtoByBomRef(nodeId);
  }

  private getSbomDetailDtoByBomRef(bomRef: string | undefined): GenericSbomReportDetailDto | undefined {
    return this.fullSbomDataDto()?.details?.find((x) => x.bomRef == bomRef);
  }

  onActiveNodeIdChange(event: string) {
    const sbomDetailDto = this.fullSbomDataDto()?.details?.find((x) => x.bomRef == event);
    if (sbomDetailDto) {
      this.getDataDtosByNodeId(event);
      const selectedSbomDetailDto = this.dependsOnBoms?.find((x) => x.level == 'Base');
      this.selectedSbomDetailDto = selectedSbomDetailDto;
      this.selectedSbomDetailBomRef = selectedSbomDetailDto?.bomRef ?? undefined;
      this.selectedSbomDetail = { source: 'programmatic', selectedDtos: (selectedSbomDetailDto ? [selectedSbomDetailDto] : []) }
    }
  }

  private getGroupFromSbomReportDetail(dto: GenericSbomReportDetailDto): string {
    if (dto.properties?.find((x) => x[1] == 'nuget')) {
      return `${dto.name?.split('.')[0] ?? ''} (nuget)`;
    }
    if (dto.properties?.find((x) => x[1] == 'dotnet-core')) {
      return `${dto.name?.split('.')[0] ?? ''} (dotnet-core)`;
    }
    if (dto.properties?.find((x) => x[1] == 'gobinary')) {
      return `${dto.name?.split('/')[0] ?? ''} (gobinary)`;
    }
    const todGroup = dto.properties?.find((x) => x[0] == 'tod.group');
    if (todGroup && todGroup[1]) {
      return `${todGroup[1]}`;
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
      case 'Dive In':
        const bomRefId = this.selectedSbomDetailDto?.bomRef ?? undefined;
        if (bomRefId) {
          this.onActiveNodeIdChange(bomRefId);
        }
        break;
      case 'Compare with...':
        this.goToComparePage();
        break;
      default:
        // let the parent handle it
        this.multiActionEventChange.emit(event);
    }
  }

  private goToComparePage() {
    if (!this.fullSbomDataDto || !this.namespacedImageDtos) return;

    this.isTrivyReportsCompareVisible.set(true);
  }

  rowExpandResponse?: TrivyTableExpandRowData<GenericSbomReportDetailDto>;
  onRowExpandChange(dto: GenericSbomReportDetailDto) {
    this.rowExpandResponse = {
      rowKey: dto,
      colStyles: [
        { width: '70px', 'min-width': '70px', height: '50px' },
        { 'white-space': 'normal', display: 'flex', 'align-items': 'center', height: '50px' },
      ],
      details:
        dto.properties?.map((x) => {
          return [
            { label: x[0] ?? '' },
            { label: x[1] ?? '' },
          ];
        }) ?? [],
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
