import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class EngagementService {
  private baseUrl = 'http://localhost:5145/api/Engagement';

  constructor(private http: HttpClient) {}
  exportEngagements(format: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/ExportEngagements?format=${format}`, {
      responseType: 'blob'
    });
  }
}
