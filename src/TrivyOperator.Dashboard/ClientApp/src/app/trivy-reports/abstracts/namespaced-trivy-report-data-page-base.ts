import { TrivyReportDataPageBase } from './trivy-report-data-page-base';

export abstract class NamespacedTrivyReportDataPageBase<TData> extends TrivyReportDataPageBase<TData>{
  protected activeNamespaces: string[] = [];
}

export abstract class TrivyReportDataPageBaseWithNamespaceFilter<TData> extends NamespacedTrivyReportDataPageBase<TData> {
  
}
