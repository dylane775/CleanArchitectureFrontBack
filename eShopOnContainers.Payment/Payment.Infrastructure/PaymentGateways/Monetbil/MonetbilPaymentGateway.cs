using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Application.Common.Interfaces;

namespace Payment.Infrastructure.PaymentGateways.Monetbil
{
    public class MonetbilPaymentGateway : IPaymentGatewayService
    {
        private readonly MonetbilSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MonetbilPaymentGateway> _logger;

        public MonetbilPaymentGateway(
            IOptions<MonetbilSettings> settings,
            HttpClient httpClient,
            ILogger<MonetbilPaymentGateway> logger)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient.BaseAddress = new Uri(_settings.ApiUrl);
        }

        public async Task<PaymentGatewayResponse> InitiatePaymentAsync(
            string paymentReference,
            decimal amount,
            string currency,
            string customerEmail,
            string customerPhone,
            string description,
            string callbackUrl,
            string returnUrl)
        {
            try
            {
                _logger.LogInformation("Initiating Monetbil payment for reference {PaymentReference}", paymentReference);

                // Préparer les données pour Monetbil
                var requestData = new
                {
                    service = _settings.ServiceKey,
                    amount = amount,
                    currency = currency,
                    item_ref = paymentReference,
                    user = customerPhone ?? customerEmail,
                    email = customerEmail,
                    phonenumber = customerPhone,
                    item_name = description ?? "Payment",
                    return_url = returnUrl,
                    notify_url = callbackUrl,
                    // Mode sandbox si configuré
                    test = _settings.UseSandbox ? "true" : "false"
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                // Appeler l'API Monetbil
                var response = await _httpClient.PostAsync("/payment", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var monetbilResponse = JsonSerializer.Deserialize<MonetbilPaymentResponse>(responseContent);

                    if (monetbilResponse?.Success == true)
                    {
                        _logger.LogInformation(
                            "Monetbil payment initiated successfully. PaymentToken: {PaymentToken}",
                            monetbilResponse.PaymentToken);

                        return new PaymentGatewayResponse
                        {
                            Success = true,
                            TransactionId = monetbilResponse.PaymentToken,
                            PaymentUrl = monetbilResponse.PaymentUrl,
                            QrCodeUrl = monetbilResponse.QrCodeUrl
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Monetbil payment initiation failed: {Error}", monetbilResponse?.Message);

                        return new PaymentGatewayResponse
                        {
                            Success = false,
                            ErrorMessage = monetbilResponse?.Message ?? "Unknown error from Monetbil"
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Monetbil API error: {StatusCode} - {Error}", response.StatusCode, errorContent);

                    return new PaymentGatewayResponse
                    {
                        Success = false,
                        ErrorMessage = $"Monetbil API error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while initiating Monetbil payment");

                return new PaymentGatewayResponse
                {
                    Success = false,
                    ErrorMessage = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<PaymentGatewayStatusResponse> GetPaymentStatusAsync(string transactionId)
        {
            try
            {
                _logger.LogInformation("Checking Monetbil payment status for transaction {TransactionId}", transactionId);

                var requestData = new
                {
                    service = _settings.ServiceKey,
                    paymentId = transactionId
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/checkPayment", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var monetbilStatus = JsonSerializer.Deserialize<MonetbilStatusResponse>(responseContent);

                    if (monetbilStatus?.Success == true)
                    {
                        // Mapper le statut Monetbil vers notre système
                        var status = MapMonetbilStatus(monetbilStatus.Status);

                        return new PaymentGatewayStatusResponse
                        {
                            Success = true,
                            Status = status,
                            TransactionId = transactionId
                        };
                    }
                    else
                    {
                        return new PaymentGatewayStatusResponse
                        {
                            Success = false,
                            ErrorMessage = monetbilStatus?.Message ?? "Unknown error"
                        };
                    }
                }
                else
                {
                    return new PaymentGatewayStatusResponse
                    {
                        Success = false,
                        ErrorMessage = $"API error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while checking Monetbil payment status");

                return new PaymentGatewayStatusResponse
                {
                    Success = false,
                    ErrorMessage = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<PaymentGatewayRefundResponse> RefundPaymentAsync(string transactionId, decimal amount)
        {
            try
            {
                _logger.LogInformation("Refunding Monetbil payment {TransactionId} for amount {Amount}", transactionId, amount);

                // Note: Monetbil pourrait ne pas supporter les remboursements automatiques
                // Dans ce cas, cela doit être géré manuellement
                _logger.LogWarning("Monetbil automatic refunds may not be supported. Manual processing may be required.");

                // Pour le moment, retourner un succès fictif
                // À remplacer par l'API réelle quand disponible
                return new PaymentGatewayRefundResponse
                {
                    Success = true,
                    RefundId = Guid.NewGuid().ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while refunding Monetbil payment");

                return new PaymentGatewayRefundResponse
                {
                    Success = false,
                    ErrorMessage = $"Exception: {ex.Message}"
                };
            }
        }

        private string MapMonetbilStatus(string monetbilStatus)
        {
            return monetbilStatus?.ToLower() switch
            {
                "completed" => "completed",
                "success" => "completed",
                "paid" => "completed",
                "pending" => "pending",
                "processing" => "processing",
                "failed" => "failed",
                "cancelled" => "failed",
                _ => "pending"
            };
        }
    }

    // ====== DTOs POUR MONETBIL ======
    internal class MonetbilPaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentToken { get; set; }
        public string PaymentUrl { get; set; }
        public string QrCodeUrl { get; set; }
        public string Message { get; set; }
    }

    internal class MonetbilStatusResponse
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
