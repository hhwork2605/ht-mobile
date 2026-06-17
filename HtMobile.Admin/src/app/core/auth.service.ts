import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthUser } from './models';

interface LoginResponse {
  token: string;
  expiresAt: string;
  email: string;
  fullName: string;
  roles: string[];
}

const TOKEN_KEY = 'ht_admin_token';
const USER_KEY = 'ht_admin_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  /** User hiện tại (signal để UI phản ứng). */
  readonly user = signal<AuthUser | null>(this.readUser());

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiBase}/auth/login`, { email, password }).pipe(
      tap((r) => {
        localStorage.setItem(TOKEN_KEY, r.token);
        const u: AuthUser = { email: r.email, fullName: r.fullName, roles: r.roles };
        localStorage.setItem(USER_KEY, JSON.stringify(u));
        this.user.set(u);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.user.set(null);
  }

  get token(): string | null { return localStorage.getItem(TOKEN_KEY); }
  get isLoggedIn(): boolean { return !!this.token; }

  private readUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) as AuthUser : null;
  }
}
