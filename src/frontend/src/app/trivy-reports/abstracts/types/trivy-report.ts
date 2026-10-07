import { TrivyReportResourceInfoDto } from '../../../../api/models/trivy-report-resource-info-dto';
import { DataSet, DataSetDetail } from './data-set';

// Simple interfaces to help with structural typing and generics
export interface HasUid {
    uid: string;
}

export interface HasNamespace {
    resourceNamespace: string;
}

export interface HasNamespaces {
    namespaceNames: string[];
}

export interface HasResources {
    resources: Array<TrivyReportResourceInfoDto>;
}

// trivy report interface with generic detail type
export interface TrivyReport<TDetail extends TrivyReportDetail>
  extends DataSet<TDetail> {
  criticalCount: number;
  highCount: number;
  mediumCount: number;
  lowCount: number;
}

export interface TrivyReportDetail extends DataSetDetail {
  matchKey: string;
}

// Clustered Scoped Trivy report
export interface ClusteredScopedResourceTrivyReport<
  TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReport<TTrivyReportDetail> {
  resourceName: string;
}

// Namespaced Trivy report
export interface NamespacedResourceTrivyReport<
  TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReport<TTrivyReportDetail> {
  resourceNamespace: string;
  resourceName: string;
}

// Namespaced aggregate Trivy report
export interface NamespacedAggregateTrivyReport<
  TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReport<TTrivyReportDetail> {
  digest: string;
  lastImageNameAndTag: string;
  namespaceNames: string[];
}

// Comparable Trivy report
export interface TrivyReportComparable<
    TTrivyReportDetail extends TrivyReportComparableDetail,
> {
    uid: string;
    details: Array<TTrivyReportDetail>;
}

export interface TrivyReportComparableDetail extends TrivyReportDetail {
    matchKey: string;
}
