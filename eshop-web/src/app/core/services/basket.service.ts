import { Injectable, signal, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, switchMap, forkJoin, of } from 'rxjs';
import { Basket, AddBasketItemRequest, UpdateBasketItemRequest, CreateBasketRequest } from '../models/basket.model';
import { environment } from '../../../environments/environment';
import { GuestBasketService } from './guest-basket.service';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  private readonly apiUrl = environment.basketApiUrl;
  private readonly http = inject(HttpClient);
  private readonly guestBasketService = inject(GuestBasketService);
  private readonly authService = inject(AuthService);

  currentBasket = signal<Basket | null>(null);
  itemCount = signal<number>(0);

  constructor() {}

  /**
   * Récupère le customerId actuel (userId authentifié ou guestBasketId)
   * @returns Le customerId à utiliser pour les opérations panier
   */
  private getCurrentCustomerId(): string {
    const user = this.authService.currentUser();

    if (user && user.id) {
      // Utilisateur connecté: utiliser son userId
      return user.id;
    } else {
      // Utilisateur non connecté: utiliser ou créer un guestBasketId
      return this.guestBasketService.getOrCreateGuestBasketId();
    }
  }

  /**
   * Vérifie si l'utilisateur actuel est un guest
   * @returns true si c'est un guest (non connecté)
   */
  private isGuest(): boolean {
    return !this.authService.isAuthenticated();
  }

  /**
   * Crée un nouveau panier pour un client
   */
  createBasket(customerId: string): Observable<string> {
    const request: CreateBasketRequest = { customerId };
    return this.http.post<string>(`${this.apiUrl}/baskets`, request);
  }

  /**
   * Récupère le panier d'un client ou le crée s'il n'existe pas
   * Utilise automatiquement le customerId approprié (userId ou guestBasketId)
   */
  getOrCreateBasket(customerId?: string): Observable<Basket> {
    const id = customerId || this.getCurrentCustomerId();

    return this.http.get<Basket>(`${this.apiUrl}/baskets/customer/${id}`)
      .pipe(
        tap(basket => this.updateBasketState(basket)),
        catchError((error: HttpErrorResponse) => {
          // Si le panier n'existe pas (404), on le crée automatiquement
          if (error.status === 404) {
            return this.createBasket(id).pipe(
              switchMap(basketId => this.http.get<Basket>(`${this.apiUrl}/baskets/${basketId}`)),
              tap(basket => this.updateBasketState(basket))
            );
          }
          throw error;
        })
      );
  }

  /**
   * Récupère le panier actuel (guest ou utilisateur connecté)
   */
  getCurrentBasket(): Observable<Basket> {
    return this.getOrCreateBasket();
  }

  /**
   * Récupère le panier d'un client spécifique
   */
  getBasket(customerId?: string): Observable<Basket> {
    const id = customerId || this.getCurrentCustomerId();

    return this.http.get<Basket>(`${this.apiUrl}/baskets/customer/${id}`)
      .pipe(
        tap(basket => this.updateBasketState(basket))
      );
  }

  /**
   * Ajoute un item au panier (guest ou utilisateur connecté)
   */
  addItemToBasket(item: AddBasketItemRequest, customerId?: string): Observable<void> {
    const id = customerId || this.getCurrentCustomerId();

    return this.getOrCreateBasket(id).pipe(
      switchMap(basket =>
        this.http.post<void>(`${this.apiUrl}/baskets/${basket.id}/items`, item)
      ),
      tap(() => {
        // Rafraîchir le panier après l'ajout
        this.getBasket(id).subscribe();
      })
    );
  }

  /**
   * Met à jour la quantité d'un item dans le panier
   */
  updateBasketItem(request: UpdateBasketItemRequest, customerId?: string): Observable<void> {
    const id = customerId || this.getCurrentCustomerId();

    return this.getBasket(id).pipe(
      switchMap(basket =>
        this.http.put<void>(`${this.apiUrl}/baskets/${basket.id}/items`, request)
      ),
      tap(() => {
        // Rafraîchir le panier après la mise à jour
        this.getBasket(id).subscribe();
      })
    );
  }

  /**
   * Supprime un item du panier
   */
  removeItemFromBasket(catalogItemId: string, customerId?: string): Observable<void> {
    const id = customerId || this.getCurrentCustomerId();

    return this.getBasket(id).pipe(
      switchMap(basket =>
        this.http.delete<void>(`${this.apiUrl}/baskets/${basket.id}/items/${catalogItemId}`)
      ),
      tap(() => {
        // Rafraîchir le panier après la suppression
        this.getBasket(id).subscribe();
      })
    );
  }

  /**
   * Vide le panier (supprime tous les items)
   */
  clearBasket(customerId?: string): Observable<void> {
    const id = customerId || this.getCurrentCustomerId();

    return this.getBasket(id).pipe(
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
  deleteBasket(customerId?: string): Observable<void> {
    const id = customerId || this.getCurrentCustomerId();

    return this.getBasket(id).pipe(
      switchMap(basket =>
        this.http.delete<void>(`${this.apiUrl}/baskets/${basket.id}`)
      ),
      tap(() => {
        this.currentBasket.set(null);
        this.itemCount.set(0);
      })
    );
  }

  /**
   * Fusionne le panier guest avec le panier de l'utilisateur connecté
   * Appelé lors de la connexion d'un utilisateur
   * @param userId L'ID de l'utilisateur nouvellement connecté
   */
  mergeGuestBasketOnLogin(userId: string): Observable<void> {
    // Vérifier si un panier guest existe
    const guestBasketId = this.guestBasketService.getGuestBasketId();

    if (!guestBasketId) {
      // Pas de panier guest à fusionner, charger simplement le panier utilisateur
      this.getBasket(userId).subscribe();
      return of(void 0);
    }

    console.log('Fusion du panier guest avec le panier utilisateur...');

    // Récupérer le panier guest
    return this.getBasket(guestBasketId).pipe(
      switchMap(guestBasket => {
        // Si le panier guest est vide, le supprimer simplement
        if (!guestBasket.items || guestBasket.items.length === 0) {
          this.guestBasketService.clearGuestBasketId();
          this.getBasket(userId).subscribe();
          return of(void 0);
        }

        // Récupérer ou créer le panier utilisateur
        return this.getOrCreateBasket(userId).pipe(
          switchMap(userBasket => {
            // Fusionner les items
            const mergePromises: Observable<void>[] = [];

            guestBasket.items.forEach(guestItem => {
              // Vérifier si l'item existe déjà dans le panier utilisateur
              const existingItem = userBasket.items.find(
                item => item.catalogItemId === guestItem.catalogItemId
              );

              if (existingItem) {
                // Item existe: additionner les quantités
                const newQuantity = existingItem.quantity + guestItem.quantity;
                const updateRequest: UpdateBasketItemRequest = {
                  catalogItemId: guestItem.catalogItemId,
                  newQuantity: newQuantity
                };

                mergePromises.push(
                  this.updateBasketItem(updateRequest, userId)
                );
              } else {
                // Item n'existe pas: l'ajouter
                const addRequest: AddBasketItemRequest = {
                  catalogItemId: guestItem.catalogItemId,
                  productName: guestItem.productName,
                  unitPrice: guestItem.unitPrice,
                  quantity: guestItem.quantity,
                  pictureUrl: guestItem.pictureUrl
                };

                mergePromises.push(
                  this.addItemToBasket(addRequest, userId)
                );
              }
            });

            // Exécuter toutes les opérations de fusion
            if (mergePromises.length === 0) {
              return of(void 0);
            }

            return forkJoin(mergePromises).pipe(
              switchMap(() => {
                // Supprimer le panier guest après fusion
                return this.deleteBasket(guestBasketId);
              }),
              tap(() => {
                // Nettoyer le localStorage
                this.guestBasketService.clearGuestBasketId();
                console.log('Panier guest fusionné et supprimé avec succès');

                // Charger le panier utilisateur final
                this.getBasket(userId).subscribe();
              })
            );
          })
        );
      }),
      catchError(error => {
        console.error('Erreur lors de la fusion des paniers:', error);
        // En cas d'erreur, charger quand même le panier utilisateur
        this.getBasket(userId).subscribe();
        return of(void 0);
      })
    );
  }

  /**
   * Nettoie le panier guest du localStorage
   * Utile lors de la déconnexion ou pour forcer un nouveau panier
   */
  clearGuestBasket(): void {
    this.guestBasketService.clearGuestBasketId();
    this.currentBasket.set(null);
    this.itemCount.set(0);
  }

  /**
   * Vérifie si l'utilisateur actuel est un guest
   */
  isGuestUser(): boolean {
    return this.isGuest();
  }

  /**
   * Récupère le nombre de jours restants avant expiration du panier guest
   */
  getGuestBasketRemainingDays(): number {
    return this.guestBasketService.getRemainingDays();
  }

  private updateBasketState(basket: Basket): void {
    this.currentBasket.set(basket);
    this.itemCount.set(basket.itemCount);
  }
}
