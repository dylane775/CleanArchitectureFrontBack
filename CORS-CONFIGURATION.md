# Configuration CORS pour eShopOnContainers

## Résumé

Tous les microservices backend ont été configurés pour accepter les requêtes du frontend Angular.

## Configuration appliquée

### Policy CORS : `AllowFrontend`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### Caractéristiques

- **Origin autorisé** : `http://localhost:4200` (serveur de développement Angular)
- **Méthodes** : Toutes (GET, POST, PUT, DELETE, PATCH, etc.)
- **Headers** : Tous les headers sont autorisés
- **Credentials** : Activé (permet l'envoi de cookies et headers d'authentification)

## Microservices configurés

| Microservice | Port | Fichier modifié | Status |
|--------------|------|----------------|--------|
| Identity API | 5000 | `Identity.API/Program.cs` | ✅ |
| Catalog API | 5234 | `Catalog.API/Program.cs` | ✅ |
| Basket API | 5235 | `Basket.API/Program.cs` | ✅ |
| Ordering API | 5236 | `Ordering.API/Program.cs` | ✅ |

## Pipeline HTTP

La politique CORS est appliquée dans le bon ordre du pipeline :

```csharp
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Important** : `UseCors()` doit être appelé **avant** `UseAuthentication()` et `UseAuthorization()`.

## Testing CORS

### Depuis le frontend Angular

Le frontend Angular (`http://localhost:4200`) peut maintenant :

1. Faire des requêtes vers tous les microservices
2. Envoyer des headers d'authentification (JWT)
3. Recevoir et traiter les réponses

### Test manuel avec curl

```bash
# Test preflight request
curl -X OPTIONS http://localhost:5234/api/catalog/items \
  -H "Origin: http://localhost:4200" \
  -H "Access-Control-Request-Method: GET" \
  -H "Access-Control-Request-Headers: authorization" \
  -v

# Vérifier que les headers CORS sont présents dans la réponse:
# Access-Control-Allow-Origin: http://localhost:4200
# Access-Control-Allow-Methods: GET, POST, PUT, DELETE, etc.
# Access-Control-Allow-Credentials: true
```

## Production

⚠️ **Important** : Pour la production, il faudra :

1. Ajouter le domaine de production dans la liste des origins :

```csharp
policy.WithOrigins(
    "http://localhost:4200",      // Dev
    "https://yourdomain.com",     // Prod
    "https://www.yourdomain.com"  // Prod with www
)
```

2. Ou utiliser une configuration depuis `appsettings.json` :

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://yourdomain.com"
    ]
  }
}
```

```csharp
var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## Troubleshooting

### Erreur : "CORS policy: No 'Access-Control-Allow-Origin' header"

**Cause** : Le backend n'envoie pas les headers CORS appropriés

**Solution** :
- Vérifier que `UseCors("AllowFrontend")` est bien appelé dans Program.cs
- Vérifier que le frontend fait bien des requêtes depuis `http://localhost:4200`
- Vérifier que la policy "AllowFrontend" est bien configurée

### Erreur : "Credentials flag is true, but Access-Control-Allow-Credentials is not"

**Cause** : Le frontend envoie des credentials mais le backend ne les autorise pas

**Solution** :
- Vérifier que `.AllowCredentials()` est bien présent dans la configuration CORS
- Note: `AllowCredentials()` est incompatible avec `AllowAnyOrigin()`, il faut utiliser `WithOrigins()`

### Erreur : Preflight request fails

**Cause** : Le backend ne répond pas correctement aux requêtes OPTIONS

**Solution** :
- ASP.NET Core gère automatiquement les requêtes OPTIONS avec CORS
- Vérifier que `UseCors()` est appelé AVANT `UseAuthorization()`

## Références

- [ASP.NET Core CORS Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [Angular HttpClient CORS](https://angular.io/guide/http)
