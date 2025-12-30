using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Domain.Common
{
     /// <summary>
    /// Marker interface pour identifier les agrégats racines
    /// Un agrégat racine est le point d'entrée pour accéder à un groupe d'entités liées
    /// Toutes les modifications doivent passer par l'agrégat racine
    /// </summary>
    public interface IAggregateRoot
    {
        
    }
}