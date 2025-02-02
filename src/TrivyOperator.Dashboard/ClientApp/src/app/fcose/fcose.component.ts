import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';

import cytoscape, { EdgeSingular, ElementDefinition, NodeSingular } from 'cytoscape';
import fcose, { FcoseLayoutOptions } from 'cytoscape-fcose';

import { MenuItem } from 'primeng/api';
import { BreadcrumbItemClickEvent, BreadcrumbModule } from 'primeng/breadcrumb';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

// TODO: change to dedicated interface
import { SbomReportDetailDto } from '../../api/models/sbom-report-detail-dto';

cytoscape.use(fcose);

//

@Component({
  selector: 'app-fcose',
  standalone: true,
  imports: [BreadcrumbModule, ButtonModule, InputTextModule, ReactiveFormsModule],
  templateUrl: './fcose.component.html',
  styleUrl: './fcose.component.scss',
})
export class FcoseComponent implements AfterViewInit, OnInit {
  @ViewChild('graphContainer', { static: true }) graphContainer!: ElementRef;
  testText: string = '';

  @Input() set selectedInnerNodeId(value: string | undefined) {
    this._selectedInnerNodeId = value;
    this.selectedInnerNodeIdChange.emit(value);
    if (value) {
      this.graphDiveIn(value);
    }
  }

  get selectedInnerNodeId(): string | undefined {
    return this._selectedInnerNodeId;
  }

  @Output() selectedInnerNodeIdChange: EventEmitter<string> = new EventEmitter<string>();
  private _selectedInnerNodeId: string | undefined = this.rootNodeId;

  @Input() set rootNodeId(value: string) {
    this._rootNodeId = value;
    this.initNavMenuItems();
  }

  get rootNodeId(): string {
    return this._rootNodeId;
  }

  private _rootNodeId: string = '00000000-0000-0000-0000-000000000000';

  navItems: MenuItem[] = [];
  navHome: MenuItem = { id: this.rootNodeId, icon: 'pi pi-sitemap' };
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

  get dataDtos(): SbomReportDetailDto[] {
    return this._dataDtos;
  }

  @Input() set dataDtos(sbomDto: SbomReportDetailDto[]) {
    this._dataDtos = sbomDto;
  }

  private _dataDtos: SbomReportDetailDto[] = [];

  private isDivedIn: boolean = false;

  private get hoveredNode(): NodeSingular | null {
    return this._hoveredNode;
  }

  private set hoveredNode(node: NodeSingular | null) {
    this._hoveredNode = node;
    if (node) {
      const x = this.getDataDetailDtoById(node.id());
      if (x) {
        this.testText = `<b>Name:</b> ${x.name} - <b>Version:</b> ${x.version} - <b>Dependencies:</b> ${x.dependsOn?.length ?? 0}`;
      }
    } else {
      this.testText = 'no info...';
    }
  }

  private _hoveredNode: NodeSingular | null = null;

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
      this.isDivedIn = false;
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

  private selectNode(node: NodeSingular) {
    if (node.isParent() || node.hasClass('nodeLeaf')) {
      return;
    }
    this.hoveredNode = null;
    this.graphDiveIn(node.id());
  }

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

