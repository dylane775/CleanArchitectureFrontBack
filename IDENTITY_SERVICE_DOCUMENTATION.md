# Documentation du Service Identity - eShop Microservices

Service d'authentification et de gestion des utilisateurs pour l'architecture microservices eShopOnContainers.

## 📋 Table des matières

- [Vue d'ensemble](#vue-densemble)
- [Architecture](#architecture)
- [Entités du domaine](#entités-du-domaine)
- [API Endpoints](#api-endpoints)
- [Authentification JWT](#authentification-jwt)
- [Configuration](#configuration)
- [Démarrage](#démarrage)
- [Tests](#tests)

---

## 🎯 Vue d'ensemble

Le service Identity gère:
- **Authentification** - Login, logout, refresh token
- **Enregistrement** - Inscription des nouveaux utilisateurs
- **Gestion des utilisateurs** - Profils, rôles, permissions
- **Tokens JWT** - Génération et validation des tokens d'accès
- **Événements** - Publication d'événements d'intégration vers RabbitMQ

### Informations du service

| Propriété | Valeur |
|-----------|--------|
| **Port HTTP** | 5245 |
| **Port HTTPS** | 7245 |
| **Base de données** | IdentityDb (SQL Server) |
| **Architecture** | Clean Architecture + DDD |
| **Patterns** | CQRS, Repository, Domain Events |
| **Framework** | .NET 9.0 |

---

## 🏗️ Architecture

### Structure du projet

```
eShopOnContainers.Identity/
├── Identity.Domain/              # Couche Domaine (Entities, Events, Exceptions)
│   ├── Common/
│   │   ├── BaseEntity.cs
│   │   └── BaseDomainEvent.cs
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   └── RefreshToken.cs
│   ├── Events/
│   │   ├── UserRegisteredDomainEvent.cs
│   │   ├── UserLoggedInDomainEvent.cs
│   │   └── EmailConfirmedDomainEvent.cs
│   └── Exceptions/
│       └── IdentityDomainException.cs
│
├── Identity.Application/         # Couche Application (CQRS, DTOs, Interfaces)
│   ├── Commands/
│   │   ├── Register/
│   │   ├── Login/
│   │   ├── RefreshToken/
│   │   ├── ConfirmEmail/
│   │   ├── ChangePassword/
│   │   └── UpdateProfile/
│   ├── Queries/
│   │   ├── GetUserById/
│   │   ├── GetUserByEmail/
│   │   ├── GetAllUsers/
│   │   └── GetUserRoles/
│   ├── DTOs/
│   │   ├── Input/
│   │   └── Output/
│   ├── Common/
│   │   ├── Interfaces/
│   │   ├── Behaviors/
│   │   └── Mappings/
│   └── DependencyInjection.cs
│
├── Identity.Infrastructure/      # Couche Infrastructure (EF Core, Services)
│   ├── Data/
│   │   ├── Configurations/
│   │   └── IdentityDbContext.cs
│   ├── Services/
│   │   ├── TokenService.cs
│   │   ├── PasswordHasher.cs
│   │   ├── CurrentUserService.cs
│   │   └── JwtSettings.cs
│   ├── Messaging/
│   │   ├── Events/
│   │   └── DomainEventHandlers/
│   └── DependencyInjection.cs
│
├── Identity.API/                 # Couche API (Controllers, Middleware)
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── UsersController.cs
│   ├── Middlewares/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── appsettings.json
│   └── Program.cs
│
└── Identity.UnitTests/           # Tests unitaires
```

---

## 👥 Entités du domaine

### User (Utilisateur)

Entité principale représentant un utilisateur du système.

**Propriétés:**
- `Id` (Guid) - Identifiant unique
- `Email` (string) - Email unique (requis)
- `PasswordHash` (string) - Hash BCrypt du mot de passe
- `FirstName` (string) - Prénom
- `LastName` (string) - Nom
- `PhoneNumber` (string) - Numéro de téléphone
- `IsEmailConfirmed` (bool) - Email confirmé
- `IsActive` (bool) - Compte actif
- `LastLoginAt` (DateTime?) - Dernière connexion

**Relations:**
- `RefreshTokens` - Collection de tokens de rafraîchissement
- `Roles` - Collection de rôles assignés

**Méthodes métier:**
- `UpdateProfile()` - Met à jour le profil
- `ChangePassword()` - Change le mot de passe
- `ConfirmEmail()` - Confirme l'email
- `Activate()` / `Deactivate()` - Active/désactive le compte
- `RecordLogin()` - Enregistre une connexion
- `AddRole()` / `RemoveRole()` - Gestion des rôles
- `AddRefreshToken()` / `RevokeRefreshToken()` - Gestion des tokens

**Événements de domaine:**
- `UserRegisteredDomainEvent` - Levé à l'inscription
- `UserLoggedInDomainEvent` - Levé à la connexion
- `EmailConfirmedDomainEvent` - Levé à la confirmation d'email

### Role (Rôle)

Représente un rôle utilisateur avec permissions.

**Propriétés:**
- `Id` (Guid) - Identifiant unique
- `Name` (string) - Nom du rôle (unique)
- `Description` (string) - Description
- `Permissions` (string) - Permissions en JSON

**Rôles système:**
- `Admin` - Administrateur
- `Customer` - Client
- `Manager` - Gestionnaire

**Méthodes métier:**
- `UpdateRole()` - Met à jour le rôle
- `AddPermission()` / `RemovePermission()` - Gestion des permissions
- `HasPermission()` - Vérifie une permission

### RefreshToken (Token de rafraîchissement)

Token pour renouveler les tokens d'accès JWT.

**Propriétés:**
- `Id` (Guid) - Identifiant unique
- `Token` (string) - Token unique
- `UserId` (Guid) - Utilisateur propriétaire
- `ExpiresAt` (DateTime) - Date d'expiration
- `CreatedByIp` (string) - IP de création
- `RevokedAt` (DateTime?) - Date de révocation
- `RevokedByIp` (string?) - IP de révocation
- `ReplacedByToken` (string?) - Token de remplacement

**Propriétés calculées:**
- `IsExpired` - Token expiré
- `IsRevoked` - Token révoqué
- `IsActive` - Token valide

**Méthodes métier:**
- `Revoke()` - Révoque le token
- `CanBeUsed()` - Vérifie la validité
- `GetRemainingTime()` - Temps restant

---

## 🔌 API Endpoints

### AuthController (`/api/Auth`)

Endpoints d'authentification (accès public).

#### 1. Enregistrer un utilisateur
```http
POST /api/Auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecureP@ssw0rd",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+33612345678"
}

Response: 201 Created
{
  "id": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### 2. Se connecter
```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecureP@ssw0rd",
  "ipAddress": "192.168.1.1"
}

Response: 200 OK
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "2f9d8e7c6b5a4...",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "roles": ["Customer"],
  "expiresAt": "2025-12-28T13:00:00Z"
}
```

#### 3. Rafraîchir le token
```http
POST /api/Auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "2f9d8e7c6b5a4...",
  "ipAddress": "192.168.1.1"
}

Response: 200 OK
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "new-token-here...",
  ...
}
```

#### 4. Confirmer l'email
```http
POST /api/Auth/confirm-email
Content-Type: application/json

{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "confirmationToken": "abc123..."
}

Response: 204 No Content
```

#### 5. Se déconnecter
```http
POST /api/Auth/logout
Authorization: Bearer {accessToken}

Response: 204 No Content
```

### UsersController (`/api/Users`)

Endpoints de gestion des utilisateurs (nécessite authentification).

#### 1. Obtenir l'utilisateur actuel
```http
GET /api/Users/me
Authorization: Bearer {accessToken}

Response: 200 OK
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+33612345678",
  "isEmailConfirmed": true,
  "isActive": true,
  "roles": ["Customer"],
  "createdAt": "2025-12-25T10:00:00Z",
  "lastLoginAt": "2025-12-28T12:00:00Z"
}
```

#### 2. Obtenir un utilisateur par ID
```http
GET /api/Users/{id}
Authorization: Bearer {accessToken}

Response: 200 OK
{UserDto}
```

#### 3. Obtenir tous les utilisateurs (paginé)
```http
GET /api/Users?page=1&pageSize=10&isActive=true
Authorization: Bearer {accessToken}

Response: 200 OK
[
  {UserDto},
  {UserDto},
  ...
]
```

#### 4. Mettre à jour le profil
```http
PUT /api/Users/me/profile
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+33698765432"
}

Response: 204 No Content
```

#### 5. Changer le mot de passe
```http
PUT /api/Users/me/password
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "currentPassword": "OldP@ssw0rd",
  "newPassword": "NewSecureP@ssw0rd"
}

Response: 204 No Content
```

#### 6. Obtenir les rôles d'un utilisateur
```http
GET /api/Users/{id}/roles
Authorization: Bearer {accessToken}

Response: 200 OK
[
  {
    "id": "role-guid",
    "name": "Customer",
    "description": "Standard customer role",
    "permissions": ["read:products", "write:orders"]
  }
]
```

---

## 🔐 Authentification JWT

### Configuration JWT

Le service utilise JSON Web Tokens (JWT) pour l'authentification stateless.

**Paramètres (appsettings.json):**
```json
{
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-characters-long-for-security",
    "Issuer": "IdentityService",
    "Audience": "eShopOnContainers",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Structure du Token

**Claims inclus dans le JWT:**
- `sub` - User ID (Subject)
- `email` - Email de l'utilisateur
- `userId` - ID utilisateur (custom claim)
- `role` - Rôles de l'utilisateur (multiple)
- `iss` - Issuer (IdentityService)
- `aud` - Audience (eShopOnContainers)
- `exp` - Expiration timestamp
- `iat` - Issued at timestamp

**Exemple de token décodé:**
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "role": ["Customer"],
  "iss": "IdentityService",
  "aud": "eShopOnContainers",
  "exp": 1735398000,
  "iat": 1735394400
}
```

### Mécanisme de Refresh Token

1. L'utilisateur se connecte → Reçoit `accessToken` + `refreshToken`
2. Utilise `accessToken` pour les requêtes API
3. Quand `accessToken` expire → Appel `/api/Auth/refresh-token` avec `refreshToken`
4. Reçoit nouveau `accessToken` + nouveau `refreshToken`
5. Ancien `refreshToken` est révoqué automatiquement

**Sécurité:**
- Les refresh tokens sont stockés en base de données
- Suivi de l'IP de création et révocation
- Révocation en cascade (si un token parent est révoqué)
- Expiration configurable (7 jours par défaut)

---

## ⚙️ Configuration

### Base de données

**Connection String (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "IdentityDb": "Server=localhost,1433;Database=IdentityDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### RabbitMQ

**Configuration (appsettings.json):**
```json
{
  "RabbitMQSettings": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Événements d'intégration

Le service publie ces événements vers RabbitMQ:

| Événement | Déclenché par | Données |
|-----------|---------------|---------|
| `UserRegisteredIntegrationEvent` | Inscription | UserId, Email, FirstName, LastName |
| `UserLoggedInIntegrationEvent` | Connexion | UserId, Email, IpAddress, LoginTime |
| `EmailConfirmedIntegrationEvent` | Confirmation email | UserId, Email, ConfirmedAt |

---

## 🚀 Démarrage

### Prérequis

1. **Démarrer l'infrastructure Docker:**
```bash
cd c:\Users\stage.pmo\Desktop\EshopOnContainerCleanArchitecture
docker-compose up -d
```

Cela démarre:
- SQL Server (port 1433)
- RabbitMQ (ports 5672, 15672)
- Redis (port 6379)

2. **Vérifier les services:**
```bash
docker ps
```

### Démarrer le service Identity

```bash
cd eShopOnContainers.Identity\Identity.API
dotnet run
```

Le service démarre sur:
- **HTTP:** http://localhost:5245
- **Swagger UI:** http://localhost:5245/swagger

### Migration de la base de données

La migration est appliquée automatiquement au démarrage. Pour la faire manuellement:

```bash
cd eShopOnContainers.Identity\Identity.Infrastructure
dotnet ef database update --startup-project ../Identity.API/Identity.API.csproj
```

### Initialisation des données

Pour créer les rôles par défaut et un utilisateur admin, vous pouvez:

1. **Via Swagger UI:**
   - Aller sur http://localhost:5245/swagger
   - Exécuter `POST /api/Auth/register` pour créer des utilisateurs

2. **Via script SQL:** (optionnel)
```sql
USE IdentityDb;

-- Créer les rôles par défaut
INSERT INTO Roles (Id, Name, Description, Permissions, CreatedAt, CreatedBy, IsDeleted)
VALUES
  (NEWID(), 'Admin', 'Administrator role', '["*"]', GETUTCDATE(), 'system', 0),
  (NEWID(), 'Customer', 'Standard customer role', '["read:products","write:orders"]', GETUTCDATE(), 'system', 0),
  (NEWID(), 'Manager', 'Manager role', '["read:*","write:products"]', GETUTCDATE(), 'system', 0);
```

---

## 🧪 Tests

### Scénario de test complet

#### 1. Enregistrer un nouvel utilisateur

```bash
curl -X POST http://localhost:5245/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "SecureP@ssw0rd123",
    "firstName": "Test",
    "lastName": "User",
    "phoneNumber": "+33612345678"
  }'
```

**Réponse attendue:**
```json
{
  "id": "new-user-guid-here"
}
```

#### 2. Se connecter

```bash
curl -X POST http://localhost:5245/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "SecureP@ssw0rd123",
    "ipAddress": "127.0.0.1"
  }'
```

**Conservez le `accessToken` et `refreshToken` de la réponse.**

#### 3. Obtenir le profil utilisateur

```bash
curl -X GET http://localhost:5245/api/Users/me \
  -H "Authorization: Bearer {accessToken}"
```

#### 4. Mettre à jour le profil

```bash
curl -X PUT http://localhost:5245/api/Users/me/profile \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Updated",
    "lastName": "Name",
    "phoneNumber": "+33698765432"
  }'
```

#### 5. Changer le mot de passe

```bash
curl -X PUT http://localhost:5245/api/Users/me/password \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "SecureP@ssw0rd123",
    "newPassword": "NewP@ssw0rd456"
  }'
```

#### 6. Rafraîchir le token

```bash
curl -X POST http://localhost:5245/api/Auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "{refreshToken}",
    "ipAddress": "127.0.0.1"
  }'
```

### Vérifier les événements dans RabbitMQ

1. Ouvrir http://localhost:15672 (login: guest/guest)
2. Aller dans l'onglet **Exchanges**
3. Vérifier les exchanges:
   - `UserRegisteredIntegrationEvent`
   - `UserLoggedInIntegrationEvent`
   - `EmailConfirmedIntegrationEvent`

### Health Check

```bash
curl http://localhost:5245/health
```

**Réponse:**
```json
{
  "status": "Healthy",
  "service": "Identity API",
  "timestamp": "2025-12-28T12:00:00Z",
  "database": "Connected"
}
```

---

## 📊 Validation des mots de passe

### Règles de sécurité

Les mots de passe doivent respecter:
- **Longueur minimale:** 8 caractères
- **Au moins 1 majuscule** (A-Z)
- **Au moins 1 minuscule** (a-z)
- **Au moins 1 chiffre** (0-9)
- **Au moins 1 caractère spécial** (@, #, $, %, etc.)

**Implémenté dans:** `IPasswordHasher.IsPasswordStrong()`

---

## 🔒 Sécurité

### Protection implémentée

1. **Hachage des mots de passe:**
   - BCrypt avec work factor 12
   - Jamais de stockage en clair

2. **Tokens sécurisés:**
   - JWT avec signature HMAC-SHA256
   - Refresh tokens cryptographiquement sécurisés
   - Expiration automatique

3. **Protection contre les attaques:**
   - Validation stricte des entrées (FluentValidation)
   - Protection CORS configurable
   - HTTPS redirection
   - Middleware de gestion d'erreurs global

4. **Audit:**
   - Traçabilité complète (Created, Modified, Deleted)
   - Soft delete (IsDeleted)
   - Enregistrement des IP pour les tokens

---

## 📚 Ressources

- [Architecture Documentation](ARCHITECTURE_DOCUMENTATION.md)
- [Test Documentation](TEST_DOCUMENTATION.md)
- [Events Testing Guide](TESTS_EVENEMENTS_GUIDE.md)
- [JWT.io](https://jwt.io/) - Décodeur de tokens JWT
- [BCrypt Calculator](https://bcrypt-generator.com/) - Générateur de hash

---

## 🛠️ Dépannage

### Problème: La base de données ne se connecte pas

**Solution:**
```bash
# Vérifier que SQL Server est démarré
docker ps | grep sqlserver

# Redémarrer si nécessaire
docker-compose restart sqlserver
```

### Problème: Les événements ne sont pas publiés

**Solution:**
```bash
# Vérifier RabbitMQ
docker ps | grep rabbitmq

# Vérifier les logs
docker logs eshop-rabbitmq
```

### Problème: Token invalide

**Vérifications:**
1. Le token n'est pas expiré (vérifier `exp` claim)
2. La `Secret` dans appsettings.json est correcte
3. Le `Issuer` et `Audience` correspondent

---

**Date de création:** 2025-12-28
**Auteur:** Claude Code
**Version:** 1.0
