import { Component } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/AuthService';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent {
  form: FormGroup;
  submitted = false;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {
    const namePattern = /^[a-zA-Z0-9]+$/;
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$/;

    this.form = this.fb.group({
      Username: ['', [Validators.required, Validators.pattern(namePattern), Validators.minLength(3)]],
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', [Validators.required, Validators.pattern(passwordPattern)]],
      Role: ['', [Validators.required]]
    });
  }

  get f() {
    return this.form.controls;
  }

  onSubmit() {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;

    const userData = {
      Username: this.form.value.Username,
      Email: this.form.value.Email,
      Password: this.form.value.Password,
      Role: this.form.value.Role
    };

    console.log('🔼 Sending registration data:', userData);

    this.auth.register(userData).subscribe({
      next: res => {
        console.log('✅ Registration successful:', res);
        this.toastr.success('Registration complete! A confirmation email has been sent to your email.', 'Success');

        setTimeout(() => {
          this.router.navigate(['/login']);
          this.form.reset();
          this.submitted = false;
          this.isLoading = false;
        }, 100);
      },
      error: err => {
        console.error('❌ Registration failed:', err);
        this.isLoading = false;

        if (err.status === 400 && err.error?.message) {
          this.toastr.error(err.error.message, 'Registration Failed');
        } else if (err.status === 500 && err.error?.details) {
          this.toastr.error(err.error.details, 'Server Error');
        } else {
          this.toastr.error('Server error. Please try again later.', 'Error');
        }
      }
    });
  }
}
