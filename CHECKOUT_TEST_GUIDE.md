# 🧪 Guide de Test - Système de Checkout

## 🚀 Démarrage Rapide

### 1. Lancer les Services

```bash
# Terminal 1 - Backend Ordering Service
cd eShopOnContainers.Ordering/Ordering.API
dotnet run

# Terminal 2 - Backend Basket Service
cd eShopOnContainers.Basket/Basket.API
dotnet run

# Terminal 3 - Backend Catalog Service
cd eShopOnContainers.Catalog/Catalog.API
dotnet run

# Terminal 4 - Frontend Angular
cd eshop-web
npm start
```

### 2. URLs à Vérifier

- Frontend: http://localhost:4200
- Ordering API: http://localhost:5239
- Basket API: http://localhost:5235
- Catalog API: http://localhost:5237

---

## ✅ Test Checklist

### **Test 1: Checkout en tant que Guest** ⭐ PRIORITAIRE

**Objectif:** Vérifier que les utilisateurs non connectés peuvent passer commande.

#### Étapes:

1. ✅ **Ouvrir en navigation privée**
   - Ouvrir http://localhost:4200 en mode incognito
   - Vérifier que vous n'êtes PAS connecté

2. ✅ **Ajouter des produits au panier**
   - Aller sur `/catalog`
   - Ajouter 2-3 produits différents
   - Vérifier que le badge panier se met à jour

3. ✅ **Accéder au panier**
   - Cliquer sur l'icône panier dans le header
   - Vérifier que tous les produits sont affichés
   - Vérifier les calculs (sous-total, shipping, tax, total)

4. ✅ **Cliquer sur "Proceed to Checkout"**
   - **Attendu:** Redirection vers `/checkout`
   - **Attendu:** Stepper Material affiché avec 3 étapes

5. ✅ **Étape 1: Remplir les informations de livraison**
   - First Name: "John"
   - Last Name: "Doe"
   - Email: "john.doe@example.com"
   - Phone: "+1234567890" (optionnel)
   - Shipping Street: "123 Main Street"
   - Shipping City: "New York"
   - Shipping State: "NY"
   - Shipping ZIP: "10001"
   - Shipping Country: "USA"
   - Cocher "Same as shipping address"
   - **Cliquer "Next"**

6. ✅ **Étape 2: Choisir la méthode de paiement**
   - **Option A:** Sélectionner "Cash on Delivery"
     - **Attendu:** Aucun champ supplémentaire
     - **Attendu:** Message info affiché
   - **Option B:** Sélectionner "Credit Card"
     - **Attendu:** Formulaire carte apparaît
     - Cardholder Name: "John Doe"
     - Card Number: "4111111111111111"
     - Expiry: "12/25"
     - CVV: "123"
   - **Cliquer "Next"**

7. ✅ **Étape 3: Vérifier le récapitulatif**
   - **Attendu:** Informations de livraison affichées correctement
   - **Attendu:** Méthode de paiement affichée
   - **Attendu:** Liste des produits avec images
   - **Attendu:** Totaux calculés correctement
   - **Cliquer "Place Order"**

8. ✅ **Confirmation de commande**
   - **Attendu:** Spinner pendant le traitement
   - **Attendu:** Redirection vers `/checkout/confirmation/:orderId`
   - **Attendu:** Icône de succès animée ✓
   - **Attendu:** Numéro de commande affiché
   - **Attendu:** Tous les détails présents
   - **Attendu:** Statut = "Pending"

9. ✅ **Vérifier le panier**
   - Retourner au panier
   - **Attendu:** Panier vide (supprimé après checkout)

#### ✅ Critères de Succès:
- [ ] Checkout accessible sans authentification
- [ ] Tous les champs validés correctement
- [ ] Commande créée avec customerId = guestBasketId
- [ ] Redirection vers confirmation
- [ ] Panier vidé automatiquement

---

### **Test 2: Checkout en tant qu'Utilisateur Connecté**

**Objectif:** Vérifier l'expérience utilisateur authentifié.

#### Étapes:

1. ✅ **Se connecter**
   - Aller sur `/auth/login`
   - Se connecter avec un compte existant

2. ✅ **Ajouter des produits**
   - Ajouter 2-3 produits au panier
   - Vérifier le panier

3. ✅ **Accéder au checkout**
   - **Attendu:** Email, prénom, nom **pré-remplis** automatiquement

