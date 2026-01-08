import { Component, effect, inject, model, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AlertsService } from '../../services/alerts.service';
import { AlertDto } from '../../../api/models/alert-dto';
import { GenericObjectArraySummaryPipe } from '../../pipes/generic-object-array-summary.pipe';
import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';
import { TrivyToolbarComponent } from '../../ui-elements/trivy-toolbar/trivy-toolbar.component';
import { NumberStringUtil } from '../../utils/number-string.utils';

import { ButtonModule } from 'primeng/button';
import { SelectButtonModule, SelectButtonOptionClickEvent } from 'primeng/selectbutton';
import { TagModule } from 'primeng/tag';
import { TreeTableModule, TreeTableNodeCollapseEvent, TreeTableNodeExpandEvent } from 'primeng/treetable';
import { TreeNode } from 'primeng/api';

interface AlertNodeData {
  key: string,
  level: number,
  label: string;
  message?: string;
  childrenLabels?: string,
  children: AlertNodeData[],
  statistics: {
    category: string,
    count: number;
  }[],
  count: number;
  severityId: number;
  isLeaf: boolean;
  isRoot: boolean;
  isExpanded: boolean;
}

@Component({
  selector: 'app-alerts',
  imports: [
    ButtonModule, SelectButtonModule, TagModule, TreeTableModule,
    SeverityCssStyleByIdPipe, VulnerabilityCountPipe, TrivyToolbarComponent, GenericObjectArraySummaryPipe, FormsModule,
  ],
  templateUrl: './alerts.component.html',
  styleUrl: './alerts.component.scss'
})
export class AlertsComponent implements OnInit {
  treeData: TreeNode<AlertNodeData>[] = [];
  private allTreeNodes: TreeNode<AlertNodeData>[] = [];

  treeExpandLevelOptions: { id: number, label: string }[] = [];
  treeExpandLevelOptionValue = model<number>(0);

  alertsService: AlertsService = inject(AlertsService);

  private readonly severityOrder: Record<string, number> = {
    'Error': 0,
    'Warning': 1,
    'Info': 4,
  };

  constructor() {
    effect(() => {
      const events = this.alertsService.refreshEvents();
      this.loadData();
    })
  }

  ngOnInit() {
   setTimeout(() => {
      this.loadData();
   }, 100);
  }

  onNodeExpand(event: TreeTableNodeExpandEvent) {
    event.node.data.isExpanded = true;
    this.checkIfFullLevelExpanded();
  }

  onNodeCollapse(event: TreeTableNodeCollapseEvent) {
    event.node.data.isExpanded = false;
    this.checkIfFullLevelExpanded();
  }

  loadData() {
    const alerts = this.alertsService.getAlerts();
    this.treeData = this.buildTree(alerts);
    this.allTreeNodes
      .sort((a, b) => (a.data?.level ?? 999) - (b.data?.level ?? 999));
    this.treeExpandLevelOptionValue.set(0);
  }

  private buildTree(alerts: AlertDto[]): TreeNode<AlertNodeData>[] {
    const dtoMap = new Map<string, AlertNodeData>();
    let treeMaxLevel = 0;

    for (const alert of alerts) {
      const { severity, emitter, emitterKey, category, message } = alert;
      if (!severity || !emitter || !emitterKey || !category) continue;

      const keyPath = [severity, emitter, ...emitterKey.split("|")];
      let key = "";
      let prevNode: AlertNodeData | undefined;
      treeMaxLevel = Math.max(treeMaxLevel, keyPath.length - 1);

      for (let i = keyPath.length - 1; i >= 0; i--) {
        key = keyPath.slice(0, i + 1).join("|");

        let node = dtoMap.get(key);
        if (node) {
          const statistic = node.statistics?.find(s => s.category === category);
          if (statistic) {
            statistic.count++;
          } else {
            node.statistics.push({category, count: 1});
          }
          node.count++;
          if (prevNode) {
            node.children.push(prevNode);
          }
          prevNode = undefined; // Reset prevNode since we found an existing node
        }
        else {
          node = {
            key,
            level: i,
            label: keyPath[i],
            message: i === keyPath.length - 1 ? message ?? "" : undefined,
            children: prevNode ? [prevNode] : [],
            statistics: [{category, count: 1}],
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
      value.childrenLabels = value.children.map(child => child.label).join(", ");
    });

    const rootNodes = Array.from(dtoMap.values())
      .filter(node => node.isRoot)
      .sort((a, b) => a.severityId - b.severityId);
    return this.convertToTreeNodes(rootNodes);
  }

  private convertToTreeNodes(data: AlertNodeData[]): TreeNode<AlertNodeData>[] {
    const treeNodes = data.map(node => {
      const treeNode: TreeNode<AlertNodeData> = {
        key: node.key,
        label: node.label, // optional, if your tree uses the `label` field directly
        data: node,
        expanded: node.isExpanded,
        leaf: node.isLeaf,
        children: this.convertToTreeNodes(node.children || [])
      };

      return treeNode;
    });
    this.allTreeNodes = [...this.allTreeNodes, ...treeNodes];

    return treeNodes;
  }



  // on load; this method fills the treeExpandLevelOptions array with options for the select button
  private fillTreeExpandLevelOptions(treeMaxLevel: number) {
    this.treeExpandLevelOptions = [];
    for (let i = 0; i <= treeMaxLevel; i++) {
      this.treeExpandLevelOptions.push({ id: i, label: NumberStringUtil.FormatOrdinal(i + 1) });
    }
  }

  private expandTreeNodesToLevel(level: number) {
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

    this.treeData = [...this.treeData];
  }

  onOptionClickTreeExpandLevel(event: SelectButtonOptionClickEvent) {
    if (event.index === undefined) return;
    this.expandTreeNodesToLevel(event.index);
  }

  private checkIfFullLevelExpanded() {
    let currentLevel = this.allTreeNodes[0]?.data?.level ?? -1;
    let previousLevel: number = -1;


    for (let i = 0; i < this.allTreeNodes.length; i++) {
      if (this.allTreeNodes[i].data?.level !== currentLevel) {
        previousLevel = currentLevel;
        currentLevel = this.allTreeNodes[i].data?.level ?? 999;
      }

      if (this.allTreeNodes[i].data?.level === currentLevel && !this.allTreeNodes[i].data?.isExpanded) {
        currentLevel = previousLevel;
        break;
      }
    }

    // If we get through the loop, last level was fully expanded
    if (currentLevel !== this.treeExpandLevelOptionValue() - 1) {
      this.treeExpandLevelOptionValue.set(currentLevel + 1);
    }
  }
}
