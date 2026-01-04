import { Component, effect, HostListener, input, OnInit } from '@angular/core';

import { FcoseComponent } from '../fcose/fcose.component';
import { NodeDataDto } from '../fcose/fcose.types';

import { TrivyReportDependencyService } from '../../../api/services/trivy-report-dependency.service';
import { TrivyReportDependencyDto } from '../../../api/models/trivy-report-dependency-dto';

import { ButtonModule } from 'primeng/button'
import { SplitterModule } from 'primeng/splitter';
import { TagModule } from 'primeng/tag';
import { TreeTableModule } from 'primeng/treetable';
import { TreeNode, TreeTableNode } from 'primeng/api';

import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { TrivyReportDependencyDetailDto } from "../../../api/models/trivy-report-dependency-detail-dto";
import { Router } from '@angular/router';

interface TrivyReportTreeNodeData {
  id: string;
  objectType: string;
  description: string;
  hasSeverities: boolean;
  isTrivyReport: boolean;
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
  imports: [FcoseComponent, ButtonModule, SplitterModule, TagModule, TreeTableModule, SeverityCssStyleByIdPipe, VulnerabilityCountPipe],
  templateUrl: './trivy-dependency.component.html',
  styleUrl: './trivy-dependency.component.scss'
})
export class TrivyDependencyComponent {
  nodeDataDtos: NodeDataDto[] = [];

  trivyImage = input<ImageInfo | undefined>();
  protected trivyReportDependencyDto?: TrivyReportDependencyDto;

  screenSize: string = this.getScreenSize();

  treeNodes: TreeNode<TrivyReportTreeNodeData>[] = [];
  selectedTreeNode?: TreeNode<TrivyReportTreeNodeData>;
  selectedNodeId?: string;

  extraColorClasses: {name: string, code: string}[] = [
    { name: 'buttermilk',     code: '#FFF2B2' },
    { name: 'sunbeam-yellow', code: '#F7C948' },
    { name: 'amber-glow',     code: '#E8B04B' },
    { name: 'harvest-orange', code: '#F4A261' },
    { name: 'spiced-apricot', code: '#D88B4A' },
    { name: 'burnt-sienna',   code: '#B25C33' }
  ];

  constructor(private service: TrivyReportDependencyService, private router: Router) {
    effect(() => {
      const currentTrivyImage = this.trivyImage();
      this.nodeDataDtos = [];
      this.treeNodes = [];
      this.selectedTreeNode = undefined;
      this.selectedNodeId = undefined;

      if (currentTrivyImage) {
        this.getDataDto(currentTrivyImage);
      }
    });
  }

  getDataDto(trivyImage: ImageInfo) {
    this.service
      .getTrivyReportDependecyDtoByDigestNamespace({digest: trivyImage.digest, namespaceName: trivyImage.namespaceName})
      .subscribe({
        next: (res) => this.onGetDataDto(res),
        error: (err) => console.error(err),
      });
  }

  onGetDataDto(res: TrivyReportDependencyDto) {
    this.trivyReportDependencyDto = res;
    this.getTreeNodes(res);
    setTimeout(() => {this.getFcoseNodes(res)}, 0)
    ;
  }

  private getTreeNodes(res: TrivyReportDependencyDto): void {
    const tree: TreeNode<TrivyReportTreeNodeData>[] = [];

    if (res.image) {
      const imageKey = res.image.id || res.image.imageDigest || 'image-root';

      const imageNode: TreeNode<TrivyReportTreeNodeData> = {
        key: imageKey,
        data: {
          id: res.image.id ?? '',
          objectType: 'Image',
          description: `${res.image.imageRepository ?? ''}/${res.image.imageName ?? ''}:${res.image.imageTag ?? ''}`,
          isTrivyReport: false,
          hasSeverities: false,
          critical: 0,
          high: 0,
          medium: 0,
          low: 0,
          unknown: 0
        },
        expanded: true,
        children: []
      };

      res.kubernetesDependencies?.forEach((dep, depIndex) => {
        if (dep.kubernetesResource) {
          const resourceKey = dep.kubernetesResource.id ?? `${imageKey}-resource-${depIndex}`;

          const resourceNode: TreeNode<TrivyReportTreeNodeData> = {
            key: resourceKey,
            data: {
              id: dep.kubernetesResource.id ?? '',
              objectType: `${dep.kubernetesResource.resourceKind ?? 'K8s Resource'}`,
              description: `${dep.kubernetesResource.resourceName ?? ''}`,
              isTrivyReport: false,
              hasSeverities: false,
              critical: 0,
              high: 0,
              medium: 0,
              low: 0,
              unknown: 0
            },
            expanded: true,
            children: []
          };

          dep.trivyReportDependencies?.forEach((detail, detailIndex) => {
            const detailKey = detail.uid ?? `${resourceKey}-detail-${detailIndex}`;

            resourceNode.children!.push({
              key: detailKey,
              data: {
                id: detail.uid ?? '',
                objectType: detail.trivyReport || 'unknown',
                description: '',
                isTrivyReport: true,
                hasSeverities: detail.trivyReport !== 'Sbom',
                critical: detail.criticalCount ?? 0,
                high: detail.highCount ?? 0,
                medium: detail.mediumCount ?? 0,
                low: detail.lowCount ?? 0,
                unknown: detail.unknownCount ?? 0
              },
              expanded: true
            });
          });

          imageNode.children!.push(resourceNode);
        }
      });

      tree.push(imageNode);
    }

    this.treeNodes = tree;
  }