4. ✅ **Compléter le checkout**
   - Remplir uniquement l'adresse
   - Choisir paiement
   - Place Order

5. ✅ **Vérifier dans "Orders"**
   - Aller sur `/orders`
   - **Attendu:** Commande apparaît dans la liste

#### ✅ Critères de Succès:
- [ ] Pré-remplissage des données utilisateur
- [ ] Commande créée avec customerId = userId
- [ ] Commande visible dans /orders

---

### **Test 3: Validation des Formulaires**

**Objectif:** Vérifier que la validation fonctionne correctement.

#### Tests à Effectuer:

1. ✅ **Champs requis - Étape 1**
   - Laisser tous les champs vides
   - Essayer de cliquer "Next"
   - **Attendu:** Erreurs affichées sous chaque champ
   - **Attendu:** Impossible de passer à l'étape 2

2. ✅ **Validation Email**
   - Entrer "invalid-email"
   - **Attendu:** Erreur "Valid email is required"
   - Corriger avec "test@example.com"
   - **Attendu:** Erreur disparaît

3. ✅ **Same as Billing Checkbox**
   - Cocher "Same as billing"
   - **Attendu:** Champs billing grisés/masqués
   - Décocher
   - **Attendu:** Champs billing apparaissent et sont requis

4. ✅ **Validation Paiement**
   - Sélectionner "Credit Card"
   - Laisser champs carte vides
   - Essayer "Next"
   - **Attendu:** Erreurs sur les champs carte
   - Sélectionner "PayPal"
   - **Attendu:** Validation carte désactivée

#### ✅ Critères de Succès:
- [ ] Validation en temps réel
- [ ] Messages d'erreur clairs
- [ ] Navigation bloquée si formulaire invalide

---

### **Test 4: Méthodes de Paiement**

**Objectif:** Tester toutes les options de paiement.

#### Tests:

1. ✅ **Credit Card**
   - Sélectionner
   - **Attendu:** Formulaire carte visible et requis

2. ✅ **Debit Card**
   - Sélectionner
   - **Attendu:** Formulaire carte visible et requis

3. ✅ **PayPal**
   - Sélectionner
   - **Attendu:** Message "You will be redirected to PayPal"
   - **Attendu:** Pas de champs carte

4. ✅ **Bank Transfer**
   - Sélectionner
   - **Attendu:** Message "Details will be provided after confirmation"

5. ✅ **Cash on Delivery**
   - Sélectionner
   - **Attendu:** Message "Pay with cash when delivered"

#### ✅ Critères de Succès:
- [ ] Toutes les méthodes affichées
- [ ] Formulaires conditionnels fonctionnent
- [ ] Icônes appropriées affichées

---

### **Test 5: Calculs Financiers**

**Objectif:** Vérifier que les calculs sont corrects.

#### Scénario:

1. ✅ **Ajouter au panier:**
   - Produit A: 50 FCFA × 2 = 100 FCFA
   - Produit B: 75 FCFA × 1 = 75 FCFA

2. ✅ **Vérifier dans le panier:**
   - Subtotal: **175 FCFA** ✓
   - Shipping: **0 FCFA** (> 100 FCFA) ✓
   - Tax (10%): **17.5 FCFA** → affiché comme **18 FCFA** (arrondi) ✓
   - Total: **193 FCFA** ✓

3. ✅ **Vérifier dans le checkout (Étape 3):**
   - Mêmes calculs affichés

4. ✅ **Vérifier dans la confirmation:**
   - Subtotal: 175 FCFA
   - Shipping: FREE
   - Tax: 18 FCFA
   - Total: 193 FCFA

#### ✅ Critères de Succès:
- [ ] Calculs cohérents partout
- [ ] Livraison gratuite si > 100 FCFA
- [ ] Tax = 10% du subtotal

---

### **Test 6: Responsive Design**

**Objectif:** Vérifier le comportement mobile.

#### Tests:

1. ✅ **Ouvrir DevTools**
   - F12 → Mode mobile (375×667)

2. ✅ **Vérifier le checkout:**
   - **Attendu:** Formulaire en 1 colonne
   - **Attendu:** Boutons pleine largeur
   - **Attendu:** Images produits adaptées

3. ✅ **Vérifier la confirmation:**
   - **Attendu:** Layout adapté
   - **Attendu:** Boutons empilés verticalement

