import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CampaignDto } from './campaign.model';
import { PagedResultDto } from './paged-result.model'; 

@Injectable({ providedIn: 'root' })
export class MarketingDashboardService {
  private baseUrl = 'http://localhost:5145/api/Campaign';

  constructor(private http: HttpClient) {}

  getCampaigns(page: number, size: number, sortBy: string, sortDesc: boolean): Observable<PagedResultDto<CampaignDto>> {
  const params = new HttpParams()
    .set('pageNumber', page.toString())
    .set('pageSize', size.toString())
    .set('sortBy', sortBy)
    .set('sortDesc', sortDesc.toString());

  return this.http.get<PagedResultDto<CampaignDto>>(`${this.baseUrl}/GetAllCampaigns`, { params });
}
  planCampaign(dto: CampaignDto): Observable<CampaignDto> {
    return this.http.post<CampaignDto>(`${this.baseUrl}/PlanCampaign`, dto);
  }

  updateCampaign(id: number, dto: CampaignDto): Observable<CampaignDto> {
  return this.http.put<CampaignDto>(`${this.baseUrl}/UpdateCampaign/${id}`, dto);
}

getCampaignById(id: number): Observable<CampaignDto> {
  return this.http.get<CampaignDto>(`${this.baseUrl}/GetCampaignById/${id}`);
}

scheduleCampaign(id: number, storeList: string): Observable<CampaignDto> {
  return this.http.request<CampaignDto>('PUT', `${this.baseUrl}/ScheduleCampaign/${id}`, {
    body: storeList,
    headers: {
      'Content-Type': 'text/plain' // ✅ Must be plain text
    },
    responseType: 'json'
  });
}

  cancelCampaign(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/CancelCampaign/${id}`);
  }


exportCampaignsAsPdf(): Observable<Blob> {
  return this.http.get(`${this.baseUrl}/export/pdf`, {
    responseType: 'blob',
    headers: { Accept: 'application/pdf' }
  });
}

uploadMarketingReport(file: File): Observable<any> {
  const formData = new FormData();
  formData.append('file', file);
  return this.http.post('http://localhost:5145/api/blob/upload', formData);
}


}
