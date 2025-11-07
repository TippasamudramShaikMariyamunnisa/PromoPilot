import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/AuthService';
import { catchError, switchMap } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = typeof window !== 'undefined' ? localStorage.getItem('accessToken') : null;
  const refreshToken = typeof window !== 'undefined' ? localStorage.getItem('refreshToken') : null;

  // Skip token for registration and refresh endpoints
  if (token && !req.url.includes('/api/Auth/Register') && !req.url.includes('/api/Auth/Refresh')) {
    console.log('🔐 Attaching token:', token);
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError(err => {
      if (err.status === 401 && refreshToken && !req.url.includes('/api/Auth/Refresh')) {
        console.warn('🔁 Token expired, attempting refresh...');
        return authService.refreshToken().pipe(
          switchMap((response: any) => {
            const newAccessToken = response.accessToken;
            const newRefreshToken = response.refreshToken;

            localStorage.setItem('accessToken', newAccessToken);
            if (newRefreshToken) {
              localStorage.setItem('refreshToken', newRefreshToken);
            }

            const retryReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${newAccessToken}`
              }
            });

            return next(retryReq);
          }),
          catchError(refreshErr => {
            console.error('❌ Refresh failed:', refreshErr);
            authService.logout();
            return throwError(() => refreshErr);
          })
        );
      }

      return throwError(() => err);
    })
  );
};
