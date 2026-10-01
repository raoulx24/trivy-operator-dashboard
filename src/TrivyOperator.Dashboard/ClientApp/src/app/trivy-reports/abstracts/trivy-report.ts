import { TrivyReportResourceInfoDto } from '../../../api/models/trivy-report-resource-info-dto';

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

export interface TrivyReport<TTrivyReportDetail extends TrivyReportDetail> extends HasUid {
  details: Array<TTrivyReportDetail>;
}

export interface TrivyReportDetail {
  id: string;
}

export interface TrivyReportComparable<TTrivyReportDetail extends TrivyReportComparableDetail> {
  uid: string;
  details: Array<TTrivyReportDetail>;
}

export interface TrivyReportComparableDetail extends TrivyReportDetail {
  matchKey: string;
}
