# Guide de Tests des Événements - eShop Microservices

Ce guide vous permet de tester le système d'événements (Domain Events + Integration Events) dans votre architecture microservices.

## 📋 Table des matières

- [Architecture des événements](#architecture-des-événements)
- [Prérequis](#prérequis)
- [Tests du service Ordering](#tests-du-service-ordering)
- [Tests du service Catalog](#tests-du-service-catalog)
- [Tests du service Basket](#tests-du-service-basket)
- [Vérification RabbitMQ](#vérification-rabbitmq)
- [Scénarios de tests complets](#scénarios-de-tests-complets)

---

## 🎯 Architecture des événements

### Flow des événements

```
┌─────────────────────────────────────────────────────────────────┐
│                    ARCHITECTURE DES ÉVÉNEMENTS                   │
└─────────────────────────────────────────────────────────────────┘

1. Action utilisateur (API Call)
      ↓
2. Command Handler exécute la logique métier
      ↓
3. Entité de domaine lève un Domain Event
      ↓
4. UnitOfWork.SaveChangesAsync() dispatche les Domain Events via MediatR
      ↓
5. Domain Event Handler intercepte l'événement
      ↓
6. Transformation en Integration Event
      ↓
7. Publication vers RabbitMQ via MassTransit
      ↓
8. Autres microservices consomment l'événement (si consumer configuré)
```

### Types d'événements

| Type | Scope | Transport | Exemple |
|------|-------|-----------|---------|
| **Domain Event** | Interne au service | MediatR (in-process) | `OrderCreatedDomainEvent` |
| **Integration Event** | Entre services | RabbitMQ (MassTransit) | `OrderCreatedIntegrationEvent` |

---

## 🚀 Prérequis

### 1. Démarrer l'infrastructure Docker

```bash
cd c:\Users\stage.pmo\Desktop\EshopOnContainerCleanArchitecture
docker-compose up -d
```

Vérifier que les services sont démarrés:
```bash
docker ps
```

Vous devriez voir:
- `sqlserver` (Port 1433)
- `rabbitmq` (Ports 5672, 15672)
- `redis` (Port 6379)

### 2. Démarrer les microservices

**Terminal 1 - Ordering Service:**
```bash
cd eShopOnContainers.Ordering\Ordering.API
dotnet run
```
L'API démarre sur: http://localhost:5240

**Terminal 2 - Catalog Service (optionnel):**
```bash
cd eShopOnContainers.Catalog\Catalog.API
dotnet run
```

**Terminal 3 - Basket Service (optionnel):**
```bash
cd eShopOnContainers.Basket\Basket.API
dotnet run
```

### 3. Vérifier les services

```bash
# Ordering Service
curl http://localhost:5240/health

# Catalog Service
curl http://localhost:5000/health

# Basket Service
curl http://localhost:5235/health
```

---

## 📦 Tests du service Ordering

Le service Ordering publie **4 types d'événements** via le système Domain Events → Integration Events.

### Événements du service Ordering

| Domain Event | Integration Event | Déclenché par |
|--------------|-------------------|---------------|
| `OrderCreatedDomainEvent` | `OrderCreatedIntegrationEvent` | Création d'une commande |
| `OrderSubmittedDomainEvent` | `OrderSubmittedIntegrationEvent` | Soumission d'une commande |
| `OrderShippedDomainEvent` | `OrderShippedIntegrationEvent` | Expédition d'une commande |
| `OrderCancelledDomainEvent` | `OrderCancelledIntegrationEvent` | Annulation d'une commande |

### Test 1: Événement OrderCreated

**Objectif:** Vérifier que la création d'une commande déclenche bien les événements.

**Requête HTTP:**
```bash
curl -X POST http://localhost:5240/api/Orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "customerEmail": "test@example.com",
    "customerPhone": "+33612345678",
    "shippingAddress": "123 Rue de Paris, 75001 Paris, France",
    "billingAddress": "123 Rue de Paris, 75001 Paris, France",
    "paymentMethod": "CreditCard",
    "items": [
      {
        "catalogItemId": "660e8400-e29b-41d4-a716-446655440000",
        "productName": "iPhone 15 Pro",
        "unitPrice": 1299.99,
        "quantity": 1,
        "pictureUrl": "iphone15.jpg",
        "discount": 0
      }
    ]
  }'
```

**Logs attendus:**
```
info: Ordering.API.Controllers.OrdersController[0]
      Creating order for customer 550e8400-e29b-41d4-a716-446655440000

info: Ordering.Application.Common.Behaviors.LoggingBehavior[0]
      Handling CreateOrderCommand - Request: CreateOrderCommand { ... }

info: Ordering.Application.Common.Behaviors.TransactionBehavior[0]
      Executing command CreateOrderCommand with transaction

info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (...)
      INSERT INTO [Orders] ...

info: Ordering.Infrastructure.Messaging.DomainEventHandlers.OrderCreatedDomainEventHandler[0]
      Handling Domain Event: OrderCreated - OrderId: {...}

info: Ordering.Infrastructure.Messaging.DomainEventHandlers.OrderCreatedDomainEventHandler[0]
      Published Integration Event: OrderCreated - OrderId: {...}

info: Ordering.API.Controllers.OrdersController[0]
      Order {...} created successfully
```

**Vérification:**
- ✅ Domain Event Handler est exécuté
- ✅ Integration Event est publié
- ✅ Code HTTP 201 Created reçu

**Note:** Conservez l'`orderId` retourné pour les tests suivants.

---

### Test 2: Événement OrderSubmitted

**Objectif:** Vérifier que la soumission d'une commande déclenche les événements.

**Requête HTTP:**
```bash
# Remplacez {orderId} par l'ID obtenu au test précédent
curl -X POST http://localhost:5240/api/Orders/{orderId}/submit \
  -H "Content-Type: application/json"
```

**Exemple:**
```bash
curl -X POST http://localhost:5240/api/Orders/91c41cb7-3ddc-47c0-bc18-87e2a1f16430/submit \
  -H "Content-Type: application/json"
```

**Logs attendus:**
```
info: Ordering.API.Controllers.OrdersController[0]
      Submitting order {orderId}

info: Ordering.Infrastructure.Messaging.DomainEventHandlers.OrderSubmittedDomainEventHandler[0]
      Handling Domain Event: OrderSubmitted - OrderId: {orderId}

info: Ordering.Infrastructure.Messaging.DomainEventHandlers.OrderSubmittedDomainEventHandler[0]
      Published Integration Event: OrderSubmitted - OrderId: {orderId}

info: Ordering.API.Controllers.OrdersController[0]
      Order {orderId} submitted
```

**Vérification:**
- ✅ Statut de la commande passe de `Pending` à `Processing`
- ✅ Domain Event Handler est exécuté
- ✅ Integration Event est publié
- ✅ Code HTTP 204 No Content

---

### Test 3: Événement OrderShipped

**Objectif:** Vérifier que l'expédition d'une commande déclenche les événements.

**Requête HTTP:**
```bash
# Remplacez {orderId} par l'ID de votre commande
curl -X POST http://localhost:5240/api/Orders/{orderId}/ship \
  -H "Content-Type: application/json"
```

**Logs attendus:**
```
info: Ordering.API.Controllers.OrdersController[0]
      Shipping order {orderId}

info: Ordering.Infrastructure.Messaging.DomainEventHandlers.OrderShippedDomainEventHandler[0]
      Handling Domain Event: OrderShipped - OrderId: {orderId}, ShippingAddress: {...}

info: Ordering.Infrastructure.Messaging.DomainEventHandlers.OrderShippedDomainEventHandler[0]
      Published Integration Event: OrderShipped - OrderId: {orderId}

info: Ordering.API.Controllers.OrdersController[0]
      Order {orderId} shipped
```

**Vérification:**
- ✅ Statut de la commande passe de `Processing` à `Shipped`
- ✅ Domain Event Handler est exécuté
- ✅ Integration Event publié avec `ShippingAddress`
- ✅ Code HTTP 204 No Content

---

### Test 4: Événement OrderCancelled

**Objectif:** Vérifier que l'annulation d'une commande déclenche les événements.

**Important:** Pour tester ce scénario, créez d'abord une nouvelle commande (Test 1), puis annulez-la **avant** de l'expédier.

**Requête HTTP:**
```bash
# Créer une nouvelle commande
curl -X POST http://localhost:5240/api/Orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "customerEmail": "test2@example.com",
    "customerPhone": "+33612345678",
    "shippingAddress": "456 Avenue Victor Hugo, 75016 Paris, France",
    "billingAddress": "456 Avenue Victor Hugo, 75016 Paris, France",
    "paymentMethod": "PayPal",
    "items": [
      {
        "catalogItemId": "660e8400-e29b-41d4-a716-446655440001",
        "productName": "Samsung Galaxy S24",
        "unitPrice": 899.99,
        "quantity": 1,
        "pictureUrl": "galaxy-s24.jpg",
        "discount": 0.05
      }
    ]
  }'

# Puis annuler la commande
curl -X POST http://localhost:5240/api/Orders/{newOrderId}/cancel \
  -H "Content-Type: application/json" \
  -d '{
    "reason": "Client a changé d avis"
  }'
```

**Logs attendus:**
```
info: Ordering.API.Controllers.OrdersController[0]
      Cancelling order {orderId}

info: Ordering.Infrastructure.Messaging.DomainEventHandlers.OrderCancelledDomainEventHandler[0]
      Handling Domain Event: OrderCancelled - OrderId: {orderId}, Reason: Client a changé d'avis

info: Ordering.Infrastructure.Messaging.DomainEventHandlers.OrderCancelledDomainEventHandler[0]
      Published Integration Event: OrderCancelled - OrderId: {orderId}

info: Ordering.API.Controllers.OrdersController[0]
      Order {orderId} cancelled
```

**Vérification:**
- ✅ Statut de la commande passe à `Cancelled`
- ✅ Domain Event Handler est exécuté
- ✅ Integration Event publié avec la raison d'annulation
- ✅ Code HTTP 204 No Content

---

## 🛍️ Tests du service Catalog

Le service Catalog publie des événements lors des changements de prix.

### Événements du service Catalog

| Domain Event | Integration Event | Déclenché par |
|--------------|-------------------|---------------|
| `ProductPriceChangedDomainEvent` | `ProductPriceChangedIntegrationEvent` | Changement de prix d'un produit |

### Test: Événement ProductPriceChanged

**Étape 1: Créer un produit**
```bash
curl -X POST http://localhost:5000/api/catalogitems \
  -H "Content-Type: application/json" \
  -d '{
    "name": "MacBook Pro M3",
    "description": "Laptop professionnel Apple",
    "price": 2499.99,
    "pictureFileName": "macbook-m3.jpg",
    "catalogTypeId": 1,
    "catalogBrandId": 1,
    "availableStock": 10,
    "restockThreshold": 5,
    "maxStockThreshold": 50
  }'
```

**Étape 2: Modifier le prix**
```bash
# Remplacez {catalogItemId} par l'ID du produit créé
curl -X PUT http://localhost:5000/api/catalogitems/{catalogItemId} \
  -H "Content-Type: application/json" \
  -d '{
    "name": "MacBook Pro M3",
    "description": "Laptop professionnel Apple - PROMO",
    "price": 2199.99,
    "pictureFileName": "macbook-m3.jpg",
    "catalogTypeId": 1,
    "catalogBrandId": 1
  }'
```

**Logs attendus:**
```
info: Catalog.Infrastructure.Messaging.DomainEventHandlers.ProductPriceChangedDomainEventHandler[0]
      Handling Domain Event: ProductPriceChanged - ProductId: {catalogItemId}, OldPrice: 2499.99, NewPrice: 2199.99

info: Catalog.Infrastructure.Messaging.DomainEventHandlers.ProductPriceChangedDomainEventHandler[0]
      Published Integration Event: ProductPriceChanged - ProductId: {catalogItemId}
```

**Vérification:**
- ✅ Domain Event Handler exécuté
- ✅ Integration Event publié avec ancien et nouveau prix
- ✅ Si Basket Service écoute, les paniers sont mis à jour automatiquement

---

## 🛒 Tests du service Basket

Le service Basket peut **consommer** des événements du Catalog (ProductPriceChanged) pour mettre à jour les prix dans les paniers.

### Consumer du service Basket

| Integration Event consommé | Action |
|---------------------------|--------|
| `ProductPriceChangedIntegrationEvent` | Met à jour le prix dans tous les paniers contenant ce produit |

### Test: Propagation du changement de prix

**Prérequis:** Basket Service doit être démarré et configuré avec un consumer.

**Scénario complet:**

1. **Créer un panier avec un produit**
```bash
# Créer un panier
curl -X POST http://localhost:5235/api/baskets \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000"
  }'

# Ajouter un produit au panier
curl -X POST http://localhost:5235/api/baskets/{basketId}/items \
  -H "Content-Type: application/json" \
  -d '{
    "catalogItemId": "660e8400-e29b-41d4-a716-446655440000",
    "productName": "MacBook Pro M3",
    "unitPrice": 2499.99,
    "quantity": 1,
    "pictureUrl": "macbook-m3.jpg"
  }'
```

2. **Vérifier le panier**
```bash
curl http://localhost:5235/api/baskets/{basketId}
```

Résultat attendu:
```json
{
  "id": "...",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "items": [
    {
      "catalogItemId": "660e8400-e29b-41d4-a716-446655440000",
      "productName": "MacBook Pro M3",
      "unitPrice": 2499.99,
      "quantity": 1
    }
  ],
  "totalAmount": 2499.99
}
```

3. **Changer le prix dans le Catalog**
```bash
curl -X PUT http://localhost:5000/api/catalogitems/660e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "MacBook Pro M3",
    "description": "Laptop professionnel Apple - PROMO",
    "price": 2199.99,
    ...
  }'
```

4. **Vérifier que le panier est automatiquement mis à jour**
```bash
curl http://localhost:5235/api/baskets/{basketId}
```

Résultat attendu après quelques secondes:
```json
{
  "id": "...",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "items": [
    {
      "catalogItemId": "660e8400-e29b-41d4-a716-446655440000",
      "productName": "MacBook Pro M3",
      "unitPrice": 2199.99,  ← Prix mis à jour automatiquement
      "quantity": 1
    }
  ],
  "totalAmount": 2199.99  ← Total recalculé
}
```

**Vérification:**
- ✅ Le prix dans le panier est automatiquement mis à jour
- ✅ Le total est recalculé
- ✅ Événement RabbitMQ consommé avec succès

---

## 🐰 Vérification RabbitMQ

### Accéder à l'interface RabbitMQ

Ouvrez votre navigateur: http://localhost:15672

**Credentials:**
- Username: `guest`
- Password: `guest`

### Vérifier les exchanges

Allez dans l'onglet **Exchanges**. Vous devriez voir:

| Exchange | Type | Description |
|----------|------|-------------|
| `OrderCreatedIntegrationEvent` | fanout | Événement de création de commande |
| `OrderSubmittedIntegrationEvent` | fanout | Événement de soumission |
| `OrderShippedIntegrationEvent` | fanout | Événement d'expédition |
| `OrderCancelledIntegrationEvent` | fanout | Événement d'annulation |
| `ProductPriceChangedIntegrationEvent` | fanout | Événement de changement de prix |

### Vérifier les queues

Allez dans l'onglet **Queues**.

Vous devriez voir des queues pour chaque consumer configuré, par exemple:
- `basket-service-product-price-changed` (si consumer configuré)

### Publier manuellement un message de test (optionnel)

1. Allez dans l'onglet **Exchanges**
2. Cliquez sur `OrderShippedIntegrationEvent`
3. Section **Publish message**
4. Entrez dans **Payload**:
```json
{
  "orderId": "91c41cb7-3ddc-47c0-bc18-87e2a1f16430",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "shippingAddress": "123 Rue Test, 75001 Paris"
}
```
5. Cliquez sur **Publish message**

Si un consumer écoute, il devrait traiter le message.

---

## 🎬 Scénarios de tests complets

### Scénario 1: Parcours complet d'une commande

**Objectif:** Tester tous les événements du cycle de vie d'une commande.

```bash
# 1. Créer une commande
ORDER_ID=$(curl -s -X POST http://localhost:5240/api/Orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "customerEmail": "complete-flow@example.com",
    "customerPhone": "+33612345678",
    "shippingAddress": "789 Boulevard Haussmann, 75008 Paris",
    "billingAddress": "789 Boulevard Haussmann, 75008 Paris",
    "paymentMethod": "CreditCard",
    "items": [
      {
        "catalogItemId": "660e8400-e29b-41d4-a716-446655440000",
        "productName": "iPad Air",
        "unitPrice": 699.99,
        "quantity": 2,
        "pictureUrl": "ipad-air.jpg",
        "discount": 0.1
      }
    ]
  }' | jq -r '.id')

echo "Order created: $ORDER_ID"

# Attendre 2 secondes
sleep 2

# 2. Soumettre la commande
curl -X POST http://localhost:5240/api/Orders/$ORDER_ID/submit
echo "\nOrder submitted"

# Attendre 2 secondes
sleep 2

# 3. Expédier la commande
curl -X POST http://localhost:5240/api/Orders/$ORDER_ID/ship
echo "\nOrder shipped"

# Attendre 2 secondes
sleep 2

# 4. Livrer la commande
curl -X POST http://localhost:5240/api/Orders/$ORDER_ID/deliver
echo "\nOrder delivered"

# 5. Vérifier la commande finale
curl http://localhost:5240/api/Orders/$ORDER_ID | jq
```

**Événements déclenchés (dans l'ordre):**
1. ✅ `OrderCreatedIntegrationEvent`
2. ✅ `OrderSubmittedIntegrationEvent`
3. ✅ `OrderShippedIntegrationEvent`
4. ✅ (Pas d'événement pour deliver dans la config actuelle)

---

### Scénario 2: Test d'annulation

```bash
# 1. Créer une commande
ORDER_ID=$(curl -s -X POST http://localhost:5240/api/Orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "customerEmail": "cancel-test@example.com",
    "customerPhone": "+33612345678",
    "shippingAddress": "123 Test Street",
    "billingAddress": "123 Test Street",
    "paymentMethod": "PayPal",
    "items": [
      {
        "catalogItemId": "660e8400-e29b-41d4-a716-446655440000",
        "productName": "Test Product",
        "unitPrice": 99.99,
        "quantity": 1,
        "pictureUrl": "test.jpg",
        "discount": 0
      }
    ]
  }' | jq -r)

echo "Order created: $ORDER_ID"
sleep 2

# 2. Annuler immédiatement
curl -X POST http://localhost:5240/api/Orders/$ORDER_ID/cancel \
  -H "Content-Type: application/json" \
  -d '{
    "reason": "Test d annulation - stock insuffisant"
  }'

echo "\nOrder cancelled"

# 3. Vérifier le statut
curl http://localhost:5240/api/Orders/$ORDER_ID | jq '.orderStatus'
```

**Événements déclenchés:**
1. ✅ `OrderCreatedIntegrationEvent`
2. ✅ `OrderCancelledIntegrationEvent` (avec raison)

---

### Scénario 3: Propagation de prix Catalog → Basket

**Prérequis:** Basket Service démarré avec consumer configuré.

```bash
# 1. Créer un produit dans le Catalog
PRODUCT_ID=$(curl -s -X POST http://localhost:5000/api/catalogitems \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Price Propagation",
    "description": "Produit de test",
    "price": 100.00,
    "pictureFileName": "test.jpg",
    "catalogTypeId": 1,
    "catalogBrandId": 1,
    "availableStock": 100,
    "restockThreshold": 10,
    "maxStockThreshold": 200
  }' | jq -r '.id')

echo "Product created: $PRODUCT_ID"

# 2. Créer un panier avec ce produit
BASKET_ID=$(curl -s -X POST http://localhost:5235/api/baskets \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000"
  }' | jq -r '.id')

curl -X POST http://localhost:5235/api/baskets/$BASKET_ID/items \
  -H "Content-Type: application/json" \
  -d "{
    \"catalogItemId\": \"$PRODUCT_ID\",
    \"productName\": \"Test Price Propagation\",
    \"unitPrice\": 100.00,
    \"quantity\": 2,
    \"pictureUrl\": \"test.jpg\"
  }"

echo "\nBasket created with product"

# 3. Vérifier le prix initial
curl http://localhost:5235/api/baskets/$BASKET_ID | jq '.items[0].unitPrice'
# Devrait afficher: 100.00

# 4. Changer le prix dans le Catalog
curl -X PUT http://localhost:5000/api/catalogitems/$PRODUCT_ID \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Price Propagation",
    "description": "Produit de test - PROMO",
    "price": 79.99,
    "pictureFileName": "test.jpg",
    "catalogTypeId": 1,
    "catalogBrandId": 1
  }'

echo "\nPrice changed in catalog to 79.99"

# 5. Attendre la propagation (quelques secondes)
sleep 5

# 6. Vérifier le nouveau prix dans le panier
curl http://localhost:5235/api/baskets/$BASKET_ID | jq '.items[0].unitPrice'
# Devrait afficher: 79.99
```

**Événements déclenchés:**
1. ✅ `ProductPriceChangedIntegrationEvent` (publié par Catalog)
2. ✅ Basket Consumer traite l'événement
3. ✅ Prix mis à jour automatiquement dans le panier

---

## 📊 Tableau récapitulatif des tests

| Test # | Service | Événement | Endpoint | Code HTTP | Vérification |
|--------|---------|-----------|----------|-----------|--------------|
| 1 | Ordering | OrderCreated | POST /api/Orders | 201 | Logs + RabbitMQ |
| 2 | Ordering | OrderSubmitted | POST /api/Orders/{id}/submit | 204 | Logs + RabbitMQ |
| 3 | Ordering | OrderShipped | POST /api/Orders/{id}/ship | 204 | Logs + RabbitMQ |
| 4 | Ordering | OrderCancelled | POST /api/Orders/{id}/cancel | 204 | Logs + RabbitMQ |
| 5 | Catalog | ProductPriceChanged | PUT /api/catalogitems/{id} | 204 | Logs + RabbitMQ |
| 6 | Basket | Consumer | (automatique) | - | Basket mis à jour |

---

## ✅ Checklist de validation

Après chaque test, vérifiez:

- [ ] Le log indique "Handling Domain Event: ..."
- [ ] Le log indique "Published Integration Event: ..."
- [ ] Code HTTP de retour correct (201, 204, etc.)
- [ ] RabbitMQ affiche le message dans l'exchange correspondant
- [ ] Si consumer configuré, le message est consommé

---

## 🐛 Dépannage

### Problème: Aucun log "Handling Domain Event"

**Cause:** Les Domain Event Handlers ne sont pas enregistrés dans MediatR.

**Solution:** Vérifiez que dans `Program.cs` vous avez:
```csharp
builder.Services.AddApplication(typeof(Ordering.Infrastructure.DependencyInjection).Assembly);
```

### Problème: Domain Event Handler s'exécute mais pas "Published Integration Event"

**Cause:** MassTransit n'est pas correctement configuré ou RabbitMQ n'est pas démarré.

**Solution:**
```bash
# Vérifier RabbitMQ
docker ps | grep rabbitmq

# Redémarrer RabbitMQ si nécessaire
docker-compose restart rabbitmq
```

### Problème: Messages dans RabbitMQ mais pas consommés

**Cause:** Aucun consumer configuré pour écouter cet événement.

**Solution:** Créez un consumer dans le service destinataire:
```csharp
public class OrderShippedConsumer : IConsumer<OrderShippedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderShippedIntegrationEvent> context)
    {
        var message = context.Message;
        // Traiter l'événement
    }
}
```

---

## 📚 Ressources

- [Architecture Documentation](ARCHITECTURE_DOCUMENTATION.md)
- [Test Documentation](TEST_DOCUMENTATION.md)
- [MassTransit Documentation](https://masstransit.io/)
- [RabbitMQ Management](http://localhost:15672)

---

**Date de création:** 2025-12-25
**Auteur:** Claude Code
**Version:** 1.0
