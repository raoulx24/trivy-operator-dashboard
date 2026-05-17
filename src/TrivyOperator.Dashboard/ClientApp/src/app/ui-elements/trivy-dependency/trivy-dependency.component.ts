import { Component, HostListener, effect, input } from '@angular/core';
import { Router } from '@angular/router';
import { TreeNode, TreeTableNode } from 'primeng/api';
import { v4 as uuid } from 'uuid';

import { DigestNode } from '../../../api/models/digest-node';
import { TrivyDependencyTreeDto } from '../../../api/models/trivy-dependency-tree-dto';
import { TrivyReportNode } from '../../../api/models/trivy-report-node';
import { VrHistoryEntryNode } from '../../../api/models/vr-history-entry-node';
import { WorkloadNode } from '../../../api/models/workload-node';
import { TrivyReportDependenciesService } from '../../../api/services/trivy-report-dependencies.service';

import { NodeDataDto } from '../fcose/fcose.types';
import { FcoseComponent } from '../fcose/fcose.component';
import { ButtonModule } from 'primeng/button';
import { SplitterModule } from 'primeng/splitter';
import { TagModule } from 'primeng/tag';
import { TreeTableModule } from 'primeng/treetable';
import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';

interface TrivyReportTreeNodeData {
  id: string;
  objectType: string;
  description: string;
  isTrivyReport: boolean;
  hasSeverities: boolean;
  critical: number;
  high: number;
  medium: number;
  low: number;
  unknown: number;
}

export interface ImageInfo {
  digest: string;
  namespaceName: string;
}

@Component({
  selector: 'app-trivy-dependency',
  imports: [
    FcoseComponent,
    ButtonModule,
    SplitterModule,
    TagModule,
    TreeTableModule,
    SeverityCssStyleByIdPipe,
    VulnerabilityCountPipe,
  ],
  templateUrl: './trivy-dependency.component.html',
  styleUrl: './trivy-dependency.component.scss',
})
export class TrivyDependencyComponent {
  trivyImage = input<ImageInfo | undefined>();

  trivyReportDependencyDto?: TrivyDependencyTreeDto;

  treeNodes: TreeNode<TrivyReportTreeNodeData>[] = [];
  selectedTreeNode?: TreeNode<TrivyReportTreeNodeData>;
  selectedNodeId?: string;

  nodeDataDtos: NodeDataDto[] = [];

  screenSize = this.getScreenSize();

  extraColorClasses = [
    { name: 'buttermilk', code: '#FFF2B2' },
    { name: 'sunbeam-yellow', code: '#F7C948' },
    { name: 'amber-glow', code: '#E8B04B' },
    { name: 'harvest-orange', code: '#F4A261' },
    { name: 'spiced-apricot', code: '#D88B4A' },
    { name: 'burnt-sienna', code: '#B25C33' },
  ];

  constructor(
    private service: TrivyReportDependenciesService,
    private router: Router,
  ) {
    effect(() => {
      const img = this.trivyImage();
      this.resetState();
      if (img) this.loadData(img);
    });
  }

  // ---------------------------------------------------------------------
  // LOAD + RESET
  // ---------------------------------------------------------------------

  private resetState() {
    this.treeNodes = [];
    this.nodeDataDtos = [];
    this.selectedTreeNode = undefined;
    this.selectedNodeId = undefined;
  }

  private loadData(img: ImageInfo) {
    this.service
      .getTrivyReportDependencyDtoByDigestNamespace({
        digest: img.digest,
        namespaceName: img.namespaceName,
      })
      .subscribe({
        next: (res) => this.onData(res),
        error: (err) => console.error(err),
      });
  }

  private onData(res: TrivyDependencyTreeDto) {
    this.trivyReportDependencyDto = res;
    this.treeNodes = this.buildTree(res.digest);
    setTimeout(() => (this.nodeDataDtos = this.buildGraph(res.digest)), 0);
  }

  // ---------------------------------------------------------------------
  // TREE BUILDERS
  // ---------------------------------------------------------------------

