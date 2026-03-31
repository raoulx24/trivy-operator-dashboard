import { ChangeDetectionStrategy, Component, effect, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AlertDto } from '../../../api/models/alert-dto';
import { GenericObjectArraySummaryPipe } from '../../pipes/generic-object-array-summary.pipe';
import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { AlertsService } from '../../services/alerts.service';
import { TrivyToolbarComponent } from '../../ui-elements/trivy-toolbar/trivy-toolbar.component';
import { NumberStringUtil } from '../../utils/number-string.utils';

import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { SelectButtonModule, SelectButtonOptionClickEvent } from 'primeng/selectbutton';
import { TagModule } from 'primeng/tag';
import { TreeTableModule, TreeTableNodeCollapseEvent, TreeTableNodeExpandEvent } from 'primeng/treetable';

interface AlertNodeData {
  key: string;
  level: number;
  label: string;
  message?: string;
  childrenLabels?: string;
  children: AlertNodeData[];
  statistics: {
    category: string;
    count: number;
  }[];
  count: number;
  severityId: number;
  isLeaf: boolean;
  isRoot: boolean;
  isExpanded: boolean;
}

interface TreeExpandLevelOption {
  id: number;
  label: string;
}

@Component({
  selector: 'app-alerts',
  imports: [
    ButtonModule,
    SelectButtonModule,
    TagModule,
    TreeTableModule,
    SeverityCssStyleByIdPipe,
    VulnerabilityCountPipe,
    TrivyToolbarComponent,
    GenericObjectArraySummaryPipe,
    FormsModule,
  ],
  templateUrl: './alerts.component.html',
  styleUrl: './alerts.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AlertsComponent implements OnInit {
  treeData = signal<TreeNode<AlertNodeData>[]>([]);
  private allTreeNodes: TreeNode<AlertNodeData>[] = [];

  treeExpandLevelOptions = signal<TreeExpandLevelOption[]>([]);
  treeExpandLevelOptionValue = signal<number>(0);

  alertsService: AlertsService = inject(AlertsService);

  private readonly severityOrder: Record<string, number> = {
    Error: 0,
    Warning: 1,
    Info: 4,
  };

  constructor() {
    effect(() => {
      const events = this.alertsService.refreshEvents();
    });
    effect(() => {
      const levelOptionValue = this.treeExpandLevelOptionValue();
      this.expandTreeNodesToLevel(levelOptionValue);
    });
  }

  ngOnInit() {
    this.loadData();
  }

  onNodeExpand(event: TreeTableNodeExpandEvent) {
    event.node.data.isExpanded = true;
    this.checkIfFullLevelExpanded(event.node);
  }

  onNodeCollapse(event: TreeTableNodeCollapseEvent) {
    this.collapseRecursively(event.node);
    event.node.data.isExpanded = false;
    this.checkIfFullLevelExpanded(event.node);
  }

  loadData() {
    this.allTreeNodes = [];
    const alerts = this.alertsService.getAlerts();
    this.treeData.set(this.buildTree(alerts));
    this.allTreeNodes.sort((a, b) => (a.data?.level ?? 999) - (b.data?.level ?? 999));
    this.treeExpandLevelOptionValue.set(0);
  }

  private buildTree(alerts: AlertDto[]): TreeNode<AlertNodeData>[] {
    const dtoMap = new Map<string, AlertNodeData>();
    let treeMaxLevel = 0;

    for (const alert of alerts) {
      const { severity, emitter, emitterKey, category, message } = alert;
      if (!severity || !emitter || !emitterKey || !category) continue;

      const keyPath = [severity, emitter, ...emitterKey.split('|')];
      let key = '';
      let prevNode: AlertNodeData | undefined;
      treeMaxLevel = Math.max(treeMaxLevel, keyPath.length - 1);

      for (let i = keyPath.length - 1; i >= 0; i--) {
        key = keyPath.slice(0, i + 1).join('|');

        let node = dtoMap.get(key);
        if (node) {
          const statistic = node.statistics?.find((s) => s.category === category);
          if (statistic) {
            statistic.count++;
          } else {
            node.statistics.push({ category, count: 1 });
          }
          node.count++;
          if (prevNode) {
            node.children.push(prevNode);
          }
          prevNode = undefined; // Reset prevNode since we found an existing node
        } else {
          node = {
            key,
            level: i,
            label: keyPath[i],
            message: i === keyPath.length - 1 ? (message ?? '') : undefined,
            children: prevNode ? [prevNode] : [],
            statistics: [{ category, count: 1 }],
            count: 1,
            severityId: this.severityOrder[severity] ?? 4, // Default to Info if not found
            isLeaf: i === keyPath.length - 1,
            isRoot: i === 0,
            isExpanded: false,
          };
          dtoMap.set(key, node);
          prevNode = node;
        }
      }
    }

    this.fillTreeExpandLevelOptions(treeMaxLevel);

    dtoMap.forEach((value) => {
      value.children = value.children.sort((a, b) => a.label.localeCompare(b.label));
      value.childrenLabels = value.children.map((child) => child.label).join(', ');
    });

    const rootNodes = Array.from(dtoMap.values())
      .filter((node) => node.isRoot)
      .sort((a, b) => a.severityId - b.severityId);
    return this.convertToTreeNodes(rootNodes);
  }

  private convertToTreeNodes(data: AlertNodeData[]): TreeNode<AlertNodeData>[] {
    const treeNodes = data.map((node) => {
      const treeNode: TreeNode<AlertNodeData> = {
        key: node.key,
        label: node.label, // optional, if your tree uses the `label` field directly
        data: node,
        expanded: node.isExpanded,
        leaf: node.isLeaf,
        children: this.convertToTreeNodes(node.children || []),
      };

      return treeNode;
    });
    this.allTreeNodes = [...this.allTreeNodes, ...treeNodes];

    return treeNodes;
  }

  // on load; this method fills the treeExpandLevelOptions array with options for the select button
  private fillTreeExpandLevelOptions(treeMaxLevel: number) {
    const localTreeExpandLevelOptions: TreeExpandLevelOption[] = [];
    for (let i = 0; i <= treeMaxLevel; i++) {
      localTreeExpandLevelOptions.push({ id: i, label: NumberStringUtil.FormatOrdinal(i + 1) });
    }

    this.treeExpandLevelOptions.set(localTreeExpandLevelOptions);
  }

  private expandTreeNodesToLevel(level: number) {
    this.treeData.update((arr) => {
      for (let i = 0; i < this.allTreeNodes.length; i++) {
        const node = this.allTreeNodes[i];
        const nodeLevel = node.data?.level ?? 999;

        if (nodeLevel <= level) {
          node.expanded = nodeLevel < level;
          if (node.data) node.data.isExpanded = nodeLevel < level;
        } else {
          break;
        }
      }

      return [...arr];
    });
  }

  onOptionClickTreeExpandLevel(event: SelectButtonOptionClickEvent) {
    if (event.index === undefined) return;
    this.expandTreeNodesToLevel(event.index);
  }

  private checkIfFullLevelExpanded(changedNode: TreeNode<AlertNodeData>) {
    const changedLevel = changedNode.data?.level ?? -1;

    // All nodes at this level
    const nodesAtLevel = this.allTreeNodes.filter((n) => (n.data?.level ?? -1) === changedLevel);

    const expandedCount = nodesAtLevel.filter((n) => n.data?.isExpanded).length;
    const allExpanded = expandedCount === nodesAtLevel.length;
    const allCollapsed = expandedCount === 0;

    const currentValue = this.treeExpandLevelOptionValue();

    // --- CASE 1: Increase level when fully expanded ---
    if (allExpanded && currentValue < changedLevel + 1) {
      this.treeExpandLevelOptionValue.set(changedLevel + 1);
      return;
    }

    // --- CASE 2: Decrease level only if ALL deeper levels are collapsed ---
    if (allCollapsed && currentValue > changedLevel) {
      // Check deeper levels
      const deeperLevelsHaveExpandedNodes = this.allTreeNodes.some(
        (n) => (n.data?.level ?? -1) > changedLevel && n.data?.isExpanded === true,
      );

      // Only decrease if deeper levels are fully collapsed
      if (!deeperLevelsHaveExpandedNodes) {
        this.treeExpandLevelOptionValue.set(changedLevel);
      }

      return;
    }
  }

  private collapseRecursively(node: TreeNode<AlertNodeData>) {
    node.expanded = false;
    if (node.data) node.data.isExpanded = false;

    if (node.children) {
      for (const child of node.children) {
        this.collapseRecursively(child);
      }
    }
  }
}
