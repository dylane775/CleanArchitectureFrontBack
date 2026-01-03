import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.currentUser();

  if (!user) {
    router.navigate(['/auth/login']);
    return false;
  }

  const isAdmin = user.roles?.includes('Admin') ?? false;

  if (!isAdmin) {
    router.navigate(['/']);
    return false;
  }

  return true;
};
