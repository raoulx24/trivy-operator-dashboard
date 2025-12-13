import { Component, inject, OnInit } from '@angular/core';

import { VulnerabilityReportDenormalizedDto } from '../../api/models/vulnerability-report-denormalized-dto';
import { VulnerabilityReportsService } from '../../api/services/vulnerability-reports.service';

import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-tests',
  imports: [TableModule],
  templateUrl: './tests.component.html',
  styleUrl: './tests.component.scss'
})
export class TestsComponent implements OnInit {
  dataDtos: VulnerabilityReportDenormalizedDto[] = [];
  isLoading: boolean = false;

  private readonly dataDtoService = inject(VulnerabilityReportsService);

  ngOnInit(): void {
    this.getTableDataDtos();
  }

  public getTableDataDtos() {
    this.isLoading = true;
    this.dataDtoService.getVulnerabilityReportDenormalizedDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => console.error(err),
    });
  }

  onGetDataDtos(dtos: VulnerabilityReportDenormalizedDto[]) {
    this.dataDtos = dtos;
    this.isLoading = false;
  }
}
