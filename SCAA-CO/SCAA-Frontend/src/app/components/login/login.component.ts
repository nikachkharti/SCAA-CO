import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './auth-form.css'
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  userName = '';
  password = '';

  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  submit(): void {
    if (!this.userName.trim() || !this.password) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    this.authService.login({ userName: this.userName.trim(), password: this.password }).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/categories';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(this.extractError(err));
        this.submitting.set(false);
      }
    });
  }

  private extractError(err: HttpErrorResponse): string {
    return err.error?.message || err.message || 'Login failed.';
  }
}
