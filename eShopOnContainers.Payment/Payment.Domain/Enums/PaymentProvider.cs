namespace Payment.Domain.Enums
{
    public enum PaymentProvider
    {
        Monetbil = 0,       // Agrégateur Monetbil (Orange Money, MTN Mobile Money, etc.)
        Stripe = 1,         // Pour les paiements par carte (futur)
        PayPal = 2,         // Pour PayPal (futur)
        CashOnDelivery = 3  // Paiement à la livraison
    }
}
