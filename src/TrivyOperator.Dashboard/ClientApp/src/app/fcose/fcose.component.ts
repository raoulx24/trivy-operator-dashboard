import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';

import cytoscape, { EdgeSingular, ElementDefinition, NodeSingular } from 'cytoscape';
import fcose, { FcoseLayoutOptions } from 'cytoscape-fcose';

import { MenuItem } from 'primeng/api';
import { BreadcrumbItemClickEvent, BreadcrumbModule } from 'primeng/breadcrumb';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';

import { NodeDataDto } from './fcose.types'


cytoscape.use(fcose);

@Component({
  selector: 'app-fcose',
  standalone: true,
  imports: [BreadcrumbModule, ButtonModule, InputTextModule, TagModule, CommonModule, ReactiveFormsModule],
  templateUrl: './fcose.component.html',
  styleUrl: './fcose.component.scss',
})
export class FcoseComponent implements AfterViewInit, OnInit {
  @ViewChild('graphContainer', { static: true }) graphContainer!: ElementRef;

  // #region activeNodeId
  activeNodeId: string | undefined = undefined;
  @Output() activeNodeIdChange: EventEmitter<string> = new EventEmitter<string>();
  //@Input() set rootNodeId(value: string) {
  //  this._rootNodeId = value;
  //  this.initNavMenuItems();
  //}
  //get rootNodeId(): string {
  //  return this._rootNodeId;
  //}
  private _rootNodeId: string = '00000000-0000-0000-0000-000000000000';
  // #endregion
  // #region main nodeDataDtos
  get nodeDataDtos(): NodeDataDto[] {
    return this._nodeDataDtos;
  }
  @Input() set nodeDataDtos(nodeDataDtos: NodeDataDto[]) {
    this._nodeDataDtos = nodeDataDtos;
    if (nodeDataDtos.length == 0) {
      this.activeNodeId = undefined;
      this.navHome = undefined;
      this.navItems = [];
    }
    else {
      if (!this.activeNodeId) {
        this.initNavMenuItems();
      }
      this.activeNodeId = nodeDataDtos.find(x => x.isMain)?.id;
      this.graphDiveIn();
    }
  }
  private _nodeDataDtos: NodeDataDto[] = [];
  // #endregion
  // #region hoveredNode
  get hoveredNode(): NodeSingular | null {
    return this._hoveredNode;
  }
  private set hoveredNode(node: NodeSingular | null) {
    this._hoveredNode = node;
    const hoveredNodeDto = this.getDataDetailDtoById(node?.id());
    this.hoveredNodeDto = hoveredNodeDto;
    this.hoveredNodeDtoChange.emit(hoveredNodeDto);
  }
  private _hoveredNode: NodeSingular | null = null;
  hoveredNodeDto: NodeDataDto | undefined = undefined;
  @Output() hoveredNodeDtoChange: EventEmitter<NodeDataDto> = new EventEmitter<NodeDataDto>();
  // #endregion
  navItems: MenuItem[] = [];
  navHome: MenuItem | undefined = undefined;
  private cy!: cytoscape.Core;
  private fcoseLayoutOptions: FcoseLayoutOptions = {
    name: 'fcose',
    nodeRepulsion: (node: NodeSingular) => {
      return 20000;
    },
    numIter: 2500,
    animate: false,
    fit: true,
    padding: 10,
    sampleSize: 50,
    nodeSeparation: 500,
    tilingPaddingHorizontal: 1000,
    tilingPaddingVertical: 1000,
    idealEdgeLength: (edge: EdgeSingular) => {
      return 150;
    },
    edgeElasticity: (edge: EdgeSingular) => {
      return 0.15;
    },
  };
  private isDivedIn: boolean = false;
  inputFilterByNameControl = new FormControl();
  private inputFilterByNameValue: string = "";

  ngOnInit() {
    this.inputFilterByNameControl.valueChanges.pipe(debounceTime(500)).subscribe((value) => {
      this.onInputChange(value);
    });
  }

