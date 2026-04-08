import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    void router.navigate(['/backoffice/login'], { queryParams: { returnUrl: router.url } });
    return false;
  }
  if (!auth.hasRole('Admin')) {
    void router.navigate(['/']);
    return false;
  }
  return true;
};
