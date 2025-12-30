# Configuration de l'envoi d'emails

Ce document explique comment configurer l'envoi d'emails pour le service Identity.

## Options de configuration

Le service supporte plusieurs providers d'email:
- **SMTP** (Gmail, Outlook, etc.)
- **SendGrid** (à implémenter)

## Configuration SMTP avec Gmail

### Étape 1: Activer l'authentification à deux facteurs (2FA)

1. Allez sur votre compte Google: https://myaccount.google.com/
2. Sécurité → Validation en deux étapes
3. Activez la validation en deux étapes

### Étape 2: Générer un mot de passe d'application

1. Allez sur: https://myaccount.google.com/apppasswords
2. Sélectionnez "Autre (nom personnalisé)"
3. Entrez "eShop Identity Service"
4. Cliquez sur "Générer"
5. **Copiez le mot de passe généré** (16 caractères sans espaces)

### Étape 3: Configurer appsettings.json

Modifiez le fichier `Identity.API/appsettings.json`:

```json
{
  "EmailSettings": {
    "Provider": "SMTP",
    "FromEmail": "votre-email@gmail.com",
    "FromName": "eShop - Service d'Identité",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "votre-email@gmail.com",
    "SmtpPassword": "VOTRE_MOT_DE_PASSE_APPLICATION",
    "EnableSsl": true,
    "ConfirmEmailUrl": "http://localhost:5245/api/Auth/confirm-email",
    "ResetPasswordUrl": "http://localhost:5245/api/Auth/reset-password"
  }
}
```

**Remplacez:**
- `votre-email@gmail.com` par votre vrai email Gmail
- `VOTRE_MOT_DE_PASSE_APPLICATION` par le mot de passe d'application généré (16 caractères)

## Configuration SMTP avec Outlook/Hotmail

```json
{
  "EmailSettings": {
    "Provider": "SMTP",
    "FromEmail": "votre-email@outlook.com",
    "FromName": "eShop - Service d'Identité",
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SmtpUsername": "votre-email@outlook.com",
    "SmtpPassword": "votre-mot-de-passe",
    "EnableSsl": true,
    "ConfirmEmailUrl": "http://localhost:5245/api/Auth/confirm-email",
    "ResetPasswordUrl": "http://localhost:5245/api/Auth/reset-password"
  }
}
```

## Configuration SMTP avec un serveur personnalisé

```json
{
  "EmailSettings": {
    "Provider": "SMTP",
    "FromEmail": "noreply@votredomaine.com",
    "FromName": "Votre Entreprise",
    "SmtpHost": "smtp.votredomaine.com",
    "SmtpPort": 587,
    "SmtpUsername": "votre-username",
    "SmtpPassword": "votre-mot-de-passe",
    "EnableSsl": true,
    "ConfirmEmailUrl": "https://votredomaine.com/api/Auth/confirm-email",
    "ResetPasswordUrl": "https://votredomaine.com/api/Auth/reset-password"
  }
}
```

## Configuration pour le développement local

Si vous ne voulez pas configurer d'email réel pendant le développement, vous pouvez:

### Option 1: Utiliser Mailtrap (recommandé pour le dev)

1. Créez un compte gratuit sur https://mailtrap.io
2. Créez une inbox de test
3. Utilisez les credentials fournis:

```json
{
  "EmailSettings": {
    "Provider": "SMTP",
    "FromEmail": "test@eshop.com",
    "FromName": "eShop Test",
    "SmtpHost": "sandbox.smtp.mailtrap.io",
    "SmtpPort": 2525,
    "SmtpUsername": "votre-username-mailtrap",
    "SmtpPassword": "votre-password-mailtrap",
    "EnableSsl": true,
    "ConfirmEmailUrl": "http://localhost:5245/api/Auth/confirm-email",
    "ResetPasswordUrl": "http://localhost:5245/api/Auth/reset-password"
  }
}
```

### Option 2: Papercut SMTP (serveur local)

1. Téléchargez Papercut: https://github.com/ChangemakerStudios/Papercut-SMTP
2. Lancez Papercut
3. Configuration:

