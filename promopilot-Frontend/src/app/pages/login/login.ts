import { Component } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/AuthService';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  form: FormGroup;
  submitted = false;
  isLoading = false;
  lockout = false;
  lockoutDuration = 15 * 60 * 1000; // 15 minutes

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$/;

    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.pattern(passwordPattern)]]
    });
  }

  get f() {
    return this.form.controls;
  }

  onSubmit() {
    this.submitted = true;

    if (this.form.invalid || this.lockout) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const loginData = this.form.value;

    this.auth.login(loginData).subscribe({
      next: (res: any) => {
        console.log('✅ Login successful. Response:', res);

        localStorage.setItem('accessToken', res.accessToken);
        localStorage.setItem('refreshToken', res.refreshToken);
        localStorage.setItem('userRole', res.role || res.userType);
        localStorage.setItem('username', res.username || res.name);

        this.toastr.success('Login successful!', 'Welcome');

        const role = res.role?.toLowerCase() || res.userType?.toString();
        switch (role) {
          case 'marketing':
          case '1':
            this.router.navigate(['/dashboard/marketing']);
            break;
          case 'finance':
          case '2':
            this.router.navigate(['/dashboard/finance']);
            break;
          case 'storemanager':
          case '3':
            this.router.navigate(['/dashboard/storemanager']);
            break;
          default:
            this.toastr.info('Unknown role. Redirecting to homepage.', 'Info');
            this.router.navigate(['/']);
        }

        this.form.reset();
        this.submitted = false;
        this.isLoading = false;
      },
      error: (err: any) => {
  console.error('❌ Login failed. Error:', err);
  this.isLoading = false;

  const rawMessage = err.error?.message || err.message || '';
  const message = rawMessage.toLowerCase();

  if (err.status === 0) {
    this.toastr.error('Server not reachable. Is your backend running?', 'Connection Error');
  } else if (err.status === 401) {
    if (message.includes('locked')) {
      this.toastr.warning(
        'Your account is locked due to multiple failed attempts. You can try again after 15 minutes.',
        'Access Denied'
      );
      this.lockout = true;
      setTimeout(() => this.lockout = false, this.lockoutDuration);
    } else if (message.includes('invalid')) {
      this.toastr.error('Invalid credentials. Please try again.', 'Login Failed');
    } else {
      this.toastr.warning(rawMessage || 'Access denied.', 'Login Error');
    }
  } else {
    this.toastr.error(rawMessage || 'Unexpected error. Please try again later.', 'Error');
  }
}

    });
  }
}
