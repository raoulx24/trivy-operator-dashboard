import { Component } from '@angular/core';
import { ICellRendererParams } from "ag-grid-community";
import { ICellRendererAngularComp } from "ag-grid-angular";

@Component({
  standalone: true,
  template: `
        <span style="color:{{ frontColor }};background-color:{{ backgroundColor }}">
            <b>{{ newValue }}</b>
        </span>
    `
})
export class VulnerabilitSeverityRenderer implements ICellRendererAngularComp {
  public value: any;

  public backgroundColor!: string;
  public frontColor!: string;
  public newValue!: string;

  agInit(params: ICellRendererParams): void {
    switch (params.value) {
      case "CRITICAL":
        this.backgroundColor = "lightsalmon";
        this.newValue = "C"
        break;
      case "HIGH":
        this.backgroundColor = "orange";
        this.newValue = "H"
        break;
      case "MEDIUM":
        this.backgroundColor = "yellow";
        this.newValue = "M"
        break;
      case "LOW":
        this.backgroundColor = "lightyellow";
        this.newValue = "L"
        break;
      case "UNKNOWN":
        this.backgroundColor = "aqua";
        this.newValue = "U"
        break;
    }

    this.frontColor = "black";
    this.value = params.value;
  }

  refresh(params: ICellRendererParams) {
    return false;
  }
}