  private getFcoseNodes(res: TrivyReportDependencyDto){
    const nodes: NodeDataDto[] = [];
    if (!res.image) {
      nodes.push({ id: 'undefined', name: 'undefined', isMain: true});
      return;
    }
    const rootNodeDepIds: string[] = [];
    const imageNodeLabel = `${res.image.imageName ?? 'n/a'}:${res.image.imageTag ?? 'n/a'}`;
    nodes.push({id: res.image.id, name: imageNodeLabel, isMain: true, dependsOn: rootNodeDepIds, colorClass: 'sunbeam-yellow', });

    (res.kubernetesDependencies ?? []).forEach((dependency) => {
      rootNodeDepIds.push(dependency.kubernetesResource?.id ?? 'n/a');

      const trivyRepIds: string[] = [];
      nodes.push({id: dependency.kubernetesResource?.id, name: dependency.kubernetesResource?.resourceName, isMain: false, dependsOn: trivyRepIds, colorClass: 'buttermilk', });
      (dependency.trivyReportDependencies ?? []).forEach((trivyRep) => {
        trivyRepIds.push(trivyRep.uid ?? 'n/a');
        nodes.push({id: trivyRep.uid ?? 'n/a', name: this.getTrivyRepLabel(trivyRep), colorClass: 'spiced-apricot'}, );
      })
    });
    this.nodeDataDtos = nodes;
  }

  private getTrivyRepLabel(trivyRep: TrivyReportDependencyDetailDto) {
    switch (trivyRep.trivyReport?.toLowerCase()) {
      case "configaudit":
      case "exposedsecret":
        return `${trivyRep.trivyReport} - ${trivyRep.criticalCount ?? 0} / ${trivyRep.highCount ?? 0} / ${trivyRep.mediumCount ?? 0} / ${trivyRep.lowCount ?? 0}`;
      case "vulnerability":
        return `${trivyRep.trivyReport} - ${trivyRep.criticalCount ?? 0} / ${trivyRep.highCount ?? 0} / ${trivyRep.mediumCount ?? 0} / ${trivyRep.lowCount ?? 0} / ${trivyRep.unknownCount ?? 0}`;
      default:
        return `${trivyRep.trivyReport}`;
    }
  }

  private getColorClass(trivyReportName: string): string {
    switch (trivyReportName.toLowerCase()) {
      case "configaudit":
        return "purple";
      case "exposedsecret":
        return "orange";
      case "sbom":
        return "turquoise";
      case "vulnerability":
        return "lime";
      default:
        return "aqua";
    }
  }

  // open Trivy Report
  onOpenTrivyReport(treeNodeData: TrivyReportTreeNodeData) {
    let url: string | undefined = undefined;
    switch (treeNodeData.objectType.toLowerCase()) {
      case "vulnerability":
        url = this.router.serializeUrl(
          this.router.createUrlTree(['vulnerability-reports'], { queryParams: {
              namespaceName: this.trivyReportDependencyDto?.image?.namespaceName,
              digest: this.trivyReportDependencyDto?.image?.imageDigest,
            }}));
        break;
      case "configaudit":
        url = this.router.serializeUrl(
          this.router.createUrlTree(['config-audit-reports'], { queryParams: { uid: treeNodeData.id } }));
        break;
      case "exposedsecret":
        url = this.router.serializeUrl(
          this.router.createUrlTree(['exposed-secret-reports'], { queryParams: {
              namespaceName: this.trivyReportDependencyDto?.image?.namespaceName,
              digest: this.trivyReportDependencyDto?.image?.imageDigest,
            }}));
        break;
      case "sbom":
        url = this.router.serializeUrl(
          this.router.createUrlTree(['sbom-reports'], { queryParams: {
              namespaceName: this.trivyReportDependencyDto?.image?.namespaceName,
              digest: this.trivyReportDependencyDto?.image?.imageDigest,
            }}));
        break;
    }

    if (url) {
      window.open(url, '_blank');
    }
  }

  // node change

  onGraphSelectedNodeIdChange(event: string | undefined) {
    const currentSelectedTreeNodeId = this.selectedTreeNode?.data?.id ?? undefined;
    if (event !== currentSelectedTreeNodeId) {
      if (!event) {
        this.selectedTreeNode = undefined;
        return;
      }
      this.selectedTreeNode = this.findTreeNodeById(this.treeNodes, event);
    }
  }

  onTreeTableNodeSelect(event: TreeTableNode<TrivyReportTreeNodeData>) {
    this.selectedNodeId = event.node?.data?.id;
  }

  onTreeTableNodeUnselect() {
    this.selectedNodeId = undefined;
  }

  findTreeNodeById(nodes: TreeTableNode<TrivyReportTreeNodeData>[], nodeId: string): TreeTableNode<TrivyReportTreeNodeData> | undefined {
    for (const node of nodes) {
      if (node.data.id === nodeId) return node;

      if (node.children && node.children.length > 0) {
        const found = this.findTreeNodeById(node.children as TreeTableNode<TrivyReportTreeNodeData>[], nodeId);
        if (found) return found;
      }
    }

    return undefined;
  }

  // screen size
  @HostListener('window:resize', [])
  onResize() {
    this.screenSize = this.getScreenSize();
  }

  getScreenSize(): string {
    const cssVarValue = getComputedStyle(document.documentElement)
      .getPropertyValue('--tod-screen-width-sm')
      .trim(); // Get and clean the CSS variable value

    const threshold = parseInt(cssVarValue, 10); // Convert it to a number

    return window.innerWidth < threshold ? 'sm' : 'lg';
  }
}
