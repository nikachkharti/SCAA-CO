import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './auth-form.css'
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  confirmPassword = '';

  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  submit(): void {
    if (!this.email.trim() || !this.password) {
      return;
    }
    if (this.password !== this.confirmPassword) {
      this.error.set('Passwords do not match.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.success.set(null);

    this.authService
      .register({ email: this.email.trim(), password: this.password })
      .subscribe({
        next: () => {
          this.success.set('Account created. Redirecting to sign in...');
          setTimeout(() => this.router.navigate(['/login']), 900);
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(this.extractError(err));
          this.submitting.set(false);
        }
      });
  }

  private extractError(err: HttpErrorResponse): string {
    return err.error?.message || err.message || 'Registration failed.';
  }
}