  ngAfterViewInit() {
    this.setupCyLayout();
    this.setupCyEvents();
  }

  // #region Cy Setup
  private setupCyLayout() {
    this.cy = cytoscape({
      container: this.graphContainer.nativeElement,
      elements: [],
      layout: this.fcoseLayoutOptions as FcoseLayoutOptions,
      style: [
        {
          selector: '$node > .nodeCommon',
          style: {
            'background-color': 'gray',
            'background-opacity': 0.2,
            //'label': 'data(label)',
            'text-valign': 'top',
            'text-halign': 'center',
            'text-background-color': 'aqua',
            'font-size': '14px',
            'font-weight': 'bold',
          },
        },
        {
          selector: '.nodeCommon',
          style: {
            opacity: 1,
            'transition-property': 'opacity',
            'transition-duration': 300,
            'border-width': 1,
          },
        },
        {
          selector: '.nodePackage',
          style: {
            label: 'data(label)',
            width: 'mapData(label.length, 1, 30, 20, 200)',
            height: '20px',
            'background-color': 'Aqua',
            'text-valign': 'center',
            'text-halign': 'center',
            'text-wrap': 'ellipsis',
            'text-max-width': '200px',
            'font-size': '10px',
            'border-color': '#000',
            'transition-property': 'width height background-color font-size border-color opacity',
            'transition-duration': 300,
          },
        },
        {
          selector: '.nodeBranch',
          style: {
            shape: 'roundrectangle',
          },
        },
        {
          selector: '.nodeLeaf',
          style: {
            shape: 'cut-rectangle',
          },
        },
        {
          selector: '.edgeCommon',
          style: {
            width: 1,
            'line-color': '#ccc',
            'target-arrow-color': '#ccc',
            'target-arrow-shape': 'triangle',
            'transition-property': 'width line-color opacity',
            'transition-duration': 300,
            opacity: 1,
          },
        },
        {
          selector: '.hoveredCommon',
          style: {
            width: 'mapData(label.length, 1, 30, 20, 240)',
            height: '24px',
            'font-size': '12px',
            'transition-property': 'width height background-color font-size border-color opacity',
            'transition-duration': 300,
          },
        },
        {
          selector: '.hovered',
          style: {
            'background-color': 'Silver',
          },
        },
        {
          selector: '.hoveredOutgoers',
          style: {
            'background-color': 'DeepSkyBlue',
          },
        },
        {
          selector: '.hoveredIncomers',
          style: {
            'background-color': 'RoyalBlue',
          },
        },
        {
          selector: '.hoveredHighlight',
          style: {
            'overlay-opacity': 0.5,
            'overlay-color': 'RoyalBlue',
            'font-style': 'italic',
          },
        },
        {
          selector: '.highlighted-edge',
          style: {
            width: 3,
            'line-color': 'Violet',
            'transition-property': 'line-color opacity',
            'transition-duration': 300,
          },
        },
        {
          selector: '.hidden',
          style: {
            opacity: 0,
          },
        },
        {
          selector: '.filtered-highlighted',
          style: {
            width: 'mapData(label.length, 1, 30, 20, 240)',
            height: '24px',
          },
        },
        {
          selector: '.filtered-unhighlighted',
          style: {
            opacity: 0.4,
          },
        },
      ],
    });
  }

  private setupCyEvents() {
    this.cy.on('mouseover', 'node', (event) => {
      this.highlightNode(event.target as NodeSingular);
    });

    this.cy.on('mouseout', 'node', (event) => {
      this.unhighlightNode(event.target as NodeSingular);
    });

    this.cy.on('dblclick', 'node', (event) => {
      this.selectNode(event.target as NodeSingular);
    });

    //this.cy.on('click', 'node', (event) => {
    //  const node = event.target;
    //  console.log('Single-clicked on node:', node.id());
    //});
  }
  // #endregion

