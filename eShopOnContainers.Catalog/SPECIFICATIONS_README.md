# Product Specifications - Documentation

## Vue d'ensemble

Le système de spécifications permet d'ajouter des attributs dynamiques aux produits du catalogue. Chaque type de produit peut avoir ses propres spécifications personnalisées.

## Architecture

### 1. Value Object: `ProductSpecifications`
- Localisation: `Catalog.Domain/ValueObjects/ProductSpecifications.cs`
- Stockage: Dictionnaire clé-valeur (`Dictionary<string, string>`)
- Persistance: JSON dans la base de données

### 2. Entité: `CatalogItem`
- Propriété: `Specifications` de type `ProductSpecifications`
- Méthodes:
  - `UpdateSpecifications(ProductSpecifications)`: Remplace toutes les spécifications
  - `AddOrUpdateSpecification(string key, string value)`: Ajoute/met à jour un attribut
  - `RemoveSpecification(string key)`: Supprime un attribut

### 3. Base de données
- Colonne: `Specifications` (nvarchar(max))
- Format: JSON
- Migration: `AddProductSpecifications`

## Exemples d'utilisation

### Exemple 1: Créer un produit avec spécifications (Téléphone)

```json
POST /api/catalog/items
{
  "name": "Samsung Galaxy S24",
  "description": "Smartphone haut de gamme",
  "price": 899.99,
  "pictureFileName": "galaxy-s24.jpg",
  "catalogTypeId": "guid-type-telephone",
  "catalogBrandId": "guid-brand-samsung",
  "availableStock": 50,
  "restockThreshold": 10,
  "maxStockThreshold": 200,
  "specifications": {
    "Processeur": "Snapdragon 8 Gen 3",
    "RAM": "8GB",
    "Stockage": "256GB",
    "TailleEcran": "6.2 pouces",
    "AppareilPhoto": "50MP + 12MP + 10MP",
    "Batterie": "4000 mAh",
    "Couleur": "Noir Phantom"
  }
}
```

### Exemple 2: Créer un vêtement avec spécifications

```json
POST /api/catalog/items
{
  "name": "T-Shirt Nike Dri-FIT",
  "description": "T-shirt de sport respirant",
  "price": 29.99,
  "pictureFileName": "nike-tshirt.jpg",
  "catalogTypeId": "guid-type-vetement",
  "catalogBrandId": "guid-brand-nike",
  "availableStock": 100,
  "specifications": {
    "Taille": "M",
    "Couleur": "Bleu Marine",
    "Matière": "Polyester 100%",
    "Coupe": "Regular Fit",
    "Genre": "Homme",
    "TechnologieDriFit": "Oui"
  }
}
```

### Exemple 3: Créer un livre avec spécifications

```json
POST /api/catalog/items
{
  "name": "Clean Architecture",
  "description": "Guide des bonnes pratiques de développement",
  "price": 39.99,
  "pictureFileName": "clean-arch.jpg",
  "catalogTypeId": "guid-type-livre",
  "catalogBrandId": "guid-brand-pearson",
  "availableStock": 25,
  "specifications": {
    "Auteur": "Robert C. Martin",
    "ISBN": "978-0134494166",
    "NombrePages": "432",
    "DatePublication": "2017-09-20",
    "Langue": "Anglais",
    "Format": "Broché",
    "Editeur": "Prentice Hall"
  }
}
```

### Exemple 4: Mettre à jour les spécifications

```json
PUT /api/catalog/items/{id}
{
  "name": "Samsung Galaxy S24",
  "description": "Smartphone haut de gamme - Édition mise à jour",
  "price": 849.99,
  "specifications": {
    "Processeur": "Snapdragon 8 Gen 3",
    "RAM": "12GB",
    "Stockage": "512GB",
    "TailleEcran": "6.2 pouces",
    "AppareilPhoto": "50MP + 12MP + 10MP",
    "Batterie": "4500 mAh",
    "Couleur": "Violet Titanium",
    "5G": "Oui"
  }
}
```

### Exemple 5: Réponse GET avec spécifications

