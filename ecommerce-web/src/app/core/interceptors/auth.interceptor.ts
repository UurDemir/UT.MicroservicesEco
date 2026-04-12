import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

function isApiPath(url: string): boolean {
  if (url.startsWith('/api')) {
    return true;
  }
  if (url.startsWith('http://') || url.startsWith('https://')) {
    try {
      return new URL(url).pathname.startsWith('/api');
    } catch {
      return false;
    }
  }
  return false;
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getToken();
  if (token && isApiPath(req.url)) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next(req);
};
