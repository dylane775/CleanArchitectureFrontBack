# 🎨 INTÉGRATION FRONTEND - SERVICE PAYMENT

## ✅ Modifications effectuées

### 1️⃣ **Service PaymentService** (`payment.service.ts`)

Créé un service Angular complet pour communiquer avec l'API Payment :

```typescript
// Méthodes disponibles :
- initiatePayment(request): Initie un paiement Monetbil
- getPaymentById(id): Récupère un paiement par ID
- getPaymentByOrderId(orderId): Récupère le paiement d'une commande
- getPaymentByReference(reference): Récupère par référence
- getPaymentsByCustomerId(customerId): Liste des paiements d'un client
- cancelPayment(id): Annule un paiement
```

### 2️⃣ **Configuration Environment**

Ajouté l'URL de l'API Payment dans `environment.ts` :

```typescript
paymentApiUrl: 'http://localhost:5246/api'
```

### 3️⃣ **Méthodes de paiement**

Ajouté **Monetbil** comme première option dans `order.model.ts` :

```typescript
export const PAYMENT_METHODS: PaymentMethod[] = [
  { type: 'Monetbil', label: 'Mobile Money (Monetbil)' }, // ✅ NOUVEAU
  { type: 'CreditCard', label: 'Credit Card' },
  { type: 'DebitCard', label: 'Debit Card' },
  { type: 'PayPal', label: 'PayPal' },
  { type: 'BankTransfer', label: 'Bank Transfer' },
  { type: 'CashOnDelivery', label: 'Cash on Delivery' }
];
```

### 4️⃣ **Checkout Component**

Modifié `checkout.ts` pour gérer le paiement Monetbil :

**Nouvelles méthodes :**

1. **`initiateMonetbilPayment()`** :
   - Appelle le service Payment pour créer un paiement
   - Reçoit l'URL de paiement Monetbil
   - Redirige l'utilisateur vers la page Monetbil

2. **`finalizeOrder()`** :
   - Finalise la commande pour les paiements non-Monetbil
   - Vide le panier
   - Redirige vers la page de confirmation

**Flux de checkout modifié :**

```typescript
onSubmitOrder() {
  // 1. Créer la commande
  this.orderService.checkout(checkoutRequest).subscribe({
    next: (orderId) => {
      const paymentMethod = this.paymentFormGroup.get('paymentMethod')?.value;

      if (paymentMethod === 'Monetbil') {
        // 2a. Initier le paiement Monetbil
        this.initiateMonetbilPayment(orderId, formData, basketData);
      } else {
        // 2b. Finaliser directement (Cash, etc.)
        this.finalizeOrder(basketData.customerId, orderId);
      }
    }
  });
}
```

---

## 🔄 FLUX COMPLET DE PAIEMENT MONETBIL

### Étape 1 : Checkout
```
User sur /checkout
  → Remplit formulaire (shipping + payment)
  → Sélectionne "Mobile Money (Monetbil)"
  → Clique "Place Order"
```

### Étape 2 : Création de la commande
```
Frontend Angular
  → POST /api/orders (Ordering API)
  → Reçoit orderId
```

### Étape 3 : Initiation du paiement
```
Frontend Angular
  → POST /api/payments (Payment API)
  Body: {
    orderId: "xxx",
    customerId: "xxx",
    amount: 25000,
    currency: "XAF",
    paymentProvider: "Monetbil",
    customerEmail: "user@example.com",
    customerPhone: "+237670000000",
    description: "Paiement pour la commande xxx",
    callbackUrl: "http://localhost:5246/api/payments/webhook/monetbil",
    returnUrl: "http://localhost:4200/checkout/confirmation/xxx"
  }

  → Reçoit PaymentInitiateResponse:
  {
    paymentId: "yyy",
    paymentReference: "PAY-20260113-ABC123",
    status: "Pending",
    paymentUrl: "https://monetbil.com/payment/xyz", // ← URL Monetbil
    qrCodeUrl: "https://monetbil.com/qr/xyz"
  }
```

### Étape 4 : Redirection vers Monetbil
```
Frontend Angular
  → window.location.href = paymentResponse.paymentUrl
  → User est redirigé vers la page Monetbil
```

### Étape 5 : Paiement sur Monetbil
```
User sur Monetbil
  → Choisit Orange Money / MTN / etc.
  → Entre numéro de téléphone
  → Confirme le paiement
  → Reçoit notification USSD/SMS
  → Confirme sur son téléphone
```