```json
{
  "EmailSettings": {
    "Provider": "SMTP",
    "FromEmail": "test@eshop.com",
    "FromName": "eShop Test",
    "SmtpHost": "localhost",
    "SmtpPort": 25,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "EnableSsl": false,
    "ConfirmEmailUrl": "http://localhost:5245/api/Auth/confirm-email",
    "ResetPasswordUrl": "http://localhost:5245/api/Auth/reset-password"
  }
}
```

## Emails envoyés par le système

Le service envoie automatiquement ces emails:

### 1. Confirmation d'email (lors de l'inscription)
- **Quand**: Après l'inscription d'un nouvel utilisateur
- **Contenu**: Lien de confirmation avec token
- **Action**: L'utilisateur doit cliquer pour confirmer son email

### 2. Email de bienvenue (après confirmation)
- **Quand**: Après la confirmation de l'email
- **Contenu**: Message de bienvenue
- **Action**: Aucune action requise

### 3. Réinitialisation de mot de passe
- **Quand**: L'utilisateur demande un reset de mot de passe
- **Contenu**: Lien de réinitialisation avec token
- **Action**: L'utilisateur clique pour réinitialiser

## Test de la configuration

### 1. Créer un utilisateur

```bash
POST http://localhost:5245/api/Auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test@123456",
  "firstName": "John",
  "lastName": "Doe"
}
```

### 2. Vérifier les logs

Vous devriez voir dans les logs:
```
info: Sending email to test@example.com with subject: Confirmez votre adresse email - eShop
info: Email sent successfully to test@example.com
```

### 3. Vérifier votre boîte email

- Avec Gmail/Outlook: Vérifiez votre boîte de réception
- Avec Mailtrap: Vérifiez votre inbox Mailtrap
- Avec Papercut: Vérifiez l'interface Papercut

## Dépannage

### Erreur: "Authentication failed"

**Cause**: Mauvais username/password ou 2FA non activé (Gmail)

**Solution**:
1. Vérifiez que vous utilisez un mot de passe d'application (pas votre mot de passe Gmail)
2. Vérifiez que la 2FA est activée
3. Générez un nouveau mot de passe d'application

### Erreur: "Unable to connect to the remote server"

**Cause**: Mauvais host ou port

**Solution**:
- Gmail: `smtp.gmail.com:587`
- Outlook: `smtp-mail.outlook.com:587`
- Vérifiez votre firewall

### Erreur: "Mailbox unavailable"

**Cause**: L'adresse "From" n'est pas autorisée

**Solution**:
- Pour Gmail, le `FromEmail` doit être votre adresse Gmail
- Ou une adresse que vous avez ajoutée dans "Envoyer un e-mail en tant que"

### Les emails arrivent dans les spams

**Solution**:
1. Ajoutez l'expéditeur à vos contacts
2. En production, configurez SPF, DKIM et DMARC pour votre domaine
3. Utilisez un service professionnel comme SendGrid en production

## Recommandations pour la production

⚠️ **Ne pas utiliser Gmail en production!**

Pour la production, utilisez un service professionnel:

1. **SendGrid** (recommandé)
   - 100 emails/jour gratuits
   - Bonne délivrabilité
   - Analytics inclus

2. **AWS SES** (Amazon Simple Email Service)
   - Très économique
   - Haute délivrabilité
   - Intégration AWS

3. **Mailgun**
   - API simple
   - Bon support

4. **Postmark**
   - Spécialisé dans les emails transactionnels
   - Excellente délivrabilité

## Sécurité

⚠️ **IMPORTANT**: Ne commitez JAMAIS vos credentials dans Git!

Utilisez:
- **appsettings.Development.json** (git-ignored)
- **Variables d'environnement**
- **Azure Key Vault** ou **AWS Secrets Manager** en production
- **User Secrets** pour le développement local

Exemple avec User Secrets:
```bash
cd Identity.API
dotnet user-secrets set "EmailSettings:SmtpUsername" "votre-email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "votre-mot-de-passe"
```