  private buildTree(root: DigestNode): TreeNode<TrivyReportTreeNodeData>[] {
    const rootId = this.idRoot(root);

    const rootNode = this.node(
      rootId,
      'Image',
      `${root.imageRepository}/${root.imageName}:${root.imageTag}`,
      false,
    );

    rootNode.children = [
      this.buildTrivyReports(root),
      this.buildHistory(root),
      this.buildWorkloads(root),
    ];

    return [rootNode];
  }

  private buildTrivyReports(root: DigestNode): TreeNode<TrivyReportTreeNodeData> {
    const groupId = this.idTrivyReportsGroup(root);

    return {
      key: groupId,
      data: {
        id: groupId,
        objectType: 'TrivyReports',
        description: 'Trivy Reports',
        isTrivyReport: false,
        hasSeverities: false,
        critical: 0,
        high: 0,
        medium: 0,
        low: 0,
        unknown: 0,
      },
      expanded: true,
      children: root.trivyReports.map((r, i) =>
        this.node(
          this.idTrivyReport(root, r, i),
          r.type,
          r.description,
          true,
          r.type !== 'Sbom',
          r.criticalCount,
          r.highCount,
          r.mediumCount,
          r.lowCount,
          r.unknownCount,
        )
      ),
    };
  }

  private buildHistory(root: DigestNode): TreeNode<TrivyReportTreeNodeData> {
    const groupId = this.idHistoryGroup(root);

    return {
      key: groupId,
      data: {
        id: groupId,
        objectType: 'History',
        description: 'Vulnerability Report History',
        isTrivyReport: false,
        hasSeverities: false,
        critical: 0,
        high: 0,
        medium: 0,
        low: 0,
        unknown: 0,
      },
      expanded: true,
      children: root.vrHistory.entries
        .sort((a, b) => a.firstSeenAt.localeCompare(b.firstSeenAt))
        .map((e, i) =>
          this.node(
            this.idHistoryEntry(root, e, i),
            'HistoryEntry',
            `First seen: ${e.firstSeenAt.replace(/[TZ]/g, ' ')}`,
            true,
            true,
            e.criticalCount,
            e.highCount,
            e.mediumCount,
            e.lowCount,
            e.unknownCount,
          ),
        ),
    };
  }

  private buildWorkloads(root: DigestNode): TreeNode<TrivyReportTreeNodeData> {
    const groupId = this.idWorkloadsGroup(root);

    return {
      key: groupId,
      data: {
        id: groupId,
        objectType: 'Workloads',
        description: 'Workloads',
        isTrivyReport: false,
        hasSeverities: false,
        critical: 0,
        high: 0,
        medium: 0,
        low: 0,
        unknown: 0,
      },
      expanded: true,
      children: root.workloads.workloads.map((w, i) => {
        const wlId = this.idWorkload(root, w, i);
        const caId = this.idConfigAudit(root, w, i);

        const wlNode = this.node(
          wlId,
          w.resourceKind,
          `${w.resourceKind}/${w.resourceName}`,
          false,
        );

        wlNode.children = [
          this.node(
            caId,
            'ConfigAudit',
            w.configAudit.description,
            true,
            true,
            w.configAudit.criticalCount,
            w.configAudit.highCount,
            w.configAudit.mediumCount,
            w.configAudit.lowCount,
            0,
          ),
        ];

        return wlNode;
      }),
    };
  }

  // ---------------------------------------------------------------------
  // GRAPH BUILDER
  // ---------------------------------------------------------------------