#### ✅ Critères de Succès:
- [ ] Pas de scrollbar horizontal
- [ ] Tous les éléments visibles
- [ ] Boutons facilement cliquables

---

### **Test 7: Gestion d'Erreurs**

**Objectif:** Tester la robustesse.

#### Scénarios:

1. ✅ **Panier vide au checkout**
   - Vider le panier
   - Aller sur `/checkout` directement
   - Essayer de placer la commande
   - **Attendu:** Snackbar "Your basket is empty"

2. ✅ **Backend down**
   - Arrêter l'API Ordering
   - Essayer de placer une commande
   - **Attendu:** Snackbar d'erreur
   - **Attendu:** Utilisateur reste sur la page
   - **Attendu:** Peut réessayer

3. ✅ **Commande inexistante**
   - Aller sur `/checkout/confirmation/00000000-0000-0000-0000-000000000000`
   - **Attendu:** Message "Failed to load order details"

#### ✅ Critères de Succès:
- [ ] Erreurs gérées gracieusement
- [ ] Messages clairs pour l'utilisateur
- [ ] Pas de crash de l'application

---

### **Test 8: Navigation**

**Objectif:** Tester le flux de navigation.

#### Tests:

1. ✅ **Bouton "Back" du stepper**
   - Étape 2 → Cliquer "Back"
   - **Attendu:** Retour à l'étape 1
   - **Attendu:** Données conservées

2. ✅ **Boutons de la confirmation**
   - "Continue Shopping" → `/catalog`
   - "View All Orders" → `/orders`
   - "Print Order" → Ouvre dialogue d'impression

3. ✅ **Navigation directe**
   - Aller sur `/checkout` sans panier
   - **Attendu:** Formulaire affiché mais erreur au submit

#### ✅ Critères de Succès:
- [ ] Navigation fluide
- [ ] Données conservées lors du "Back"
- [ ] Redirections correctes

---

## 📊 Résultats Attendus

### ✅ **SUCCÈS si:**

- [ ] Guest peut commander sans compte
- [ ] User connecté a données pré-remplies
- [ ] Toutes les étapes du stepper fonctionnent
- [ ] Validation formulaires opérationnelle
- [ ] Calculs financiers corrects
- [ ] Panier supprimé après checkout
- [ ] Page confirmation affiche tous les détails
- [ ] Responsive mobile fonctionne
- [ ] Erreurs gérées proprement

### ❌ **ÉCHEC si:**

- Backend ne répond pas (erreur 500/404)
- Formulaires ne valident pas
- Calculs incorrects
- Panier non supprimé
- Redirection confirmation échoue
- Crash de l'application

---

## 🐛 Problèmes Connus et Solutions

### **Problème 1: CORS Error**

**Symptôme:** `Access to XMLHttpRequest blocked by CORS policy`

**Solution:**
```csharp
// Dans Program.cs de chaque API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

app.UseCors("AllowAll");
```

### **Problème 2: customerId not found**

**Symptôme:** Erreur 404 lors de la création de commande

**Cause:** Le customerId (userId ou guestBasketId) n'existe pas dans la base

**Solution:** Vérifier que le panier a été créé avant le checkout

### **Problème 3: JWT Token expiré**

**Symptôme:** 401 Unauthorized pour utilisateur connecté

**Solution:** Se reconnecter ou implémenter le refresh token automatique

---

## 📝 Checklist Finale

Avant de considérer le checkout comme validé:

- [ ] Test 1: Guest checkout ✅
- [ ] Test 2: User checkout ✅
- [ ] Test 3: Validation ✅
- [ ] Test 4: Méthodes paiement ✅
- [ ] Test 5: Calculs ✅
- [ ] Test 6: Responsive ✅
- [ ] Test 7: Erreurs ✅
- [ ] Test 8: Navigation ✅
- [ ] Documentation à jour
- [ ] Pas de console errors

---

## 🎯 Prochaines Étapes après Tests

1. **Si succès:**
   - Marquer le checkout comme ✅ Production-ready
   - Passer aux features suivantes (Footer, Avis clients)

2. **Si échecs:**
   - Noter les bugs dans un fichier BUGS.md
   - Prioriser les corrections
   - Ré-tester après corrections

---

**Bonne chance avec les tests ! 🚀**
