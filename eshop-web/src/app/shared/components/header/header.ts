import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';
import { BasketService } from '../../../core/services/basket.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatMenuModule,
    MatDividerModule,
    MatInputModule,
    MatFormFieldModule,
    MatSnackBarModule
  ],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class HeaderComponent {
  isAuthenticated = computed(() => this.authService.isAuthenticated());
  currentUser = computed(() => this.authService.currentUser());
  basketItemCount = computed(() => this.basketService.itemCount());
  searchQuery = signal('');

  constructor(
    private authService: AuthService,
    private basketService: BasketService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  getUserDisplayName(): string {
    const user = this.currentUser();
    if (!user) return '';

    if (user.firstName && user.lastName) {
      return `${user.firstName} ${user.lastName}`;
    }
    if (user.firstName) {
      return user.firstName;
    }
    return user.email;
  }

  isAdmin(): boolean {
    const user = this.currentUser();
    return user?.roles?.includes('Admin') ?? false;
  }

  navigateToProfile(): void {
    this.router.navigate(['/profile']);
  }

  navigateToOrders(): void {
    this.router.navigate(['/orders']);
  }

  logout(): void {
    this.authService.logout();
    this.snackBar.open('You have been logged out successfully', '✓', {
      duration: 2500,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
    this.router.navigate(['/auth/login']);
  }

  onSearch(): void {
    if (this.searchQuery().trim()) {
      this.router.navigate(['/catalog'], {
        queryParams: { search: this.searchQuery() }
      });
    }
  }
}
