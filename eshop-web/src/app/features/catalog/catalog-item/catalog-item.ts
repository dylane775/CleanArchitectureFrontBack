import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { CatalogItem as CatalogItemModel } from '../../../core/models/catalog.model';
import { environment } from '../../../../environments/environment';

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
    // Calcul fictif pour démo - en production, utiliser item.discount
    return Math.floor(Math.random() * 30) + 10;
  }

  isNew(): boolean {
    // Logique fictive pour démo - en production, vérifier item.createdDate
    return Math.random() > 0.7;
  }

  getRating(): number {
    // Note fictive pour démo (3.5 à 5 étoiles)
    return Math.floor(Math.random() * 1.5) + 3.5;
  }

  getReviewCount(): number {
    // Nombre d'avis fictif pour démo
    return Math.floor(Math.random() * 5000) + 100;
  }

  hasPrime(): boolean {
    // Badge Prime fictif pour démo
    return Math.random() > 0.6;
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
