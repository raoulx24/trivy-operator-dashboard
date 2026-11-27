import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  DestroyRef,
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

import cytoscape, { LayoutOptions, NodeSingular } from 'cytoscape';
import fcose from 'cytoscape-fcose';

import { MenuItem } from 'primeng/api';
import { BreadcrumbItemClickEvent, BreadcrumbModule } from 'primeng/breadcrumb';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';

import { IconComponent } from '../icon/icon.component';
import { ScrollerComponent } from '../scroller/scroller.component';

import { GraphDeleteHistory } from './cytoscape-graph.delete-history';
import { buildElements } from './cytoscape-graph.elements';
import { GraphHighlighter } from './cytoscape-graph.highlighter';
import { buildLayeredPositions, buildLayoutOptions } from './cytoscape-graph.layout';
import { DEFAULT_ROOT_NODE_ID, GraphNavigation } from './cytoscape-graph.navigation';
import { buildStyles } from './cytoscape-graph.styles';
import { DarkLightMode, GRAPH_THEMES } from './cytoscape-graph.theme';
import { DeletedNodes, GraphLayout, NodeDataDto } from './cytoscape-graph.types';

import { DarkModeService } from '@core/services/dark-mode.service';

cytoscape.use(fcose);

// a container resize settles (e.g. after an animated layout change) before the canvas follows
const RESIZE_DEBOUNCE_MS = 150;

const NO_NODES: NodeSingular[] = [];
const NO_DELETED_NODES: DeletedNodes[] = [];

// the layered layout: around the graph when it is fitted, and the largest zoom (a few nodes are not blown up)
const LAYERED_FIT_PADDING = 10;
const LAYERED_MAX_ZOOM = 1.25;

