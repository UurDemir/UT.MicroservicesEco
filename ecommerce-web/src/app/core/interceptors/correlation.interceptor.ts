import { HttpInterceptorFn } from '@angular/common/http';

function newCorrelationId(): string {
  const bytes = new Uint8Array(16);
  crypto.getRandomValues(bytes);
  return Array.from(bytes, (b) => b.toString(16).padStart(2, '0')).join('');
}

export const correlationInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('/api')) {
    req = req.clone({
      setHeaders: { 'X-Correlation-ID': newCorrelationId() }
    });
  }
  return next(req);
};
