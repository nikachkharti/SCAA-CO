import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  AuthUser,
  LoginRequestDto,
  LoginResponseDto,
  RegistrationRequestDto
} from '../models/auth.model';
import { CommonResponse } from '../models/common-response.model';

const TOKEN_STORAGE_KEY = 'scaa.auth.token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/auth`;

  private readonly tokenSignal = signal<string | null>(this.readToken());

  readonly currentUser = computed<AuthUser | null>(() => {
    const token = this.tokenSignal();
    return token ? this.decodeToken(token) : null;
  });

  readonly isAuthenticated = computed(() => {
    const user = this.currentUser();
    if (!user) return false;
    if (user.expiresAt && user.expiresAt * 1000 < Date.now()) return false;
    return true;
  });

  readonly isAdmin = computed(
    () => this.currentUser()?.roles.includes('admin') ?? false
  );

  getToken(): string | null {
    return this.tokenSignal();
  }

  login(model: LoginRequestDto): Observable<LoginResponseDto> {
    return this.http
      .post<CommonResponse<LoginResponseDto>>(`${this.endpoint}/login`, model)
      .pipe(
        map(response => response.result),
        tap(result => this.persistToken(result.accessToken))
      );
  }

  register(model: RegistrationRequestDto): Observable<string> {
    return this.http
      .post<CommonResponse<string>>(`${this.endpoint}/registeradmin`, model)
      .pipe(map(response => response.result));
  }

  logout(): void {
    try {
      localStorage.removeItem(TOKEN_STORAGE_KEY);
    } catch {
      /* ignore storage errors */
    }
    this.tokenSignal.set(null);
  }

  private persistToken(token: string): void {
    try {
      localStorage.setItem(TOKEN_STORAGE_KEY, token);
    } catch {
      /* ignore storage errors */
    }
    this.tokenSignal.set(token);
  }

  private readToken(): string | null {
    try {
      return localStorage.getItem(TOKEN_STORAGE_KEY);
    } catch {
      return null;
    }
  }

  private decodeToken(token: string): AuthUser | null {
    try {
      const payload = token.split('.')[1];
      if (!payload) return null;
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      const claims = JSON.parse(json) as Record<string, unknown>;

      const rawRole =
        claims['role'] ??
        claims['roles'] ??
        claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      const roles = Array.isArray(rawRole)
        ? rawRole.map(String)
        : rawRole != null
          ? [String(rawRole)]
          : [];

      const email =
        (claims['email'] as string | undefined) ??
        (claims['unique_name'] as string | undefined) ??
        (claims[
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
        ] as string | undefined) ??
        null;

      const userId =
        (claims['sub'] as string | undefined) ??
        (claims['nameid'] as string | undefined) ??
        (claims[
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
        ] as string | undefined) ??
        null;

      return {
        email,
        userId,
        roles,
        expiresAt: typeof claims['exp'] === 'number' ? claims['exp'] : null
      };
    } catch {
      return null;
    }
  }
}
