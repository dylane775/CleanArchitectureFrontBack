import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { CatalogItem as CatalogItemModel } from '../../../core/models/catalog.model';

@Component({
  selector: 'app-catalog-item',
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule
  ],
  templateUrl: './catalog-item.html',
  styleUrl: './catalog-item.scss',
})
export class CatalogItemComponent {
  @Input() item!: CatalogItemModel;
  @Input() compact = false; // Mode compact pour carrousels
  @Output() addToBasket = new EventEmitter<CatalogItemModel>();

  constructor(private router: Router) {}

  onAddToBasket(): void {
    this.addToBasket.emit(this.item);
  }

  navigateToDetail(): void {
    this.router.navigate(['/product', this.item.id]);
  }

  getDiscountPercentage(): number {
    // Calcul fictif pour démo
    return Math.floor(Math.random() * 30) + 10;
  }

  isNew(): boolean {
    // Logique fictive pour démo
    return Math.random() > 0.7;
  }
}