  // #region Node Highlightning
  private highlightNode(node: NodeSingular) {
    if (this.hoveredNode?.id() == node.id() || node.isParent()) {
      return;
    }
    if (this.hoveredNode) {
      this.unhighlightNode(this.hoveredNode);
    }
    this.hoveredNode = node;
    this.hoveredNode.addClass('hoveredCommon hovered');
    this.hoveredNode.incomers('node').forEach((depNode: NodeSingular) => {
      depNode.addClass('hoveredCommon ');
      // WTF? why it might be null?
      if (this.hoveredNode!.outgoers('node').has(depNode)) {
        depNode.addClass('hoveredHighlight');
      } else {
        depNode.addClass('hoveredIncomers');
      }
    });
    this.hoveredNode.outgoers('node').forEach((depNode: NodeSingular) => {
      depNode.addClass('hoveredCommon hoveredOutgoers');
    });

    this.hoveredNode.connectedEdges().forEach((edge: EdgeSingular) => {
      edge.addClass('highlighted-edge');
    });
  }

  private unhighlightNode(node: NodeSingular) {
    if (node.isParent()) {
      return;
    }
    if (this.isDivedIn) {
      // this.isDivedIn = false;
      return;
    }
    node.removeClass('hoveredCommon hovered');

    node.outgoers('node').forEach((depNode: NodeSingular) => {
      depNode.removeClass('hoveredCommon hoveredOutgoers hoveredHighlight');
    });
    node.incomers('node').forEach((depNode: NodeSingular) => {
      depNode.removeClass('hoveredCommon hoveredIncomers');
    });

    node.connectedEdges().forEach((edge: EdgeSingular) => {
      edge.removeClass('highlighted-edge');
    });
    this.hoveredNode = null;
  }
  // #endregion

  private selectNode(node: NodeSingular) {
    if (node.isParent() || node.hasClass('nodeLeaf')) {
      return;
    }
    //this.graphDiveIn(node.id());
    if (this.activeNodeId !== node.id()) {
      this.activeNodeIdChange.emit(node.id());
    }
  }

  // #region Menu Bar Events
  onZoomIn(_event: MouseEvent) {
    this.cy.animate({
      zoom: this.cy.zoom() + 0.1,
      duration: 300,
    });
  }

  onZoomOut(_event: MouseEvent) {
    this.cy.animate({
      zoom: this.cy.zoom() - 0.1,
      duration: 300,
    });
  }

  onZoomFit(_event?: MouseEvent) {
    this.cy.animate({
      fit: {
        eles: this.cy.elements(),
        padding: 10,
      },
      duration: 300,
    });
  }
  // #endregion

  private graphDiveIn() {
    this.cy.elements().addClass('hidden');

    setTimeout(() => {
      this.cy.elements().remove();

      this.cy.add(this.getElements());

      this.cy.elements().addClass('hidden');

      this.cy.layout(this.fcoseLayoutOptions as FcoseLayoutOptions).run();
      this.onZoomFit();

      this.updateNavMenuItems(this.activeNodeId ?? "");
      setTimeout(() => {
        this.cy.elements().removeClass('hidden');
        const newRootNode = this.cy.$(`#${this.activeNodeId}`);
        if (newRootNode) {
          this.highlightNode(newRootNode);
        }
        if (this.inputFilterByNameValue) {
          this.onNodesHighlightByName(this.inputFilterByNameValue);
        }
        this.isDivedIn = false;
      }, 500);
    }, 350);
    this.isDivedIn = true;
    
  }

  private onNodesHighlightByName(value: string) {
    value = value.toLowerCase();
    this.cy.batch(() => {
      if (value) {
        this.cy.nodes().forEach((node: NodeSingular) => {
          if (!node.isParent()) {
            const label = node.data('label').toLowerCase();
            if (label.includes(value)) {
              node.removeClass('filtered-unhighlighted');
              node.addClass('filtered-highlighted');
            } else {
              node.removeClass('filtered-highlighted');
              node.addClass('filtered-unhighlighted');
            }
          }
        });
      }
      else {
        this.cy.nodes().forEach((node: NodeSingular) => {
          node.removeClass('filtered-highlighted');
          node.removeClass('filtered-unhighlighted');
        })
      }
    });
  }

