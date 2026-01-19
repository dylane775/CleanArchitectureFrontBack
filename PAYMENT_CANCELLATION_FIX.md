# Fix du Bug d'Annulation de Paiement

## 🐛 Problème Identifié

Lorsqu'un utilisateur annule un paiement sur Monetbil:
- ✅ L'utilisateur est redirigé vers la page de confirmation
- ❌ La page affiche "Commande confirmée !"
- ❌ La page affiche "Votre paiement est en cours de traitement"
- ❌ La commande reste en statut "Pending" au lieu de "Cancelled"

## 🔍 Analyse de la Cause Racine

### Problème 1: Parametres de Retour Monetbil
Monetbil redirige vers `return_url` mais **ne garantit PAS l'envoi de paramètres de statut** dans l'URL.

- URL attendue avec statut: `http://localhost:4200/checkout/confirmation/{orderId}?status=failed&payment_ref=XXX`
- URL réelle reçue: `http://localhost:4200/checkout/confirmation/{orderId}` (SANS paramètres!)

### Problème 2: Webhook Non Appelé
Le webhook Monetbil (`notify_url`) ne peut pas être appelé en localhost:
- Monetbil ne peut pas atteindre `http://localhost:5246/api/payments/webhook/monetbil`
- Nécessite ngrok ou un déploiement pour fonctionner
- Donc: Le statut du paiement n'est JAMAIS mis à jour en "Failed"

### Problème 3: Affichage Incorrect
La page de confirmation charge la commande depuis la BD et l'affiche comme "réussie" alors que:
- Le paiement est toujours en statut "Processing"
- L'utilisateur a annulé sur Monetbil
- Aucune notification n'a été reçue

## ✅ Solution Implémentée

### 1. Détection Intelligente des Annulations

**Fichier**: `eshop-web/src/app/features/checkout/confirmation/confirmation.ts`

#### A. Debug des Paramètres
```typescript
// Ajout de logs pour voir ce que Monetbil envoie réellement
console.log('Confirmation page loaded for order:', this.orderId);
console.log('Query params:', this.route.snapshot.queryParamMap.keys);
console.log('All query parameters:', allParams);
```

#### B. Support de Multiples Paramètres
```typescript
// Vérifier différents noms de paramètres possibles
const paymentStatusParam = this.route.snapshot.queryParamMap.get('status');
const monetbilStatus = this.route.snapshot.queryParamMap.get('monetbil_status');

if ((paymentStatusParam === 'failed' || paymentStatusParam === 'cancelled' ||
     monetbilStatus === 'failed' || monetbilStatus === 'cancelled') &&
    paymentReference) {
  // Marquer comme échoué
}
```

#### C. Auto-Détection par Timeout
Si le paiement reste en "Processing" après le retour de l'utilisateur:

1. **Attendre 5 secondes** - Laisser le temps au webhook de se déclencher (si ngrok actif)
2. **Vérifier le statut** - Recharger le statut du paiement
3. **Répéter 3 fois** - Attendre jusqu'à 15 secondes au total
4. **Marquer comme annulé** - Si toujours "Processing" après 15s, c'est une annulation

```typescript
private checkPaymentStatusAgain(orderId: string, paymentReference: string, attempt: number) {
  this.paymentService.getPaymentByOrderId(orderId).subscribe({
    next: (payment) => {
      if (payment.status === 'Processing' || payment.status === 'Pending') {
        if (attempt >= 3) {
          // Après 15 secondes, marquer comme annulé
          this.paymentService.failPaymentByReference(
            paymentReference,
            'Paiement annulé - L\'utilisateur est revenu sans compléter le paiement'
          ).subscribe({
            next: () => {
              this.paymentFailed.set(true);
              this.error.set('Le paiement a été annulé ou a expiré.');
              this.order.set(null);
            }
          });
        } else {
          // Réessayer dans 5 secondes
          setTimeout(() => {
            this.checkPaymentStatusAgain(orderId, paymentReference, attempt + 1);
          }, 5000);
        }
      }
    }
  });
}
```

