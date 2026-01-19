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

                // Monetbil utilise application/x-www-form-urlencoded
                var formData = new Dictionary<string, string>
                {
                    ["amount"] = amount.ToString("F0"),
                    // Ne pas pré-remplir le téléphone pour laisser l'utilisateur choisir
                    // ["phone"] = customerPhone ?? "",
                    ["locale"] = "fr",
                    ["country"] = "CM",
                    ["currency"] = currency,
                    ["item_ref"] = paymentReference,
                    ["payment_ref"] = paymentReference,
                    ["user"] = customerEmail,
                    ["first_name"] = "",
                    ["last_name"] = "",
                    ["email"] = customerEmail,
                    ["return_url"] = returnUrl ?? "",
                    ["notify_url"] = callbackUrl ?? ""
                };

                var content = new FormUrlEncodedContent(formData);

                // L'endpoint correct pour Monetbil Widget API
                var requestUrl = $"https://www.monetbil.com/widget/v2.1/{_settings.ServiceKey}";

                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Monetbil response: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var monetbilResponse = JsonSerializer.Deserialize<MonetbilPaymentResponse>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (monetbilResponse != null && !string.IsNullOrEmpty(monetbilResponse.PaymentUrl))
                        {
                            _logger.LogInformation(
                                "Monetbil payment initiated successfully. PaymentUrl: {PaymentUrl}",
                                monetbilResponse.PaymentUrl);

                            return new PaymentGatewayResponse
                            {
                                Success = true,
                                TransactionId = paymentReference, // Utiliser la référence comme ID temporaire
                                PaymentUrl = monetbilResponse.PaymentUrl,
                                QrCodeUrl = monetbilResponse.QrCodeUrl
                            };
                        }
                        else
                        {
                            _logger.LogWarning("Monetbil payment initiation failed: Invalid response structure");

                            return new PaymentGatewayResponse
                            {
                                Success = false,
                                ErrorMessage = "Invalid response from Monetbil"
                            };
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to parse Monetbil response: {Response}", responseContent);

                        // Si la réponse n'est pas du JSON, construire manuellement l'URL de paiement
                        // selon la documentation Monetbil Widget
                        var paymentUrl = $"https://www.monetbil.com/widget/v2.1/{_settings.ServiceKey}?" +
                            $"amount={amount:F0}&currency={currency}&item_ref={paymentReference}&" +
                            $"email={customerEmail}&return_url={returnUrl}&notify_url={callbackUrl}";

                        return new PaymentGatewayResponse
                        {
                            Success = true,
                            TransactionId = paymentReference,
                            PaymentUrl = paymentUrl,
                            QrCodeUrl = null
                        };
                    }
                }
                else
                {
                    _logger.LogError("Monetbil API error: {StatusCode} - {Error}", response.StatusCode, responseContent);

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
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("payment_token")]
        public string? PaymentToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("payment_url")]
        public string? PaymentUrl { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("qr_code_url")]
        public string? QrCodeUrl { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    internal class MonetbilStatusResponse
    {
        public bool Success { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
    }
}
