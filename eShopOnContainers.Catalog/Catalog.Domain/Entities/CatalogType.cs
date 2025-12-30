using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Common;

namespace Catalog.Domain.Entities
{
    /// <summary>
    /// Représente une catégorie de produits (Type)
    /// Exemple : Électronique, Vêtements, Livres, etc.
    /// </summary>
    public class CatalogType : Entity, IAggregateRoot
    {
        public string Type { get; private set; }

        // Constructeur protégé pour Entity Framework
        protected CatalogType() { }

        //Constructeur public pour créer un nouveau type 
        public CatalogType(string type)
        {
            //Validation métier
            if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException("Type cannot be null or empty",nameof(type));
            Type = type;
        }

        //Méthode métier pour changer le type
        public void UpdateType(string newType)
        {
            if (string.IsNullOrWhiteSpace(newType))
            throw new ArgumentNullException("Type cannot be null or empty",nameof(newType));
            Type = newType;
        }
    }
}