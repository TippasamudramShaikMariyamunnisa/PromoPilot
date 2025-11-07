import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BudgetDto } from './budget.model';

@Injectable({ providedIn: 'root' })
export class FinanceDashboardService {
  private readonly BASE_URL = 'http://localhost:5145/api/Budget';

  constructor(private http: HttpClient) {}

  allocateBudget(data: any): Observable<any> {
    return this.http.post(`${this.BASE_URL}/AllocateBudget`, data);
  }

  getPagedBudgets(page: number, pageSize: number, sortBy: string, sortDesc: boolean): Observable<any> {
    const params = new HttpParams()
      .set('pageNumber', page.toString())
      .set('pageSize', pageSize.toString())
      .set('sortBy', sortBy)
      .set('sortDesc', sortDesc.toString());

    return this.http.get<any>(`${this.BASE_URL}/ViewAllBudgets`, { params });
  }

  updateBudget(id: number, data: any): Observable<any> {
  return this.http.put(`${this.BASE_URL}/UpdateBudget/${id}`, data);
}

deleteBudget(id: number): Observable<void> {
  return this.http.delete<void>(`${this.BASE_URL}/DeleteBudget/${id}`);
}

  exportBudgets(format: 'csv' | 'excel'): Observable<Blob> {
    const endpoint = format === 'csv' ? 'export/csv' : 'export/excel';
    return this.http.get(`${this.BASE_URL}/${endpoint}`, {
      responseType: 'blob',
      headers: { Accept: 'application/octet-stream' }
    });
  }

  exportBudgetsAsPdf(): Observable<Blob> {
    return this.http.get(`${this.BASE_URL}/export/pdf`, {
      responseType: 'blob',
      headers: { Accept: 'application/pdf' }
    });
  }

  uploadFinanceReport(file: File): Observable<any> {
  const formData = new FormData();
  formData.append('file', file);

  return this.http.post('http://localhost:5145/api/blob/upload', formData);
}

}
