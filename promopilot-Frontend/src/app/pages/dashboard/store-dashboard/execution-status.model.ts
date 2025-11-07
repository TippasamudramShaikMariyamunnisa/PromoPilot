export interface ExecutionStatusDto {
  statusID: number;
  campaignID: number;
  storeID: number;
  status: 'Pending' | 'InProgress' | 'Completed';
  feedback?: string;
}
