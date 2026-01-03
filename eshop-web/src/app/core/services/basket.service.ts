import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, switchMap } from 'rxjs';
import { Basket, AddBasketItemRequest, UpdateBasketItemRequest, CreateBasketRequest } from '../models/basket.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  private readonly apiUrl = environment.basketApiUrl;

  currentBasket = signal<Basket | null>(null);
  itemCount = signal<number>(0);

  constructor(private http: HttpClient) {}

  /**
   * Crée un nouveau panier pour un client
   */
  createBasket(customerId: string): Observable<string> {
    const request: CreateBasketRequest = { customerId };
    return this.http.post<string>(`${this.apiUrl}/baskets`, request);
  }

  /**
   * Récupère le panier d'un client ou le crée s'il n'existe pas
   */
  getOrCreateBasket(customerId: string): Observable<Basket> {
    return this.http.get<Basket>(`${this.apiUrl}/baskets/customer/${customerId}`)
      .pipe(
        tap(basket => this.updateBasketState(basket)),
        catchError((error: HttpErrorResponse) => {
          // Si le panier n'existe pas (404), on le crée automatiquement
          if (error.status === 404) {
            return this.createBasket(customerId).pipe(
              switchMap(basketId => this.http.get<Basket>(`${this.apiUrl}/baskets/${basketId}`)),
              tap(basket => this.updateBasketState(basket))
            );
          }
          throw error;
        })
      );
  }

  /**
   * Récupère le panier d'un client
   */
  getBasket(customerId: string): Observable<Basket> {
    return this.http.get<Basket>(`${this.apiUrl}/baskets/customer/${customerId}`)
      .pipe(
        tap(basket => this.updateBasketState(basket))
      );
  }

  /**
   * Ajoute un item au panier
   */
  addItemToBasket(customerId: string, item: AddBasketItemRequest): Observable<void> {
    return this.getOrCreateBasket(customerId).pipe(
      switchMap(basket =>
        this.http.post<void>(`${this.apiUrl}/baskets/${basket.id}/items`, item)
      ),
      tap(() => {
        // Rafraîchir le panier après l'ajout
        this.getBasket(customerId).subscribe();
      })
    );
  }

  /**
   * Met à jour la quantité d'un item dans le panier
   */
  updateBasketItem(customerId: string, request: UpdateBasketItemRequest): Observable<void> {
    return this.getBasket(customerId).pipe(
      switchMap(basket =>
        this.http.put<void>(`${this.apiUrl}/baskets/${basket.id}/items`, request)
      ),
      tap(() => {
        // Rafraîchir le panier après la mise à jour
        this.getBasket(customerId).subscribe();
      })
    );
  }

  /**
   * Supprime un item du panier
   */
  removeItemFromBasket(customerId: string, catalogItemId: string): Observable<void> {
    return this.getBasket(customerId).pipe(
      switchMap(basket =>
        this.http.delete<void>(`${this.apiUrl}/baskets/${basket.id}/items/${catalogItemId}`)
      ),
      tap(() => {
        // Rafraîchir le panier après la suppression
        this.getBasket(customerId).subscribe();
      })
    );
  }

  /**
   * Vide le panier (supprime tous les items)
   */
  clearBasket(customerId: string): Observable<void> {
    return this.getBasket(customerId).pipe(
      switchMap(basket =>
        this.http.delete<void>(`${this.apiUrl}/baskets/${basket.id}/clear`)
      ),
      tap(() => {
        this.currentBasket.set(null);
        this.itemCount.set(0);
      })
    );
  }

  /**
   * Supprime complètement le panier
   */
  deleteBasket(customerId: string): Observable<void> {
    return this.getBasket(customerId).pipe(
      switchMap(basket =>
        this.http.delete<void>(`${this.apiUrl}/baskets/${basket.id}`)
      ),
      tap(() => {
        this.currentBasket.set(null);
        this.itemCount.set(0);
      })
    );
  }

  private updateBasketState(basket: Basket): void {
    this.currentBasket.set(basket);
    this.itemCount.set(basket.itemCount);
  }
}