### 2. Masquage de la Commande en Cas d'Échec

Quand un paiement échoue:
```typescript
this.paymentFailed.set(true);
this.error.set('Le paiement a échoué...');
this.order.set(null);  // ← Masquer la commande
this.loading.set(false);
```

### 3. Ordre Correct de Rendu du Template

**Fichier**: `eshop-web/src/app/features/checkout/confirmation/confirmation.html`

```html
@if (loading()) {
  <!-- Spinner -->
}
@else if (paymentFailed()) {
  <!-- ❌ Erreur de paiement - PRIORITÉ 1 -->
  <mat-card class="error-card">
    <mat-icon class="error-icon">cancel</mat-icon>
    <h2>Paiement échoué</h2>
    <p>{{ error() }}</p>
  </mat-card>
}
@else if (error()) {
  <!-- ❌ Erreur générique - PRIORITÉ 2 -->
}
@else if (order()) {
  <!-- ✅ Succès - PRIORITÉ 3 -->
  <h1>Commande confirmée !</h1>
}
```

## 🧪 Test de la Solution

### Scénario 1: Annulation Sans ngrok (Local)

1. Démarrer les services:
   ```bash
   # Terminal 1: Backend
   cd eShopOnContainers.Payment/Payment.API
   dotnet run

   # Terminal 2: Frontend
   cd eshop-web
   ng serve
   ```

2. Créer une commande et aller sur Monetbil

3. **Annuler le paiement** sur la page Monetbil

4. **Résultat attendu**:
   - Page de confirmation s'affiche
   - **Pendant 15 secondes**: Message "Votre paiement est en cours de traitement" (warning orange)
   - **Après 15 secondes**:
     - ❌ Icône "cancel"
     - Message: "Paiement échoué"
     - "Le paiement a été annulé ou a expiré. Votre commande n'a pas été validée."
     - Bouton: "Continuer mes achats"

5. **Vérifier dans la BD**:
   ```sql
   -- Le statut de la commande devrait être "Cancelled"
   SELECT OrderId, OrderStatus
   FROM ordering.Orders
   WHERE OrderId = '{votre-order-id}';

   -- Le statut du paiement devrait être "Failed"
   SELECT PaymentReference, Status, FailureReason
   FROM payment.Payments
   WHERE OrderId = '{votre-order-id}';
   ```

### Scénario 2: Annulation Avec ngrok

1. Démarrer ngrok:
   ```bash
   ngrok http 5246
   ```

2. Configurer l'URL webhook dans Monetbil dashboard:
   ```
   https://YOUR-NGROK-URL.ngrok-free.app/api/payments/webhook/monetbil
   ```

3. Créer une commande et annuler sur Monetbil

4. **Résultat attendu**:
   - Le webhook est appelé IMMÉDIATEMENT par Monetbil
   - Le statut passe à "Failed" en moins de 2 secondes
   - La page affiche l'erreur sans attendre 15 secondes

### Scénario 3: Paiement Réussi

1. Compléter le paiement sur Monetbil

2. **Résultat attendu**:
   - Webhook reçu avec statut "success"
   - Paiement passe à "Completed"
   - Commande passe à "Confirmed"
   - Page affiche: "Commande confirmée !" avec ✅ icône verte

## 📊 Logs de Debug

### Console Navigateur (F12)

Quand vous arrivez sur la page de confirmation:
```
Confirmation page loaded for order: 974FD4E4-E13D-468A-9B57-AFD94EFA3C1B
Query params: []  ou  ['status', 'payment_ref']
All query parameters: {}  ou  {status: 'failed', payment_ref: 'PAY-...'}
Payment status loaded: Processing
Payment reference: PAY-20260114-XXXXXX
Payment is still processing, will check again in 5 seconds...
If still processing after 15 seconds, will mark as cancelled
```

