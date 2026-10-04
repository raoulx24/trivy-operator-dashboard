// generic data set type
export interface DataSetDetail {
  id: string;
}

export interface DataSet<TDetail extends DataSetDetail> {
  uid: string;
  details: TDetail[];
}
