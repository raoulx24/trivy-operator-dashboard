import { TrivyReportResourceInfoDto } from '../../../../api/models/trivy-report-resource-info-dto';

// Simple interfaces to help with structural typing and generics.
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

// Main Trivy report interfaces.
export interface TrivyReport<TTrivyReportDetail extends TrivyReportDetail> {
    uid: string;

    criticalCount: number;
    highCount: number;
    mediumCount: number;
    lowCount: number;

    details: Array<TTrivyReportDetail>;
}

export interface TrivyReportDetail {
    id: string;
    matchKey: string;
}

// Namespaced Trivy report.
export interface NamespacedResourceTrivyReport<
    TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReport<TTrivyReportDetail> {
    resourceNamespace: string;
}

// Namespaced aggregate Trivy report.
export interface NamespacedAggregateTrivyReport<
    TTrivyReportDetail extends TrivyReportDetail,
> extends TrivyReport<TTrivyReportDetail> {
    resources: Array<TrivyReportResourceInfoDto>;
    digest: string;
    namespaceNames: string[];
}

// Comparable Trivy report.
export interface TrivyReportComparable<
    TTrivyReportDetail extends TrivyReportComparableDetail,
> {
    uid: string;
    details: Array<TTrivyReportDetail>;
}

export interface TrivyReportComparableDetail extends TrivyReportDetail {
    matchKey: string;
}