Après 5 secondes:
```
Payment status rechecked (attempt 1): Processing
Will check again in 5 seconds (attempt 2/3)...
```

Après 15 secondes:
```
Payment status rechecked (attempt 3): Processing
Payment still processing after 15 seconds, marking as cancelled
Payment marked as failed due to timeout
```

### Logs Backend Payment.API

Quand `failPaymentByReference` est appelé:
```
info: Payment.API.Controllers.PaymentsController[0]
      Marking payment PAY-20260114-XXXXXX as failed
info: Payment.Application.Commands.FailPayment.FailPaymentCommandHandler[0]
      Payment {PaymentId} marked as failed: Paiement annulé - L'utilisateur est revenu sans compléter le paiement
```

### Logs Backend Ordering.API

Quand PaymentFailedConsumer reçoit l'événement:
```
info: Ordering.Infrastructure.Messaging.Consumers.PaymentFailedConsumer[0]
      Received PaymentFailedEvent for order 974FD4E4-E13D-468A-9B57-AFD94EFA3C1B
info: Ordering.Application.Commands.CancelOrder.CancelOrderCommandHandler[0]
      Order {OrderId} cancelled due to payment failure
```

## 🎯 Résultat Final

| Scénario | Avant le Fix | Après le Fix |
|----------|--------------|--------------|
| Annulation Monetbil (sans ngrok) | ✅ "Commande confirmée!" + Pending | ❌ "Paiement échoué" + Cancelled |
| Annulation Monetbil (avec ngrok) | ✅ "Commande confirmée!" + Pending | ❌ "Paiement échoué" + Cancelled |
| Paiement réussi | ✅ "Commande confirmée!" + Confirmed | ✅ "Commande confirmée!" + Confirmed |

## 📝 Notes Importantes

### Délai de 15 Secondes
- **Pourquoi 15s?** Laisser le temps au webhook de se déclencher si ngrok est actif
- **Amélioration possible**: Réduire à 10s ou ajouter un bouton "J'ai annulé" pour annulation immédiate

### Ordre en Base de Données
- **Problème de design**: L'ordre est créé AVANT la confirmation du paiement
- **Conséquence**: Des ordres "orphelins" peuvent rester en BD avec statut "Pending"
- **Solution future**: Implémenter le pattern Saga pour ne créer l'ordre qu'APRÈS confirmation du paiement

### Webhook Monetbil
- **En production**: Utiliser ngrok ou déployer sur un serveur accessible
- **En développement local**: Le timeout de 15s compense l'absence de webhook
- **Signature validation**: Le webhook vérifie la signature HMAC-SHA256 pour la sécurité

## 🔗 Ressources

Pour plus d'informations sur la configuration du webhook avec ngrok, voir:
- [NGROK_WEBHOOK_SETUP.md](./NGROK_WEBHOOK_SETUP.md)
- [STABILISATION_COMPLETE.md](./STABILISATION_COMPLETE.md)

## 🎓 Leçons Apprises

1. **Ne jamais faire confiance aux redirections**: Les payment gateways ne garantissent pas toujours l'envoi de paramètres
2. **Implémenter des timeouts**: Si un paiement reste "Processing" trop longtemps après le retour de l'utilisateur, c'est suspect
3. **Webhooks > Redirections**: Les webhooks sont la source de vérité, pas les redirections utilisateur
4. **Log tout**: En cas de problème, les logs permettent de comprendre le flux réel

## ✅ Checklist de Validation

- [x] Logs ajoutés pour debug des paramètres URL
- [x] Support de multiples noms de paramètres (status, monetbil_status)
- [x] Auto-détection par timeout (15 secondes)
- [x] Appel automatique à `failPaymentByReference`
- [x] Masquage de la commande en cas d'échec (`order.set(null)`)
- [x] Ordre correct du template (paymentFailed > error > order)
- [x] Messages d'erreur clairs en français
- [x] PaymentFailedConsumer annule la commande
- [x] Documentation complète avec scénarios de test
