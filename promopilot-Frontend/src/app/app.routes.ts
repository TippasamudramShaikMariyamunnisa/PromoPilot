import { Routes, RouterModule, Router } from '@angular/router';
import { LoginComponent } from './pages/login/login';
import { RegisterComponent } from './pages/register/register';
import { HomeComponent } from './pages/home/home';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { AuthGuard } from './services/auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'dashboard/marketing',
    loadComponent: () =>
      import('./pages/dashboard/marketing-dashboard/marketing-dashboard').then(m => m.MarketingDashboardComponent),
    canActivate: [AuthGuard],
    data: { role: 'marketing' }
  },
  {
    path: 'dashboard/finance',
    loadComponent: () =>
      import('./pages/dashboard/finance-dashboard/finance-dashboard').then(m => m.FinanceDashboardComponent),
    canActivate: [AuthGuard],
    data: { role: 'finance' }
  },
  {
    path: 'dashboard/storemanager',
    loadComponent: () =>
      import('./pages/dashboard/store-dashboard/store-dashboard').then(m => m.StoreDashboardComponent),
    canActivate: [AuthGuard],
    data: { role: 'storemanager' }
  }
];
