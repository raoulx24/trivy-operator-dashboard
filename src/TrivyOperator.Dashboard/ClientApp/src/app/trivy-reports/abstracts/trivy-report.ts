export interface TrivyReport<TTrivyReportDetail extends TrivyReportDetail> {
  uid: string;
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