  private graphDiveIn(nodeId: string) {
    this.cy.elements().addClass('hidden');

    setTimeout(() => {
      this.cy.elements().remove();

      const newElements = this.getElementsByNodeId(nodeId);
      this.cy.add(newElements);

      this.cy.elements().addClass('hidden');

      this.cy.layout(this.fcoseLayoutOptions as FcoseLayoutOptions).run();
      this.onZoomFit();

      this.updateNavMenuItems(nodeId);
      setTimeout(() => {
        this.cy.elements().removeClass('hidden');
        const newRootNode = this.cy.$(`#${nodeId}`);
        if (newRootNode) {
          this.highlightNode(newRootNode);
        }
        if (this.inputFilterByNameValue) {
          this.onNodesHighlightByName(this.inputFilterByNameValue);
        }
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

  private getElementsByNodeId(nodeId: string): ElementDefinition[] {
    const sbomDetailDtos: SbomReportDetailDto[] = [];
    const rootSbomDto = this.dataDtos.find((x) => x.bomRef == nodeId);
    if (rootSbomDto) {
      sbomDetailDtos.push(rootSbomDto);
      this.getParentsSbomDtos(rootSbomDto, sbomDetailDtos);
      this.getChildrenSbomDtos(rootSbomDto, sbomDetailDtos);
    }

    const groupMap = new Map<string, number>();
    sbomDetailDtos.forEach((sbomDetailDto) => {
      if (sbomDetailDto.purl?.startsWith('pkg:nuget/')) {
        const potentialNs = sbomDetailDto.name?.split('.')[0] ?? 'unknown';
        const currentCount = (groupMap.get(potentialNs) || 0) + 1;
        groupMap.set(potentialNs, currentCount);
      }
    });

    const elements: ElementDefinition[] = [];
    sbomDetailDtos.forEach((sbomDetailDto) => {
      if (sbomDetailDto) {
        let parentId: string | undefined = undefined;
        if (sbomDetailDto.purl?.startsWith('pkg:nuget/')) {
          // && !sbomDetailDto.name.includes("Runtime.linux-x64")
          const potentialNs = sbomDetailDto.name?.split('.')[0] ?? 'unknown';
          if ((groupMap.get(potentialNs) || 0) > 1) {
            elements.push({ data: { id: potentialNs, label: potentialNs }, classes: 'nodeCommon' });
            parentId = potentialNs;
          }
        }
        elements.push({
          data: {
            id: sbomDetailDto.bomRef,
            label: sbomDetailDto.name ?? '',
            parent: parentId,
          },
          classes: `nodeCommon nodePackage ${sbomDetailDto.dependsOn?.length ? 'nodeBranch' : 'nodeLeaf'}`,
        });
        sbomDetailDto.dependsOn?.forEach((depends) => {
          elements.push({
            data: {
              source: sbomDetailDto.bomRef,
              target: depends,
            },
            classes: 'edgeCommon',
          });
        });
      }
    });

    return elements;
  }

  private getParentsSbomDtos(sbomDetailDto: SbomReportDetailDto, sbomDetailDtos: SbomReportDetailDto[]) {
    const parents = this.dataDtos
      .filter((x) => x.dependsOn?.includes(sbomDetailDto.bomRef ?? ""))
      .map((y) => {
        const parentSbom: SbomReportDetailDto = JSON.parse(JSON.stringify(y));
        parentSbom.dependsOn = [sbomDetailDto.bomRef ?? ""];
        return parentSbom;
      }) ?? [];

    sbomDetailDtos.push(...parents);
  }

  private getChildrenSbomDtos(sbomDetailDto: SbomReportDetailDto, sbomDetailDtos: SbomReportDetailDto[]) {
    if (!sbomDetailDto) {
      return;
    }
    const detailBomRefIds = sbomDetailDto.dependsOn;
    if (!detailBomRefIds) {
      return;
    }
    const newDetailBomRefIds: string[] = [];
    detailBomRefIds.forEach((bomRefId) => {
      if (!sbomDetailDtos.find((x) => x.bomRef === bomRefId)) {
        newDetailBomRefIds.push(bomRefId);
      }
    });
    const newSbomDetailDtos = this.dataDtos.filter((x) => newDetailBomRefIds.includes(x.bomRef ?? '')) ?? [];
    sbomDetailDtos.push(...newSbomDetailDtos);
    newSbomDetailDtos.forEach((sbomDetailDto) => this.getChildrenSbomDtos(sbomDetailDto, sbomDetailDtos));
  }

  private initNavMenuItems() {
    this.navItems = [];
    this.navHome = { id: this.rootNodeId, icon: 'pi pi-sitemap' };
  }

  onNavItemClick(event: BreadcrumbItemClickEvent) {
    console.log(event.item);
    if (event.item.icon) {
      this.graphDiveIn(this.rootNodeId);
      return;
    }
    if (event.item.id) {
      this.graphDiveIn(event.item.id);
    }
  }

  private updateNavMenuItems(nodeId: string) {
    if (this.selectedInnerNodeId === nodeId) {
      return;
    }

    if (this.rootNodeId === nodeId) {
      this.selectedInnerNodeId = nodeId;
      this.navItems = [];
      return;
    }

    const potentialIndex = this.navItems.map((x) => x.id).indexOf(nodeId);
    if (potentialIndex !== -1) {
      this.navItems = this.navItems.slice(0, potentialIndex + 1);
      this.navItems[potentialIndex].styleClass = 'breadcrumb-size';
      this.selectedInnerNodeId = nodeId;
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
    this.selectedInnerNodeId = nodeId;
  }

  private getDataDetailDtoById(id: string): SbomReportDetailDto | undefined {
    return this.dataDtos.find((x) => x.bomRef == id);
  }

  onInputChange(value: string) {
    this.inputFilterByNameValue = value;
    this.onNodesHighlightByName(value);
  }
}
