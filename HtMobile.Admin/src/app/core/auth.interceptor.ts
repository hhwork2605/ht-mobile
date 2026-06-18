import { HttpInterceptorFn, HttpErrorResponse, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

const withBearer = (req: HttpRequest<unknown>, token: string | null) =>
  token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

/**
 * Gắn Bearer access token cho request tới API. Khi access token hết hạn (401), tự gọi /auth/refresh
 * một lần (gộp các request đồng thời ở AuthService) rồi thử lại request gốc. Refresh thất bại → về /login.
 * Bỏ qua refresh cho chính các endpoint /auth/* (tránh vòng lặp).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const isApi = req.url.startsWith(environment.apiBase);
  const isAuthEndpoint = req.url.includes('/auth/');
  const sent = isApi ? withBearer(req, auth.token) : req;

  return next(sent).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && isApi && !isAuthEndpoint) {
        return auth.refresh().pipe(
          switchMap((newToken) => next(withBearer(req, newToken))),
          catchError((refreshErr) => { redirectToLogin(auth, router); return throwError(() => refreshErr); }),
        );
      }
      if ((err.status === 401 || err.status === 403) && isApi) {
        redirectToLogin(auth, router);
      }
      return throwError(() => err);
    }),
  );
};

function redirectToLogin(auth: AuthService, router: Router): void {
  auth.clear();
  router.navigate(['/login']);
}
