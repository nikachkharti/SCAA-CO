import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly currentUser = this.authService.currentUser;

  readonly links = [
    { path: '/categories', label: 'Categories' },
    { path: '/products', label: 'Products' },
    { path: '/suppliers', label: 'Suppliers' },
    { path: '/orders', label: 'Orders' }
  ];

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
