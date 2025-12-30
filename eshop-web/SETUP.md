# eShop Web - Frontend Angular

Application Angular pour l'e-commerce eShopOnContainers

## Prérequis

- Node.js 24.5.0 ou supérieur
- npm 11.5.1 ou supérieur
- Angular CLI 20.3.6

## Installation

```bash
cd eshop-web
npm install
```

## Configuration

Les URLs des APIs backend sont configurées dans `src/environments/environment.ts` :

```typescript
export const environment = {
  production: false,
  identityApiUrl: 'http://localhost:5000/api',
  catalogApiUrl: 'http://localhost:5234/api',
  basketApiUrl: 'http://localhost:5235/api',
  orderingApiUrl: 'http://localhost:5236/api'
};
```

Assurez-vous que les APIs backend correspondantes sont démarrées avant de lancer le frontend.

## Démarrage du serveur de développement

```bash
npm start
# ou
ng serve
```

L'application sera accessible à l'adresse : `http://localhost:4200/`

## Build de production

```bash
npm run build
```

Les fichiers de build seront générés dans le dossier `dist/eshop-web/`

## Structure du projet

```
src/
├── app/
│   ├── core/                  # Services, guards, interceptors, models
│   │   ├── guards/
│   │   │   └── auth.guard.ts
│   │   ├── interceptors/
│   │   │   └── auth.interceptor.ts
│   │   ├── models/
│   │   │   ├── auth.model.ts
│   │   │   ├── catalog.model.ts
│   │   │   └── basket.model.ts
│   │   └── services/
│   │       ├── auth.service.ts
│   │       ├── catalog.service.ts
│   │       └── basket.service.ts
│   ├── features/              # Composants fonctionnels
│   │   ├── auth/
│   │   │   ├── login/
│   │   │   └── register/
│   │   └── catalog/
│   │       ├── catalog-list/
│   │       └── catalog-item/
│   ├── app.config.ts
│   └── app.routes.ts
└── environments/
    └── environment.ts
```

## Fonctionnalités implémentées

### Authentification
- ✅ Page de connexion (`/auth/login`)
- ✅ Page d'inscription (`/auth/register`)
- ✅ JWT Token Management
- ✅ Auto-refresh token
- ✅ Auth Guard pour protéger les routes

### Catalogue de produits
- ✅ Liste paginée des produits (`/catalog`)
- ✅ Affichage des détails produit
- ✅ Filtrage par marque et type
- ✅ Ajout au panier

### Panier (à compléter)
- Affichage du panier
- Modification des quantités
- Suppression d'articles
- Validation de commande

## Routes disponibles

| Route | Description | Protection |
|-------|-------------|------------|
| `/` | Redirection vers `/catalog` | Non |
| `/auth/login` | Page de connexion | Non |
| `/auth/register` | Page d'inscription | Non |
| `/catalog` | Liste des produits | Non |

## Services disponibles

### AuthService
- `login(request: LoginRequest): Observable<AuthResponse>`
- `register(request: RegisterRequest): Observable<AuthResponse>`
- `logout(): void`
- `refreshToken(): Observable<AuthResponse>`
- `isAuthenticated(): Signal<boolean>`
- `currentUser(): Signal<User | null>`

### CatalogService
- `getCatalogItems(pageIndex, pageSize): Observable<PaginatedItems<CatalogItem>>`
- `getCatalogItemById(id): Observable<CatalogItem>`
- `getCatalogTypes(): Observable<CatalogType[]>`
- `getCatalogBrands(): Observable<CatalogBrand[]>`

### BasketService
- `getBasket(buyerId): Observable<Basket>`
- `addItemToBasket(buyerId, request): Observable<Basket>`
- `updateBasketItem(buyerId, itemId, request): Observable<Basket>`
- `removeItemFromBasket(buyerId, itemId): Observable<void>`
- `clearBasket(buyerId): Observable<void>`

## Utilisation

1. Démarrez tous les microservices backend
2. Lancez l'application Angular : `npm start`
3. Ouvrez votre navigateur à `http://localhost:4200`
4. Inscrivez-vous ou connectez-vous
5. Naviguez dans le catalogue et ajoutez des produits au panier

## Technologies utilisées

- Angular 20.3.15
- Angular Material 20.2.14
- RxJS pour la programmation réactive
- Signals pour la gestion d'état
- TypeScript
- SCSS pour le styling

## À faire

- [ ] Module Basket complet
- [ ] Module Ordering
- [ ] Page de profil utilisateur
- [ ] Recherche de produits
- [ ] Filtres avancés
- [ ] Gestion des favoris
- [ ] Historique des commandes
