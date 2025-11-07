import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { AuthInterceptor } from '../src/app/services/auth.interceptor';
import { HttpClientModule } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';

bootstrapApplication(App, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([AuthInterceptor])),
    provideAnimations(),
    HttpClientModule,
    provideToastr({
      positionClass: 'toast-top-right',
      timeOut: 3000,
      closeButton: true
    })
  ]
});
