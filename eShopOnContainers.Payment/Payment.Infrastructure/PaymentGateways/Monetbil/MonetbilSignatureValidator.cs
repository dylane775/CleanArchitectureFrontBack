using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Payment.Infrastructure.PaymentGateways.Monetbil
{
    /// <summary>
    /// Service pour valider les signatures HMAC-SHA256 des webhooks Monetbil
    /// </summary>
    public interface IMonetbilSignatureValidator
    {
        bool ValidateSignature(string payload, string receivedSignature);
        string ComputeSignature(string payload);
    }

    public class MonetbilSignatureValidator : IMonetbilSignatureValidator
    {
        private readonly MonetbilSettings _settings;
        private readonly ILogger<MonetbilSignatureValidator> _logger;

        public MonetbilSignatureValidator(
            IOptions<MonetbilSettings> settings,
            ILogger<MonetbilSignatureValidator> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Valide la signature HMAC-SHA256 du webhook Monetbil
        /// </summary>
        /// <param name="payload">Données JSON du webhook</param>
        /// <param name="receivedSignature">Signature reçue dans le header</param>
        /// <returns>True si la signature est valide</returns>
        public bool ValidateSignature(string payload, string receivedSignature)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                _logger.LogWarning("Payload is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(receivedSignature))
            {
                _logger.LogWarning("Received signature is empty");
                return false;
            }

            var computedSignature = ComputeSignature(payload);

            // Comparaison sécurisée pour éviter les timing attacks
            var isValid = SecureCompare(computedSignature, receivedSignature);

            if (!isValid)
            {
                _logger.LogWarning(
                    "Signature mismatch. Expected: {Expected}, Received: {Received}",
                    computedSignature,
                    receivedSignature);
            }

            return isValid;
        }

        /// <summary>
        /// Calcule la signature HMAC-SHA256 pour un payload donné
        /// </summary>
        public string ComputeSignature(string payload)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.ServiceSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash).ToLower();
        }

        /// <summary>
        /// Comparaison sécurisée de chaînes pour éviter les timing attacks
        /// </summary>
        private static bool SecureCompare(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            var result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}
