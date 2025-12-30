import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Basket, AddBasketItemRequest, UpdateBasketItemRequest } from '../models/basket.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  private readonly apiUrl = environment.basketApiUrl;

  currentBasket = signal<Basket | null>(null);
  itemCount = signal<number>(0);

  constructor(private http: HttpClient) {}

  getBasket(buyerId: string): Observable<Basket> {
    return this.http.get<Basket>(`${this.apiUrl}/basket/${buyerId}`)
      .pipe(
        tap(basket => this.updateBasketState(basket))
      );
  }

  addItemToBasket(buyerId: string, request: AddBasketItemRequest): Observable<Basket> {
    return this.http.post<Basket>(`${this.apiUrl}/basket/${buyerId}/items`, request)
      .pipe(
        tap(basket => this.updateBasketState(basket))
      );
  }

  updateBasketItem(buyerId: string, itemId: string, request: UpdateBasketItemRequest): Observable<Basket> {
    return this.http.put<Basket>(`${this.apiUrl}/basket/${buyerId}/items/${itemId}`, request)
      .pipe(
        tap(basket => this.updateBasketState(basket))
      );
  }

  removeItemFromBasket(buyerId: string, itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/basket/${buyerId}/items/${itemId}`)
      .pipe(
        tap(() => {
          const basket = this.currentBasket();
          if (basket) {
            basket.items = basket.items.filter(item => item.id !== itemId);
            this.updateBasketState(basket);
          }
        })
      );
  }

  clearBasket(buyerId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/basket/${buyerId}`)
      .pipe(
        tap(() => {
          this.currentBasket.set(null);
          this.itemCount.set(0);
        })
      );
  }

  private updateBasketState(basket: Basket): void {
    this.currentBasket.set(basket);
    this.itemCount.set(basket.items.reduce((count, item) => count + item.quantity, 0));
  }
}