### Étape 6 : Callback Webhook (Backend)
```
Monetbil
  → POST http://localhost:5246/api/payments/webhook/monetbil
  Body: {
    ItemRef: "PAY-20260113-ABC123",
    TransactionId: "MONETBIL-XYZ",
    Status: "success",
    Message: "Payment completed"
  }

Payment API
  → Trouve le paiement via PaymentReference
  → Appelle payment.MarkAsCompleted()
  → Publie PaymentCompletedEvent (RabbitMQ)

Ordering API (Consumer)
  → Écoute PaymentCompletedEvent
  → Met à jour le statut de la commande → "Confirmed"
```

### Étape 7 : Redirection de retour (Frontend)
```
Monetbil
  → Redirige user vers returnUrl
  → http://localhost:4200/checkout/confirmation/orderId

Confirmation Component
  → Affiche les détails de la commande
  → Affiche le statut du paiement
  → "Votre paiement est en cours de traitement..."
  → (Optionnel) Poll l'API pour vérifier le statut
```

---

## 📋 URLS IMPORTANTES

### Frontend Angular
```
http://localhost:4200/checkout
http://localhost:4200/checkout/confirmation/{orderId}
http://localhost:4200/checkout/payment-failed
```

### Backend APIs
```
# Payment API
http://localhost:5246/api/payments
http://localhost:5246/api/payments/webhook/monetbil
http://localhost:5246/swagger

# Ordering API
http://localhost:5240/api/orders
```

### Monetbil
```
# Dashboard
https://www.monetbil.com/dashboard

# Page de paiement (générée dynamiquement)
https://monetbil.com/payment/{paymentToken}
```

---

## 🧪 TESTER LE FLUX

### 1. Démarrer les services backend

```bash
# Terminal 1 - Identity
cd eShopOnContainers.Identity/Identity.API
dotnet run

# Terminal 2 - Catalog
cd eShopOnContainers.Catalog/Catalog.API
dotnet run

# Terminal 3 - Basket
cd eShopOnContainers.Basket/Basket.API
dotnet run

# Terminal 4 - Ordering
cd eShopOnContainers.Ordering/Ordering.API
dotnet run

# Terminal 5 - Payment
cd eShopOnContainers.Payment/Payment.API
dotnet run
```

### 2. Démarrer le frontend

```bash
cd eshop-web
npm start
```

### 3. Test complet

1. Aller sur `http://localhost:4200`
2. Se connecter (ou continuer en guest)
3. Ajouter des produits au panier
4. Aller au checkout
5. **Sélectionner "Mobile Money (Monetbil)"**
6. Remplir les informations
7. Cliquer "Place Order"
8. **Vérifier la redirection vers Monetbil**
9. (En sandbox) Tester avec les numéros de test Monetbil

---

## ⚠️ POINTS IMPORTANTS

### 1. Webhook URL
Pour que Monetbil puisse appeler ton webhook en développement local, tu dois :

**Option A : Utiliser ngrok**
```bash
ngrok http 5246
```
Puis mettre l'URL ngrok dans le dashboard Monetbil :
```
https://abc123.ngrok.io/api/payments/webhook/monetbil
```

**Option B : Déployer en production**
```
https://ton-domaine.com/api/payments/webhook/monetbil
```

### 2. Configuration Monetbil Dashboard

Dans ton dashboard Monetbil, configure :

- **URL de redirection (succès)** : `http://localhost:4200/checkout/confirmation`
- **URL de redirection (échec)** : `http://localhost:4200/checkout/payment-failed`
- **URL de notification** : `https://XXXXX.ngrok.io/api/payments/webhook/monetbil`
- **Méthode** : `POST`

### 3. Clés API

Les clés sont déjà configurées dans `appsettings.json` :
```json
"MonetbilSettings": {
  "ServiceKey": "tHUIvKmpo6QvAikF4rLUArUs6nSvfofA",
  "ServiceSecret": "s2I2h3bbxJWI48enmD7tFjWzQpQQRL3rANbC3UIZVJVAarRUbPZzUW4e5FVpH7gl",
  "UseSandbox": true
}
```

---

## 🎉 RÉSUMÉ

L'intégration frontend pour le paiement Monetbil est **complète** !

✅ Service Angular créé
✅ Checkout modifié pour gérer Monetbil
✅ Redirection automatique vers Monetbil
✅ Gestion du retour utilisateur
✅ Monetbil ajouté comme première option de paiement

**Prochaine étape** : Tester le flux complet avec ngrok + Monetbil sandbox ! 🚀
