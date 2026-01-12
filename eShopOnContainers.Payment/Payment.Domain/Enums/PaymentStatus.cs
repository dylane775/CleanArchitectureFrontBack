namespace Payment.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,        // Paiement en attente
        Processing = 1,     // En cours de traitement
        Completed = 2,      // Paiement réussi
        Failed = 3,         // Paiement échoué
        Cancelled = 4,      // Paiement annulé
        Refunded = 5,       // Paiement remboursé
        PartiallyRefunded = 6  // Partiellement remboursé
    }
}
