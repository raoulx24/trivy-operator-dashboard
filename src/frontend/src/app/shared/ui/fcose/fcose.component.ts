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

import cytoscape, { NodeSingular } from 'cytoscape';
import fcose, { FcoseLayoutOptions } from 'cytoscape-fcose';

import { MenuItem } from 'primeng/api';
import { BreadcrumbItemClickEvent, BreadcrumbModule } from 'primeng/breadcrumb';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';

import { IconComponent } from '../icon/icon.component';
import { TrivyToolbarComponent } from '../trivy-toolbar/trivy-toolbar.component';

import { FcoseDeleteHistory } from './fcose.delete-history';
import { buildElements } from './fcose.elements';
import { FcoseHighlighter } from './fcose.highlighter';
import { buildLayoutOptions } from './fcose.layout';
import { DEFAULT_ROOT_NODE_ID, FcoseNavigation } from './fcose.navigation';
import { buildStyles } from './fcose.styles';
import { DarkLightMode, FCOSE_THEMES } from './fcose.theme';
import { DeletedNodes, NodeDataDto } from './fcose.types';

import { DarkModeService } from '@core/services/dark-mode.service';

cytoscape.use(fcose);

const NO_NODES: NodeSingular[] = [];
const NO_DELETED_NODES: DeletedNodes[] = [];

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

  activeNodeId?: string;
  activeNodeIdChange = output<string>();

  nodeDataDtos = input<NodeDataDto[]>([]);
  hoveredNodeIdChange = output<string | undefined>();

  // outside world communication for selected node (red)
  selectedNodeId = input<string | undefined>(undefined);
  selectedNodeIdChange = output<string | undefined>();

  deletedNodeIdsChange = output<string[]>();
  undeletedNodeIdsChange = output<string[]>();

  isStatic = input<boolean>(false);
  staticSelectedNodeId = input<string | undefined>(undefined);
  staticHighlightedNodeId = input<string | undefined>(undefined);
  staticInputFilterByNameValue = input<string>('');

  fcoseClasses = input<string>('fcose fcose-half');
  extraColorClasses = input<{ name: string; code: string }[]>([]);
  idealEdgeLength = input<number>(150);

  inputFilterByNameControl = new FormControl('');
  isHelpDialogVisible: boolean = false;

  private cy!: cytoscape.Core;
  private highlighter!: FcoseHighlighter;
  private deleteHistory!: FcoseDeleteHistory;
  private readonly navigation = new FcoseNavigation({
    getNodeName: (nodeId) => this.getDataDetailDtoById(nodeId)?.name ?? undefined,
    onNavigate: (nodeId) => this.activeNodeIdChange.emit(nodeId),
  });

  private readonly fcoseLayoutOptions: FcoseLayoutOptions = buildLayoutOptions(() => this.idealEdgeLength());
  private isLayoutRedraw: boolean = false;
  private inputFilterByNameValue: string = '';
  private darkLightMode: DarkLightMode = 'Dark';

  private darkModeService = inject(DarkModeService);

  // #region template bindings
  get navItems(): MenuItem[] {
    return this.navigation.navItems;
  }

  get navHome(): MenuItem | undefined {
    return this.navigation.navHome;
  }

  get graphSelectedNodes(): NodeSingular[] {
    return this.highlighter?.selectedNodes ?? NO_NODES;
  }

  get deletedNodes(): DeletedNodes[] {
    return this.deleteHistory?.entries ?? NO_DELETED_NODES;
  }

  get currentDeletedNodesIndex(): number {
    return this.deleteHistory?.currentIndex ?? -1;
  }
  // #endregion

  constructor() {
    effect(() => {
      this.darkLightMode = this.darkModeService.isDarkMode() ? 'Dark' : 'Light';
      this.applyStyles();
    });
    effect(() => {
      const nodeDataDtos = this.nodeDataDtos();
      this.isLayoutRedraw = true;
      if (nodeDataDtos.length == 0) {
        this.navigation.clear();
        this.activeNodeId = undefined;
        this.resetGraphState();
        this.cy?.elements()?.remove();
        this.isLayoutRedraw = false;
      } else {
        if (!this.activeNodeId) {
          this.navigation.init(nodeDataDtos.find((x) => x.isMain)?.id ?? DEFAULT_ROOT_NODE_ID);
        }
        this.activeNodeId = nodeDataDtos.find((x) => x.isMain)?.id;
        this.resetGraphState();
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

      const selectedNodes = this.graphSelectedNodes;
      if (selectedNodes.length > 1) {
        return;
      }
      if (nodeId) {
        const node = this.cy?.$(`#${nodeId}`);
        if (node && selectedNodes.length == 1 && selectedNodes[0] == node) {
          return;
        }
        if (node) {
          selectedNodes[0]?.unselect();
          node.select();
        }
      } else if (selectedNodes.length == 1) {
        selectedNodes[0].unselect();
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
    this.navigation.init();
  }

  ngAfterViewInit() {
    this.setupCy();
    this.setupCyEvents();
    this.redrawGraph();
  }

  // #region Cy setup
  private setupCy() {
    this.cy = cytoscape({
      container: this.graphContainer.nativeElement,
      elements: [],
      layout: this.fcoseLayoutOptions,
      style: buildStyles(this.extraColorClasses(), FCOSE_THEMES[this.darkLightMode]),
    });

    this.highlighter = new FcoseHighlighter(this.cy, {
      isRedrawing: () => this.isLayoutRedraw,
      onHoveredNodeIdChange: (nodeId) => this.hoveredNodeIdChange.emit(nodeId),
      onSelectedNodeIdChange: (nodeId) => this.selectedNodeIdChange.emit(nodeId),
    });
    this.deleteHistory = new FcoseDeleteHistory(this.cy, {
      getSelectedNodes: () => this.highlighter.selectedNodes,
      getNodeDataDtos: () => this.nodeDataDtos(),
      onDeleted: (nodeIds) => this.deletedNodeIdsChange.emit(nodeIds),
      onUndeleted: (nodeIds) => this.undeletedNodeIdsChange.emit(nodeIds),
    });
  }

  /** Re-applies the stylesheet for the current theme (dark/light mode switch). */
  private applyStyles() {
    const extraColorClasses = this.extraColorClasses();
    this.cy
      ?.style()
      .fromJson(buildStyles(extraColorClasses, FCOSE_THEMES[this.darkLightMode]))
      .update();
  }

  private setupCyEvents() {
    if (this.isStatic()) {
      return;
    }
    this.cy.on('mouseover', 'node', (event) => this.highlighter.onMouseOver(event.target as NodeSingular));
    this.cy.on('mouseout', 'node', (event) => this.highlighter.onMouseOut(event.target as NodeSingular));
    this.cy.on('dbltap', 'node', (event) => this.diveInNode(event.target as NodeSingular));
    this.cy.on('select', 'node', (event) => this.highlighter.highlightSelectedNode(event.target as NodeSingular));
    this.cy.on('unselect', 'node', (event) => this.highlighter.unhighlightSelectedNode(event.target as NodeSingular));
  }
  // #endregion

  private resetGraphState() {
    this.highlighter?.reset();
    this.deleteHistory?.reset();
  }

  private diveInNode(node: NodeSingular) {
    if (node.isParent() || node.hasClass('nodeLeaf')) {
      return;
    }
    if (this.activeNodeId !== node.id()) {
      this.activeNodeIdChange.emit(node.id());
    }
  }

  // #region Menu bar events
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
    this.deleteHistory.deleteNodes(false);
  }

  onDeleteNodeAndChildren() {
    this.deleteHistory.deleteNodes(true);
  }

  onUndo() {
    this.deleteHistory.undo();
  }

  onRedo() {
    this.deleteHistory.redo();
  }

  onShowAll() {
    this.deleteHistory.showAll();
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

      this.cy.add(buildElements(this.nodeDataDtos()));

      this.cy.elements().addClass('hidden');

      this.cy.layout(this.fcoseLayoutOptions).run();
      this.onZoomFit();

      this.updateActiveNode(this.activeNodeId ?? '');
      setTimeout(() => {
        this.cy.elements().removeClass('hidden');

        const node = this.cy.$(`#${this.selectedNodeId()}`);
        if (node) {
          node.select();
        }
        if (this.inputFilterByNameValue) {
          this.highlighter.filterByName(this.inputFilterByNameValue);
        }
        if (this.isStatic()) {
          let node = this.cy.$(`#${this.staticSelectedNodeId()}`);
          if (node) {
            this.highlighter.highlightSelectedNode(node);
          }
          node = this.cy.$(`#${this.staticHighlightedNodeId()}`);
          if (node) {
            this.highlighter.highlightHoveredNode(node);
          }
        }
        this.isLayoutRedraw = false;
      }, 500);
    }, 350);
  }

  // #region Navigation
  onNavItemClick(event: BreadcrumbItemClickEvent) {
    this.navigation.onItemClick(event);
  }

  private updateActiveNode(nodeId: string) {
    this.navigation.update(nodeId);
    this.activeNodeId = nodeId;
  }
  // #endregion

  getDataDetailDtoById(id: string | undefined | null): NodeDataDto | undefined {
    return this.nodeDataDtos().find((x) => x.id == id);
  }

  onInputChange(value: string) {
    this.inputFilterByNameValue = value;
    this.highlighter.filterByName(value);
  }
}
