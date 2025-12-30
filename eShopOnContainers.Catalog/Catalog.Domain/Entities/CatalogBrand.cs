using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Common;

namespace Catalog.Domain.Entities
{
    /// <summary>
    /// Représente une marque de produits
    /// Exemple : Nike, Apple, Samsung, Adidas, etc.
    /// </summary>
    public class CatalogBrand : Entity,IAggregateRoot
    {
        public string Brand { get; private set; }

        //Constructeur protégé pour Entity Framework
        protected CatalogBrand(){}

        public CatalogBrand(string brand)
        {
            // Validation métier
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Brand cannot be null or empty", nameof(brand));

            Brand = brand;

        }

         // Méthode métier pour changer la marque
        public void UpdateBrand(string newBrand)
        {
            if (string.IsNullOrWhiteSpace(newBrand))
                throw new ArgumentException("Brand cannot be null or empty", nameof(newBrand));

            Brand = newBrand;
        }
    }
}