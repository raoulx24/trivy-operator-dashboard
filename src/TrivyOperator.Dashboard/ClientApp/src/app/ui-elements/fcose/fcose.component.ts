import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  OnInit,
  ViewChild,
  ViewContainerRef,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';

import cytoscape, { EdgeSingular, ElementDefinition, NodeSingular } from 'cytoscape';
import fcose, { FcoseLayoutOptions } from 'cytoscape-fcose';

import { MenuItem } from 'primeng/api';
import { BreadcrumbItemClickEvent, BreadcrumbModule } from 'primeng/breadcrumb';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';

import { IconComponent } from '../icon/icon.component';
import { TrivyToolbarComponent } from '../trivy-toolbar/trivy-toolbar.component';

import { DeletedNodes, NodeDataDto } from './fcose.types';

import { DarkModeService } from '../../services/dark-mode.service';

cytoscape.use(fcose);

@Component({
  selector: 'app-fcose',
  standalone: true,
  imports: [
    BreadcrumbModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    TagModule,
    IconComponent,
    TrivyToolbarComponent,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './fcose.component.html',
  styleUrl: './fcose.component.scss',
})
export class FcoseComponent implements AfterViewInit, OnInit {
  @ViewChild('graphContainer', { static: true }) graphContainer!: ElementRef;
  @ViewChild('helpContainer', { read: ViewContainerRef }) helpContainer!: ViewContainerRef;

  // #region activeNodeId
  activeNodeId?: string;
  activeNodeIdChange = output<string>();
  private readonly _defaultRootNodeId: string = '00000000-0000-0000-0000-000000000000';
  private _rootNodeId: string = this._defaultRootNodeId;
  // #endregion
  // #region main nodeDataDtos
  nodeDataDtos = input<NodeDataDto[]>([]);
  private get hoveredNode(): NodeSingular | undefined {
    return this._hoveredNode;
  }
  private set hoveredNode(node: NodeSingular | undefined) {
    if ((node && node.isParent()) || node === this._hoveredNode) {
      return;
    }
    this._hoveredNode = node;
    this.hoveredNodeIdChange.emit(node?.id());
  }
  private _hoveredNode?: NodeSingular;
  hoveredNodeIdChange = output<string | undefined>();
  private highlightedHoverNode?: NodeSingular;
  // #endregion
  // #region selectedNode - outside world communication for selected node (red)
  selectedNodeId = input<string | undefined>(undefined);
  selectedNodeIdChange = output<string | undefined>();
  graphSelectedNodes: NodeSingular[] = [];
  // #endregion
  // #region "Deleted" Nodes
  deletedNodes: DeletedNodes[] = [];
  currentDeletedNodesIndex: number = -1;

  deletedNodeIdsChange = output<string[]>();
  undeletedNodeIdsChange = output<string[]>();
  // #endregion
  navItems: MenuItem[] = [];
  navHome?: MenuItem;
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
    nodeSeparation: 50,
    tilingPaddingHorizontal: 100,
    tilingPaddingVertical: 100,
    idealEdgeLength: (edge: EdgeSingular) => {
      let denominator = 1;
      if (edge.source().parent().nonempty()) denominator = 8;
      if (edge.target().parent().nonempty()) denominator = 4;

      return this.idealEdgeLength() / denominator;
    },
    edgeElasticity: (edge: EdgeSingular) => {
      return 0.15;
    },
  };
  private isLayoutRedraw: boolean = false;
  inputFilterByNameControl = new FormControl('');
  private inputFilterByNameValue: string = '';

  private darkLightMode: 'Dark' | 'Light' = 'Dark';

  isStatic = input<boolean>(false);
  staticSelectedNodeId = input<string | undefined>(undefined);
  staticHighlightedNodeId = input<string | undefined>(undefined);
  staticInputFilterByNameValue = input<string>('');

  fcoseClasses = input<string>('fcose fcose-half');

  extraColorClasses = input<{ name: string; code: string }[]>([]);

  idealEdgeLength = input<number>(150);

  isHelpDialogVisible: boolean = false;

  private darkModeService = inject(DarkModeService);

  constructor() {
    effect(() => {
      const isDark = this.darkModeService.isDarkMode();
      this.darkLightMode = isDark ? 'Dark' : 'Light';
      const newMode = isDark ? 'Dark' : 'Light';
      const oldMode = isDark ? 'Light' : 'Dark';
      this.swapClassDarkLikghtMode(oldMode, newMode);
    });
    effect(() => {
      const nodeDataDtos = this.nodeDataDtos();
      this.isLayoutRedraw = true;
      if (nodeDataDtos.length == 0) {
        this._rootNodeId = this._defaultRootNodeId;
        this.navHome = undefined;
        this.navItems = [];
        this.activeNodeId = undefined;
        this.hoveredNode = undefined;
        this.highlightedHoverNode = undefined;
        this.graphSelectedNodes = [];
        this.deletedNodes = [];
        this.currentDeletedNodesIndex = -1;
        this.cy?.elements()?.remove();
        this.isLayoutRedraw = false;
      } else {
        if (!this.activeNodeId) {
          this._rootNodeId = nodeDataDtos.find((x) => x.isMain)?.id ?? this._defaultRootNodeId;
          this.initNavMenuItems();
        }
        this.activeNodeId = nodeDataDtos.find((x) => x.isMain)?.id;
        this.hoveredNode = undefined;
        this.highlightedHoverNode = undefined;
        this.graphSelectedNodes = [];
        this.deletedNodes = [];
        this.currentDeletedNodesIndex = -1;
        if (this.cy) {
          this.redrawGraph();
        }
      }
    });
    effect(() => {
      // only for the graph, to select the "master" mode
      const nodeId = this.selectedNodeId();
      if (this.isLayoutRedraw) {
        return;
      }

      if (this.graphSelectedNodes.length > 1) {
        return;
      }
      if (nodeId) {
        const node = this.cy?.$(`#${nodeId}`);
        if (node && this.graphSelectedNodes.length == 1 && this.graphSelectedNodes[0] == node) {
          return;
        }
        if (node) {
          this.graphSelectedNodes[0]?.unselect();
          node.select();
        }
      } else {
        if (this.graphSelectedNodes.length == 1) {
          this.graphSelectedNodes[0].unselect();
        }
      }
    });
  }

  ngOnInit() {
    this.inputFilterByNameControl.valueChanges.pipe(debounceTime(500)).subscribe((value) => {
      this.onInputChange(value ?? '');
    });
    if (this.isStatic()) {
      this.inputFilterByNameControl.setValue(this.staticInputFilterByNameValue());
      this.inputFilterByNameControl.disable();
    }
    this.initNavMenuItems();
  }

  ngAfterViewInit() {
    this.setupCyLayout();
    this.setupCyEvents();
    this.redrawGraph();
  }

  // #region Cy Setup
  private setupCyLayout() {
    this.cy = cytoscape({
      container: this.graphContainer.nativeElement,
      elements: [],
      layout: this.fcoseLayoutOptions as FcoseLayoutOptions,
      style: [
        {
          selector: ':parent',
          style: {
            'background-color': 'gray',
            'background-opacity': 0.2,
            label: 'data(label)',
            // @ts-ignore
            padding: 20,
          },
        },
        {
          selector: '.parentNode',
          style: {
            'text-valign': 'top',
            'text-halign': 'center',
            'text-background-color': 'aqua',
            'text-margin-y': 18,
            'font-size': '14px',
            'font-weight': 'bold',
          },
        },
        {
          selector: '.parentNodeDark',
          style: {
            color: 'Gainsboro',
          },
        },
        {
          selector: '.parentNodeLight',
          style: {
            color: 'Black',
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
          selector: '.mainBackgroundColor',
          style: {
            'background-color': 'Aqua',
          },
        },
        {
          selector: '.nodePackage',
          style: {
            label: 'data(label)',
            width: 'mapData(label.length, 1, 30, 20, 200)',
            height: '20px',
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
            shape: 'rectangle',
          },
        },
        {
          selector: '.nodeLeaf',
          style: {
            shape: 'round-rectangle',
            // @ts-ignore
            'corner-radius': '12px',
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
          selector: '.hoveredCommon, .selectedCommon',
          style: {
            width: 'mapData(label.length, 1, 30, 20, 240)',
            height: '24px',
            'font-size': '12px',
            'transition-property': 'width height background-color font-size border-color opacity',
            'transition-duration': 300,
          },
        },
        {
          selector: '.hoveredDark, .selectedDark',
          style: {
            'border-width': 4,
            'border-color': 'Aqua',
            'border-style': 'solid',
          },
        },
        {
          selector: '.hoveredLight, .selectedLight',
          style: {
            'border-width': 4,
            'border-color': 'Black',
            'border-style': 'solid',
          },
        },
        {
          selector: '.hovered',
          style: {
            // @ts-ignore
            'background-fill': 'linear-gradient',
            //'background-gradient-stop-colors': 'RoyalBlue Aqua RoyalBlue', //DeepSkyBlue
            // @ts-ignore
            'background-gradient-stop-colors': 'RoyalBlue DeepSkyBlue', //DeepSkyBlue
            'background-gradient-direction': 'to-right',
          },
        },
        {
          selector: '.hoveredIncomers',
          style: {
            'background-color': 'RoyalBlue',
          },
        },
        {
          selector: '.hoveredOutgoers',
          style: {
            'background-color': 'DeepSkyBlue',
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
          selector: '.selected',
          style: {
            // @ts-ignore
            'background-fill': 'linear-gradient',
            // @ts-ignore
            'background-gradient-stop-colors': 'Red Salmon', //Salmon
            'background-gradient-direction': 'to-right',
          },
        },
        {
          selector: '.selectedIncomers',
          style: {
            'background-color': 'Red',
          },
        },
        {
          selector: '.selectedOutgoers',
          style: {
            'background-color': 'Salmon',
          },
        },

        {
          selector: '.selectedHighlight',
          style: {
            'overlay-opacity': 0.5,
            'overlay-color': 'Salmon',
            'font-style': 'italic',
          },
        },
        {
          selector: '.hoveredEdge',
          style: {
            width: 3,
            'line-color': 'Violet',
            'transition-property': 'line-color opacity',
            'transition-duration': 300,
          },
        },
        {
          selector: '.selectedEdge',
          style: {
            width: 3,
            'line-color': 'LightCoral',
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
          selector: '.deleted',
          style: {
            visibility: 'hidden',
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
        {
          selector: '.filtered-semihighlighted',
          style: {
            opacity: 0.8,
          },
        },
        {
          selector: '.graph-selected',
          style: {
            'font-style': 'italic',
            'font-weight': 'bold',
            //  'shape': 'round-rectangle',
            //  // @ts-ignore
            //  'corner-radius': '12px',
            //  'background-fill': 'linear-gradient',
            //  'background-gradient-stop-colors': 'red pink magenta blue',
            //  'background-gradient-direction': 'to-right'
            //  /*'background-gradient-stop-positions': '0% 100%'*/
          },
        },
      ],
    });

    const extraStyles = this.extraColorClasses().map((c) => ({
      selector: `.${c.name}`,
      style: {
        'background-color': c.code,
      },
    }));
    const mergedStyles = [...extraStyles, ...(this.cy.style() as any).json()];
    this.cy.style().fromJson(mergedStyles).update();
  }

  private xxx: boolean = false;
  private setupCyEvents() {
    if (this.isStatic()) {
      return;
    }
    this.cy.on('mouseover', 'node', (event) => {
      const node = event.target as NodeSingular;
      this.hoveredNode = node;
      this.highlightHoveredNode(node);
    });

    this.cy.on('mouseout', 'node', (event) => {
      this.hoveredNode = undefined;
      this.unhighlightHoveredNode(event.target as NodeSingular);
    });

    // this.cy.on('click', 'node', (event) => {
    //   this.xxx=false;
    //   const rawEvent = event.originalEvent;
    //   const isCtrlShiftClick = rawEvent?.ctrlKey && rawEvent?.shiftKey;
    //
    //   if (isCtrlShiftClick) {
    //     this.xxx = true;
    //     console.log("mama");
    //   }
    //
    // })

    this.cy.on('dbltap', 'node', (event) => {
      this.diveInNode(event.target as NodeSingular);
    });

    this.cy.on('select', 'node', (event) => {
      this.highlightSelectedNode(event.target as NodeSingular);
    });

    this.cy.on('unselect', 'node', (event) => {
      this.unhighlightSelectedNode(event.target as NodeSingular);
    });
  }
  // #endregion

  // #region Node Select
  private highlightSelectedNode(node: NodeSingular) {
    if (node.isParent()) {
      node.unselect();
      return;
    }
    if (this.graphSelectedNodes.some((x) => x === node)) {
      return;
    }
    this.graphSelectedNodes.push(node);
    if (this.graphSelectedNodes.length == 2) {
      this.unhighlightNode(this.graphSelectedNodes[0], 'selected');
      this.highlightNode(this.graphSelectedNodes[0], 'selected', true);
    }
    if (this.highlightedHoverNode) {
      this.unhighlightHoveredNode(this.highlightedHoverNode);
    }
    this.highlightNode(node, 'selected', this.graphSelectedNodes.length > 1);
    this.selectedNodeIdChange.emit(node.id());
  }

  private unhighlightSelectedNode(node: NodeSingular) {
    this.graphSelectedNodes = this.graphSelectedNodes.filter((x) => x != node);
    this.unhighlightNode(node, 'selected', this.graphSelectedNodes.length > 1);
    if (this.highlightedHoverNode) {
      this.unhighlightHoveredNode(this.highlightedHoverNode);
    }
    if (this.graphSelectedNodes.length == 1) {
      this.highlightNode(this.graphSelectedNodes[0], 'selected');
    }
    if (this.hoveredNode === node) {
      this.highlightHoveredNode(node);
    }
    this.selectedNodeIdChange.emit(undefined);
  }
  // #endregion

  // #region Node Hover
  private highlightHoveredNode(node: NodeSingular) {
    if (this.highlightedHoverNode == node || node.isParent()) {
      return;
    }
    if (this.highlightedHoverNode) {
      this.unhighlightHoveredNode(this.highlightedHoverNode);
    }
    this.highlightNode(node, 'hovered');
    this.highlightedHoverNode = node;
  }

  private unhighlightHoveredNode(node: NodeSingular) {
    if (node.isParent() || this.isLayoutRedraw) {
      return;
    }
    this.unhighlightNode(node, 'hovered');
    this.highlightedHoverNode = undefined;
  }
  // #endregion

  // #region Node Highlight
  private highlightNode(node: NodeSingular, stylePrefix: 'hovered' | 'selected', justNode: boolean = false) {
    node.addClass(`${stylePrefix}Common ${stylePrefix} ${stylePrefix}${this.darkLightMode}`);
    this.swapClassNodesHighlightedByName(node, 'highlight');
    if (justNode) {
      return;
    }
    node.incomers('node').forEach((depNode: NodeSingular) => {
      if (!depNode.hasClass('selectedIncomers') && !depNode.hasClass('selectedCommon')) {
        depNode.addClass(`${stylePrefix}Common`);
        if (node.outgoers('node').has(depNode)) {
          depNode.addClass(`${stylePrefix}Highlight`);
        } else {
          depNode.addClass(`${stylePrefix}Incomers`);
        }
      }
      this.swapClassNodesHighlightedByName(depNode, 'highlight');
    });
    node.outgoers('node').forEach((depNode: NodeSingular) => {
      if (!depNode.hasClass('selectedOutgoers') && !depNode.hasClass('selectedCommon')) {
        depNode.addClass(`${stylePrefix}Common ${stylePrefix}Outgoers`);
      }
      this.swapClassNodesHighlightedByName(depNode, 'highlight');
    });

    node.connectedEdges().forEach((edge: EdgeSingular) => {
      edge.addClass(`${stylePrefix}Edge`);
    });
  }

  private unhighlightNode(node: NodeSingular, stylePrefix: 'hovered' | 'selected', justNode: boolean = false) {
    node.removeClass(`${stylePrefix}Common ${stylePrefix} ${stylePrefix}${this.darkLightMode}`);
    this.swapClassNodesHighlightedByName(node, 'unhighlight');
    if (justNode) {
      return;
    }
    node.outgoers('node').forEach((depNode: NodeSingular) => {
      depNode.removeClass(`${stylePrefix}Common ${stylePrefix}Outgoers ${stylePrefix}Highlight`);
      this.swapClassNodesHighlightedByName(depNode, 'unhighlight');
    });
    node.incomers('node').forEach((depNode: NodeSingular) => {
      depNode.removeClass(`${stylePrefix}Common ${stylePrefix}Incomers`);
      this.swapClassNodesHighlightedByName(depNode, 'unhighlight');
    });

    node.connectedEdges().forEach((edge: EdgeSingular) => {
      edge.removeClass(`${stylePrefix}Edge`);
    });
  }
  // #endregion

  // #region Node Select/Hover helpers
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
      } else {
        this.cy.nodes().forEach((node: NodeSingular) => {
          node.removeClass('filtered-highlighted');
          node.removeClass('filtered-unhighlighted');
        });
      }
    });
  }

  private swapClassNodesHighlightedByName(node: NodeSingular, action: 'highlight' | 'unhighlight') {
    if (node.hasClass('filtered-unhighlighted') && action == 'highlight') {
      node.removeClass('filtered-unhighlighted');
      node.addClass('filtered-semihighlighted');
      return;
    }
    if (node.hasClass('filtered-semihighlighted') && action == 'unhighlight') {
      node.removeClass('filtered-semihighlighted');
      node.addClass('filtered-unhighlighted');
    }
  }

  private swapClassDarkLikghtMode(oldMode: 'Dark' | 'Light', newMode: 'Dark' | 'Light') {
    this.cy?.nodes(`.selected${oldMode}`).forEach((node) => {
      node.removeClass(`selected${oldMode}`);
      node.addClass(`selected${newMode}`);
    });
    this.cy?.nodes(`.parentNode${oldMode}`).forEach((node) => {
      node.removeClass(`parentNode${oldMode}`);
      node.addClass(`parentNode${newMode}`);
    });
  }
  // #endregion

  private diveInNode(node: NodeSingular) {
    if (node.isParent() || node.hasClass('nodeLeaf')) {
      return;
    }
    if (this.activeNodeId !== node.id()) {
      this.activeNodeIdChange.emit(node.id());
    }
  }

  // #region Menu Bar Events
  onZoomIn() {
    this.cy.animate({
      zoom: this.cy.zoom() + 0.1,
      duration: 300,
    });
  }

  onZoomOut() {
    this.cy.animate({
      zoom: this.cy.zoom() - 0.1,
      duration: 300,
    });
  }

  onZoomFit() {
    const visibleElements = this.cy.elements().filter((ele) => !ele.hasClass('deleted'));
    this.cy.animate({
      fit: {
        eles: visibleElements,
        padding: 10,
      },
      duration: 300,
    });
  }

  onDeleteNode() {
    this.deleteNodesAndOrphans(false);
  }

  onDeleteNodeAndChildren() {
    this.deleteNodesAndOrphans(true);
  }

  onUndo() {
    if (this.currentDeletedNodesIndex == -1 || this.deletedNodes.length == 0) {
      return;
    }
    this.undeletedNodeIdsChange.emit(this.deletedNodes[this.currentDeletedNodesIndex].nodeIds);
    this.recreateNodes(this.deletedNodes[this.currentDeletedNodesIndex].nodeIds);
    const lastNodeIds: string[] = [...this.deletedNodes[this.currentDeletedNodesIndex].mainNodeIds];
    setTimeout(() => {
      this.graphSelectedNodes.forEach((node) => {
        node.unselect();
      });
      lastNodeIds.forEach((nodeId) => {
        const node = this.cy.getElementById(nodeId) as NodeSingular;
        node?.select();
      });
    }, 0);
    this.currentDeletedNodesIndex--;
  }

  onRedo() {
    if (this.currentDeletedNodesIndex >= this.deletedNodes.length - 1) {
      return;
    }
    const mainNodeIds = this.deletedNodes[this.currentDeletedNodesIndex + 1].mainNodeIds;
    const nodes = mainNodeIds
      .map((id) => this.cy.getElementById(id) as NodeSingular)
      .filter((node) => node !== undefined);
    switch (this.deletedNodes[this.currentDeletedNodesIndex + 1].deleteType) {
      case 'node':
        this.deleteNodesAndOrphans(false, nodes);
        break;
      case 'nodeAndChildren':
        this.deleteNodesAndOrphans(true, nodes);
        break;
    }
  }

  onShowAll() {
    if (this.deletedNodes.length == 0 || this.currentDeletedNodesIndex == -1) {
      return;
    }
    for (let i = this.currentDeletedNodesIndex; i >= 0; i--) {
      this.undeletedNodeIdsChange.emit(this.deletedNodes[i].nodeIds);
      this.recreateNodes(this.deletedNodes[i].nodeIds);
    }
    this.deletedNodes = [];
    this.currentDeletedNodesIndex = -1;
  }

  async onHelp() {
    const { FcoseHelpComponent } = await import('../fcose-help/fcose-help.component'); // Lazy load the component
    this.helpContainer.clear();
    this.helpContainer.createComponent(FcoseHelpComponent);
    this.isHelpDialogVisible = true;
  }

  // #endregion

  private redrawGraph() {
    this.cy.elements().addClass('hidden');

    setTimeout(() => {
      this.cy.elements().remove();

      this.cy.add(this.getElements());

      this.cy.elements().addClass('hidden');

      this.cy.layout(this.fcoseLayoutOptions as FcoseLayoutOptions).run();
      this.onZoomFit();

      this.updateNavMenuItems(this.activeNodeId ?? '');
      setTimeout(() => {
        this.cy.elements().removeClass('hidden');

        const node = this.cy.$(`#${this.selectedNodeId()}`);
        if (node) {
          node.select();
        }
        if (this.inputFilterByNameValue) {
          this.onNodesHighlightByName(this.inputFilterByNameValue);
        }
        if (this.isStatic()) {
          let node = this.cy.$(`#${this.staticSelectedNodeId()}`);
          if (node) {
            this.highlightSelectedNode(node);
          }
          node = this.cy.$(`#${this.staticHighlightedNodeId()}`);
          if (node) {
            this.highlightHoveredNode(node);
          }
        }
        this.isLayoutRedraw = false;
      }, 500);
    }, 350);
  }

  private getElements(): ElementDefinition[] {
    const elements: ElementDefinition[] = [];
    const groupMap = new Map<string, number>();
    this.nodeDataDtos()
      .filter((x) => x.groupName)
      .forEach((x) => {
        const currentCount = (groupMap.get(x.groupName ?? '') || 0) + 1;
        groupMap.set(x.groupName ?? '', currentCount);
      });
    groupMap.forEach((value, key) => {
      if (value > 1) {
        //elements.push({ data: { id: key, label: key }, classes: 'nodeCommon' });
        elements.push({ data: { id: key, label: key }, classes: `parentNode parentNode${this.darkLightMode}` });
      }
    });

    this.nodeDataDtos().forEach((nodeData) => {
      const parentId = (groupMap.get(nodeData.groupName ?? '') || 0) > 1 ? nodeData.groupName : undefined;
      elements.push({
        data: {
          id: nodeData.id,
          label: nodeData.name ? nodeData.name : '\u2003', // so called 'em space' - just a 'long' space,
          parent: parentId,
        },
        classes: `nodeCommon nodePackage ${nodeData.colorClass ?? 'mainBackgroundColor'} ${nodeData.dependsOn?.length ? 'nodeBranch' : 'nodeLeaf'}`,
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
      this.navItems[potentialIndex].class = 'breadcrumb-size';
      this.activeNodeId = nodeId;
      return;
    }

    if (this.navItems.length > 0) {
      this.navItems[this.navItems.length - 1].class = 'breadcrumb-pointer';
    }
    const newDataDetailDto = this.getDataDetailDtoById(nodeId);
    if (newDataDetailDto?.name) {
      this.navItems = [
        ...this.navItems,
        {
          id: nodeId,
          label: newDataDetailDto?.name ? newDataDetailDto?.name : '\u2003', // so called 'em space' - just a 'long' space,
          class: 'breadcrumb-size',
        },
      ];
    }
    this.activeNodeId = nodeId;
  }
  // #endregion

  getDataDetailDtoById(id: string | undefined | null): NodeDataDto | undefined {
    return this.nodeDataDtos().find((x) => x.id == id);
  }

  onInputChange(value: string) {
    this.inputFilterByNameValue = value;
    this.onNodesHighlightByName(value);
  }

  // region delete nodes
  private deleteNodesAndOrphans(areChildrenIncluded: boolean, mainNodes: NodeSingular[] = []) {
    const isRedo = mainNodes.length > 0;

    if (!isRedo) {
      mainNodes = [...this.graphSelectedNodes];
    }
    this.graphSelectedNodes.forEach((node) => node.unselect());
    const mainNodeIds: string[] = [];
    const deletedNodes: string[] = [];
    mainNodes.forEach((node) => {
      mainNodeIds.push(node.id());
      areChildrenIncluded
        ? this.deleteNodeChildrenAndOrphans(node, deletedNodes)
        : this.deleteNodeAndOrphans(node, deletedNodes);
    });
    this.cleanupParentsAndOrphans(deletedNodes);
    this.processDeletedNodeIds(mainNodeIds, deletedNodes, areChildrenIncluded, isRedo);
  }

  private deleteNodeAndOrphans(node: NodeSingular, deletedNodes: string[]) {
    deletedNodes.push(node.id());
    node.addClass('deleted');
    node.connectedEdges().addClass('deleted');
    node
      .outgoers('node')
      .filter((x: NodeSingular) => !x.hasClass('deleted'))
      .filter((x: NodeSingular) => x.connectedEdges().filter((x) => !x.hasClass('deleted')).length === 0)
      .forEach((x: NodeSingular) => {
        deletedNodes.push(x.id());
        x.addClass('deleted');
      });
  }

  private deleteNodeChildrenAndOrphans(node: NodeSingular, deletedNodes: string[]) {
    this.deleteNodeAndOrphans(node, deletedNodes);
    node
      .outgoers('node')
      .filter((node: NodeSingular) => !node.hasClass('deleted'))
      .forEach((x: NodeSingular) => this.deleteNodeAndOrphans(x, deletedNodes));
  }

  private cleanupParentsAndOrphans(deletedNodeIds: string[]) {
    this.cy
      .nodes()
      .filter((node) => node.isParent())
      .filter((parentNode: NodeSingular) => parentNode.children().filter((x) => !x.hasClass('deleted')).length < 2)
      .forEach((parent) => {
        parent.children().forEach((node: NodeSingular) => {
          node.move({ parent: null });
        });
        parent.addClass('deleted');
        deletedNodeIds.push(parent.id());
      });
    this.cy
      .nodes()
      .filter(
        (x) =>
          !x.isParent() &&
          x.connectedEdges().filter((y) => !y.hasClass('deleted')).length === 0 &&
          !x.hasClass('deleted'),
      )
      .forEach((x) => {
        deletedNodeIds.push(x.id());
        x.addClass('deleted');
      });
  }

  recreateNodes(deletedNodes: string[]) {
    deletedNodes.forEach((nodeId) => {
      const node = this.cy.getElementById(nodeId);
      if (node) {
        node.removeClass('deleted');
        node
          .connectedEdges()
          .filter((edge) => !edge.source().hasClass('deleted') && !edge.target().hasClass('deleted'))
          .forEach((edge) => {
            edge.removeClass('deleted');
          });
      }
    });
    const x = this.cy.nodes('.parentNode').filter((x) => x.children().length === 0 && !x.hasClass('deleted'));
    x.forEach((x) => {
      this.nodeDataDtos()
        .filter((y) => y.groupName == x.id())
        .forEach((y) => {
          this.cy.nodes(`#${y.id}`).move({ parent: x.id() });
        });
    });
  }

  private processDeletedNodeIds(
    mainNodeIds: string[],
    deletedNodes: string[],
    areChildrenIncluded: boolean,
    isRedo: boolean,
  ) {
    if (deletedNodes.length > 0) {
      this.deletedNodeIdsChange.emit(deletedNodes);
      const deleteType = areChildrenIncluded ? 'nodeAndChildren' : 'node';
      if (!isRedo) {
        this.deletedNodes = this.deletedNodes.slice(0, this.currentDeletedNodesIndex + 1);
        this.deletedNodes.push({ deleteType: deleteType, mainNodeIds: mainNodeIds, nodeIds: deletedNodes });
      }
      this.currentDeletedNodesIndex++;
    }
  }
  // endregion
}
