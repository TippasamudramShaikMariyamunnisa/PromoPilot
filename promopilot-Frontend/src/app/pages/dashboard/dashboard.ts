import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/AuthService';
import { MarketingDashboardComponent } from '../../pages/dashboard/marketing-dashboard/marketing-dashboard';
import { FinanceDashboardComponent } from '../../pages/dashboard/finance-dashboard/finance-dashboard';
import { StoreDashboardComponent } from './store-dashboard/store-dashboard';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
  imports: [
    CommonModule,
    RouterModule,
    MarketingDashboardComponent,
    FinanceDashboardComponent,
    StoreDashboardComponent
  ]
})
export class DashboardComponent {

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  onRefreshToken() {
    this.auth.refreshToken().subscribe({
      next: (res) => {
        localStorage.setItem('accessToken', res.accessToken);
        localStorage.setItem('refreshToken', res.refreshToken);
        console.log('🔁 Token refreshed successfully');
        this.toastr.info('Token refreshed successfully.', 'Auth Updated');
      },
      error: (err) => {
        console.error('❌ Refresh failed:', err);
        this.toastr.error('Session expired. Please log in again.', 'Auth Error');
        this.auth.logout();
        this.router.navigate(['/login']);
      }
    });
  }

  onLogout() {
    this.auth.logout();
    this.router.navigate(['/login']);
    this.toastr.info('Logged out successfully.', 'Session Ended');
  }
}
