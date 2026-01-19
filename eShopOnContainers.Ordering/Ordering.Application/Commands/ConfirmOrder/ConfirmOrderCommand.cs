using System;
using MediatR;

namespace Ordering.Application.Commands.ConfirmOrder
{
    /// <summary>
    /// Commande pour confirmer une commande après paiement réussi
    /// </summary>
    public record ConfirmOrderCommand : IRequest<Unit>
    {
        public Guid OrderId { get; init; }
    }
}