```json
GET /api/catalog/items/{id}

Response:
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Samsung Galaxy S24",
  "description": "Smartphone haut de gamme",
  "price": 899.99,
  "pictureUri": "/images/products/galaxy-s24.jpg",
  "availableStock": 50,
  "onReorder": false,
  "catalogTypeId": "guid-type-telephone",
  "catalogTypeName": "Téléphones",
  "catalogBrandId": "guid-brand-samsung",
  "catalogBrandName": "Samsung",
  "specifications": {
    "Processeur": "Snapdragon 8 Gen 3",
    "RAM": "8GB",
    "Stockage": "256GB",
    "TailleEcran": "6.2 pouces",
    "AppareilPhoto": "50MP + 12MP + 10MP",
    "Batterie": "4000 mAh",
    "Couleur": "Noir Phantom"
  },
  "createdAt": "2024-01-06T10:30:00Z",
  "createdBy": "system"
}
```

## Cas d'usage par type de produit

### Électronique
- Processeur, RAM, Stockage
- Taille écran, Résolution
- Batterie, Autonomie
- Connectivité (WiFi, Bluetooth, 5G)
- Garantie

### Vêtements
- Taille (XS, S, M, L, XL, XXL)
- Couleur
- Matière (Coton, Polyester, Laine)
- Coupe (Slim, Regular, Loose)
- Genre (Homme, Femme, Unisexe)
- Instructions de lavage

### Chaussures
- Pointure
- Couleur
- Matériau (Cuir, Textile, Synthétique)
- Type de semelle
- Utilisation (Running, Casual, Basket)

### Livres
- Auteur
- ISBN
- Nombre de pages
- Date de publication
- Langue
- Format (Broché, Relié, Ebook)
- Éditeur

### Électroménager
- Puissance (Watts)
- Dimensions
- Poids
- Capacité
- Classe énergétique
- Couleur
- Garantie

## Bonnes pratiques

### 1. Nommage des clés
- Utiliser PascalCase pour les clés: `"TailleEcran"` plutôt que `"taille_ecran"`
- Être cohérent dans le nommage selon le type de produit
- Éviter les espaces et caractères spéciaux

### 2. Valeurs
- Stocker des chaînes de caractères simples
- Pour les booléens: utiliser `"Oui"/"Non"` ou `"true"/"false"`
- Pour les nombres: les stocker en string `"256GB"`, `"8GB"`
- Inclure les unités dans la valeur si pertinent

### 3. Organisation
- Grouper logiquement les spécifications par catégorie
- Les spécifications les plus importantes en premier
- Éviter la duplication d'informations déjà dans les champs fixes

### 4. Validation
- Valider les spécifications côté client avant envoi
- Limiter la taille du JSON (recommandé: < 4000 caractères)
- Vérifier que les clés et valeurs ne sont pas vides

## Requêtes SQL

### Rechercher des produits par spécification
```sql
-- Rechercher tous les téléphones avec 8GB de RAM
SELECT * FROM CatalogItems
WHERE JSON_VALUE(Specifications, '$.RAM') = '8GB'

-- Rechercher tous les vêtements de couleur bleue
SELECT * FROM CatalogItems
WHERE JSON_VALUE(Specifications, '$.Couleur') LIKE '%Bleu%'
```

### Extraire une spécification
```sql
-- Extraire toutes les RAM disponibles
SELECT DISTINCT JSON_VALUE(Specifications, '$.RAM') as RAM
FROM CatalogItems
WHERE JSON_VALUE(Specifications, '$.RAM') IS NOT NULL
```

## Frontend Angular - Affichage des spécifications

```typescript
// Dans le composant TypeScript
interface CatalogItem {
  id: string;
  name: string;
  // ... autres propriétés
  specifications?: { [key: string]: string };
}

// Dans le template HTML
<div *ngIf="product.specifications" class="specifications">
  <h3>Spécifications techniques</h3>
  <table>
    <tr *ngFor="let spec of product.specifications | keyvalue">
      <td class="spec-key">{{ spec.key }}</td>
      <td class="spec-value">{{ spec.value }}</td>
    </tr>
  </table>
</div>
```

## Migration et compatibilité

- Les produits existants sans spécifications auront un dictionnaire vide
- La colonne `Specifications` accepte les valeurs NULL
- Pas d'impact sur les produits créés avant la migration
- Compatible avec EF Core 9.0+

## Notes techniques

- **Stockage**: JSON dans SQL Server (nvarchar(max))
- **Performance**: Index possible sur des valeurs JSON spécifiques
- **Limite**: Environ 2GB par colonne (limite SQL Server)
- **Sérialisation**: System.Text.Json
- **Value Converter**: Automatique via EF Core configuration
