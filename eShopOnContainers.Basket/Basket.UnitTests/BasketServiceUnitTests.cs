using Basket.Application.Commands.AddItemToBasket;
using Basket.Application.Commands.ClearBasket;
using Basket.Application.Commands.CreateBasket;
using Basket.Application.Commands.DeleteBasket;
using Basket.Application.Commands.RemoveItemFromBasket;
using Basket.Application.Commands.UpdateItemQuantity;
using Basket.Application.Queries.GetBasket;
using Basket.Application.Queries.GetBasketByCustomer;
using Basket.Domain.Entities;
using Basket.Domain.Repositories;
using FluentValidation;
using MediatR;
using Moq;
using Xunit;

namespace Basket.UnitTests;

/// <summary>
/// Tests unitaires pour le microservice Basket
/// </summary>
public class BasketServiceUnitTests
{
    private readonly Mock<IBasketRepository> _mockBasketRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public BasketServiceUnitTests()
    {
        _mockBasketRepository = new Mock<IBasketRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
    }

    #region Test 1: Création d'un panier vide

    [Fact]
    public async Task CreateBasket_ShouldCreateEmptyBasket_WhenCustomerIdIsValid()
    {
        // Arrange
        var customerId = "customer-123";
        var command = new CreateBasketCommand { CustomerId = customerId };

        _mockBasketRepository.Setup(x => x.AddAsync(It.IsAny<CustomerBasket>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateBasketCommandHandler(_mockBasketRepository.Object, _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.BasketId);
        Assert.Equal(customerId, result.CustomerId);
        _mockBasketRepository.Verify(x => x.AddAsync(It.IsAny<CustomerBasket>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Test 2: Validation - CustomerId requis

    [Fact]
    public async Task CreateBasket_ShouldThrowValidationException_WhenCustomerIdIsEmpty()
    {
        // Arrange
        var command = new CreateBasketCommand { CustomerId = "" };
        var validator = new CreateBasketCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(CreateBasketCommand.CustomerId));
    }

    #endregion

    #region Test 3: Ajouter un item au panier

    [Fact]
    public async Task AddItemToBasket_ShouldAddNewItem_WhenItemDoesNotExist()
    {
        // Arrange
        var basketId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var basket = CustomerBasket.Create("customer-123");

        var command = new AddItemToBasketCommand
        {
            BasketId = basketId,
            CatalogItemId = catalogItemId,
            ProductName = "Test Product",
            UnitPrice = 99.99m,
            Quantity = 2,
            PictureUrl = "http://test.com/image.jpg"
        };

        _mockBasketRepository.Setup(x => x.GetByIdAsync(basketId))
            .ReturnsAsync(basket);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new AddItemToBasketCommandHandler(_mockBasketRepository.Object, _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(basket.Items);
        Assert.Equal(catalogItemId, basket.Items.First().CatalogItemId);
        Assert.Equal(2, basket.Items.First().Quantity);
        Assert.Equal(99.99m, basket.Items.First().UnitPrice);
    }

    #endregion

    #region Test 4: Validation - Quantité doit être positive

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task AddItemToBasket_ShouldThrowValidationException_WhenQuantityIsNotPositive(int quantity)
    {
        // Arrange
        var command = new AddItemToBasketCommand
        {
            BasketId = Guid.NewGuid(),
            CatalogItemId = Guid.NewGuid(),
            ProductName = "Test Product",
            UnitPrice = 99.99m,
            Quantity = quantity,
            PictureUrl = "http://test.com/image.jpg"
        };
        var validator = new AddItemToBasketCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(AddItemToBasketCommand.Quantity));
    }

    #endregion

    #region Test 5: Validation - Prix unitaire doit être positif

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public async Task AddItemToBasket_ShouldThrowValidationException_WhenUnitPriceIsNotPositive(decimal unitPrice)
    {
        // Arrange
        var command = new AddItemToBasketCommand
        {
            BasketId = Guid.NewGuid(),
            CatalogItemId = Guid.NewGuid(),
            ProductName = "Test Product",
            UnitPrice = unitPrice,
            Quantity = 1,
            PictureUrl = "http://test.com/image.jpg"
        };
        var validator = new AddItemToBasketCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(AddItemToBasketCommand.UnitPrice));
    }

    #endregion

    #region Test 6: Supprimer un item du panier

    [Fact]
    public async Task RemoveItemFromBasket_ShouldRemoveItem_WhenItemExists()
    {
        // Arrange
        var basketId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var basket = CustomerBasket.Create("customer-123");
        basket.AddItem(catalogItemId, "Test Product", 99.99m, 2, "http://test.com/image.jpg");

        var command = new RemoveItemFromBasketCommand
        {
            BasketId = basketId,
            CatalogItemId = catalogItemId
        };

        _mockBasketRepository.Setup(x => x.GetByIdAsync(basketId))
            .ReturnsAsync(basket);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RemoveItemFromBasketCommandHandler(_mockBasketRepository.Object, _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Empty(basket.Items);
    }

    #endregion

    #region Test 7: Mettre à jour la quantité d'un item

    [Fact]
    public async Task UpdateItemQuantity_ShouldUpdateQuantity_WhenItemExists()
    {
        // Arrange
        var basketId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var basket = CustomerBasket.Create("customer-123");
        basket.AddItem(catalogItemId, "Test Product", 99.99m, 2, "http://test.com/image.jpg");

        var command = new UpdateItemQuantityCommand
        {
            BasketId = basketId,
            CatalogItemId = catalogItemId,
            NewQuantity = 5
        };

        _mockBasketRepository.Setup(x => x.GetByIdAsync(basketId))
            .ReturnsAsync(basket);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateItemQuantityCommandHandler(_mockBasketRepository.Object, _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, basket.Items.First().Quantity);
    }

    #endregion

    #region Test 8: Vider le panier

    [Fact]
    public async Task ClearBasket_ShouldRemoveAllItems_WhenBasketHasItems()
    {
        // Arrange
        var basketId = Guid.NewGuid();
        var basket = CustomerBasket.Create("customer-123");
        basket.AddItem(Guid.NewGuid(), "Product 1", 99.99m, 2, "http://test.com/1.jpg");
        basket.AddItem(Guid.NewGuid(), "Product 2", 49.99m, 1, "http://test.com/2.jpg");
        basket.AddItem(Guid.NewGuid(), "Product 3", 149.99m, 3, "http://test.com/3.jpg");

        var command = new ClearBasketCommand { BasketId = basketId };

        _mockBasketRepository.Setup(x => x.GetByIdAsync(basketId))
            .ReturnsAsync(basket);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new ClearBasketCommandHandler(_mockBasketRepository.Object, _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Empty(basket.Items);
    }

    #endregion

    #region Test 9: Supprimer un panier

    [Fact]
    public async Task DeleteBasket_ShouldDeleteBasket_WhenBasketExists()
    {
        // Arrange
        var basketId = Guid.NewGuid();
        var basket = CustomerBasket.Create("customer-123");

        var command = new DeleteBasketCommand { BasketId = basketId };

        _mockBasketRepository.Setup(x => x.GetByIdAsync(basketId))
            .ReturnsAsync(basket);
        _mockBasketRepository.Setup(x => x.Delete(basket))
            .Verifiable();
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteBasketCommandHandler(_mockBasketRepository.Object, _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockBasketRepository.Verify(x => x.Delete(basket), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Test 10: Récupérer un panier par ID

    [Fact]
    public async Task GetBasket_ShouldReturnBasket_WhenBasketExists()
    {
        // Arrange
        var basketId = Guid.NewGuid();
        var basket = CustomerBasket.Create("customer-123");
        basket.AddItem(Guid.NewGuid(), "Product 1", 99.99m, 2, "http://test.com/1.jpg");

        var query = new GetBasketQuery { BasketId = basketId };

        _mockBasketRepository.Setup(x => x.GetByIdAsync(basketId))
            .ReturnsAsync(basket);

        var handler = new GetBasketQueryHandler(_mockBasketRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(basket.CustomerId, result.CustomerId);
        Assert.Single(result.Items);
    }

    #endregion

    #region Test 11: Récupérer un panier par CustomerId

    [Fact]
    public async Task GetBasketByCustomer_ShouldReturnBasket_WhenCustomerHasBasket()
    {
        // Arrange
        var customerId = "customer-123";
        var basket = CustomerBasket.Create(customerId);
        basket.AddItem(Guid.NewGuid(), "Product 1", 99.99m, 2, "http://test.com/1.jpg");

        var query = new GetBasketByCustomerQuery { CustomerId = customerId };

        _mockBasketRepository.Setup(x => x.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(basket);

        var handler = new GetBasketByCustomerQueryHandler(_mockBasketRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Single(result.Items);
    }

    #endregion

    #region Test 12: Ajouter plusieurs fois le même produit incrémente la quantité

    [Fact]
    public async Task AddItemToBasket_ShouldIncrementQuantity_WhenSameProductAddedTwice()
    {
        // Arrange
        var basketId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var basket = CustomerBasket.Create("customer-123");
        basket.AddItem(catalogItemId, "Test Product", 99.99m, 2, "http://test.com/image.jpg");

        var command = new AddItemToBasketCommand
        {
            BasketId = basketId,
            CatalogItemId = catalogItemId,
            ProductName = "Test Product",
            UnitPrice = 99.99m,
            Quantity = 3,
            PictureUrl = "http://test.com/image.jpg"
        };

        _mockBasketRepository.Setup(x => x.GetByIdAsync(basketId))
            .ReturnsAsync(basket);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new AddItemToBasketCommandHandler(_mockBasketRepository.Object, _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(basket.Items);
        Assert.Equal(5, basket.Items.First().Quantity); // 2 + 3
    }

    #endregion

    #region Test 13: Validation - ProductName requis

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddItemToBasket_ShouldThrowValidationException_WhenProductNameIsEmpty(string productName)
    {
        // Arrange
        var command = new AddItemToBasketCommand
        {
            BasketId = Guid.NewGuid(),
            CatalogItemId = Guid.NewGuid(),
            ProductName = productName,
            UnitPrice = 99.99m,
            Quantity = 1,
            PictureUrl = "http://test.com/image.jpg"
        };
        var validator = new AddItemToBasketCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(AddItemToBasketCommand.ProductName));
    }

    #endregion
}
