import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { SafeStorage } from './safe-storage.service';
import { ToastrService } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(
    private router: Router,
    private toastr: ToastrService
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const token = SafeStorage.get('accessToken');
    const expectedRole = route.data['role']?.toLowerCase();
    const userRole = SafeStorage.get('userRole')?.toLowerCase();

    if (!token) {
      this.toastr.warning('Please log in to access this page.', 'Access Denied');
      this.router.navigate(['/login']);
      return false;
    }

    if (expectedRole && userRole !== expectedRole) {
      this.toastr.error('You do not have permission to view this page.', 'Access Denied');
      this.router.navigate(['/dashboard']);
      return false;
    }

    return true;
  }
}
