import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSliderModule } from '@angular/material/slider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router, ActivatedRoute } from '@angular/router';
import { CatalogService } from '../../../core/services/catalog.service';
import { BasketService } from '../../../core/services/basket.service';
import { AuthService } from '../../../core/services/auth.service';
import { CatalogItem } from '../../../core/models/catalog.model';
import { CatalogItemComponent } from '../catalog-item/catalog-item';

interface FilterOption {
  label: string;
  value: string;
  count?: number;
  checked: boolean;
}

@Component({
  selector: 'app-catalog-list',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
    MatSliderModule,
    MatExpansionModule,
    MatIconModule,
    MatChipsModule,
    MatSnackBarModule,
    CatalogItemComponent
  ],
  templateUrl: './catalog-list.html',
  styleUrl: './catalog-list.scss',
})
export class CatalogList implements OnInit {
  catalogItems = signal<CatalogItem[]>([]);
  loading = signal(false);
  totalItems = signal(0);
  pageSize = 12;
  pageIndex = 0;

  // Filters
  priceRange = signal<number[]>([0, 1000]);
  priceMin = 0;
  priceMax = 1000;
  maxPrice = 1000;
  categories = signal<FilterOption[]>([
    { label: 'Electronics', value: 'electronics', count: 45, checked: false },
    { label: 'Fashion', value: 'fashion', count: 32, checked: false },
    { label: 'Home & Garden', value: 'home', count: 28, checked: false },
    { label: 'Sports', value: 'sports', count: 19, checked: false },
    { label: 'Books', value: 'books', count: 15, checked: false }
  ]);
  brands = signal<FilterOption[]>([
    { label: 'Apple', value: 'apple', count: 12, checked: false },
    { label: 'Samsung', value: 'samsung', count: 18, checked: false },
    { label: 'Sony', value: 'sony', count: 9, checked: false },
    { label: 'Nike', value: 'nike', count: 15, checked: false },
    { label: 'Adidas', value: 'adidas', count: 11, checked: false }
  ]);
  sortBy = signal<string>('featured');
  searchQuery = signal<string>('');
  filteredItems = signal<CatalogItem[]>([]);

  constructor(
    private catalogService: CatalogService,
    private basketService: BasketService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    // Listen for search query params
    this.route.queryParams.subscribe(params => {
      if (params['search']) {
        this.searchQuery.set(params['search']);
        this.filterItems();
      }
    });

    this.loadCatalogItems();
    this.priceMin = this.priceRange()[0];
    this.priceMax = this.priceRange()[1];
  }

  onPriceChange(): void {
    this.priceRange.set([this.priceMin, this.priceMax]);
  }

  onSortChange(event: any): void {
    this.sortBy.set(event.value);
    this.applyFilters();
  }

  loadCatalogItems(): void {
    this.loading.set(true);
    this.catalogService.getCatalogItems(this.pageIndex, this.pageSize).subscribe({
      next: (result) => {
        this.catalogItems.set(result.data);
        this.filteredItems.set(result.data);
        this.totalItems.set(result.count);
        this.loading.set(false);

        // Apply search filter if query exists
        if (this.searchQuery()) {
          this.filterItems();
        }
      },
      error: (error) => {
        console.error('Error loading catalog items:', error);
        this.loading.set(false);
      }
    });
  }

  filterItems(): void {
    const query = this.searchQuery().toLowerCase().trim();

    if (!query) {
      this.filteredItems.set(this.catalogItems());
      return;
    }

    const filtered = this.catalogItems().filter(item =>
      item.name.toLowerCase().includes(query) ||
      item.description?.toLowerCase().includes(query)
    );

    this.filteredItems.set(filtered);
    this.totalItems.set(filtered.length);
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadCatalogItems();
  }

  onAddToBasket(item: CatalogItem): void {
    const user = this.authService.currentUser();
    if (!user) {
      this.snackBar.open('Please login to add items to basket', 'Close', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top'
      });
      this.router.navigate(['/auth/login']);
      return;
    }

    this.basketService.addItemToBasket(user.id, {
      catalogItemId: item.id,
      productName: item.name,
      unitPrice: item.price,
      quantity: 1,
      pictureUrl: item.pictureUri || ''
    }).subscribe({
      next: () => {
        this.snackBar.open(`${item.name} added to basket!`, 'View Basket', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        }).onAction().subscribe(() => {
          this.router.navigate(['/basket']);
        });
      },
      error: (error) => {
        console.error('Error adding item to basket:', error);
        this.snackBar.open('Failed to add item to basket. Please try again.', 'Close', {
          duration: 4000,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        });
      }
    });
  }

  formatPrice(value: number): string {
    return `$${value}`;
  }

  applyFilters(): void {
    // TODO: Implement filter logic
    this.loadCatalogItems();
  }

  clearAllFilters(): void {
    this.priceMin = 0;
    this.priceMax = this.maxPrice;
    this.priceRange.set([0, this.maxPrice]);
    this.categories.update(cats => cats.map(c => ({ ...c, checked: false })));
    this.brands.update(brs => brs.map(b => ({ ...b, checked: false })));
    this.applyFilters();
  }

  getActiveFiltersCount(): number {
    const categoryCount = this.categories().filter(c => c.checked).length;
    const brandCount = this.brands().filter(b => b.checked).length;
    const priceChanged = this.priceRange()[0] !== 0 || this.priceRange()[1] !== this.maxPrice;
    return categoryCount + brandCount + (priceChanged ? 1 : 0);
  }
}
