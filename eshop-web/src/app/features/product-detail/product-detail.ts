import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CatalogService } from '../../core/services/catalog.service';
import { BasketService } from '../../core/services/basket.service';
import { AuthService } from '../../core/services/auth.service';
import { CatalogItem } from '../../core/models/catalog.model';
import { environment } from '../../../environments/environment';
import { BreadcrumbComponent, BreadcrumbItem } from '../../shared/components/breadcrumb/breadcrumb';
import { ProductReviewsComponent } from '../../shared/components/product-reviews/product-reviews';
import { CatalogItemComponent } from '../catalog/catalog-item/catalog-item';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    BreadcrumbComponent,
    ProductReviewsComponent,
    CatalogItemComponent
  ],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
})
export class ProductDetail implements OnInit {
  product = signal<CatalogItem | null>(null);
  relatedProducts = signal<CatalogItem[]>([]);
  loading = signal(false);
  quantity = signal(1);

  breadcrumbItems = computed<BreadcrumbItem[]>(() => {
    const prod = this.product();
    if (!prod) return [];
    return [
      { label: 'Accueil', url: '/', icon: 'home' },
      { label: 'Catalogue', url: '/catalog' },
      { label: prod.catalogTypeName, url: `/catalog?type=${prod.catalogTypeName}` },
      { label: prod.catalogBrandName, url: `/catalog?brand=${prod.catalogBrandName}` },
      { label: prod.name }
    ];
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private catalogService: CatalogService,
    private basketService: BasketService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const productId = params['id'];
      if (productId) {
        this.loadProduct(productId);
      }
    });
  }

  loadProduct(productId: string): void {
    this.loading.set(true);
    this.catalogService.getCatalogItemById(productId).subscribe({
      next: (product) => {
        this.product.set(product);
        this.loading.set(false);
        // Charger les produits similaires
        this.loadRelatedProducts(productId);
      },
      error: (err: any) => {
        console.error('Error loading product:', err);
        this.loading.set(false);
        this.snackBar.open('Product not found', 'Close', { duration: 3000 });
        this.router.navigate(['/catalog']);
      }
    });
  }

  loadRelatedProducts(productId: string): void {
    this.catalogService.getRelatedProducts(productId, 4).subscribe({
      next: (products) => {
        this.relatedProducts.set(products);
      },
      error: (err: any) => {
        console.error('Error loading related products:', err);
      }
    });
  }

  onAddRelatedToBasket(item: CatalogItem): void {
    this.basketService.addItemToBasket({
      catalogItemId: item.id,
      productName: item.name,
      unitPrice: item.price,
      pictureUrl: item.pictureUri,
      quantity: 1
    }).subscribe({
      next: () => {
        this.snackBar.open(`${item.name} added to basket!`, 'View', {
          duration: 2500,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        }).onAction().subscribe(() => {
          this.router.navigate(['/basket']);
        });
      }
    });
  }

  increaseQuantity(): void {
    const currentQty = this.quantity();
    const product = this.product();
    if (product && currentQty < product.availableStock) {
      this.quantity.set(currentQty + 1);
    }
  }

  decreaseQuantity(): void {
    const currentQty = this.quantity();
    if (currentQty > 1) {
      this.quantity.set(currentQty - 1);
    }
  }

  addToBasket(): void {
  const product = this.product();
  if (!product) return;

  this.basketService.addItemToBasket({
    catalogItemId: product.id,
    productName: product.name,
    unitPrice: product.price,
    quantity: this.quantity(),
    pictureUrl: product.pictureUri || ''
  }).subscribe({
    next: () => {
      this.snackBar.open(
        `${product.name} (x${this.quantity()}) added to basket!`,
        'View Basket',
        {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        }
      ).onAction().subscribe(() => {
        this.router.navigate(['/basket']);
      });
    },
    error: (err: any) => {
      console.error('Error adding to basket:', err);
      this.snackBar.open(
        'Failed to add item to basket. Please try again.',
        'Close',
        {
          duration: 4000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        }
      );
    }
  });
}


  backToCatalog(): void {
    this.router.navigate(['/catalog']);
  }

  isInStock(): boolean {
    const product = this.product();
    return product ? product.availableStock > 0 : false;
  }

  getStockStatus(): string {
    const product = this.product();
    if (!product) return 'Unknown';

    if (product.availableStock === 0) return 'Out of Stock';
    if (product.availableStock < 10) return `Only ${product.availableStock} left`;
    return 'In Stock';
  }

  getStockStatusClass(): string {
    const product = this.product();
    if (!product) return '';

    if (product.availableStock === 0) return 'out-of-stock';
    if (product.availableStock < 10) return 'low-stock';
    return 'in-stock';
  }

  getImageUrl(pictureUri: string): string {
    if (!pictureUri) {
      return 'https://via.placeholder.com/600x800?text=No+Image';
    }

    // Si l'URL commence déjà par http, la retourner telle quelle
    if (pictureUri.startsWith('http')) {
      return pictureUri;
    }

    // Sinon, construire l'URL complète avec le serveur Catalog
    const catalogBaseUrl = environment.catalogApiUrl.replace('/api', '');
    return `${catalogBaseUrl}${pictureUri}`;
  }

  hasSpecifications(): boolean {
    const product = this.product();
    return !!(product?.specifications && Object.keys(product.specifications).length > 0);
  }

  getSpecificationsArray(): Array<{ key: string; value: string }> {
    const product = this.product();
    if (!product?.specifications) return [];

    return Object.entries(product.specifications).map(([key, value]) => ({
      key,
      value
    }));
  }
}
