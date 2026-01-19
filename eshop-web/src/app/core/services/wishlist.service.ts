import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap, catchError } from 'rxjs';
import { WishlistItem, AddToWishlistRequest, ToggleWishlistResponse } from '../models/wishlist.model';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private readonly apiUrl = `${environment.basketApiUrl}/wishlist`;

  // State (private writable signals)
  private _wishlistItems = signal<WishlistItem[]>([]);
  private _wishlistIds = signal<Set<string>>(new Set());
  private _loading = signal(false);

  // Public readonly signals
  readonly wishlistItems = this._wishlistItems.asReadonly();
  readonly loading = this._loading.asReadonly();

  // Public computed signals
  readonly items = computed(() => this._wishlistItems());
  readonly itemCount = computed(() => this._wishlistItems().length);
  readonly wishlistCount = computed(() => this._wishlistItems().length);
  readonly isLoading = computed(() => this._loading());

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {
    // Charger la wishlist quand l'utilisateur se connecte
    this.authService.isAuthenticated();
  }

  // Vérifier si un produit est dans la wishlist (synchrone, basé sur le cache local)
  isInWishlist(catalogItemId: string): boolean {
    return this._wishlistIds().has(catalogItemId);
  }

  // Charger la wishlist depuis le serveur
  loadWishlist(): Observable<WishlistItem[]> {
    if (!this.authService.isAuthenticated()) {
      this._wishlistItems.set([]);
      this._wishlistIds.set(new Set());
      return of([]);
    }

    this._loading.set(true);

    return this.http.get<WishlistItem[]>(this.apiUrl).pipe(
      tap(items => {
        this._wishlistItems.set(items);
        this._wishlistIds.set(new Set(items.map(i => i.catalogItemId)));
        this._loading.set(false);
      }),
      catchError(error => {
        console.error('Error loading wishlist:', error);
        this._loading.set(false);
        return of([]);
      })
    );
  }

  // Récupérer le nombre d'articles
  getWishlistCount(): Observable<number> {
    if (!this.authService.isAuthenticated()) {
      return of(0);
    }
    return this.http.get<number>(`${this.apiUrl}/count`);
  }

  // Vérifier si un produit est dans la wishlist (depuis le serveur)
  checkInWishlist(catalogItemId: string): Observable<boolean> {
    if (!this.authService.isAuthenticated()) {
      return of(false);
    }
    return this.http.get<boolean>(`${this.apiUrl}/check/${catalogItemId}`);
  }

  // Ajouter à la wishlist
  addToWishlist(item: AddToWishlistRequest): Observable<WishlistItem> {
    return this.http.post<WishlistItem>(this.apiUrl, item).pipe(
      tap(newItem => {
        this._wishlistItems.update((items: WishlistItem[]) => [...items, newItem]);
        this._wishlistIds.update((ids: Set<string>) => {
          const newIds = new Set(ids);
          newIds.add(newItem.catalogItemId);
          return newIds;
        });
      })
    );
  }

  // Retirer de la wishlist
  removeFromWishlist(catalogItemId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${catalogItemId}`).pipe(
      tap(() => {
        this._wishlistItems.update((items: WishlistItem[]) =>
          items.filter((i: WishlistItem) => i.catalogItemId !== catalogItemId)
        );
        this._wishlistIds.update((ids: Set<string>) => {
          const newIds = new Set(ids);
          newIds.delete(catalogItemId);
          return newIds;
        });
      })
    );
  }

  // Toggle wishlist (ajouter ou retirer)
  toggleWishlist(catalogItemId: string, item: AddToWishlistRequest): Observable<ToggleWishlistResponse> {
    return this.http.post<ToggleWishlistResponse>(`${this.apiUrl}/toggle/${catalogItemId}`, item).pipe(
      tap(response => {
        if (response.added && response.item) {
          this._wishlistItems.update((items: WishlistItem[]) => [...items, response.item!]);
          this._wishlistIds.update((ids: Set<string>) => {
            const newIds = new Set(ids);
            newIds.add(catalogItemId);
            return newIds;
          });
        } else {
          this._wishlistItems.update((items: WishlistItem[]) =>
            items.filter((i: WishlistItem) => i.catalogItemId !== catalogItemId)
          );
          this._wishlistIds.update((ids: Set<string>) => {
            const newIds = new Set(ids);
            newIds.delete(catalogItemId);
            return newIds;
          });
        }
      })
    );
  }

  // Vider la wishlist côté serveur
  clearWishlist(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/clear`).pipe(
      tap(() => {
        this._wishlistItems.set([]);
        this._wishlistIds.set(new Set());
      })
    );
  }

  // Vider le cache local (appelé lors de la déconnexion)
  clearLocalWishlist(): void {
    this._wishlistItems.set([]);
    this._wishlistIds.set(new Set());
  }
}
