import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-open-trivy-report',
  standalone: true,
  template: '',
})
export class OpenTrivyReportComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  ngOnInit(): void {
    const target = this.route.snapshot.paramMap.get('target');

    const allowedTargets = new Set([
      'config-audit-reports',
      'exposed-secret-reports',
      'sbom-reports',
      'vulnerability-reports',
    ]);

    if (!target || !allowedTargets.has(target)) {
      this.router.navigate(['']);
      return;
    }

    const state: Record<string, unknown> = {};

    for (const key of this.route.snapshot.queryParamMap.keys) {
      state[key] = this.route.snapshot.queryParamMap.get(key);
    }

    this.router.navigate([target], {
      state,
      replaceUrl: true,
    });
  }
}
