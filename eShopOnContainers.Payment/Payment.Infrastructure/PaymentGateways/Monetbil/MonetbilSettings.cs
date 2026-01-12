namespace Payment.Infrastructure.PaymentGateways.Monetbil
{
    public class MonetbilSettings
    {
        public string ServiceKey { get; set; }
        public string ServiceSecret { get; set; }
        public string ApiUrl { get; set; }
        public bool UseSandbox { get; set; }

        public MonetbilSettings()
        {
            // Valeurs par défaut
            ApiUrl = "https://api.monetbil.com/v2.1";
            UseSandbox = true;
        }
    }
}
