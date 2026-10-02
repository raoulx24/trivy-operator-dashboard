import { TrivyReportResourceInfoDto } from '../../../api/models/trivy-report-resource-info-dto';

// simple interfaces to help with typing and generics
export interface HasUid  {
  uid: string;
}

export interface HasNamespace {
  resourceNamespace: string;
}

export interface HasNamespaces {
  namespaceNames: string[];
}

export interface HasResources {
  resources: Array<TrivyReportResourceInfoDto> | null;
}

// main trivy report interfaces, to be extended by specific report types
export interface TrivyReport<TTrivyReportDetail extends TrivyReportDetail> extends HasUid {
  details: Array<TTrivyReportDetail>;
}

export interface TrivyReportDetail {
  id: string;
}

// interfaces for namespaced trivy reports
export interface NamespacedResourceTrivyReport<TTrivyReportDetail extends TrivyReportDetail>
  extends TrivyReport<TTrivyReportDetail> {
  resourceNamespace: string;
}

// interfaces for namespaced aggregate trivy reports
export interface NamespacedAggregateTrivyReport<TTrivyReportDetail extends TrivyReportDetail>
  extends TrivyReport<TTrivyReportDetail>, HasResources {
  namespaceNames: string[];
}


export interface TrivyReportComparable<TTrivyReportDetail extends TrivyReportComparableDetail> {
  uid: string;
  details: Array<TTrivyReportDetail>;
}

export interface TrivyReportComparableDetail extends TrivyReportDetail {
  matchKey: string;
}
