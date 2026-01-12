using Basket.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Basket.Application.Services;

/// <summary>
/// Service de nettoyage automatique des paniers expirés (guests)
/// S'exécute en arrière-plan pour supprimer les paniers non utilisés
/// </summary>
public class BasketCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BasketCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Nettoyage toutes les 6 heures
    private readonly int _basketExpirationDays = 7; // Paniers guests expirés après 7 jours

    public BasketCleanupService(
        IServiceProvider serviceProvider,
        ILogger<BasketCleanupService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Basket Cleanup Service démarré");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredBaskets(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du nettoyage des paniers expirés");
            }

            // Attendre avant le prochain nettoyage
            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Basket Cleanup Service arrêté");
    }

    private async Task CleanupExpiredBaskets(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Début du nettoyage des paniers expirés...");

        using var scope = _serviceProvider.CreateScope();
        var basketRepository = scope.ServiceProvider.GetRequiredService<IBasketRepository>();

        try
        {
            // Date limite d'expiration
            var expirationDate = DateTime.UtcNow.AddDays(-_basketExpirationDays);

            // Récupérer tous les paniers (normalement on filtrerait côté repository)
            var allBaskets = await basketRepository.GetAllAsync();

            int deletedCount = 0;
            int guestBasketsChecked = 0;

            foreach (var basket in allBaskets)
            {
                // Vérifier si le panier est expiré et est un panier guest
                if (IsGuestBasket(basket.CustomerId) && basket.CreatedAt < expirationDate)
                {
                    guestBasketsChecked++;

                    // Supprimer le panier expiré
                    await basketRepository.DeleteAsync(basket.Id);
                    deletedCount++;

                    _logger.LogInformation(
                        "Panier guest expiré supprimé - ID: {BasketId}, CustomerId: {CustomerId}, Créé le: {CreatedAt}",
                        basket.Id,
                        basket.CustomerId,
                        basket.CreatedAt
                    );
                }
            }

            _logger.LogInformation(
                "Nettoyage terminé - {GuestBasketsChecked} paniers guests vérifiés, {DeletedCount} paniers expirés supprimés",
                guestBasketsChecked,
                deletedCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression des paniers expirés");
            throw;
        }
    }

    /// <summary>
    /// Vérifie si le customerId correspond à un panier guest
    /// Les paniers guests commencent par "guest-"
    /// </summary>
    private bool IsGuestBasket(string customerId)
    {
        return !string.IsNullOrEmpty(customerId) &&
               customerId.StartsWith("guest-", StringComparison.OrdinalIgnoreCase);
    }
}
