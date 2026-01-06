using System;
using System.Collections.Generic;
using System.Linq;

namespace Catalog.Domain.ValueObjects
{
    /// <summary>
    /// Value Object représentant les spécifications dynamiques d'un produit
    /// Permet de stocker des attributs personnalisés selon le type de produit
    /// </summary>
    public class ProductSpecifications
    {
        /// <summary>
        /// Dictionnaire clé-valeur pour les spécifications
        /// Exemple pour téléphone: { "Processeur": "Snapdragon 8 Gen 2", "RAM": "8GB", "Stockage": "256GB" }
        /// Exemple pour vêtement: { "Taille": "M", "Couleur": "Bleu", "Matière": "Coton 100%" }
        /// </summary>
        public Dictionary<string, string> Attributes { get; private set; }

        public ProductSpecifications()
        {
            Attributes = new Dictionary<string, string>();
        }

        public ProductSpecifications(Dictionary<string, string> attributes)
        {
            Attributes = attributes ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Ajoute ou met à jour un attribut
        /// </summary>
        public void AddOrUpdateAttribute(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Attribute key cannot be null or empty", nameof(key));

            Attributes[key] = value;
        }

        /// <summary>
        /// Supprime un attribut
        /// </summary>
        public void RemoveAttribute(string key)
        {
            if (Attributes.ContainsKey(key))
            {
                Attributes.Remove(key);
            }
        }

        /// <summary>
        /// Récupère la valeur d'un attribut
        /// </summary>
        public string? GetAttribute(string key)
        {
            return Attributes.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Vérifie si un attribut existe
        /// </summary>
        public bool HasAttribute(string key)
        {
            return Attributes.ContainsKey(key);
        }

        /// <summary>
        /// Récupère tous les attributs
        /// </summary>
        public IReadOnlyDictionary<string, string> GetAllAttributes()
        {
            return Attributes;
        }

        /// <summary>
        /// Crée des spécifications vides
        /// </summary>
        public static ProductSpecifications Empty()
        {
            return new ProductSpecifications();
        }

        /// <summary>
        /// Crée des spécifications à partir d'un dictionnaire
        /// </summary>
        public static ProductSpecifications Create(Dictionary<string, string> attributes)
        {
            return new ProductSpecifications(attributes);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var other = (ProductSpecifications)obj;

            if (Attributes.Count != other.Attributes.Count)
                return false;

            return Attributes.All(kvp =>
                other.Attributes.TryGetValue(kvp.Key, out var value) && value == kvp.Value);
        }

        public override int GetHashCode()
        {
            return Attributes.Aggregate(0, (hash, kvp) =>
                hash ^ kvp.Key.GetHashCode() ^ (kvp.Value?.GetHashCode() ?? 0));
        }
    }
}