  private getElements(): ElementDefinition[] {
    const elements: ElementDefinition[] = [];
    const groupMap = new Map<string, number>();
    this.nodeDataDtos
      .filter(x => x.groupName)
      .forEach((x) => {
        const currentCount = (groupMap.get(x.groupName ?? "") || 0) + 1;
        groupMap.set(x.groupName ?? "", currentCount);
      }
    );
    groupMap.forEach((value, key) => {
      if (value > 1) {
        elements.push({ data: { id: key, label: key }, classes: 'nodeCommon' });
      }
    });

    this.nodeDataDtos.forEach(nodeData => {
      const parentId = (groupMap.get(nodeData.groupName ?? "") || 0) > 1 ? nodeData.groupName : undefined;
      elements.push({
        data: {
          id: nodeData.id,
          label: nodeData.name ?? '',
          parent: parentId,
        },
        classes: `nodeCommon nodePackage ${nodeData.dependsOn?.length ? 'nodeBranch' : 'nodeLeaf'}`,
      });
      nodeData.dependsOn?.forEach((depends) => {
        elements.push({
          data: {
            source: nodeData.id,
            target: depends,
          },
          classes: 'edgeCommon',
        });
      });
    });

    return elements;
  }

  // #region Get Parent and Children Nodes - To be moved in SBOM
  //private getElementsByNodeId(nodeId: string): ElementDefinition[] {
  //  const sbomDetailDtos: NodeDataDto[] = [];
  //  const rootSbomDto = this.nodeDataDtos.find((x) => x.id == nodeId);
  //  if (rootSbomDto) {
  //    sbomDetailDtos.push(rootSbomDto);
  //    this.getParentsSbomDtos(rootSbomDto, sbomDetailDtos);
  //    this.getChildrenSbomDtos(rootSbomDto, sbomDetailDtos);
  //  }

  //  const groupMap = new Map<string, number>();
  //  // TODO: change to speciffic BelongsToGroupName (or smth) field in NodeDataDto
  //  //sbomDetailDtos.forEach((sbomDetailDto) => {
  //  //  if (sbomDetailDto.purl?.startsWith('pkg:nuget/')) {
  //  //    const potentialNs = sbomDetailDto.name?.split('.')[0] ?? 'unknown';
  //  //    const currentCount = (groupMap.get(potentialNs) || 0) + 1;
  //  //    groupMap.set(potentialNs, currentCount);
  //  //  }
  //  //});

  //  const elements: ElementDefinition[] = [];
  //  sbomDetailDtos.forEach((sbomDetailDto) => {
  //    if (sbomDetailDto) {
  //      let parentId: string | undefined = undefined;
  //      // TODO: change to speciffic BelongsToGroupName (or smth) field in NodeDataDto
  //      //if (sbomDetailDto.purl?.startsWith('pkg:nuget/')) {
  //      //  // && !sbomDetailDto.name.includes("Runtime.linux-x64")
  //      //  const potentialNs = sbomDetailDto.name?.split('.')[0] ?? 'unknown';
  //      //  if ((groupMap.get(potentialNs) || 0) > 1) {
  //      //    elements.push({ data: { id: potentialNs, label: potentialNs }, classes: 'nodeCommon' });
  //      //    parentId = potentialNs;
  //      //  }
  //      //}
  //      elements.push({
  //        data: {
  //          id: sbomDetailDto.id,
  //          label: sbomDetailDto.name ?? '',
  //          parent: parentId,
  //        },
  //        classes: `nodeCommon nodePackage ${sbomDetailDto.dependsOn?.length ? 'nodeBranch' : 'nodeLeaf'}`,
  //      });
  //      sbomDetailDto.dependsOn?.forEach((depends) => {
  //        elements.push({
  //          data: {
  //            source: sbomDetailDto.id,
  //            target: depends,
  //          },
  //          classes: 'edgeCommon',
  //        });
  //      });
  //    }
  //  });