@Component({
  selector: 'app-cytoscape-graph',
  standalone: true,
  imports: [
    BreadcrumbModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    TagModule,
    IconComponent,
    ScrollerComponent,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './cytoscape-graph.component.html',
  styleUrl: './cytoscape-graph.component.scss',
})
export class CytoscapeGraphComponent implements AfterViewInit, OnInit {
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

  graphClasses = input<string>('graph graph-half');
  extraColorClasses = input<{ name: string; code: string }[]>([]);
  idealEdgeLength = input<number>(150);

  // false: the host shows its own breadcrumb (driven by its own GraphNavigation)
  isBreadcrumbVisible = input<boolean>(true);
  // true: after the container is resized, the graph is fitted again (zoom and pan change); false: only the canvas
  // follows the new size, the user's zoom and pan are kept
  isFitOnResize = input<boolean>(false);
  // fcose: the free force layout; layered: parents above the main node, children below it, wrapped to the width (it is
  // laid out again on every resize, fitted to the width, with labels hidden when zoomed out)
  layout = input<GraphLayout>('fcose');
  // false: no hiding of nodes (Hide Node / Hide Subtree / Undo / Redo / Show All are not shown)
  isDeleteEnabled = input<boolean>(true);
  // true: the graph takes the height of the host (which has to have one), not one of the graphClasses heights
  isFillHeight = input<boolean>(false);

  inputFilterByNameControl = new FormControl('');
  isHelpDialogVisible: boolean = false;

  private cy!: cytoscape.Core;
  private highlighter!: GraphHighlighter;
  private deleteHistory!: GraphDeleteHistory;
  private readonly navigation = new GraphNavigation({
    getNodeName: (nodeId) => this.getDataDetailDtoById(nodeId)?.name ?? undefined,
    onNavigate: (nodeId) => this.activeNodeIdChange.emit(nodeId),
  });

  private isLayoutRedraw: boolean = false;
  private inputFilterByNameValue: string = '';
  private darkLightMode: DarkLightMode = 'Dark';

  private darkModeService = inject(DarkModeService);
  private destroyRef = inject(DestroyRef);

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
        const node = this.findNode(nodeId);
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
    if (this.layout() === 'layered') {
      this.setupLayeredWheel();
    }
    this.setupResizeObserver();
    this.redrawGraph();
  }

  // cytoscape follows window resizes only; a container resize (splitter, animated layout) leaves a stale canvas
  // fcose is never re-run here, only the canvas size (and the fit, when asked for); layered is cheap and wraps to the
  // width, so it is laid out again
  private setupResizeObserver() {
    let timer: ReturnType<typeof setTimeout> | undefined;

    const observer = new ResizeObserver(() => {
      clearTimeout(timer);
      timer = setTimeout(() => {
        this.cy?.resize();
        if (this.isLayoutRedraw) {
          return;
        }
        if (this.layout() === 'layered') {
          this.cy.layout(this.getLayoutOptions()).run();
          this.fitLayered();
        } else if (this.isFitOnResize()) {
          this.onZoomFit();
        }
      }, RESIZE_DEBOUNCE_MS);
    });

    observer.observe(this.graphContainer.nativeElement);
    this.destroyRef.onDestroy(() => {
      clearTimeout(timer);
      observer.disconnect();
    });
  }

  /**
   * Layered: the graph is a tall page fitted to the width, so the wheel scrolls it (Shift + wheel or a horizontal wheel:
   * sideways), and the scroll stops at the top and the bottom of the graph; Ctrl + wheel zooms, around the pointer.
   */
  private setupLayeredWheel() {
    const container = this.graphContainer.nativeElement as HTMLElement;
    const onWheel = (event: WheelEvent) => {
      if (!this.cy || this.cy.elements().empty()) {
        return;
      }
      event.preventDefault();
      // lines / pages (some mice, Firefox) to pixels
      const unit = event.deltaMode === 1 ? 16 : event.deltaMode === 2 ? this.cy.height() : 1;
      if (event.ctrlKey) {
        const factor = Math.pow(1.002, -event.deltaY * unit);
        const rect = container.getBoundingClientRect();
        this.cy.zoom({
          level: this.cy.zoom() * factor,
          renderedPosition: { x: event.clientX - rect.left, y: event.clientY - rect.top },
        });
        return;
      }
      const deltaX = (event.shiftKey ? event.deltaY : event.deltaX) * unit;
      const deltaY = (event.shiftKey ? 0 : event.deltaY) * unit;
      this.cy.panBy({ x: -deltaX, y: -this.clampLayeredScroll(deltaY) });
    };
    container.addEventListener('wheel', onWheel, { passive: false });
    this.destroyRef.onDestroy(() => container.removeEventListener('wheel', onWheel));
  }

  /** The vertical scroll, cut so the graph's top stays at or above the top and its bottom at or below the bottom. */
  private clampLayeredScroll(deltaY: number): number {
    const box = this.cy.elements().renderedBoundingBox();
    const height = this.cy.height();
    if (box.h <= height - 2 * LAYERED_FIT_PADDING) {
      // the whole graph is visible: nothing to scroll
      return 0;
    }
    // scrolling down moves the graph up: at most until its bottom reaches the bottom edge, and the other way round
    const maxDown = Math.max(0, box.y2 - (height - LAYERED_FIT_PADDING));
    const maxUp = Math.max(0, LAYERED_FIT_PADDING - box.y1);

    return deltaY > 0 ? Math.min(deltaY, maxDown) : -Math.min(-deltaY, maxUp);
  }

  // #region Cy setup
  private setupCy() {
    this.cy = cytoscape({
      container: this.graphContainer.nativeElement,
      elements: [],
      layout: this.getLayoutOptions(),
      style: this.getStyles(),
      // layered: the wheel scrolls (see setupLayeredWheel), the zoom is on the buttons and on Ctrl + wheel
      userZoomingEnabled: this.layout() !== 'layered',
    });

    this.highlighter = new GraphHighlighter(this.cy, {
      isRedrawing: () => this.isLayoutRedraw,
      onHoveredNodeIdChange: (nodeId) => this.hoveredNodeIdChange.emit(nodeId),
      onSelectedNodeIdChange: (nodeId) => this.selectedNodeIdChange.emit(nodeId),
    });
    this.deleteHistory = new GraphDeleteHistory(this.cy, {
      getSelectedNodes: () => this.highlighter.selectedNodes,
      getNodeDataDtos: () => this.nodeDataDtos(),
      onDeleted: (nodeIds) => this.deletedNodeIdsChange.emit(nodeIds),
      onUndeleted: (nodeIds) => this.undeletedNodeIdsChange.emit(nodeIds),
    });
  }

  /** Re-applies the stylesheet for the current theme (dark/light mode switch). */
  private applyStyles() {
    const styles = this.getStyles();
    this.cy?.style().fromJson(styles).update();
  }

  private getStyles() {
    return buildStyles(this.extraColorClasses(), GRAPH_THEMES[this.darkLightMode], this.layout() === 'layered');
  }

  private getLayoutOptions(): LayoutOptions {
    return buildLayoutOptions(
      this.layout(),
      () => this.idealEdgeLength(),
      () => buildLayeredPositions(this.nodeDataDtos(), this.cy?.width() ?? 0),
    );
  }

  /**
   * Layered: fitted to the width (capped, so a few nodes are not blown up), and to the top; what does not fit in the
   * height is reached by panning. A graph that fits whole is centered.
   */
  private fitLayered() {
    const visibleElements = this.cy.elements().filter((ele) => !ele.hasClass('deleted'));
    if (visibleElements.empty()) {
      return;
    }
    const box = visibleElements.boundingBox();
    const availableWidth = this.cy.width() - 2 * LAYERED_FIT_PADDING;
    const availableHeight = this.cy.height() - 2 * LAYERED_FIT_PADDING;
    const zoom = Math.min(LAYERED_MAX_ZOOM, availableWidth / Math.max(box.w, 1));
    const isWholeVisible = box.h * zoom <= availableHeight;
    const fittedZoom = isWholeVisible ? Math.min(zoom, availableHeight / Math.max(box.h, 1)) : zoom;
    this.cy.zoom(fittedZoom);
    this.cy.pan({
      x: (this.cy.width() - box.w * fittedZoom) / 2 - box.x1 * fittedZoom,
      y: isWholeVisible
        ? (this.cy.height() - box.h * fittedZoom) / 2 - box.y1 * fittedZoom
        : LAYERED_FIT_PADDING - box.y1 * fittedZoom,
    });
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
    // layered: the main node is the one the graph is about, not the dive root, so it can be dived into too (the host
    // knows whether it already is the root)
    if (this.layout() === 'layered' || this.activeNodeId !== node.id()) {
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

  /** Zooms to the nodes highlighted by name; if there are none, to the selected node(s) + neighbours. Hover is ignored. */
  onZoomHighlighted() {
    this.highlighter?.clearHover();
    let eles = this.cy.nodes('.filtered-highlighted');
    if (eles.empty()) {
      eles = this.cy.nodes('.selectedCommon, .selected');
    }
    if (eles.empty()) {
      return;
    }
    this.cy.animate({
      fit: {
        eles: eles.union(eles.connectedEdges()),
        padding: 40,
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
    const { CytoscapeGraphHelpComponent } = await import('../cytoscape-graph-help/cytoscape-graph-help.component'); // Lazy load the component
    this.helpContainer.clear();
    this.helpContainer.createComponent(CytoscapeGraphHelpComponent);
    this.isHelpDialogVisible = true;
  }
  // #endregion

  private redrawGraph() {
    this.cy.elements().addClass('hidden');

    setTimeout(() => {
      this.cy.elements().remove();

      this.cy.add(buildElements(this.nodeDataDtos()));

      this.cy.elements().addClass('hidden');

      this.cy.layout(this.getLayoutOptions()).run();
      if (this.layout() === 'layered') {
        this.fitLayered();
      } else {
        this.onZoomFit();
      }

      this.updateActiveNode(this.activeNodeId ?? '');
      setTimeout(() => {
        this.cy.elements().removeClass('hidden');

        // layered: with no selection from outside, the main node (the one the graph is about) starts selected
        const selectedNodeId = this.selectedNodeId() ?? (this.layout() === 'layered' ? this.activeNodeId : undefined);
        this.findNode(selectedNodeId)?.select();
        if (this.inputFilterByNameValue) {
          this.highlighter.filterByName(this.inputFilterByNameValue);
        }
        if (this.isStatic()) {
          const selectedNode = this.findNode(this.staticSelectedNodeId());
          if (selectedNode) {
            this.highlighter.highlightSelectedNode(selectedNode);
          }
          const highlightedNode = this.findNode(this.staticHighlightedNodeId());
          if (highlightedNode) {
            this.highlighter.highlightHoveredNode(highlightedNode);
          }
        }
        this.isLayoutRedraw = false;
      }, 500);
    }, 350);
  }

  /**
   * The node with that id, if there is one. Not cy.$('#' + id): the ids are e.g. purls (pkg:nuget/x@1.0), which are not
   * valid selectors; such a selector matches nothing, and the empty collection it returns is still truthy.
   */
  private findNode(nodeId: string | undefined): NodeSingular | undefined {
    const node = nodeId ? this.cy?.getElementById(nodeId) : undefined;

    return node?.nonempty() ? (node as NodeSingular) : undefined;
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
