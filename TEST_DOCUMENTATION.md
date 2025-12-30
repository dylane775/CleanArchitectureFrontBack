# Documentation des Tests - eShop Microservices

Cette documentation décrit les tests unitaires et d'intégration pour l'application eShop avec Clean Architecture.

## 📋 Table des matières

- [Vue d'ensemble](#vue-densemble)
- [Tests Unitaires](#tests-unitaires)
- [Tests d'Intégration](#tests-dintégration)
- [Exécution des tests](#exécution-des-tests)
- [Couverture de code](#couverture-de-code)

---

## 🎯 Vue d'ensemble

Le projet contient **3 microservices** avec leurs tests respectifs :

### Microservices testés

1. **Basket Service** (Port: 5235) - Gestion des paniers d'achat
2. **Catalog Service** - Gestion du catalogue de produits
3. **Ordering Service** (Port: 5240) - Gestion des commandes

### Structure des tests

```
eShopOnContainerCleanArchitecture/
├── eShopOnContainers.Basket/
│   └── Basket.UnitTests/
│       └── BasketServiceUnitTests.cs (13 tests)
├── eShopOnContainers.Catalog/
│   └── Catalog.UnitTests/
│       └── CatalogServiceUnitTests.cs (14 tests)
├── eShopOnContainers.Ordering/
│   └── Ordering.UnitTests/
│       └── OrderingServiceUnitTests.cs (17 tests)
└── IntegrationTests/
    └── EShop.IntegrationTests/
        └── CompleteShoppingFlowTests.cs (10 tests)
```

**Total : 54 tests** (44 unitaires + 10 intégration)

---

## 🧪 Tests Unitaires

### Basket Service - 13 Tests

| # | Test | Description |
|---|------|-------------|
| 1 | `CreateBasket_ShouldCreateEmptyBasket_WhenCustomerIdIsValid` | Création d'un panier vide |
| 2 | `CreateBasket_ShouldThrowValidationException_WhenCustomerIdIsEmpty` | Validation CustomerId requis |
| 3 | `AddItemToBasket_ShouldAddNewItem_WhenItemDoesNotExist` | Ajout d'un item au panier |
| 4 | `AddItemToBasket_ShouldThrowValidationException_WhenQuantityIsNotPositive` | Validation quantité positive |
| 5 | `AddItemToBasket_ShouldThrowValidationException_WhenUnitPriceIsNotPositive` | Validation prix positif |
| 6 | `RemoveItemFromBasket_ShouldRemoveItem_WhenItemExists` | Suppression d'un item |
| 7 | `UpdateItemQuantity_ShouldUpdateQuantity_WhenItemExists` | Mise à jour de la quantité |
| 8 | `ClearBasket_ShouldRemoveAllItems_WhenBasketHasItems` | Vider le panier |
| 9 | `DeleteBasket_ShouldDeleteBasket_WhenBasketExists` | Supprimer un panier |
| 10 | `GetBasket_ShouldReturnBasket_WhenBasketExists` | Récupérer un panier par ID |
| 11 | `GetBasketByCustomer_ShouldReturnBasket_WhenCustomerHasBasket` | Récupérer par CustomerId |
| 12 | `AddItemToBasket_ShouldIncrementQuantity_WhenSameProductAddedTwice` | Incrément de quantité |
| 13 | `AddItemToBasket_ShouldThrowValidationException_WhenProductNameIsEmpty` | Validation ProductName |

**Fichier** : [`Basket.UnitTests/BasketServiceUnitTests.cs`](eShopOnContainers.Basket/Basket.UnitTests/BasketServiceUnitTests.cs)

---

### Catalog Service - 14 Tests

| # | Test | Description |
|---|------|-------------|
| 1 | `CreateCatalogItem_ShouldCreateProduct_WhenDataIsValid` | Création d'un produit |
| 2 | `CreateCatalogItem_ShouldThrowValidationException_WhenNameIsEmpty` | Validation nom requis |
| 3 | `CreateCatalogItem_ShouldThrowValidationException_WhenPriceIsNotPositive` | Validation prix positif |
| 4 | `UpdateCatalogItem_ShouldUpdateProduct_WhenProductExists` | Mise à jour d'un produit |
| 5 | `DeleteCatalogItem_ShouldDeleteProduct_WhenProductExists` | Suppression d'un produit |
| 6 | `UpdateStock_ShouldAddStock_WhenQuantityIsPositive` | Ajout de stock |
| 7 | `UpdateStock_ShouldRemoveStock_WhenQuantityIsNegative` | Retrait de stock |
| 8 | `GetCatalogItemById_ShouldReturnProduct_WhenProductExists` | Récupérer un produit par ID |
| 9 | `GetCatalogBrands_ShouldReturnAllBrands` | Récupérer toutes les marques |
| 10 | `GetCatalogTypes_ShouldReturnAllTypes` | Récupérer tous les types |
| 11 | `UpdateStock_ShouldThrowException_WhenStockBecomeNegative` | Stock ne peut être négatif |
| 12 | `CreateCatalogItem_ShouldThrowValidationException_WhenAvailableStockIsNegative` | Validation stock >= 0 |
| 13 | `UpdateDetails_ShouldRaisePriceChangedEvent_WhenPriceChanges` | Événement changement prix |
| 14 | `CreateCatalogItem_ShouldThrowValidationException_WhenMaxStockLessThanRestockThreshold` | Validation seuils stock |

**Fichier** : [`Catalog.UnitTests/CatalogServiceUnitTests.cs`](eShopOnContainers.Catalog/Catalog.UnitTests/CatalogServiceUnitTests.cs)

---

### Ordering Service - 17 Tests

| # | Test | Description |
|---|------|-------------|
| 1 | `CreateOrder_ShouldCreateOrder_WhenDataIsValid` | Création d'une commande |
| 2 | `CreateOrder_ShouldThrowValidationException_WhenCustomerIdIsEmpty` | Validation CustomerId |
| 3 | `CreateOrder_ShouldThrowValidationException_WhenEmailIsInvalid` | Validation email |
| 4 | `AddItemToOrder_ShouldAddItem_WhenOrderIsInInitialState` | Ajout d'un item |
| 5 | `AddItemToOrder_ShouldThrowValidationException_WhenQuantityIsNotPositive` | Validation quantité |
| 6 | `RemoveItemFromOrder_ShouldRemoveItem_WhenOrderIsInInitialState` | Suppression d'un item |
| 7 | `UpdateOrderItemQuantity_ShouldUpdateQuantity_WhenItemExists` | Mise à jour quantité |
| 8 | `SubmitOrder_ShouldChangeStatusToPending_WhenOrderIsInInitialState` | Soumission commande |
| 9 | `ShipOrder_ShouldChangeStatusToShipped_WhenOrderIsProcessing` | Expédition commande |
| 10 | `DeliverOrder_ShouldChangeStatusToDelivered_WhenOrderIsShipped` | Livraison commande |
| 11 | `CancelOrder_ShouldChangeStatusToCancelled_WhenOrderIsNotDelivered` | Annulation commande |
| 12 | `GetOrderById_ShouldReturnOrder_WhenOrderExists` | Récupérer par ID |
| 13 | `GetAllOrders_ShouldReturnAllOrders` | Récupérer toutes les commandes |
| 14 | `GetOrdersByCustomerId_ShouldReturnCustomerOrders` | Récupérer par CustomerId |
| 15 | `GetOrdersByStatus_ShouldReturnOrdersWithSpecificStatus` | Récupérer par statut |
| 16 | `AddItem_ShouldCalculateTotalWithDiscount_WhenDiscountIsApplied` | Calcul avec remise |
| 17 | `AddItem_ShouldThrowException_WhenOrderIsDelivered` | Immutabilité commande livrée |

**Fichier** : [`Ordering.UnitTests/OrderingServiceUnitTests.cs`](eShopOnContainers.Ordering/Ordering.UnitTests/OrderingServiceUnitTests.cs)

---

## 🔗 Tests d'Intégration

### Complete Shopping Flow - 10 Tests

Tests end-to-end simulant le parcours utilisateur complet.

| # | Test | Description | Services impliqués |
|---|------|-------------|-------------------|
| 1 | `CompleteShoppingFlow_ShouldSucceed_WhenAllStepsAreValid` | Flow complet : Browse → Cart → Order → Ship → Deliver | Catalog, Basket, Ordering |
| 2 | `CancelOrder_ShouldSucceed_WhenOrderIsNotYetShipped` | Annulation avant expédition | Ordering |
| 3 | `UpdateBasketItemQuantity_ShouldUpdateQuantity_WhenItemExists` | Mise à jour quantité dans le panier | Basket |
| 4 | `AddItemToBasket_ShouldCheckStockAvailability_BeforeAdding` | Vérification stock disponible | Catalog, Basket |
| 5 | `GetOrdersByStatus_ShouldReturnOnlyOrdersWithSpecifiedStatus` | Filtrage par statut | Ordering |
| 6 | `RemoveItemFromBasket_ShouldRemoveItem_WhenItemExists` | Suppression d'item du panier | Basket |
| 7 | `GetCatalogItemsByType_ShouldReturnOnlyItemsOfSpecifiedType` | Filtrage par catégorie | Catalog |
| 8 | `GetCatalogItemsByBrand_ShouldReturnOnlyItemsOfSpecifiedBrand` | Filtrage par marque | Catalog |
| 9 | `UpdateProductPrice_ShouldPropagateToExistingBaskets` | Propagation prix via RabbitMQ | Catalog, Basket (Event-Driven) |
| 10 | `GetBasketByCustomerId_ShouldReturnCustomerBasket` | Récupération panier client | Basket |

**Fichier** : [`IntegrationTests/EShop.IntegrationTests/CompleteShoppingFlowTests.cs`](IntegrationTests/EShop.IntegrationTests/CompleteShoppingFlowTests.cs)

### Flow du Test #1 (Happy Path complet)

```
┌─────────────────────────────────────────────────────────────────┐
│              COMPLETE SHOPPING FLOW - ÉTAPES                    │
└─────────────────────────────────────────────────────────────────┘

ÉTAPE 1: CATALOGUE - Consulter les produits
    GET /api/catalogitems
    ✓ Récupérer la liste des produits disponibles

ÉTAPE 2: PANIER - Créer un panier
    POST /api/baskets
    ✓ Créer un panier vide pour le client

ÉTAPE 3: PANIER - Ajouter des produits
    POST /api/baskets/{basketId}/items
    ✓ Ajouter 2 produits au panier

ÉTAPE 4: PANIER - Vérifier le contenu
    GET /api/baskets/{basketId}
    ✓ Confirmer que les items sont dans le panier

ÉTAPE 5: COMMANDE - Créer une commande
    POST /api/orders
    ✓ Créer une nouvelle commande

ÉTAPE 6: COMMANDE - Ajouter les items
    POST /api/orders/{orderId}/items
    ✓ Transférer les items du panier à la commande

ÉTAPE 7: COMMANDE - Soumettre
    POST /api/orders/{orderId}/submit
    ✓ Statut: Initial → Pending

ÉTAPE 8: PANIER - Vider après checkout
    DELETE /api/baskets/{basketId}/clear
    ✓ Le panier est vidé

ÉTAPE 9: COMMANDE - Expédier
    POST /api/orders/{orderId}/ship
    ✓ Statut: Pending → Shipped

ÉTAPE 10: COMMANDE - Livrer
    POST /api/orders/{orderId}/deliver
    ✓ Statut: Shipped → Delivered
    ✓ DeliveryDate est définie

ÉTAPE 11: COMMANDE - Historique client
    GET /api/orders/customer/{customerId}
    ✓ Vérifier que la commande apparaît dans l'historique
```

---

## 🚀 Exécution des tests

### Prérequis

1. **Infrastructure** - Démarrer les services via Docker Compose :
```bash
cd c:\Users\stage.pmo\Desktop\EshopOnContainerCleanArchitecture
docker-compose up -d
```

Cela démarre :
- SQL Server 2022 (Port 1433)
- RabbitMQ (Port 5672, Management UI: 15672)
- Redis (Port 6379)

2. **Microservices** - Démarrer les 3 microservices :
```bash
# Terminal 1 - Basket Service
cd eShopOnContainers.Basket\Basket.API
dotnet run

# Terminal 2 - Catalog Service
cd eShopOnContainers.Catalog\Catalog.API
dotnet run

# Terminal 3 - Ordering Service
cd eShopOnContainers.Ordering\Ordering.API
dotnet run
```

### Exécuter tous les tests

```bash
# Tous les tests
dotnet test

# Tests d'un projet spécifique
dotnet test eShopOnContainers.Basket\Basket.UnitTests\Basket.UnitTests.csproj
dotnet test eShopOnContainers.Catalog\Catalog.UnitTests\Catalog.UnitTests.csproj
dotnet test eShopOnContainers.Ordering\Ordering.UnitTests\Ordering.UnitTests.csproj
dotnet test IntegrationTests\EShop.IntegrationTests\EShop.IntegrationTests.csproj
```

### Exécuter un test spécifique

```bash
# Par nom de test
dotnet test --filter "FullyQualifiedName~CompleteShoppingFlow_ShouldSucceed"

# Par catégorie
dotnet test --filter "Category=Integration"
```

### Mode verbeux

```bash
dotnet test --verbosity detailed
```

---

## 📊 Couverture de code

### Avec Coverlet

```bash
# Installer l'outil
dotnet tool install --global coverlet.console

# Générer le rapport de couverture
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Avec ReportGenerator

```bash
# Installer l'outil
dotnet tool install --global dotnet-reportgenerator-globaltool

# Générer un rapport HTML
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage-report -reporttypes:Html
```

Ouvrir `coverage-report/index.html` dans un navigateur.

---

## 🎯 Objectifs de couverture

### Cibles par couche

| Couche | Objectif de couverture |
|--------|------------------------|
| **Domain** | 90%+ |
| **Application** (Commands/Queries) | 85%+ |
| **Infrastructure** | 70%+ |
| **API** | 60%+ |

### Métriques clés

- **Line Coverage** : Pourcentage de lignes de code exécutées
- **Branch Coverage** : Pourcentage de branches conditionnelles testées
- **Method Coverage** : Pourcentage de méthodes appelées

---

## 🧩 Patterns de tests utilisés

### 1. **Arrange-Act-Assert (AAA)**

Tous les tests suivent ce pattern :

```csharp
[Fact]
public async Task MyTest()
{
    // Arrange - Configuration
    var command = new CreateBasketCommand { CustomerId = "test" };

    // Act - Exécution
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert - Vérification
    Assert.NotNull(result);
}
```

### 2. **Mocking avec Moq**

Utilisation de mocks pour isoler les dépendances :

```csharp
var mockRepository = new Mock<IBasketRepository>();
mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(basket);
```

### 3. **Theory avec InlineData**

Tests paramétrés pour tester plusieurs cas :

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100)]
public async Task ValidateQuantity(int quantity)
{
    // Test avec différentes valeurs
}
```

---

## 🔍 Scénarios de tests importants

### Tests de validation (FluentValidation)

- CustomerId requis et non vide
- Email au format valide
- Quantité > 0
- Prix > 0
- Stock >= 0
- MaxStockThreshold > RestockThreshold

### Tests de logique métier

- Calcul du total avec remise
- Incrémentation de quantité pour produit déjà présent
- Stock ne peut devenir négatif
- Commande livrée est immutable
- Changements de statut de commande (State Machine)

### Tests d'événements de domaine

- `ProductPriceChangedDomainEvent` levé lors du changement de prix
- `BasketCheckoutDomainEvent` levé lors du checkout
- `OrderCreatedDomainEvent` levé lors de la création de commande

### Tests d'intégration asynchrone (RabbitMQ)

- Propagation du changement de prix du Catalog vers les Baskets
- Publication et consommation d'événements d'intégration

---

## 📝 Conventions de nommage

### Nom des tests

```
MethodName_ShouldExpectedBehavior_WhenStateUnderTest
```

**Exemples** :
- `CreateBasket_ShouldCreateEmptyBasket_WhenCustomerIdIsValid`
- `AddItemToBasket_ShouldThrowValidationException_WhenQuantityIsNotPositive`

### Organisation des fichiers

```
ServiceName.UnitTests/
├── ServiceNameUnitTests.cs
└── Helpers/ (si nécessaire)
```

---

## 🛠️ Outils et frameworks

| Outil | Version | Usage |
|-------|---------|-------|
| **xUnit** | 2.9.3 | Framework de tests |
| **xunit.runner.visualstudio** | 3.1.4 | Test runner Visual Studio |
| **Moq** | 4.20.72 | Mocking framework |
| **FluentValidation** | - | Validation des commandes |
| **FluentAssertions** | 7.0.0 | Assertions fluides |
| **Microsoft.AspNetCore.Mvc.Testing** | 9.0.0 | Tests d'intégration |
| **coverlet.collector** | 6.0.0 | Couverture de code |

---

## 📚 Ressources complémentaires

### Documentation officielle

- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [ASP.NET Core Integration Tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

### Architecture du projet

- Clean Architecture (Onion Architecture)
- CQRS avec MediatR
- Domain-Driven Design (DDD)
- Event-Driven Architecture avec RabbitMQ

---

## ✅ Checklist avant le push

- [ ] Tous les tests unitaires passent
- [ ] Tous les tests d'intégration passent
- [ ] Couverture de code > 80%
- [ ] Aucun test ignoré (`[Fact(Skip = "...")]`)
- [ ] Pas de `Console.WriteLine` ou `Debug.WriteLine`
- [ ] Tous les tests suivent le pattern AAA
- [ ] Noms de tests descriptifs et explicites

---

## 🐛 Dépannage

### Problème : Les tests d'intégration échouent

**Solution** : Vérifier que tous les microservices sont démarrés et accessibles :
```bash
# Vérifier Basket
curl http://localhost:5235/health

# Vérifier Catalog
curl http://localhost:5000/health

# Vérifier Ordering
curl http://localhost:5240/health
```

### Problème : Erreurs de connexion à la base de données

**Solution** : Vérifier que SQL Server est démarré via Docker :
```bash
docker ps | grep sqlserver
```

### Problème : Événements RabbitMQ non propagés

**Solution** : Vérifier que RabbitMQ fonctionne :
```bash
# Management UI
http://localhost:15672
# Username: guest, Password: guest
```

---

## 📧 Contact et support

Pour toute question ou problème avec les tests, veuillez créer une issue dans le dépôt Git du projet.

---

**Date de création** : 2025-12-24
**Dernière mise à jour** : 2025-12-24
**Version** : 1.0
