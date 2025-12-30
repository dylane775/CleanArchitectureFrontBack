import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { Router, RouterModule } from '@angular/router';
import { CatalogService } from '../../../core/services/catalog.service';
import { BasketService } from '../../../core/services/basket.service';
import { AuthService } from '../../../core/services/auth.service';
import { CatalogItem } from '../../../core/models/catalog.model';
import { CatalogItemComponent } from '../catalog-item/catalog-item';

@Component({
  selector: 'app-catalog-list',
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatToolbarModule,
    MatIconModule,
    MatBadgeModule,
    CatalogItemComponent
  ],
  templateUrl: './catalog-list.html',
  styleUrl: './catalog-list.scss',
})
export class CatalogList implements OnInit {
  catalogItems = signal<CatalogItem[]>([]);
  loading = signal(false);
  totalItems = signal(0);
  pageSize = 10;
  pageIndex = 0;

  constructor(
    private catalogService: CatalogService,
    public basketService: BasketService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCatalogItems();
  }

  loadCatalogItems(): void {
    this.loading.set(true);
    this.catalogService.getCatalogItems(this.pageIndex, this.pageSize).subscribe({
      next: (result) => {
        this.catalogItems.set(result.data);
        this.totalItems.set(result.count);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading catalog items:', error);
        this.loading.set(false);
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadCatalogItems();
  }

  onAddToBasket(item: CatalogItem): void {
    const user = this.authService.currentUser();
    if (!user) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.basketService.addItemToBasket(user.id, {
      productId: item.id,
      quantity: 1
    }).subscribe({
      next: () => {
        console.log('Item added to basket');
      },
      error: (error) => {
        console.error('Error adding item to basket:', error);
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
