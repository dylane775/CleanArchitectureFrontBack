import { Component, OnInit, OnDestroy, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { WishlistService } from '../../core/services/wishlist.service';
import { BasketService } from '../../core/services/basket.service';
import { WishlistItem } from '../../core/models/wishlist.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: 'wishlist.html',
  styleUrl: 'wishlist.scss',
})
export class Wishlist implements OnInit, OnDestroy {
  private wishlistService = inject(WishlistService);
  private basketService = inject(BasketService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private destroy$ = new Subject<void>();

  // Expose signals from service
  wishlistItems = this.wishlistService.wishlistItems;
  loading = this.wishlistService.loading;
  wishlistCount = this.wishlistService.wishlistCount;

  // Computed: total value of wishlist
  totalValue = computed(() => {
    return this.wishlistItems().reduce((sum, item) => sum + item.price, 0);
  });

  ngOnInit(): void {
    this.wishlistService.loadWishlist()
      .pipe(takeUntil(this.destroy$))
      .subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  removeFromWishlist(item: WishlistItem): void {
    this.wishlistService.removeFromWishlist(item.catalogItemId).subscribe({
      next: () => {
        this.snackBar.open(`${item.productName} removed from wishlist`, 'OK', {
          duration: 2500,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
      },
      error: (err: any) => {
        console.error('Error removing item from wishlist:', err);
        this.snackBar.open('Failed to remove item. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  moveToCart(item: WishlistItem): void {
    // Add to basket
    this.basketService.addItemToBasket({
      catalogItemId: item.catalogItemId,
      productName: item.productName,
      unitPrice: item.price,
      pictureUrl: item.pictureUrl,
      quantity: 1
    }).subscribe({
      next: () => {
        // Remove from wishlist after adding to cart
        this.wishlistService.removeFromWishlist(item.catalogItemId).subscribe({
          next: () => {
            this.snackBar.open(`${item.productName} moved to cart`, 'View Cart', {
              duration: 3000,
              horizontalPosition: 'end',
              verticalPosition: 'top',
              panelClass: ['success-snackbar']
            }).onAction().subscribe(() => {
              this.router.navigate(['/basket']);
            });
          }
        });
      },
      error: (err: any) => {
        console.error('Error adding to cart:', err);
        this.snackBar.open('Failed to add to cart. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  addAllToCart(): void {
    const items = this.wishlistItems();
    if (items.length === 0) return;

    let addedCount = 0;
    let errorCount = 0;

    items.forEach(item => {
      this.basketService.addItemToBasket({
        catalogItemId: item.catalogItemId,
        productName: item.productName,
        unitPrice: item.price,
        pictureUrl: item.pictureUrl,
        quantity: 1
      }).subscribe({
        next: () => {
          addedCount++;
          if (addedCount + errorCount === items.length) {
            this.showAddAllResult(addedCount, errorCount);
          }
        },
        error: () => {
          errorCount++;
          if (addedCount + errorCount === items.length) {
            this.showAddAllResult(addedCount, errorCount);
          }
        }
      });
    });
  }

  private showAddAllResult(success: number, errors: number): void {
    if (errors === 0) {
      this.snackBar.open(`${success} items added to cart`, 'View Cart', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
        panelClass: ['success-snackbar']
      }).onAction().subscribe(() => {
        this.router.navigate(['/basket']);
      });
    } else {
      this.snackBar.open(`${success} items added, ${errors} failed`, 'Close', {
        duration: 4000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
        panelClass: ['warning-snackbar']
      });
    }
  }

  clearWishlist(): void {
    this.wishlistService.clearWishlist().subscribe({
      next: () => {
        this.snackBar.open('Wishlist cleared', 'OK', {
          duration: 2500,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
      },
      error: (err: any) => {
        console.error('Error clearing wishlist:', err);
        this.snackBar.open('Failed to clear wishlist. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  viewProduct(item: WishlistItem): void {
    this.router.navigate(['/product', item.catalogItemId]);
  }

  continueShopping(): void {
    this.router.navigate(['/catalog']);
  }

  getImageUrl(pictureUrl: string): string {
    if (!pictureUrl) {
      return 'https://via.placeholder.com/150';
    }

    if (pictureUrl.startsWith('http')) {
      return pictureUrl;
    }

    const catalogBaseUrl = environment.catalogApiUrl.replace('/api', '');
    return `${catalogBaseUrl}${pictureUrl}`;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }
}
