namespace Payment.Infrastructure.PaymentGateways.Monetbil
{
    public class MonetbilSettings
    {
        public string ServiceKey { get; set; } = string.Empty;
        public string ServiceSecret { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = "https://api.monetbil.com/v2.1";
        public bool UseSandbox { get; set; } = true;
    }
}
