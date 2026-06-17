import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

/** Gắn Bearer token cho request tới API; 401/403 → đăng xuất + về /login. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  let r = req;
  if (req.url.startsWith(environment.apiBase) && auth.token) {
    r = req.clone({ setHeaders: { Authorization: `Bearer ${auth.token}` } });
  }

  return next(r).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        auth.logout();
        router.navigate(['/login']);
      }
      return throwError(() => err);
    })
  );
};
