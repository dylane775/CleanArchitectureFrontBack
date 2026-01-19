import { Component, Input, Output, EventEmitter, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CatalogItem as CatalogItemModel } from '../../../core/models/catalog.model';
import { WishlistService } from '../../../core/services/wishlist.service';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-catalog-item',
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatSnackBarModule
  ],
  templateUrl: './catalog-item.html',
  styleUrl: './catalog-item.scss',
})
export class CatalogItemComponent {
  @Input() item!: CatalogItemModel;
  @Input() compact = false; // Mode compact pour carrousels
  @Output() addToBasket = new EventEmitter<CatalogItemModel>();

  isInWishlist = computed(() => this.wishlistService.isInWishlist(this.item?.id));
  isAuthenticated = computed(() => this.authService.isAuthenticated());
  isWishlistLoading = signal(false);

  constructor(
    private router: Router,
    private wishlistService: WishlistService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  onAddToBasket(): void {
    this.addToBasket.emit(this.item);
  }

  navigateToDetail(): void {
    this.router.navigate(['/product', this.item.id]);
  }

  toggleWishlist(): void {
    if (!this.isAuthenticated()) {
      this.snackBar.open('Please login to add items to your wishlist', 'Login', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top'
      }).onAction().subscribe(() => {
        this.router.navigate(['/auth/login']);
      });
      return;
    }

    this.isWishlistLoading.set(true);

    const request = {
      catalogItemId: this.item.id,
      productName: this.item.name,
      price: this.item.price,
      pictureUrl: this.item.pictureUri,
      brandName: this.item.catalogBrandName,
      categoryName: this.item.catalogTypeName
    };

    this.wishlistService.toggleWishlist(this.item.id, request).subscribe({
      next: (response) => {
        this.isWishlistLoading.set(false);
        const message = response.added
          ? 'Added to wishlist'
          : 'Removed from wishlist';
        this.snackBar.open(message, '✓', {
          duration: 2000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: response.added ? ['success-snackbar'] : ['info-snackbar']
        });
      },
      error: (error) => {
        this.isWishlistLoading.set(false);
        console.error('Error toggling wishlist:', error);
        this.snackBar.open('Error updating wishlist', 'OK', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  getDiscountPercentage(): number {
    // Calcul déterministe basé sur l'ID du produit pour une valeur stable
    if (!this.item?.id) return 0;
    const hash = this.hashCode(this.item.id);
    return 10 + (Math.abs(hash) % 25); // Entre 10% et 34%
  }

  hasDiscount(): boolean {
    // Seulement certains produits ont une réduction (basé sur l'ID)
    if (!this.item?.id) return false;
    const hash = this.hashCode(this.item.id);
    return Math.abs(hash) % 3 === 0; // ~33% des produits ont une réduction
  }

  isNew(): boolean {
    // Vérifie si le produit a été créé dans les 30 derniers jours
    if (!this.item?.createdAt) return false;
    const createdDate = new Date(this.item.createdAt);
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
    return createdDate > thirtyDaysAgo;
  }

  getRating(): number {
    // Retourne la note moyenne depuis la base de données
    return this.item.averageRating || 0;
  }

  getReviewCount(): number {
    // Retourne le nombre d'avis depuis la base de données
    return this.item.reviewCount || 0;
  }

  hasPrime(): boolean {
    // Badge Prime déterministe basé sur l'ID du produit
    if (!this.item?.id) return false;
    const hash = this.hashCode(this.item.id);
    return Math.abs(hash) % 5 < 2; // ~40% des produits ont Prime
  }

  private hashCode(str: string): number {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32bit integer
    }
    return hash;
  }

  getImageUrl(): string {
    if (!this.item.pictureUri) {
      return 'https://via.placeholder.com/300x400?text=No+Image';
    }

    // Si l'URL commence déjà par http, la retourner telle quelle
    if (this.item.pictureUri.startsWith('http')) {
      return this.item.pictureUri;
    }

    // Sinon, construire l'URL complète avec le serveur Catalog
    const catalogBaseUrl = environment.catalogApiUrl.replace('/api', '');
    return `${catalogBaseUrl}${this.item.pictureUri}`;
  }
}
