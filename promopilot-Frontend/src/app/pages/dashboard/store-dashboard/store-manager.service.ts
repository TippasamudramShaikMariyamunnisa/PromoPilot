import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EngagementDto } from './engagement.model';
import { ExecutionStatusDto } from './execution-status.model';
import { PagedResultDto } from '../marketing-dashboard/paged-result.model';

@Injectable({ providedIn: 'root' })
export class StoreManagerService {
  private engagementUrl = 'http://localhost:5145/api/Engagement';
  private executionUrl = 'http://localhost:5145/api/ExecutionStatus';

  constructor(private http: HttpClient) {}

  // Engagements
  getPagedEngagements(page: number, size: number, sortBy?: string, sortDesc?: boolean): Observable<PagedResultDto<EngagementDto>> {
    const params = new HttpParams()
      .set('pageNumber', page)
      .set('pageSize', size)
      .set('sortBy', sortBy || '')
      .set('sortDesc', sortDesc?.toString() || 'false');

    return this.http.get<PagedResultDto<EngagementDto>>(`${this.engagementUrl}/ViewAllEngagements`, { params });
  }

  getEngagementById(id: number): Observable<EngagementDto> {
    return this.http.get<EngagementDto>(`${this.engagementUrl}/ViewEngagementById/${id}`);
  }

  trackEngagement(dto: EngagementDto): Observable<EngagementDto> {
    return this.http.post<EngagementDto>(`${this.engagementUrl}/TrackEngagement`, dto);
  }

  updateEngagement(id: number, dto: EngagementDto): Observable<EngagementDto> {
    return this.http.put<EngagementDto>(`${this.engagementUrl}/UpdateEngagement/${id}`, dto);
  }

  deleteEngagement(id: number): Observable<void> {
    return this.http.delete<void>(`${this.engagementUrl}/DeleteEngagement/${id}`);
  }

  exportEngagements(format: 'csv' | 'excel' = 'csv'): Observable<Blob> {
    return this.http.get(`${this.engagementUrl}/ExportEngagements?format=${format}`, {
      responseType: 'blob',
      headers: { Accept: format === 'csv' ? 'text/csv' : 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }
    });
  }

  // Execution Statuses
  getPagedExecutionStatuses(page: number, size: number, sortBy?: string, sortDesc?: boolean): Observable<PagedResultDto<ExecutionStatusDto>> {
    const params = new HttpParams()
      .set('pageNumber', page)
      .set('pageSize', size)
      .set('sortBy', sortBy || '')
      .set('sortDesc', sortDesc?.toString() || 'false');

    return this.http.get<PagedResultDto<ExecutionStatusDto>>(`${this.executionUrl}/ViewAllExecutionStatuses`, { params });
  }

  getExecutionStatusById(id: number): Observable<ExecutionStatusDto> {
    return this.http.get<ExecutionStatusDto>(`${this.executionUrl}/ViewExecutionStatusById/${id}`);
  }

  addExecutionStatus(dto: ExecutionStatusDto): Observable<ExecutionStatusDto> {
    return this.http.post<ExecutionStatusDto>(`${this.executionUrl}/AddExecutionStatus`, dto);
  }

  updateExecutionStatus(id: number, dto: ExecutionStatusDto): Observable<ExecutionStatusDto> {
    return this.http.put<ExecutionStatusDto>(`${this.executionUrl}/UpdateExecutionStatus/${id}`, dto);
  }

  deleteExecutionStatus(id: number): Observable<void> {
    return this.http.delete<void>(`${this.executionUrl}/DeleteExecutionStatus/${id}`);
  }

  uploadStoreReport(file: File): Observable<any> {
  const formData = new FormData();
  formData.append('file', file);

  return this.http.post('http://localhost:5145/api/blob/upload', formData);
}

}
