import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
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
  @Output() addToBasket = new EventEmitter<CatalogItemModel>();

  onAddToBasket(): void {
    this.addToBasket.emit(this.item);
  }
}
