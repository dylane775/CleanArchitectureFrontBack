namespace Notification.Domain.Enums
{
    public static class NotificationType
    {
        public const string OrderConfirmed = "ORDER_CONFIRMED";
        public const string OrderShipped = "ORDER_SHIPPED";
        public const string OrderDelivered = "ORDER_DELIVERED";
        public const string OrderCancelled = "ORDER_CANCELLED";
        public const string PriceDrop = "PRICE_DROP";
        public const string BackInStock = "BACK_IN_STOCK";
        public const string ReviewReminder = "REVIEW_REMINDER";
        public const string PromoAlert = "PROMO_ALERT";
        public const string WelcomeMessage = "WELCOME_MESSAGE";
        public const string PaymentReceived = "PAYMENT_RECEIVED";
        public const string PaymentFailed = "PAYMENT_FAILED";
    }
}