  //  return elements;
  //}

  //private getParentsSbomDtos(sbomDetailDto: NodeDataDto, sbomDetailDtos: NodeDataDto[]) {
  //  const parents = this.nodeDataDtos
  //    .filter((x) => x.dependsOn?.includes(sbomDetailDto.id ?? ""))
  //    .map((y) => {
  //      const parentSbom: NodeDataDto = JSON.parse(JSON.stringify(y));
  //      parentSbom.dependsOn = [sbomDetailDto.id ?? ""];
  //      return parentSbom;
  //    }) ?? [];

  //  sbomDetailDtos.push(...parents);
  //}

  //private getChildrenSbomDtos(sbomDetailDto: NodeDataDto, sbomDetailDtos: NodeDataDto[]) {
  //  if (!sbomDetailDto) {
  //    return;
  //  }
  //  const detailIds = sbomDetailDto.dependsOn;
  //  if (!detailIds) {
  //    return;
  //  }
  //  const newDetailIds: string[] = [];
  //  detailIds.forEach((id) => {
  //    if (!sbomDetailDtos.find((x) => x.id === id)) {
  //      newDetailIds.push(id);
  //    }
  //  });
  //  const newSbomDetailDtos = this.nodeDataDtos.filter((x) => newDetailIds.includes(x.id ?? '')) ?? [];
  //  sbomDetailDtos.push(...newSbomDetailDtos);
  //  newSbomDetailDtos.forEach((sbomDetailDto) => this.getChildrenSbomDtos(sbomDetailDto, sbomDetailDtos));
  //}
  // #endregion

  // #region navItems
  /**
   * This method initialize the NavMenu.
   */
  private initNavMenuItems() {
    this.navItems = [];
    this.navHome = { id: this._rootNodeId, icon: 'pi pi-sitemap' };
  }

  /**
   * This method initialize the NavMenu.
   * @param {BreadcrumbItemClickEvent} event - the event holding item info
   */
  onNavItemClick(event: BreadcrumbItemClickEvent) {
    if (event.item?.id === this._rootNodeId && this.navItems.length == 0) {
      return;
    }
    if (event.item?.id && this.navItems[this.navItems.length - 1]?.id !== event.item.id) {
      this.activeNodeIdChange.emit(event.item.id);
    }
  }
  /**
   * This method updates the menu with the selected item from the graph
   * @param {string} nodeId - the node id of the selected item
   */
  private updateNavMenuItems(nodeId: string) {
    if (this._rootNodeId === nodeId) {
      this.activeNodeId = nodeId;
      this.navItems = [];
      return;
    }

    const potentialIndex = this.navItems.map((x) => x.id).indexOf(nodeId);
    if (potentialIndex !== -1) {
      this.navItems = this.navItems.slice(0, potentialIndex + 1);
      this.navItems[potentialIndex].styleClass = 'breadcrumb-size';
      this.activeNodeId = nodeId;
      return;
    }

    if (this.navItems.length > 0) {
      this.navItems[this.navItems.length - 1].styleClass = 'breadcrumb-pointer';
    }
    const newDataDetailDto = this.getDataDetailDtoById(nodeId);
    this.navItems = [
      ...this.navItems,
      {
        id: nodeId,
        label: newDataDetailDto?.name ?? 'no-name',
        styleClass: 'breadcrumb-size',
      },
    ];
    this.activeNodeId = nodeId;
  }
  // #endregion

  getDataDetailDtoById(id: string | undefined | null): NodeDataDto | undefined {
    return this.nodeDataDtos.find((x) => x.id == id);
  }

  onInputChange(value: string) {
    this.inputFilterByNameValue = value;
    this.onNodesHighlightByName(value);
  }
}
