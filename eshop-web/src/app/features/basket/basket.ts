import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BasketService } from '../../core/services/basket.service';
import { AuthService } from '../../core/services/auth.service';
import { BasketItem } from '../../core/models/basket.model';

@Component({
  selector: 'app-basket',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatInputModule,
    MatFormFieldModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './basket.html',
  styleUrl: './basket.scss',
})
export class Basket implements OnInit {
  basketItems = signal<BasketItem[]>([]);
  loading = signal(false);

  // Computed values
  subtotal = computed(() => {
    return this.basketItems().reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0);
  });

  tax = computed(() => {
    return this.subtotal() * 0.1; // 10% tax
  });

  shipping = computed(() => {
    return this.subtotal() > 100 ? 0 : 10; // Free shipping over $100
  });

  total = computed(() => {
    return this.subtotal() + this.tax() + this.shipping();
  });

  constructor(
    private basketService: BasketService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadBasket();
  }

  loadBasket(): void {
    const user = this.authService.currentUser();

    if (!user) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.loading.set(true);
    this.basketService.getBasket(user.id).subscribe({
      next: (basket) => {
        this.basketItems.set(basket.items || []);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Error loading basket:', err);
        this.loading.set(false);
        this.snackBar.open('Failed to load basket. Please try again.', 'Close', {
          duration: 4000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  updateQuantity(item: BasketItem, newQuantity: number): void {
    if (newQuantity < 1) {
      this.removeItem(item);
      return;
    }

    const user = this.authService.currentUser();
    if (!user) return;

    this.basketService.updateBasketItem(user.id, {
      catalogItemId: item.catalogItemId,
      newQuantity: newQuantity
    }).subscribe({
      next: () => {
        this.loadBasket();
        this.snackBar.open('Quantity updated successfully', '✓', {
          duration: 2000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
      },
      error: (err: any) => {
        console.error('Error updating quantity:', err);
        this.snackBar.open('Failed to update quantity. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  removeItem(item: BasketItem): void {
    const user = this.authService.currentUser();
    if (!user) return;

    this.basketService.removeItemFromBasket(user.id, item.catalogItemId).subscribe({
      next: () => {
        this.loadBasket();
        this.snackBar.open(`${item.productName} removed from basket`, '✓', {
          duration: 2500,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
      },
      error: (err: any) => {
        console.error('Error removing item:', err);
        this.snackBar.open('Failed to remove item. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  clearBasket(): void {
    const user = this.authService.currentUser();
    if (!user) return;

    this.basketService.clearBasket(user.id).subscribe({
      next: () => {
        this.basketItems.set([]);
        this.snackBar.open('Basket cleared successfully', '✓', {
          duration: 2500,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
      },
      error: (err: any) => {
        console.error('Error clearing basket:', err);
        this.snackBar.open('Failed to clear basket. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  proceedToCheckout(): void {
    if (this.basketItems().length === 0) {
      this.snackBar.open('Your basket is empty', 'Close', {
        duration: 2500,
        horizontalPosition: 'end',
        verticalPosition: 'top',
        panelClass: ['warning-snackbar']
      });
      return;
    }

    this.snackBar.open('Checkout feature coming soon!', 'OK', {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['info-snackbar']
    });
  }

  continueShopping(): void {
    this.router.navigate(['/catalog']);
  }
}
