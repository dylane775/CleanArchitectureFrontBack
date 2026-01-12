import { Injectable } from '@angular/core';
import { v4 as uuidv4 } from 'uuid';

/**
 * Service de gestion du panier pour les utilisateurs non connectés (guests)
 * Génère et stocke un basketId unique dans le localStorage
 */
@Injectable({
  providedIn: 'root'
})
export class GuestBasketService {
  private readonly BASKET_ID_KEY = 'guest_basket_id';
  private readonly BASKET_CREATED_AT_KEY = 'guest_basket_created_at';
  private readonly BASKET_TTL_DAYS = 7; // Durée de vie du panier guest: 7 jours

  constructor() {
    this.cleanupExpiredBasket();
  }

  /**
   * Récupère ou génère un basketId pour un utilisateur guest
   * @returns Le basketId unique du guest
   */
  getOrCreateGuestBasketId(): string {
    let basketId = this.getGuestBasketId();

    if (!basketId) {
      basketId = this.createGuestBasketId();
    }

    return basketId;
  }

  /**
   * Récupère le basketId existant du guest depuis le localStorage
   * @returns Le basketId ou null s'il n'existe pas
   */
  getGuestBasketId(): string | null {
    try {
      const basketId = localStorage.getItem(this.BASKET_ID_KEY);
      return basketId;
    } catch (error) {
      console.error('Erreur lors de la récupération du basketId guest:', error);
      return null;
    }
  }

  /**
   * Crée un nouveau basketId pour un guest et le stocke dans le localStorage
   * @returns Le nouveau basketId généré
   */
  createGuestBasketId(): string {
    const basketId = `guest-${uuidv4()}`;
    const createdAt = new Date().toISOString();

    try {
      localStorage.setItem(this.BASKET_ID_KEY, basketId);
      localStorage.setItem(this.BASKET_CREATED_AT_KEY, createdAt);
      console.log('Nouveau panier guest créé:', basketId);
    } catch (error) {
      console.error('Erreur lors de la création du basketId guest:', error);
    }

    return basketId;
  }

  /**
   * Supprime le basketId guest du localStorage
   * Utilisé lors de la connexion d'un utilisateur ou après fusion des paniers
   */
  clearGuestBasketId(): void {
    try {
      localStorage.removeItem(this.BASKET_ID_KEY);
      localStorage.removeItem(this.BASKET_CREATED_AT_KEY);
      console.log('Panier guest supprimé du localStorage');
    } catch (error) {
      console.error('Erreur lors de la suppression du basketId guest:', error);
    }
  }

  /**
   * Vérifie si le basketId guest existe dans le localStorage
   * @returns true si un basketId guest existe
   */
  hasGuestBasket(): boolean {
    return this.getGuestBasketId() !== null;
  }

  /**
   * Vérifie si le panier guest a expiré selon le TTL défini
   * @returns true si le panier a expiré
   */
  isBasketExpired(): boolean {
    try {
      const createdAtStr = localStorage.getItem(this.BASKET_CREATED_AT_KEY);

      if (!createdAtStr) {
        return true;
      }

      const createdAt = new Date(createdAtStr);
      const now = new Date();
      const diffInDays = (now.getTime() - createdAt.getTime()) / (1000 * 60 * 60 * 24);

      return diffInDays > this.BASKET_TTL_DAYS;
    } catch (error) {
      console.error('Erreur lors de la vérification de l\'expiration du panier:', error);
      return true;
    }
  }

  /**
   * Nettoie le panier guest s'il a expiré
   * Appelé automatiquement à l'initialisation du service
   */
  private cleanupExpiredBasket(): void {
    if (this.hasGuestBasket() && this.isBasketExpired()) {
      console.log('Panier guest expiré, nettoyage automatique');
      this.clearGuestBasketId();
    }
  }

  /**
   * Récupère la date de création du panier guest
   * @returns La date de création ou null
   */
  getBasketCreatedAt(): Date | null {
    try {
      const createdAtStr = localStorage.getItem(this.BASKET_CREATED_AT_KEY);
      return createdAtStr ? new Date(createdAtStr) : null;
    } catch (error) {
      console.error('Erreur lors de la récupération de la date de création:', error);
      return null;
    }
  }

  /**
   * Récupère le nombre de jours restants avant expiration du panier
   * @returns Le nombre de jours restants ou 0 si expiré
   */
  getRemainingDays(): number {
    const createdAt = this.getBasketCreatedAt();

    if (!createdAt) {
      return 0;
    }

    const now = new Date();
    const diffInDays = (now.getTime() - createdAt.getTime()) / (1000 * 60 * 60 * 24);
    const remaining = this.BASKET_TTL_DAYS - diffInDays;

    return Math.max(0, Math.ceil(remaining));
  }
}