  private buildGraph(root: DigestNode): NodeDataDto[] {
    const nodes: NodeDataDto[] = [];

    const rootId = this.idRoot(root);
    const trGroupId = this.idTrivyReportsGroup(root);
    const histGroupId = this.idHistoryGroup(root);
    const wlGroupId = this.idWorkloadsGroup(root);

    let dependsOn: Array<string> = [];

    // root
    nodes.push(
      this.graphNode(
        rootId,
        `${root.imageName}:${root.imageTag}`,
        [trGroupId, histGroupId, wlGroupId],
        'sunbeam-yellow'
      )
    );

    // TrivyReports group
    dependsOn = [];

    root.trivyReports.forEach((r, i) => {
      const id = this.idTrivyReport(root, r, i);
      nodes.push(
        this.graphNode(
          id,
          `${r.type} (${r.criticalCount}/${r.highCount}/${r.mediumCount}/${r.lowCount}/${r.unknownCount})`,
          [],
          'spiced-apricot',
          'Trivy Reports',
        ),
      );
      dependsOn.push(id);
    });

    nodes.push(this.graphNode(trGroupId, 'TrivyReports', dependsOn, 'buttermilk', 'Trivy Reports'));

    // History group
    dependsOn = [];

    root.vrHistory.entries.forEach((e, i) => {
      const id = this.idHistoryEntry(root, e, i);
      nodes.push(
        this.graphNode(
          id,
          `Seen: ${e.firstSeenAt.replace(/[TZ]/g, ' ')} (${e.criticalCount}/${e.highCount}/${e.mediumCount}/${e.lowCount}/${e.unknownCount})`,
          [],
          'burnt-sienna',
          'Vulnerability Reports History',
        ),
      );
      dependsOn.push(id);
    });

    nodes.push(this.graphNode(histGroupId, 'History', dependsOn, 'amber-glow', 'Vulnerability Reports History'));

    // Workloads group
    dependsOn = [];

    root.workloads.workloads.forEach((w, i) => {
      const wlId = this.idWorkload(root, w, i);
      const caId = this.idConfigAudit(root, w, i);

      nodes.push(this.graphNode(wlId, `${w.resourceKind}/${w.resourceName}`, [caId], 'buttermilk', 'Workloads'));

      nodes.push(
        this.graphNode(
          caId,
          `ConfigAudit (${w.configAudit.criticalCount}/${w.configAudit.highCount}/${w.configAudit.mediumCount}/${w.configAudit.lowCount})`,
          [],
          'spiced-apricot',
          'Workloads',
        ),
      );
      dependsOn.push(wlId);
    });

    nodes.push(this.graphNode(wlGroupId, 'Workloads', dependsOn, 'harvest-orange', 'Workloads'));

    return nodes;
  }


  // ---------------------------------------------------------------------
  // HELPERS
  // ---------------------------------------------------------------------

  private node(
    id: string,
    type: string,
    description: string,
    isTrivy: boolean,
    hasSev = false,
    c = 0,
    h = 0,
    m = 0,
    l = 0,
    u = 0,
  ): TreeNode<TrivyReportTreeNodeData> {
    return {
      key: id,
      data: {
        id,
        objectType: type,
        description,
        isTrivyReport: isTrivy,
        hasSeverities: hasSev,
        critical: c,
        high: h,
        medium: m,
        low: l,
        unknown: u,
      },
      expanded: true,
    };
  }

  private groupNode(
    type: string,
    description: string,
    children: TreeNode<TrivyReportTreeNodeData>[],
  ): TreeNode<TrivyReportTreeNodeData> {
    const id = uuid();
    return {
      key: id,
      data: {
        id,
        objectType: type,
        description,
        isTrivyReport: false,
        hasSeverities: false,
        critical: 0,
        high: 0,
        medium: 0,
        low: 0,
        unknown: 0,
      },
      expanded: true,
      children,
    };
  }

  private graphNode(id: string, name: string, dependsOn: string[], colorClass: string, groupName: string | undefined = undefined): NodeDataDto {
    return { id, name, dependsOn, isMain: false, colorClass, groupName: groupName };
  }

  // ---------------------------------------------------------------------
  // OPEN REPORT
  // ---------------------------------------------------------------------

