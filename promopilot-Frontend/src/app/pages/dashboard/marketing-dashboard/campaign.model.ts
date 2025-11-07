export interface CampaignDto {
  status: any;
  campaignId: number;
  name: string;
  startDate: string; // ISO format
  endDate: string;
  targetProducts: string;
  storeList: string;
}
