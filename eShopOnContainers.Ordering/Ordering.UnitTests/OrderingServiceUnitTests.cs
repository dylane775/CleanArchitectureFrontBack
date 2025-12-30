using Ordering.Application.Commands.AddItemToOrder;
using Ordering.Application.Commands.CancelOrder;
using Ordering.Application.Commands.CreateOrder;
using Ordering.Application.Commands.DeliverOrder;
using Ordering.Application.Commands.RemoveItemFromOrder;
using Ordering.Application.Commands.ShipOrder;
using Ordering.Application.Commands.SubmitOrder;
using Ordering.Application.Commands.UpdateOrderItemQuantity;
using Ordering.Application.Queries.GetAllOrders;
using Ordering.Application.Queries.GetOrderById;
using Ordering.Application.Queries.GetOrdersByCustomerId;
using Ordering.Application.Queries.GetOrdersByStatus;
using Ordering.Domain.Entities;
using Ordering.Domain.Repositories;
using FluentValidation;
using Moq;
using Xunit;

namespace Ordering.UnitTests;

/// <summary>
/// Tests unitaires pour le microservice Ordering
/// </summary>
public class OrderingServiceUnitTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IOrderItemRepository> _mockOrderItemRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public OrderingServiceUnitTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockOrderItemRepository = new Mock<IOrderItemRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
    }

    #region Test 1: Créer une commande

    [Fact]
    public async Task CreateOrder_ShouldCreateOrder_WhenDataIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            CustomerId = customerId,
            ShippingAddress = "123 Main St, Paris, France",
            BillingAddress = "123 Main St, Paris, France",
            PaymentMethod = "CreditCard",
            CustomerEmail = "customer@example.com",
            CustomerPhone = "+33123456789"
        };

        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.OrderId);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("Initial", result.OrderStatus);
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
    }

    #endregion

    #region Test 2: Validation - CustomerId requis

    [Fact]
    public async Task CreateOrder_ShouldThrowValidationException_WhenCustomerIdIsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.Empty,
            ShippingAddress = "123 Main St",
            BillingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "customer@example.com",
            CustomerPhone = "+33123456789"
        };
        var validator = new CreateOrderCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(CreateOrderCommand.CustomerId));
    }

    #endregion

    #region Test 3: Validation - Email doit être valide

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("customer@")]
    [InlineData("")]
    public async Task CreateOrder_ShouldThrowValidationException_WhenEmailIsInvalid(string email)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddress = "123 Main St",
            BillingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = email,
            CustomerPhone = "+33123456789"
        };
        var validator = new CreateOrderCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(CreateOrderCommand.CustomerEmail));
    }

    #endregion

    #region Test 4: Ajouter un item à la commande

    [Fact]
    public async Task AddItemToOrder_ShouldAddItem_WhenOrderIsInInitialState()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");

        var command = new AddItemToOrderCommand
        {
            OrderId = orderId,
            CatalogItemId = catalogItemId,
            ProductName = "iPhone 15 Pro",
            UnitPrice = 1199.99m,
            Quantity = 2,
            Discount = 0
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new AddItemToOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(catalogItemId, order.Items.First().CatalogItemId);
        Assert.Equal(2, order.Items.First().Quantity);
        Assert.Equal(1199.99m, order.Items.First().UnitPrice);
        Assert.Equal(2399.98m, order.TotalAmount); // 1199.99 * 2
    }

    #endregion

    #region Test 5: Validation - Quantité doit être positive

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task AddItemToOrder_ShouldThrowValidationException_WhenQuantityIsNotPositive(int quantity)
    {
        // Arrange
        var command = new AddItemToOrderCommand
        {
            OrderId = Guid.NewGuid(),
            CatalogItemId = Guid.NewGuid(),
            ProductName = "Test Product",
            UnitPrice = 99.99m,
            Quantity = quantity,
            Discount = 0
        };
        var validator = new AddItemToOrderCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(AddItemToOrderCommand.Quantity));
    }

    #endregion

    #region Test 6: Supprimer un item de la commande

    [Fact]
    public async Task RemoveItemFromOrder_ShouldRemoveItem_WhenOrderIsInInitialState()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");
        order.AddItem(catalogItemId, "Test Product", 99.99m, 2, 0);

        var command = new RemoveItemFromOrderCommand
        {
            OrderId = orderId,
            CatalogItemId = catalogItemId
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RemoveItemFromOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Empty(order.Items);
        Assert.Equal(0m, order.TotalAmount);
    }

    #endregion

    #region Test 7: Mettre à jour la quantité d'un item

    [Fact]
    public async Task UpdateOrderItemQuantity_ShouldUpdateQuantity_WhenItemExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");
        order.AddItem(catalogItemId, "Test Product", 99.99m, 2, 0);

        var command = new UpdateOrderItemQuantityCommand
        {
            OrderId = orderId,
            CatalogItemId = catalogItemId,
            NewQuantity = 5
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateOrderItemQuantityCommandHandler(
            _mockOrderRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, order.Items.First().Quantity);
        Assert.Equal(499.95m, order.TotalAmount); // 99.99 * 5
    }

    #endregion

    #region Test 8: Soumettre une commande

    [Fact]
    public async Task SubmitOrder_ShouldChangeStatusToPending_WhenOrderIsInInitialState()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");
        order.AddItem(Guid.NewGuid(), "Test Product", 99.99m, 2, 0);

        var command = new SubmitOrderCommand { OrderId = orderId };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new SubmitOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Pending", order.OrderStatus);
    }

    #endregion

    #region Test 9: Expédier une commande

    [Fact]
    public async Task ShipOrder_ShouldChangeStatusToShipped_WhenOrderIsProcessing()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");
        order.AddItem(Guid.NewGuid(), "Test Product", 99.99m, 2, 0);
        order.Submit();
        // Simuler le passage à "Processing"
        typeof(Order).GetProperty("OrderStatus")!.SetValue(order, "Processing");

        var command = new ShipOrderCommand { OrderId = orderId };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new ShipOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Shipped", order.OrderStatus);
    }

    #endregion

    #region Test 10: Livrer une commande

    [Fact]
    public async Task DeliverOrder_ShouldChangeStatusToDelivered_WhenOrderIsShipped()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");
        order.AddItem(Guid.NewGuid(), "Test Product", 99.99m, 2, 0);
        order.Submit();
        // Simuler le passage à "Shipped"
        typeof(Order).GetProperty("OrderStatus")!.SetValue(order, "Shipped");

        var command = new DeliverOrderCommand { OrderId = orderId };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeliverOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Delivered", order.OrderStatus);
        Assert.NotNull(order.DeliveryDate);
    }

    #endregion

    #region Test 11: Annuler une commande

    [Fact]
    public async Task CancelOrder_ShouldChangeStatusToCancelled_WhenOrderIsNotDelivered()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");
        order.AddItem(Guid.NewGuid(), "Test Product", 99.99m, 2, 0);
        order.Submit();

        var command = new CancelOrderCommand { OrderId = orderId };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CancelOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Cancelled", order.OrderStatus);
    }

    #endregion

    #region Test 12: Récupérer une commande par ID

    [Fact]
    public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");
        order.AddItem(Guid.NewGuid(), "Test Product", 99.99m, 2, 0);

        var query = new GetOrderByIdQuery { OrderId = orderId };

        _mockOrderRepository.Setup(x => x.GetByIdWithItemsAsync(orderId))
            .ReturnsAsync(order);

        var handler = new GetOrderByIdQueryHandler(_mockOrderRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        Assert.Single(result.Items);
        Assert.Equal(199.98m, result.TotalAmount);
    }

    #endregion

    #region Test 13: Récupérer toutes les commandes

    [Fact]
    public async Task GetAllOrders_ShouldReturnAllOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            Order.Create(Guid.NewGuid(), "Address 1", "Address 1", "CreditCard", "email1@test.com", "+33111111111"),
            Order.Create(Guid.NewGuid(), "Address 2", "Address 2", "PayPal", "email2@test.com", "+33222222222"),
            Order.Create(Guid.NewGuid(), "Address 3", "Address 3", "BankTransfer", "email3@test.com", "+33333333333")
        };

        var query = new GetAllOrdersQuery();

        _mockOrderRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(orders);

        var handler = new GetAllOrdersQueryHandler(_mockOrderRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    #endregion

    #region Test 14: Récupérer les commandes par CustomerId

    [Fact]
    public async Task GetOrdersByCustomerId_ShouldReturnCustomerOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = new List<Order>
        {
            Order.Create(customerId, "Address 1", "Address 1", "CreditCard", "email@test.com", "+33111111111"),
            Order.Create(customerId, "Address 2", "Address 2", "PayPal", "email@test.com", "+33111111111")
        };

        var query = new GetOrdersByCustomerIdQuery { CustomerId = customerId };

        _mockOrderRepository.Setup(x => x.GetOrdersByCustomerIdAsync(customerId))
            .ReturnsAsync(orders);

        var handler = new GetOrdersByCustomerIdQueryHandler(_mockOrderRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, order => Assert.Equal(customerId, order.CustomerId));
    }

    #endregion

    #region Test 15: Récupérer les commandes par statut

    [Fact]
    public async Task GetOrdersByStatus_ShouldReturnOrdersWithSpecificStatus()
    {
        // Arrange
        var status = "Pending";
        var orders = new List<Order>
        {
            Order.Create(Guid.NewGuid(), "Address 1", "Address 1", "CreditCard", "email1@test.com", "+33111111111"),
            Order.Create(Guid.NewGuid(), "Address 2", "Address 2", "PayPal", "email2@test.com", "+33222222222")
        };
        orders[0].Submit();
        orders[1].Submit();

        var query = new GetOrdersByStatusQuery { Status = status };

        _mockOrderRepository.Setup(x => x.GetOrdersByStatusAsync(status))
            .ReturnsAsync(orders);

        var handler = new GetOrdersByStatusQueryHandler(_mockOrderRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, order => Assert.Equal(status, order.OrderStatus));
    }

    #endregion

    #region Test 16: Calcul du montant total avec remise

    [Fact]
    public void AddItem_ShouldCalculateTotalWithDiscount_WhenDiscountIsApplied()
    {
        // Arrange
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");

        // Act
        order.AddItem(Guid.NewGuid(), "Test Product", 100m, 2, 10m); // 10% de remise

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(180m, order.TotalAmount); // (100 * 2) - (100 * 2 * 0.10) = 180
    }

    #endregion

    #region Test 17: Ne peut pas modifier une commande livrée

    [Fact]
    public void AddItem_ShouldThrowException_WhenOrderIsDelivered()
    {
        // Arrange
        var order = Order.Create(
            Guid.NewGuid(),
            "123 Main St",
            "123 Main St",
            "CreditCard",
            "customer@example.com",
            "+33123456789");
        order.AddItem(Guid.NewGuid(), "Test Product", 99.99m, 2, 0);
        order.Submit();
        typeof(Order).GetProperty("OrderStatus")!.SetValue(order, "Delivered");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            order.AddItem(Guid.NewGuid(), "Another Product", 49.99m, 1, 0));
    }

    #endregion
}