  onOpenTrivyReport(n: TrivyReportTreeNodeData) {
    const ns = this.trivyReportDependencyDto?.digest.namespaceName;
    const digest = this.trivyReportDependencyDto?.digest.imageDigest;

    const map: Record<string, { path: string; params: any }> = {
      vulnerability: {
        path: 'open/vulnerability-reports',
        params: { namespaceName: ns, digest },
      },
      configaudit: {
        path: 'open/config-audit-reports',
        params: { uid: n.id },
      },
      exposedsecret: {
        path: 'open/exposed-secret-reports',
        params: { namespaceName: ns, digest },
      },
      sbom: {
        path: 'open/sbom-reports',
        params: { namespaceName: ns, digest },
      },
      historyentry: {
        path: 'open/vulnerability-reports-history',
        params: { namespaceName: ns, digest },
      },
    };

    const entry = map[n.objectType.toLowerCase()];
    if (entry) {
      const url = this.router.serializeUrl(this.router.createUrlTree([entry.path], { queryParams: entry.params }));
      window.open(url, '_blank');
    }
  }

  // ---------------------------------------------------------------------
  // TREE ↔ GRAPH SYNC
  // ---------------------------------------------------------------------

  onGraphSelectedNodeIdChange(id?: string) {
    if (id !== this.selectedTreeNode?.data?.id) {
      this.selectedTreeNode = id ? this.findTreeNodeById(this.treeNodes, id) : undefined;
    }
  }

  onTreeTableNodeSelect(e: TreeTableNode<TrivyReportTreeNodeData>) {
    this.selectedNodeId = e.node?.data?.id;
  }

  onTreeTableNodeUnselect() {
    this.selectedNodeId = undefined;
  }

  private findTreeNodeById(
    nodes: TreeTableNode<TrivyReportTreeNodeData>[],
    id: string,
  ): TreeTableNode<TrivyReportTreeNodeData> | undefined {
    for (const n of nodes) {
      if (n.data.id === id) return n;
      if (n.children) {
        const found = this.findTreeNodeById(n.children as any, id);
        if (found) return found;
      }
    }
    return undefined;
  }

  // ---------------------------------------------------------------------
  // SCREEN SIZE
  // ---------------------------------------------------------------------

  @HostListener('window:resize')
  onResize() {
    this.screenSize = this.getScreenSize();
  }

  private getScreenSize(): string {
    const cssVar = getComputedStyle(document.documentElement).getPropertyValue('--tod-screen-width-sm').trim();
    return window.innerWidth < parseInt(cssVar, 10) ? 'sm' : 'lg';
  }

  // ---------------------------------------------------------------------
  // SAFE ID HELPERS (only lowercase letters, numbers, and hyphens)
  // ---------------------------------------------------------------------

  private sanitize(value: string): string {
    return value
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')   // replace invalid chars with -
      .replace(/^-+|-+$/g, '');      // trim leading/trailing -
  }

  private idRoot(root: DigestNode): string {
    return this.sanitize(`root-${root.id}`);
  }

  private idTrivyReportsGroup(root: DigestNode): string {
    return this.sanitize(`root-${root.id}-trivy-reports`);
  }

  private idHistoryGroup(root: DigestNode): string {
    return this.sanitize(`root-${root.id}-history`);
  }

  private idWorkloadsGroup(root: DigestNode): string {
    return this.sanitize(`root-${root.id}-workloads`);
  }

  private idTrivyReport(root: DigestNode, r: TrivyReportNode, index: number): string {
    return this.sanitize(`root-${root.id}-tr-${r.type}-${index}`);
  }

  private idHistoryEntry(root: DigestNode, e: VrHistoryEntryNode, index: number): string {
    return this.sanitize(`root-${root.id}-vrh-${e.name}-${index}`);
  }

  private idWorkload(root: DigestNode, w: WorkloadNode, index: number): string {
    return this.sanitize(
      `root-${root.id}-wl-${w.resourceKind}-${w.resourceName}-${w.containerName}-${index}`
    );
  }

  private idConfigAudit(root: DigestNode, w: WorkloadNode, index: number): string {
    return this.sanitize(
      `root-${root.id}-wl-${w.resourceKind}-${w.resourceName}-${w.containerName}-${index}-configaudit`
    );
  }
}
