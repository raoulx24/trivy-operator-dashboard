import { Component } from '@angular/core';

import { FcoseComponent } from '../fcose/fcose.component';
import { NodeDataDto } from '../fcose/fcose.types';

@Component({
  selector: 'app-fcose-help',
  standalone: true,
  imports: [FcoseComponent],
  templateUrl: './fcose-help.component.html',
  styleUrl: './fcose-help.component.scss',
})
export class FcoseHelpComponent {
  nodeDataDtos: NodeDataDto[] = [
    { id: 'n01', name: 'root notdimmed', isMain: true, dependsOn: ['n10', 'n20', 'n30'] },
    { id: 'n10', name: 'selected parent notdimmed', dependsOn: ['n11'] },
    { id: 'n11', name: 'selected node notdimmed', dependsOn: ['n12', 'n13'], groupName: 'group title 1' },
    { id: 'n12', name: 'selected child node notdimmed', dependsOn: ['n13'], groupName: 'group title 1' },
    { id: 'n13', name: 'selected child leaf notdimmed', dependsOn: ['n11'], groupName: 'group title 1' },
    { id: 'n20', name: 'hovered parent notdimmed', dependsOn: ['n21'] },
    { id: 'n21', name: 'hovered node notdimmed', dependsOn: ['n22', 'n23'] },
    { id: 'n22', name: 'hovered child node notdimmed', dependsOn: ['n23', 'n21'], groupName: 'group title 2' },
    { id: 'n23', name: 'hovered child node notdimmed', dependsOn: ['n24'], groupName: 'group title 2' },
    { id: 'n24', name: 'leaf notdimmed', dependsOn: [] },
    { id: 'n30', name: 'leaf dimmed', dependsOn: [] },
  ];
}
