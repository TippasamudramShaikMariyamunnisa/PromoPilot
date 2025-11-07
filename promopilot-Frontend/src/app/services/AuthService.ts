import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = 'http://localhost:5145/api/Auth';

  constructor(private http: HttpClient) {}

  login(data: { email: string; password: string }): Observable<any> {
    console.log('🔼 Sending login data:', data);
    return this.http.post(`${this.baseUrl}/Login`, data);
  }

  register(data: any): Observable<any> {
    console.log('📡 Register API called with:', data);
    return this.http.post(`${this.baseUrl}/Register`, data);
  }

  refreshToken(): Observable<any> {
    const refreshToken = localStorage.getItem('refreshToken');
    console.log('🔄 Refreshing token with:', refreshToken);
    return this.http.post(`${this.baseUrl}/Refresh`, { refreshToken });
  }

  logout(): void {
    console.log('🚪 Logging out and clearing tokens');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userRole');
    localStorage.removeItem('username');
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }
}
