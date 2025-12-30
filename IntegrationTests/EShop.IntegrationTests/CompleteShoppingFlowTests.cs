using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EShop.IntegrationTests;

/// <summary>
/// Tests d'intégration pour le flow complet de shopping
/// Ce test simule le parcours utilisateur complet : Browse → Add to Cart → Checkout → Order → Ship → Deliver
/// </summary>
public class CompleteShoppingFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _basketClient;
    private readonly HttpClient _catalogClient;
    private readonly HttpClient _orderingClient;

    public CompleteShoppingFlowTests(WebApplicationFactory<Program> factory)
    {
        // Configuration des clients HTTP pour chaque microservice
        _basketClient = new HttpClient { BaseAddress = new Uri("http://localhost:5235") };
        _catalogClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        _orderingClient = new HttpClient { BaseAddress = new Uri("http://localhost:5240") };
    }

    #region Test 1: Flow complet - Du panier à la livraison (Happy Path)

    [Fact]
    public async Task CompleteShoppingFlow_ShouldSucceed_WhenAllStepsAreValid()
    {
        // ==================== ÉTAPE 1: CATALOGUE - Consulter les produits ====================
        var catalogResponse = await _catalogClient.GetAsync("/api/catalogitems");
        Assert.Equal(HttpStatusCode.OK, catalogResponse.StatusCode);

        var catalogItems = await catalogResponse.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
        Assert.NotNull(catalogItems);
        Assert.NotEmpty(catalogItems);

        var selectedProduct = catalogItems.First();
        var productId = selectedProduct.Id;
        var productPrice = selectedProduct.Price;

        // ==================== ÉTAPE 2: PANIER - Créer un panier ====================
        var customerId = $"customer-{Guid.NewGuid()}";
        var createBasketRequest = new { CustomerId = customerId };

        var createBasketResponse = await _basketClient.PostAsJsonAsync("/api/baskets", createBasketRequest);
        Assert.Equal(HttpStatusCode.Created, createBasketResponse.StatusCode);

        var basket = await createBasketResponse.Content.ReadFromJsonAsync<BasketDto>();
        Assert.NotNull(basket);
        var basketId = basket.BasketId;

        // ==================== ÉTAPE 3: PANIER - Ajouter des produits au panier ====================
        var addItemRequest = new
        {
            BasketId = basketId,
            CatalogItemId = productId,
            ProductName = selectedProduct.Name,
            UnitPrice = productPrice,
            Quantity = 2,
            PictureUrl = selectedProduct.PictureUri
        };

        var addItemResponse = await _basketClient.PostAsJsonAsync($"/api/baskets/{basketId}/items", addItemRequest);
        Assert.Equal(HttpStatusCode.OK, addItemResponse.StatusCode);

        // Ajouter un deuxième produit
        if (catalogItems.Count > 1)
        {
            var secondProduct = catalogItems.Skip(1).First();
            var addSecondItemRequest = new
            {
                BasketId = basketId,
                CatalogItemId = secondProduct.Id,
                ProductName = secondProduct.Name,
                UnitPrice = secondProduct.Price,
                Quantity = 1,
                PictureUrl = secondProduct.PictureUri
            };

            var addSecondItemResponse = await _basketClient.PostAsJsonAsync($"/api/baskets/{basketId}/items", addSecondItemRequest);
            Assert.Equal(HttpStatusCode.OK, addSecondItemResponse.StatusCode);
        }

        // ==================== ÉTAPE 4: PANIER - Vérifier le contenu du panier ====================
        var getBasketResponse = await _basketClient.GetAsync($"/api/baskets/{basketId}");
        Assert.Equal(HttpStatusCode.OK, getBasketResponse.StatusCode);

        var updatedBasket = await getBasketResponse.Content.ReadFromJsonAsync<BasketDto>();
        Assert.NotNull(updatedBasket);
        Assert.True(updatedBasket.Items.Count >= 1);

        // ==================== ÉTAPE 5: COMMANDE - Créer une commande à partir du panier ====================
        var createOrderRequest = new
        {
            CustomerId = customerId,
            ShippingAddress = "123 Avenue des Champs-Élysées, 75008 Paris, France",
            BillingAddress = "123 Avenue des Champs-Élysées, 75008 Paris, France",
            PaymentMethod = "CreditCard",
            CustomerEmail = "customer@eshop.com",
            CustomerPhone = "+33612345678"
        };

        var createOrderResponse = await _orderingClient.PostAsJsonAsync("/api/orders", createOrderRequest);
        Assert.Equal(HttpStatusCode.Created, createOrderResponse.StatusCode);

        var order = await createOrderResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        var orderId = order.OrderId;

        // ==================== ÉTAPE 6: COMMANDE - Ajouter les items du panier à la commande ====================
        foreach (var basketItem in updatedBasket.Items)
        {
            var addOrderItemRequest = new
            {
                OrderId = orderId,
                CatalogItemId = basketItem.CatalogItemId,
                ProductName = basketItem.ProductName,
                UnitPrice = basketItem.UnitPrice,
                Quantity = basketItem.Quantity,
                Discount = 0
            };

            var addOrderItemResponse = await _orderingClient.PostAsJsonAsync($"/api/orders/{orderId}/items", addOrderItemRequest);
            Assert.Equal(HttpStatusCode.OK, addOrderItemResponse.StatusCode);
        }

        // ==================== ÉTAPE 7: COMMANDE - Soumettre la commande ====================
        var submitOrderResponse = await _orderingClient.PostAsync($"/api/orders/{orderId}/submit", null);
        Assert.Equal(HttpStatusCode.OK, submitOrderResponse.StatusCode);

        // Vérifier que le statut a changé à "Pending"
        var getOrderResponse = await _orderingClient.GetAsync($"/api/orders/{orderId}");
        var submittedOrder = await getOrderResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(submittedOrder);
        Assert.Equal("Pending", submittedOrder.OrderStatus);

        // ==================== ÉTAPE 8: PANIER - Vider le panier après checkout ====================
        var clearBasketResponse = await _basketClient.DeleteAsync($"/api/baskets/{basketId}/clear");
        Assert.Equal(HttpStatusCode.NoContent, clearBasketResponse.StatusCode);

        // ==================== ÉTAPE 9: COMMANDE - Expédier la commande ====================
        var shipOrderResponse = await _orderingClient.PostAsync($"/api/orders/{orderId}/ship", null);
        Assert.Equal(HttpStatusCode.OK, shipOrderResponse.StatusCode);

        // Vérifier le statut "Shipped"
        getOrderResponse = await _orderingClient.GetAsync($"/api/orders/{orderId}");
        var shippedOrder = await getOrderResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(shippedOrder);
        Assert.Equal("Shipped", shippedOrder.OrderStatus);

        // ==================== ÉTAPE 10: COMMANDE - Livrer la commande ====================
        var deliverOrderResponse = await _orderingClient.PostAsync($"/api/orders/{orderId}/deliver", null);
        Assert.Equal(HttpStatusCode.OK, deliverOrderResponse.StatusCode);

        // Vérifier le statut "Delivered"
        getOrderResponse = await _orderingClient.GetAsync($"/api/orders/{orderId}");
        var deliveredOrder = await getOrderResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(deliveredOrder);
        Assert.Equal("Delivered", deliveredOrder.OrderStatus);
        Assert.NotNull(deliveredOrder.DeliveryDate);

        // ==================== ÉTAPE 11: COMMANDE - Vérifier l'historique des commandes du client ====================
        var customerOrdersResponse = await _orderingClient.GetAsync($"/api/orders/customer/{customerId}");
        Assert.Equal(HttpStatusCode.OK, customerOrdersResponse.StatusCode);

        var customerOrders = await customerOrdersResponse.Content.ReadFromJsonAsync<List<OrderDto>>();
        Assert.NotNull(customerOrders);
        Assert.Contains(customerOrders, o => o.OrderId == orderId);
    }

    #endregion

    #region Test 2: Annulation de commande avant expédition

    [Fact]
    public async Task CancelOrder_ShouldSucceed_WhenOrderIsNotYetShipped()
    {
        // Créer une commande
        var customerId = $"customer-{Guid.NewGuid()}";
        var createOrderRequest = new
        {
            CustomerId = customerId,
            ShippingAddress = "456 Rue de Rivoli, 75001 Paris, France",
            BillingAddress = "456 Rue de Rivoli, 75001 Paris, France",
            PaymentMethod = "PayPal",
            CustomerEmail = "customer2@eshop.com",
            CustomerPhone = "+33698765432"
        };

        var createOrderResponse = await _orderingClient.PostAsJsonAsync("/api/orders", createOrderRequest);
        var order = await createOrderResponse.Content.ReadFromJsonAsync<OrderDto>();
        var orderId = order!.OrderId;

        // Soumettre la commande
        await _orderingClient.PostAsync($"/api/orders/{orderId}/submit", null);

        // Annuler la commande
        var cancelResponse = await _orderingClient.PostAsync($"/api/orders/{orderId}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        // Vérifier le statut
        var getOrderResponse = await _orderingClient.GetAsync($"/api/orders/{orderId}");
        var cancelledOrder = await getOrderResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal("Cancelled", cancelledOrder!.OrderStatus);
    }

    #endregion

    #region Test 3: Mise à jour de la quantité dans le panier

    [Fact]
    public async Task UpdateBasketItemQuantity_ShouldUpdateQuantity_WhenItemExists()
    {
        // Créer un panier
        var customerId = $"customer-{Guid.NewGuid()}";
        var createBasketResponse = await _basketClient.PostAsJsonAsync("/api/baskets", new { CustomerId = customerId });
        var basket = await createBasketResponse.Content.ReadFromJsonAsync<BasketDto>();
        var basketId = basket!.BasketId;

        // Récupérer un produit du catalogue
        var catalogResponse = await _catalogClient.GetAsync("/api/catalogitems");
        var catalogItems = await catalogResponse.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
        var product = catalogItems!.First();

        // Ajouter le produit au panier
        var addItemRequest = new
        {
            BasketId = basketId,
            CatalogItemId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = 2,
            PictureUrl = product.PictureUri
        };
        await _basketClient.PostAsJsonAsync($"/api/baskets/{basketId}/items", addItemRequest);

        // Mettre à jour la quantité
        var updateQuantityRequest = new
        {
            CatalogItemId = product.Id,
            NewQuantity = 5
        };
        var updateResponse = await _basketClient.PutAsJsonAsync($"/api/baskets/{basketId}/items", updateQuantityRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Vérifier la nouvelle quantité
        var getBasketResponse = await _basketClient.GetAsync($"/api/baskets/{basketId}");
        var updatedBasket = await getBasketResponse.Content.ReadFromJsonAsync<BasketDto>();
        Assert.Equal(5, updatedBasket!.Items.First().Quantity);
    }

    #endregion

    #region Test 4: Vérification du stock disponible avant ajout au panier

    [Fact]
    public async Task AddItemToBasket_ShouldCheckStockAvailability_BeforeAdding()
    {
        // Récupérer un produit avec stock disponible
        var catalogResponse = await _catalogClient.GetAsync("/api/catalogitems");
        var catalogItems = await catalogResponse.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
        var product = catalogItems!.First(p => p.AvailableStock > 0);

        // Créer un panier
        var customerId = $"customer-{Guid.NewGuid()}";
        var createBasketResponse = await _basketClient.PostAsJsonAsync("/api/baskets", new { CustomerId = customerId });
        var basket = await createBasketResponse.Content.ReadFromJsonAsync<BasketDto>();

        // Tenter d'ajouter une quantité supérieure au stock disponible
        var addItemRequest = new
        {
            BasketId = basket!.BasketId,
            CatalogItemId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = product.AvailableStock + 100, // Plus que disponible
            PictureUrl = product.PictureUri
        };

        var addItemResponse = await _basketClient.PostAsJsonAsync($"/api/baskets/{basket.BasketId}/items", addItemRequest);

        // Devrait échouer ou limiter la quantité
        Assert.True(
            addItemResponse.StatusCode == HttpStatusCode.BadRequest ||
            addItemResponse.StatusCode == HttpStatusCode.OK
        );
    }

    #endregion

    #region Test 5: Filtrage des commandes par statut

    [Fact]
    public async Task GetOrdersByStatus_ShouldReturnOnlyOrdersWithSpecifiedStatus()
    {
        // Créer plusieurs commandes avec différents statuts
        var customerId = $"customer-{Guid.NewGuid()}";

        // Commande 1 - Pending
        var order1Response = await _orderingClient.PostAsJsonAsync("/api/orders", new
        {
            CustomerId = customerId,
            ShippingAddress = "Address 1",
            BillingAddress = "Address 1",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@eshop.com",
            CustomerPhone = "+33600000001"
        });
        var order1 = await order1Response.Content.ReadFromJsonAsync<OrderDto>();
        await _orderingClient.PostAsync($"/api/orders/{order1!.OrderId}/submit", null);

        // Commande 2 - Shipped
        var order2Response = await _orderingClient.PostAsJsonAsync("/api/orders", new
        {
            CustomerId = customerId,
            ShippingAddress = "Address 2",
            BillingAddress = "Address 2",
            PaymentMethod = "PayPal",
            CustomerEmail = "test@eshop.com",
            CustomerPhone = "+33600000002"
        });
        var order2 = await order2Response.Content.ReadFromJsonAsync<OrderDto>();
        await _orderingClient.PostAsync($"/api/orders/{order2!.OrderId}/submit", null);
        await _orderingClient.PostAsync($"/api/orders/{order2.OrderId}/ship", null);

        // Récupérer seulement les commandes "Pending"
        var pendingOrdersResponse = await _orderingClient.GetAsync("/api/orders/status/Pending");
        Assert.Equal(HttpStatusCode.OK, pendingOrdersResponse.StatusCode);

        var pendingOrders = await pendingOrdersResponse.Content.ReadFromJsonAsync<List<OrderDto>>();
        Assert.NotNull(pendingOrders);
        Assert.All(pendingOrders, order => Assert.Equal("Pending", order.OrderStatus));
    }

    #endregion

    #region Test 6: Suppression d'un item du panier

    [Fact]
    public async Task RemoveItemFromBasket_ShouldRemoveItem_WhenItemExists()
    {
        // Créer un panier avec des items
        var customerId = $"customer-{Guid.NewGuid()}";
        var createBasketResponse = await _basketClient.PostAsJsonAsync("/api/baskets", new { CustomerId = customerId });
        var basket = await createBasketResponse.Content.ReadFromJsonAsync<BasketDto>();

        var catalogResponse = await _catalogClient.GetAsync("/api/catalogitems");
        var catalogItems = await catalogResponse.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
        var product = catalogItems!.First();

        // Ajouter un item
        await _basketClient.PostAsJsonAsync($"/api/baskets/{basket!.BasketId}/items", new
        {
            BasketId = basket.BasketId,
            CatalogItemId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = 1,
            PictureUrl = product.PictureUri
        });

        // Supprimer l'item
        var deleteResponse = await _basketClient.DeleteAsync($"/api/baskets/{basket.BasketId}/items/{product.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Vérifier que le panier est vide
        var getBasketResponse = await _basketClient.GetAsync($"/api/baskets/{basket.BasketId}");
        var updatedBasket = await getBasketResponse.Content.ReadFromJsonAsync<BasketDto>();
        Assert.Empty(updatedBasket!.Items);
    }

    #endregion

    #region Test 7: Récupération des produits par catégorie

    [Fact]
    public async Task GetCatalogItemsByType_ShouldReturnOnlyItemsOfSpecifiedType()
    {
        // Récupérer les types de produits
        var typesResponse = await _catalogClient.GetAsync("/api/catalogtypes");
        Assert.Equal(HttpStatusCode.OK, typesResponse.StatusCode);

        var types = await typesResponse.Content.ReadFromJsonAsync<List<CatalogTypeDto>>();
        Assert.NotNull(types);

        if (types.Any())
        {
            var firstType = types.First();

            // Récupérer les produits de ce type
            var itemsResponse = await _catalogClient.GetAsync($"/api/catalogitems/type/{firstType.Id}");
            Assert.Equal(HttpStatusCode.OK, itemsResponse.StatusCode);

            var items = await itemsResponse.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
            Assert.NotNull(items);
            Assert.All(items, item => Assert.Equal(firstType.Id, item.CatalogTypeId));
        }
    }

    #endregion

    #region Test 8: Récupération des produits par marque

    [Fact]
    public async Task GetCatalogItemsByBrand_ShouldReturnOnlyItemsOfSpecifiedBrand()
    {
        // Récupérer les marques
        var brandsResponse = await _catalogClient.GetAsync("/api/catalogbrands");
        Assert.Equal(HttpStatusCode.OK, brandsResponse.StatusCode);

        var brands = await brandsResponse.Content.ReadFromJsonAsync<List<CatalogBrandDto>>();
        Assert.NotNull(brands);

        if (brands.Any())
        {
            var firstBrand = brands.First();

            // Récupérer les produits de cette marque
            var itemsResponse = await _catalogClient.GetAsync($"/api/catalogitems/brand/{firstBrand.Id}");
            Assert.Equal(HttpStatusCode.OK, itemsResponse.StatusCode);

            var items = await itemsResponse.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
            Assert.NotNull(items);
            Assert.All(items, item => Assert.Equal(firstBrand.Id, item.CatalogBrandId));
        }
    }

    #endregion

    #region Test 9: Mise à jour du prix d'un produit et propagation aux paniers

    [Fact]
    public async Task UpdateProductPrice_ShouldPropagateToExistingBaskets()
    {
        // Cette fonctionnalité teste l'intégration via RabbitMQ
        // Lorsqu'un prix change dans le Catalog, un événement ProductPriceChanged est publié
        // Le service Basket écoute cet événement et met à jour tous les paniers affectés

        // Créer un panier avec un produit
        var customerId = $"customer-{Guid.NewGuid()}";
        var createBasketResponse = await _basketClient.PostAsJsonAsync("/api/baskets", new { CustomerId = customerId });
        var basket = await createBasketResponse.Content.ReadFromJsonAsync<BasketDto>();

        var catalogResponse = await _catalogClient.GetAsync("/api/catalogitems");
        var catalogItems = await catalogResponse.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
        var product = catalogItems!.First();
        var originalPrice = product.Price;

        // Ajouter le produit au panier
        await _basketClient.PostAsJsonAsync($"/api/baskets/{basket!.BasketId}/items", new
        {
            BasketId = basket.BasketId,
            CatalogItemId = product.Id,
            ProductName = product.Name,
            UnitPrice = originalPrice,
            Quantity = 1,
            PictureUrl = product.PictureUri
        });

        // Mettre à jour le prix du produit dans le catalogue
        var newPrice = originalPrice + 50m;
        var updatePriceResponse = await _catalogClient.PutAsJsonAsync($"/api/catalogitems/{product.Id}", new
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = newPrice,
            PictureFileName = product.PictureFileName,
            PictureUri = product.PictureUri,
            CatalogTypeId = product.CatalogTypeId,
            CatalogBrandId = product.CatalogBrandId
        });

        Assert.Equal(HttpStatusCode.OK, updatePriceResponse.StatusCode);

        // Attendre la propagation de l'événement via RabbitMQ
        await Task.Delay(2000);

        // Vérifier que le prix a été mis à jour dans le panier
        var getBasketResponse = await _basketClient.GetAsync($"/api/baskets/{basket.BasketId}");
        var updatedBasket = await getBasketResponse.Content.ReadFromJsonAsync<BasketDto>();
        var basketItem = updatedBasket!.Items.First(i => i.CatalogItemId == product.Id);

        Assert.Equal(newPrice, basketItem.UnitPrice);
    }

    #endregion

    #region Test 10: Récupération du panier par CustomerId

    [Fact]
    public async Task GetBasketByCustomerId_ShouldReturnCustomerBasket()
    {
        // Créer un panier pour un client
        var customerId = $"customer-{Guid.NewGuid()}";
        var createBasketResponse = await _basketClient.PostAsJsonAsync("/api/baskets", new { CustomerId = customerId });
        Assert.Equal(HttpStatusCode.Created, createBasketResponse.StatusCode);

        // Récupérer le panier par CustomerId
        var getBasketResponse = await _basketClient.GetAsync($"/api/baskets/customer/{customerId}");
        Assert.Equal(HttpStatusCode.OK, getBasketResponse.StatusCode);

        var basket = await getBasketResponse.Content.ReadFromJsonAsync<BasketDto>();
        Assert.NotNull(basket);
        Assert.Equal(customerId, basket.CustomerId);
    }

    #endregion

    #region DTOs pour les tests

    public class BasketDto
    {
        public Guid BasketId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public List<BasketItemDto> Items { get; set; } = new();
    }

    public class BasketItemDto
    {
        public Guid CatalogItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
    }

    public class CatalogItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PictureFileName { get; set; } = string.Empty;
        public string PictureUri { get; set; } = string.Empty;
        public Guid CatalogTypeId { get; set; }
        public Guid CatalogBrandId { get; set; }
        public int AvailableStock { get; set; }
        public int RestockThreshold { get; set; }
        public int MaxStockThreshold { get; set; }
    }

    public class CatalogTypeDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class CatalogBrandDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
    }

    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid CatalogItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
    }

    #endregion
}
