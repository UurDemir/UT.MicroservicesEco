import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

const TOKEN_KEY = 'ecommerce_access_token';

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenSignal = signal<string | null>(this.readStoredToken());

  readonly isAuthenticated = computed(() => {
    const t = this.tokenSignal();
    return !!t && !this.isTokenExpired(t);
  });

  readonly roles = computed(() => {
    const t = this.tokenSignal();
    return t ? this.parseRoles(t) : [];
  });

  readonly isAdmin = computed(() => this.roles().includes('Admin'));

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  private api(path: string): string {
    const base = environment.apiBaseUrl.replace(/\/$/, '');
    return `${base}${path}`;
  }

  login(body: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(this.api('/api/auth/login'), body).pipe(
      tap((res) => {
        localStorage.setItem(TOKEN_KEY, res.accessToken);
        this.tokenSignal.set(res.accessToken);
      })
    );
  }

  register(body: RegisterRequest): Observable<unknown> {
    return this.http.post(this.api('/api/auth/register'), body);
  }

  /** Clears session and navigates (default: storefront sign-in). */
  logout(redirectUrl = '/login'): void {
    localStorage.removeItem(TOKEN_KEY);
    this.tokenSignal.set(null);
    void this.router.navigateByUrl(redirectUrl);
  }

  /** User name for `/api/baskets/{userName}` (URL-encoded by callers). */
  getBasketUserName(): string | null {
    const t = this.getToken();
    if (!t) {
      return null;
    }
    try {
      const payload = this.decodePayload(t);
      const v = payload['unique_name'] ?? payload['name'];
      if (typeof v !== 'string' || !v.trim()) {
        return null;
      }
      return v.trim();
    } catch {
      return null;
    }
  }

  getToken(): string | null {
    const t = this.tokenSignal();
    if (!t || this.isTokenExpired(t)) {
      return null;
    }
    return t;
  }

  private readStoredToken(): string | null {
    try {
      return localStorage.getItem(TOKEN_KEY);
    } catch {
      return null;
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = this.decodePayload(token);
      const exp = payload['exp'];
      if (typeof exp !== 'number') {
        return false;
      }
      return Date.now() / 1000 >= exp - 30;
    } catch {
      return true;
    }
  }

  private decodePayload(token: string): Record<string, unknown> {
    const part = token.split('.')[1];
    if (!part) {
      throw new Error('Invalid token');
    }
    const json = atob(part.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(json) as Record<string, unknown>;
  }

  private parseRoles(token: string): string[] {
    try {
      const payload = this.decodePayload(token);
      const roleUri = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
      const raw = payload['role'] ?? payload[roleUri];
      if (Array.isArray(raw)) {
        return raw.filter((x): x is string => typeof x === 'string');
      }
      if (typeof raw === 'string') {
        return [raw];
      }
      return [];
    } catch {
      return [];
    }
  }

  hasRole(role: string): boolean {
    return this.roles().includes(role);
  }

  /** Display name from JWT (unique_name / name / sub). */
  getUserDisplayName(): string {
    const t = this.getToken();
    if (!t) {
      return '—';
    }
    try {
      const payload = this.decodePayload(t);
      const v = payload['unique_name'] ?? payload['name'] ?? payload['sub'];
      return typeof v === 'string' ? v : 'unknown';
    } catch {
      return '—';
    }
  }
}
