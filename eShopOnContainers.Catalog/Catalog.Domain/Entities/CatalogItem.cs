using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Common;
using Catalog.Domain.Events;
using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities
{
    /// <summary>
    /// Agrégat racine représentant un produit du catalogue
    /// C'est l'entité principale du microservice Catalog
    /// </summary>
    public class CatalogItem : Entity, IAggregateRoot
    {
        // ====== PROPRIÉTÉS ======
        
        // Informations de base du produit
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public decimal Price { get; private set; }

        // Image
        public string PictureFileName { get; private set; }
        public string? PictureUri { get; private set; }

        // Relations Many-to-One
        public Guid CatalogTypeId { get; private set; }
        public CatalogType CatalogType  { get; private set; }
        
        public Guid CatalogBrandId { get; private set; }
        public CatalogBrand CatalogBrand { get; private set; }

        // Gestion du stock
        public int AvailableStock { get; private set; }
        public int RestockThreshold { get; private set; }
        public int MaxStockThreshold { get; private set; }
        public bool OnReorder { get; private set; }

        protected CatalogItem() { }

        public CatalogItem(
            string name,
            string description,
            decimal price,
            string pictureFileName,
            Guid catalogTypeId,
            Guid catalogBrandId,
            int availableStock = 0,
            int restockThreshold = 0,
            int maxStockThreshold = 0,
            string? pictureUri = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            if (price <= 0)
                throw new ArgumentException("Price must be positive", nameof(price));

            if (availableStock < 0)
                throw new ArgumentException("Available stock cannot be negative", nameof(availableStock));

            Name = name;
            Description = description;
            Price = price;
            PictureFileName = pictureFileName;
            PictureUri = pictureUri ?? pictureFileName; // Si pictureUri n'est pas fourni, utilise pictureFileName
            CatalogTypeId = catalogTypeId;
            CatalogBrandId = catalogBrandId;
            AvailableStock = availableStock;
            RestockThreshold = restockThreshold;
            MaxStockThreshold = maxStockThreshold;
            OnReorder = false;
        }

        public void UpdateDetails(string name, string description, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            if (price <= 0)
                throw new ArgumentException("Price must be positive", nameof(price));

            var oldPrice = Price;
            
            Name = name;
            Description = description;
            Price = price;

            if (oldPrice != price)
            {
                AddDomainEvent(new ProductPriceChangedDomainEvent(Id, oldPrice, price));
            }
        }
         public void UpdatePicture(string pictureFileName, string pictureUri)
        {
            PictureFileName = pictureFileName;
            PictureUri = pictureUri;
        }

        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            int oldStock = AvailableStock;
            int newStock = AvailableStock + quantity;

            AvailableStock = newStock > MaxStockThreshold ? MaxStockThreshold : newStock;

            if (AvailableStock > RestockThreshold && OnReorder)
            {
                OnReorder = false;
            }

            AddDomainEvent(new ProductStockUpdatedDomainEvent(Id, oldStock, AvailableStock));
        }

        public int RemoveStock(int quantityDesired)
        {
            if (AvailableStock == 0)
            {
                throw new CatalogDomainException($"Empty stock, product {Name} is sold out");
            }

            if (quantityDesired <= 0)
            {
                throw new ArgumentException("Quantity must be positive", nameof(quantityDesired));
            }

            int oldStock = AvailableStock;
            int removed = Math.Min(quantityDesired, AvailableStock);
            AvailableStock -= removed;

            if (AvailableStock <= RestockThreshold && !OnReorder)
            {
                OnReorder = true;
                AddDomainEvent(new ProductRestockThresholdReachedDomainEvent(Id, AvailableStock, RestockThreshold));
            }

            AddDomainEvent(new ProductStockUpdatedDomainEvent(Id, oldStock, AvailableStock));

            return removed;
        }

        public void UpdateStock(int quantity)
        {
            int oldStock = AvailableStock;
            AvailableStock += quantity;

            if (AvailableStock <= RestockThreshold && !OnReorder)
            {
                OnReorder = true;
                AddDomainEvent(new ProductRestockThresholdReachedDomainEvent(Id, AvailableStock, RestockThreshold));
            }

            if (AvailableStock > RestockThreshold && OnReorder)
            {
                OnReorder = false;
            }

            AddDomainEvent(new ProductStockUpdatedDomainEvent(Id, oldStock, AvailableStock));
        }

        public void UpdateStockThresholds(int restockThreshold, int maxStockThreshold)
        {
            if (restockThreshold < 0)
                throw new ArgumentException("Restock threshold cannot be negative", nameof(restockThreshold));

            if (maxStockThreshold < 0)
                throw new ArgumentException("Max stock threshold cannot be negative", nameof(maxStockThreshold));

            if (maxStockThreshold < restockThreshold)
                throw new ArgumentException("Max stock threshold must be greater than restock threshold");

            RestockThreshold = restockThreshold;
            MaxStockThreshold = maxStockThreshold;
        }

    }
}