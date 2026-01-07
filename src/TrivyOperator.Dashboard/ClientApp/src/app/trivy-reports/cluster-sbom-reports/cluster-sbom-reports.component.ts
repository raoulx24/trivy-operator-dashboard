import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

import { ClusterSbomReportService } from '../../../api/services/cluster-sbom-report.service';
import { ClusterSbomReportDto } from '../../../api/models/cluster-sbom-report-dto';

import { GenericSbomComponent } from '../../ui-elements/generic-sbom/generic-sbom.component';
import { TreeNode } from 'primeng/api';

import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '../../pipes/severity-name-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';

import { CardModule} from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { TreeTableModule } from 'primeng/treetable';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-cluster-sbom-reports',
  imports: [GenericSbomComponent,
    SeverityCssStyleByIdPipe, SeverityNameByIdPipe, VulnerabilityCountPipe,
    CardModule, DialogModule, TagModule, TreeTableModule, TableModule],
  templateUrl: './cluster-sbom-reports.component.html',
  styleUrl: './cluster-sbom-reports.component.scss'
})
export class ClusterSbomReportsComponent implements OnInit  {
  dataDtos: ClusterSbomReportDto[] = [];
  selectedImageId?: string;
  selectedSbomReportImageDto?: ClusterSbomReportDto;

  // region dialog related vars
  isSbomReportOverviewDialogVisible = signal<boolean>(false);
  sbomReportDetailStatistics: Array<number | undefined> = [];
  sbomReportDetailPropertiesTreeNodes: TreeNode[] = [];

  compareFirstSelectedDto? : ClusterSbomReportDto;
  compareSecondSelectedDto? : ClusterSbomReportDto;

  // endregion

  private readonly service = inject(ClusterSbomReportService);
  private readonly router = inject(Router);

  ngOnInit() {
    this.getTableDataDtos();
  }

  getTableDataDtos() {
    this.service.getClusterSbomReportDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  onGetDataDtos(dtos: ClusterSbomReportDto[]) {
    this.dataDtos = dtos;
  }

  onSelectedImageIdChange(imageId: string | undefined) {
    setTimeout(() => {
      this.selectedSbomReportImageDto = this.dataDtos?.find(x => x.uid === imageId);
    }, 0);
    this.sbomReportDetailPropertiesTreeNodes = [];
    this.sbomReportDetailStatistics = [];
  }

  onCompareFirstDtoRequested(id: string) {
    this.compareFirstSelectedDto = this.dataDtos?.find(x => x.uid === id);
  }

  onCompareSecondDtoRequested(id: string) {
    this.compareSecondSelectedDto = this.dataDtos?.find(x => x.uid === id);
  }

  onRefreshRequestedChange() {
    this.getTableDataDtos();
  }

  onMultiActionEventChange(value: string) {
    switch (value) {
      case "goToDetailedPage":
        this.goToDetailedPage();
        break;
      case "Info":
        this.onSbomReportOverviewDialogOpen();
        break;
      default:
        console.error("cluster sbom - multi action call back - unknown: " + event);
    }
  }

  onSbomReportOverviewDialogOpen() {
    if (this.sbomReportDetailPropertiesTreeNodes.length == 0) {
      this.sbomReportDetailPropertiesTreeNodes = this.getSbomReportPropertyTreeNodes();
    }
    if (this.sbomReportDetailStatistics.length == 0) {
      this.sbomReportDetailStatistics.push(this.selectedSbomReportImageDto?.criticalCount ?? -1);
      this.sbomReportDetailStatistics.push(this.selectedSbomReportImageDto?.highCount ?? -1);
      this.sbomReportDetailStatistics.push(this.selectedSbomReportImageDto?.mediumCount ?? -1);
      this.sbomReportDetailStatistics.push(this.selectedSbomReportImageDto?.lowCount ?? -1);
      this.sbomReportDetailStatistics.push(this.selectedSbomReportImageDto?.unknownCount ?? -1);

      this.sbomReportDetailStatistics.push(this.selectedSbomReportImageDto?.details?.length ?? 0);
      this.sbomReportDetailStatistics.push(this.selectedSbomReportImageDto?.details?.map(item => item.dependsOn)
        .filter((deps): deps is Array<string> => Array.isArray(deps))
        .reduce((sum, deps) => sum + deps.length, 0) ?? 0);
    }

    this.isSbomReportOverviewDialogVisible.set(true);
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(
      this.router.createUrlTree(['cluster-sbom-reports-detailed'])
    );
    window.open(url, '_blank');
  }

  /**
   * Get Properties TreeNodes
   * First, it creates a Map<> with all PropertyName and {propertyValue, usedBy} - usedBy is the SbomDetail Name
   * Then, for each one, it creates a Set<> with propertyValue and then counts children
   * the final usedByCount for each propertyName is a unique sum of all usedBy children
   */
  private getSbomReportPropertyTreeNodes(): TreeNode[] {
    const tree: TreeNode[] = [];

    const dataMap = new Map<string, { propValue: string, usedBy: string }[]>();

    this.selectedSbomReportImageDto?.details?.forEach(item => {
      const usedBy = item.name ?? "unknown";

      item.properties?.forEach(property => {
        const propName = property[0] ?? "unknown";
        const propValue = property[1] ?? "unknown";

        if (propName === "tod.group") return;

        if (!dataMap.has(propName)) {
          dataMap.set(propName, []);
        }

        dataMap.get(propName)!.push({ propValue, usedBy });
      });
    });

    dataMap.forEach((entries, propName) => {
      const uniqueUsedBySet = new Set<string>();

      const propNameNode: TreeNode = {
        data: { name: propName, usedByCount: 0 },
        children: []
      };

      const propValueUsedByMap = new Map<string, Set<string>>();
      entries.forEach(entry => {
        uniqueUsedBySet.add(entry.usedBy);

        if (!propValueUsedByMap.has(entry.propValue)) {
          propValueUsedByMap.set(entry.propValue, new Set<string>());
        }
        propValueUsedByMap.get(entry.propValue)!.add(entry.usedBy);
      });

      propValueUsedByMap.forEach((usedBySet, propValue) => {
        const propValueNode: TreeNode = {
          data: { name: propValue, usedByCount: usedBySet.size },
          children: []
        };

        usedBySet.forEach(usedBy => {
          propValueNode.children!.push({
            data: { name: usedBy, usedByCount: undefined },
            children: []
          });
        });

        propNameNode.children!.push(propValueNode);
      });

      propNameNode.data.usedByCount = uniqueUsedBySet.size;

      tree.push(propNameNode);
    });

    return tree;
  }
}
