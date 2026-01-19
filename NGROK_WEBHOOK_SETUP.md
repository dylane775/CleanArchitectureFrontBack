# Configuration du Webhook Monetbil avec ngrok

## 🎯 Objectif

Exposer l'API Payment (localhost:5246) sur Internet pour que Monetbil puisse appeler le webhook.

## 📋 Prérequis

1. **ngrok installé** - Télécharger sur https://ngrok.com/download
2. **Compte ngrok** (gratuit) - Inscription sur https://dashboard.ngrok.com/signup
3. **Payment.API en cours d'exécution** sur port 5246

## 🚀 Étapes de Configuration

### 1. Installer ngrok

#### Windows (via Chocolatey)
```bash
choco install ngrok
```

#### Ou téléchargement manuel
1. Télécharger depuis https://ngrok.com/download
2. Extraire le fichier zip
3. Placer `ngrok.exe` dans un dossier accessible (ex: `C:\ngrok`)

### 2. Configurer l'authtoken ngrok

```bash
ngrok config add-authtoken YOUR_AUTH_TOKEN_HERE
```

Récupérer votre token sur: https://dashboard.ngrok.com/get-started/your-authtoken

### 3. Démarrer l'API Payment

```bash
cd eShopOnContainers.Payment/Payment.API
dotnet run
```

Vérifier que l'API tourne bien sur http://localhost:5246

### 4. Exposer le port avec ngrok

Dans un nouveau terminal:

```bash
ngrok http 5246
```

**Sortie attendue:**
```
ngrok

Session Status                online
Account                       Your Name (Plan: Free)
Version                       3.x.x
Region                        United States (us)
Latency                       45ms
Web Interface                 http://127.0.0.1:4040
Forwarding                    https://XXXX-XX-XX-XXX-XX.ngrok-free.app -> http://localhost:5246

Connections                   ttl     opn     rt1     rt5     p50     p90
                             0       0       0.00    0.00    0.00    0.00
```

### 5. Noter l'URL publique

L'URL publique sera quelque chose comme:
```
https://abc123def456.ngrok-free.app
```

Cette URL change à chaque redémarrage de ngrok (sauf avec un plan payant).

## 🔧 Configuration dans le Dashboard Monetbil

### 1. Se connecter au dashboard Monetbil

https://www.monetbil.com/login

### 2. Aller dans les paramètres du service

**Navigation:** Dashboard → Settings → Services → Votre Service

### 3. Configurer les URLs

#### URL de notification (Webhook)
```
https://VOTRE_URL_NGROK.ngrok-free.app/api/payments/webhook/monetbil
```

Exemple:
```
https://abc123def456.ngrok-free.app/api/payments/webhook/monetbil
```

#### URL de retour (Success)
```
http://localhost:4200/checkout/confirmation/{order_id}
```

#### URL de retour (Failure)
```
http://localhost:4200/checkout/confirmation/{order_id}
```

**Note:** Les URLs de retour peuvent rester en localhost car elles redirigent le navigateur de l'utilisateur, pas le serveur Monetbil.

### 4. Sauvegarder les paramètres

Cliquer sur **Enregistrer** ou **Save**.

## 🧪 Tester le Webhook

### 1. Créer un paiement de test

1. Aller sur http://localhost:4200
2. Ajouter un produit au panier
3. Aller au checkout
4. Choisir "Mobile Money (Monetbil)"
5. Compléter le paiement

### 2. Vérifier les logs ngrok

Dans le terminal ngrok, vous devriez voir:

```
HTTP Requests
-------------

POST /api/payments/webhook/monetbil    200 OK
```

### 3. Interface Web ngrok

Ouvrir http://127.0.0.1:4040 pour voir:
- Toutes les requêtes HTTP
- Les headers
- Le body des requêtes
- Les réponses

Très utile pour déboguer!

### 4. Vérifier les logs Payment.API

Dans le terminal Payment.API:

```
info: Payment.API.Controllers.PaymentsController[0]
      Received Monetbil webhook for reference PAY-20260113-XXXXXX
info: Payment.API.Controllers.PaymentsController[0]
      Monetbil webhook signature validated successfully
info: Payment.API.Controllers.PaymentsController[0]
      Payment {PaymentId} confirmed via webhook
```

## ⚠️ Validation de la Signature

Le webhook vérifie maintenant la signature HMAC-SHA256 de Monetbil pour garantir l'authenticité.

### Header attendu
```
X-Monetbil-Signature: abc123def456...
```

### Si la signature est invalide

```json
{
  "message": "Invalid signature"
}
```

Status: 401 Unauthorized

### Structure de la signature

```
HMAC-SHA256(payload, ServiceSecret)
```

Où:
- `payload` = JSON du webhook
- `ServiceSecret` = Votre clé secrète Monetbil

## 🔒 Sécurité

### ✅ Validations implémentées

1. **Signature HMAC-SHA256** - Vérifie que le webhook vient bien de Monetbil
2. **Timing-safe comparison** - Protection contre les timing attacks
3. **Logs de sécurité** - Alerte en cas de signature invalide

### ⚠️ Important

- Ne JAMAIS exposer votre `ServiceSecret` dans le code
- Toujours vérifier la signature avant de traiter le webhook
- Logger les tentatives d'accès non autorisées

## 🐛 Dépannage

### Problème: ngrok affiche "Failed to complete tunnel connection"

**Solution:** Vérifier que le port 5246 est bien libre
```bash
netstat -ano | findstr :5246
```

### Problème: Monetbil ne peut pas atteindre le webhook

**Vérifications:**
1. ✅ ngrok est bien lancé et affiche une URL HTTPS
2. ✅ L'URL dans le dashboard Monetbil est correcte
3. ✅ Payment.API tourne bien
4. ✅ Pas de firewall bloquant ngrok

### Problème: Signature invalide

**Vérifications:**
1. ✅ `ServiceSecret` correct dans appsettings.json
2. ✅ Header `X-Monetbil-Signature` présent
3. ✅ Payload du webhook correspond exactement

**Astuce:** Utiliser l'interface ngrok (http://127.0.0.1:4040) pour voir le header exact envoyé par Monetbil.

### Problème: URL ngrok change à chaque restart

**Solution:** Plan ngrok payant ($8/mois) pour URL fixe, ou:
- Noter la nouvelle URL à chaque démarrage
- Mettre à jour dans Monetbil dashboard
- Utiliser un script pour automatiser

## 📝 Notes Importantes

### Plan gratuit ngrok

- ✅ HTTPS inclus
- ✅ Pas de limite de bande passante
- ❌ URL change à chaque restart
- ❌ 1 tunnel simultané max

### Alternative: Déploiement

En production, déployez sur un serveur avec une vraie URL:
- Azure App Service
- AWS EC2
- DigitalOcean
- Heroku

Et configurez l'URL directement dans Monetbil (ex: `https://api.votresite.com/api/payments/webhook/monetbil`)

## 📚 Ressources

- Documentation ngrok: https://ngrok.com/docs
- Dashboard ngrok: https://dashboard.ngrok.com
- Documentation Monetbil: https://www.monetbil.com/developer
- Support Monetbil: support@monetbil.com
