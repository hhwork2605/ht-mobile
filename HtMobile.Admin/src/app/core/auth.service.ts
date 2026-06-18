import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap, finalize, shareReplay, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthUser } from './models';

interface AuthResponse {
  accessToken: string;
  accessExpiresAt: string;
  refreshToken: string;
  refreshExpiresAt: string;
  email: string;
  fullName: string;
  roles: string[];
}

const ACCESS_KEY = 'ht_admin_token';
const REFRESH_KEY = 'ht_admin_refresh';
const USER_KEY = 'ht_admin_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  /** User hiện tại (signal để UI phản ứng). */
  readonly user = signal<AuthUser | null>(this.readUser());

  /** Single-flight: nhiều request 401 cùng lúc chỉ gọi /refresh 1 lần. */
  private refresh$?: Observable<string>;

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiBase}/auth/login`, { email, password })
      .pipe(tap((r) => this.store(r)));
  }

  /** Cấp lại access token bằng refresh token; trả access token mới. Gộp các lần gọi đồng thời. */
  refresh(): Observable<string> {
    if (this.refresh$) return this.refresh$;
    const rt = localStorage.getItem(REFRESH_KEY);
    if (!rt) return throwError(() => new Error('no refresh token'));

    this.refresh$ = this.http.post<AuthResponse>(`${environment.apiBase}/auth/refresh`, { refreshToken: rt }).pipe(
      tap((r) => this.store(r)),
      map((r) => r.accessToken),
      finalize(() => (this.refresh$ = undefined)),
      shareReplay(1),
    );
    return this.refresh$;
  }

  /** Thu hồi đúng phiên (refresh token) ở server (best-effort) rồi xoá phiên cục bộ. Phiên khác giữ nguyên. */
  logout(): void {
    const rt = localStorage.getItem(REFRESH_KEY);
    if (rt) this.http.post(`${environment.apiBase}/auth/logout`, { refreshToken: rt }).subscribe({ error: () => {} });
    this.clear();
  }

  /** Xoá phiên cục bộ (không gọi server) — dùng khi refresh thất bại. */
  clear(): void {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(USER_KEY);
    this.user.set(null);
  }

  get token(): string | null { return localStorage.getItem(ACCESS_KEY); }
  get isLoggedIn(): boolean { return !!localStorage.getItem(REFRESH_KEY); }

  private store(r: AuthResponse): void {
    localStorage.setItem(ACCESS_KEY, r.accessToken);
    localStorage.setItem(REFRESH_KEY, r.refreshToken);
    const u: AuthUser = { email: r.email, fullName: r.fullName, roles: r.roles };
    localStorage.setItem(USER_KEY, JSON.stringify(u));
    this.user.set(u);
  }

  private readUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) as AuthUser : null;
  }
}
