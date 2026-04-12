import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

/** When the SPA is on another origin than the gateway, prefix relative `/api/*` requests. */
export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const base = environment.apiBaseUrl.replace(/\/$/, '');
  if (!base || !req.url.startsWith('/api')) {
    return next(req);
  }
  return next(req.clone({ url: `${base}${req.url}` }));
};
