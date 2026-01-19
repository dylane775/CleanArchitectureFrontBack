import { Component, OnInit, signal, computed } from '@angular/core';
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
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components/breadcrumb/breadcrumb';

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
    CatalogItemComponent,
    BreadcrumbComponent
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

  // Sidebar state
  sidebarCollapsed = signal(false);

  // Breadcrumb
  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Accueil', url: '/', icon: 'home' },
    { label: 'Catalogue' }
  ];

  // Filters
  priceRange = signal<number[]>([0, 1000]);
  priceMin = 0;
  priceMax = 1000;
  maxPrice = 1000;
  categories = signal<FilterOption[]>([]);
  brands = signal<FilterOption[]>([]);
  sortBy = signal<string>('featured');
  searchQuery = signal<string>('');
  filteredItems = signal<CatalogItem[]>([]);

  // Paginated items for display
  paginatedItems = computed(() => {
    const filtered = this.filteredItems();
    const start = this.pageIndex * this.pageSize;
    const end = start + this.pageSize;
    return filtered.slice(start, end);
  });

  // Grouped items by category for carousel sections (max 5)
  categorySections = computed(() => {
    const items = this.filteredItems();
    const categories = this.categories();
    const sections: { category: FilterOption; items: CatalogItem[] }[] = [];

    // Get top 5 categories by item count
    const topCategories = [...categories]
      .filter(cat => cat.count && cat.count > 0)
      .sort((a, b) => (b.count || 0) - (a.count || 0))
      .slice(0, 5);

    // Group items by category
    topCategories.forEach(category => {
      const categoryItems = items.filter(item => item.catalogTypeId === category.value);
      if (categoryItems.length > 0) {
        sections.push({ category, items: categoryItems });
      }
    });

    return sections;
  });

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
      }
    });

    this.loadFiltersData();
    this.loadCatalogItems();
    this.priceMin = this.priceRange()[0];
    this.priceMax = this.priceRange()[1];
  }

  loadFiltersData(): void {
    // Load catalog types (categories)
    this.catalogService.getCatalogTypes().subscribe({
      next: (types) => {
        this.categories.set(types.map(type => ({
          label: type.type,
          value: type.id,
          checked: false,
          count: 0
        })));
        this.updateFilterCounts();
      },
      error: (error) => {
        console.error('Error loading catalog types:', error);
      }
    });

    // Load catalog brands
    this.catalogService.getCatalogBrands().subscribe({
      next: (brands) => {
        this.brands.set(brands.map(brand => ({
          label: brand.brand,
          value: brand.id,
          checked: false,
          count: 0
        })));
        this.updateFilterCounts();
      },
      error: (error) => {
        console.error('Error loading catalog brands:', error);
      }
    });
  }

  updateFilterCounts(): void {
    const items = this.catalogItems();

    // Update category counts
    this.categories.update(categories =>
      categories.map(cat => ({
        ...cat,
        count: items.filter(item => item.catalogTypeId === cat.value).length
      }))
    );

    // Update brand counts
    this.brands.update(brands =>
      brands.map(brand => ({
        ...brand,
        count: items.filter(item => item.catalogBrandId === brand.value).length
      }))
    );
  }

  onPriceChange(): void {
    this.priceRange.set([this.priceMin, this.priceMax]);
    this.applyFilters();
  }

  onSortChange(event: any): void {
    this.sortBy.set(event.value);
    this.applyFilters();
  }

  loadCatalogItems(): void {
    this.loading.set(true);
    // Load all items for filtering (use a large page size)
    // Backend uses 1-based pagination
    this.catalogService.getCatalogItems(1, 1000).subscribe({
      next: (result) => {
        console.log('Loaded catalog items:', result.data.length);
        this.catalogItems.set(result.data);
        this.loading.set(false);

        // Calculate max price from loaded items
        if (result.data.length > 0) {
          const prices = result.data.map(item => item.price);
          const calculatedMaxPrice = Math.max(...prices);
          this.maxPrice = Math.ceil(calculatedMaxPrice / 100) * 100; // Round up to nearest 100

          // Update slider values
          this.priceMin = 0;
          this.priceMax = this.maxPrice;
          this.priceRange.set([0, this.maxPrice]);
        }

        // Update filter counts with loaded items
        this.updateFilterCounts();

        // Apply all filters
        this.applyFilters();
      },
      error: (error) => {
        console.error('Error loading catalog items:', error);
        this.loading.set(false);
      }
    });
  }

  applyFilters(): void {
    let filtered = [...this.catalogItems()];
    console.log('Applying filters to', filtered.length, 'items');

    // Apply search filter
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      filtered = filtered.filter(item =>
        item.name.toLowerCase().includes(query) ||
        item.description?.toLowerCase().includes(query)
      );
    }

    // Apply price range filter
    const [minPrice, maxPrice] = this.priceRange();
    console.log('Price range:', minPrice, '-', maxPrice);
    filtered = filtered.filter(item =>
      item.price >= minPrice && item.price <= maxPrice
    );
    console.log('After price filter:', filtered.length);

    // Apply category filter
    const selectedCategories = this.categories().filter(c => c.checked).map(c => c.value);
    if (selectedCategories.length > 0) {
      filtered = filtered.filter(item =>
        selectedCategories.includes(item.catalogTypeId)
      );
    }

    // Apply brand filter
    const selectedBrands = this.brands().filter(b => b.checked).map(b => b.value);
    if (selectedBrands.length > 0) {
      filtered = filtered.filter(item =>
        selectedBrands.includes(item.catalogBrandId)
      );
    }

    // Apply sorting
    switch (this.sortBy()) {
      case 'price-asc':
        filtered.sort((a, b) => a.price - b.price);
        break;
      case 'price-desc':
        filtered.sort((a, b) => b.price - a.price);
        break;
      case 'newest':
        // Assuming items are ordered by creation date from backend
        filtered.reverse();
        break;
      default:
        // featured - keep original order
        break;
    }

    this.filteredItems.set(filtered);
    this.totalItems.set(filtered.length);
    console.log('Final filtered items:', filtered.length);
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    // Pagination is now handled client-side with filtered items
  }

  onAddToBasket(item: CatalogItem): void {
  this.basketService.addItemToBasket({
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
    return `${value.toLocaleString('fr-FR')} FCFA`;
  }

  clearAllFilters(): void {
    this.priceMin = 0;
    this.priceMax = this.maxPrice;
    this.priceRange.set([0, this.maxPrice]);
    this.categories.update(cats => cats.map(c => ({ ...c, checked: false })));
    this.brands.update(brs => brs.map(b => ({ ...b, checked: false })));
    this.searchQuery.set('');
    this.applyFilters();
  }

  resetPriceFilter(): void {
    this.priceMin = 0;
    this.priceMax = this.maxPrice;
    this.priceRange.set([0, this.maxPrice]);
    this.applyFilters();
  }

  getActiveFiltersCount(): number {
    const categoryCount = this.categories().filter(c => c.checked).length;
    const brandCount = this.brands().filter(b => b.checked).length;
    const priceChanged = this.priceRange()[0] !== 0 || this.priceRange()[1] !== this.maxPrice;
    return categoryCount + brandCount + (priceChanged ? 1 : 0);
  }

  toggleSidebar(): void {
    this.sidebarCollapsed.update(collapsed => !collapsed);
  }
}
