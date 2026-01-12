import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { BasketService } from '../services/basket.service';

/**
 * Initialise l'application en configurant la liaison entre les services
 * Évite les dépendances circulaires entre AuthService et BasketService
 */
export function initializeApp() {
  return () => {
    const authService = inject(AuthService);
    const basketService = inject(BasketService);

    // Injecter le BasketService dans l'AuthService pour la fusion des paniers
    authService.setBasketService(basketService);

    console.log('Application initialisée: services liés avec succès');
  };
}
