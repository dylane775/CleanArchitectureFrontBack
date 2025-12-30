using Catalog.Application.Commands.CreateCatalogItem;
using Catalog.Application.Commands.DeleteCatalogItem;
using Catalog.Application.Commands.UpdateCatalogItem;
using Catalog.Application.Commands.UpdateStock;
using Catalog.Application.Queries.GetCatalogBrands;
using Catalog.Application.Queries.GetCatalogItemById;
using Catalog.Application.Queries.GetCatalogTypes;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using FluentValidation;
using Moq;
using Xunit;

namespace Catalog.UnitTests;

/// <summary>
/// Tests unitaires pour le microservice Catalog
/// </summary>
public class CatalogServiceUnitTests
{
    private readonly Mock<ICatalogItemRepository> _mockCatalogItemRepository;
    private readonly Mock<ICatalogTypeRepository> _mockCatalogTypeRepository;
    private readonly Mock<ICatalogBrandRepository> _mockCatalogBrandRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public CatalogServiceUnitTests()
    {
        _mockCatalogItemRepository = new Mock<ICatalogItemRepository>();
        _mockCatalogTypeRepository = new Mock<ICatalogTypeRepository>();
        _mockCatalogBrandRepository = new Mock<ICatalogBrandRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
    }

    #region Test 1: Créer un produit

    [Fact]
    public async Task CreateCatalogItem_ShouldCreateProduct_WhenDataIsValid()
    {
        // Arrange
        var typeId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var command = new CreateCatalogItemCommand
        {
            Name = "iPhone 15 Pro",
            Description = "Latest Apple smartphone",
            Price = 1199.99m,
            PictureFileName = "iphone15.jpg",
            PictureUri = "http://catalog.com/iphone15.jpg",
            CatalogTypeId = typeId,
            CatalogBrandId = brandId,
            AvailableStock = 100,
            RestockThreshold = 10,
            MaxStockThreshold = 500
        };

        _mockCatalogItemRepository.Setup(x => x.AddAsync(It.IsAny<CatalogItem>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateCatalogItemCommandHandler(
            _mockCatalogItemRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("iPhone 15 Pro", result.Name);
        Assert.Equal(1199.99m, result.Price);
        _mockCatalogItemRepository.Verify(x => x.AddAsync(It.IsAny<CatalogItem>()), Times.Once);
    }

    #endregion

    #region Test 2: Validation - Nom du produit requis

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateCatalogItem_ShouldThrowValidationException_WhenNameIsEmpty(string name)
    {
        // Arrange
        var command = new CreateCatalogItemCommand
        {
            Name = name,
            Description = "Test Description",
            Price = 99.99m,
            CatalogTypeId = Guid.NewGuid(),
            CatalogBrandId = Guid.NewGuid(),
            AvailableStock = 10
        };
        var validator = new CreateCatalogItemCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(CreateCatalogItemCommand.Name));
    }

    #endregion

    #region Test 3: Validation - Prix doit être positif

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999.99)]
    public async Task CreateCatalogItem_ShouldThrowValidationException_WhenPriceIsNotPositive(decimal price)
    {
        // Arrange
        var command = new CreateCatalogItemCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = price,
            CatalogTypeId = Guid.NewGuid(),
            CatalogBrandId = Guid.NewGuid(),
            AvailableStock = 10
        };
        var validator = new CreateCatalogItemCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(CreateCatalogItemCommand.Price));
    }

    #endregion

    #region Test 4: Mettre à jour un produit

    [Fact]
    public async Task UpdateCatalogItem_ShouldUpdateProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = CatalogItem.Create(
            "Old Product Name",
            "Old Description",
            99.99m,
            "old.jpg",
            "http://catalog.com/old.jpg",
            Guid.NewGuid(),
            Guid.NewGuid(),
            10, 5, 100);

        var command = new UpdateCatalogItemCommand
        {
            Id = productId,
            Name = "Updated Product Name",
            Description = "Updated Description",
            Price = 149.99m,
            PictureFileName = "updated.jpg",
            PictureUri = "http://catalog.com/updated.jpg",
            CatalogTypeId = Guid.NewGuid(),
            CatalogBrandId = Guid.NewGuid()
        };

        _mockCatalogItemRepository.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateCatalogItemCommandHandler(
            _mockCatalogItemRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Test 5: Supprimer un produit

    [Fact]
    public async Task DeleteCatalogItem_ShouldDeleteProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CatalogItem.Create(
            "Test Product",
            "Test Description",
            99.99m,
            "test.jpg",
            "http://catalog.com/test.jpg",
            Guid.NewGuid(),
            Guid.NewGuid(),
            10, 5, 100);

        var command = new DeleteCatalogItemCommand { Id = productId };

        _mockCatalogItemRepository.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockCatalogItemRepository.Setup(x => x.Delete(product))
            .Verifiable();
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteCatalogItemCommandHandler(
            _mockCatalogItemRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockCatalogItemRepository.Verify(x => x.Delete(product), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Test 6: Ajouter du stock

    [Fact]
    public async Task UpdateStock_ShouldAddStock_WhenQuantityIsPositive()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CatalogItem.Create(
            "Test Product",
            "Test Description",
            99.99m,
            "test.jpg",
            "http://catalog.com/test.jpg",
            Guid.NewGuid(),
            Guid.NewGuid(),
            50, 10, 200);

        var command = new UpdateStockCommand
        {
            CatalogItemId = productId,
            Quantity = 30
        };

        _mockCatalogItemRepository.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateStockCommandHandler(
            _mockCatalogItemRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(80, product.AvailableStock); // 50 + 30
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Test 7: Retirer du stock

    [Fact]
    public async Task UpdateStock_ShouldRemoveStock_WhenQuantityIsNegative()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CatalogItem.Create(
            "Test Product",
            "Test Description",
            99.99m,
            "test.jpg",
            "http://catalog.com/test.jpg",
            Guid.NewGuid(),
            Guid.NewGuid(),
            50, 10, 200);

        var command = new UpdateStockCommand
        {
            CatalogItemId = productId,
            Quantity = -20
        };

        _mockCatalogItemRepository.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateStockCommandHandler(
            _mockCatalogItemRepository.Object,
            _mockUnitOfWork.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(30, product.AvailableStock); // 50 - 20
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Test 8: Récupérer un produit par ID

    [Fact]
    public async Task GetCatalogItemById_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CatalogItem.Create(
            "Test Product",
            "Test Description",
            99.99m,
            "test.jpg",
            "http://catalog.com/test.jpg",
            Guid.NewGuid(),
            Guid.NewGuid(),
            50, 10, 200);

        var query = new GetCatalogItemByIdQuery { Id = productId };

        _mockCatalogItemRepository.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        var handler = new GetCatalogItemByIdQueryHandler(_mockCatalogItemRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal(50, result.AvailableStock);
    }

    #endregion

    #region Test 9: Récupérer toutes les marques

    [Fact]
    public async Task GetCatalogBrands_ShouldReturnAllBrands()
    {
        // Arrange
        var brands = new List<CatalogBrand>
        {
            CatalogBrand.Create("Apple"),
            CatalogBrand.Create("Samsung"),
            CatalogBrand.Create("Sony")
        };

        var query = new GetCatalogBrandsQuery();

        _mockCatalogBrandRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(brands);

        var handler = new GetCatalogBrandsQueryHandler(_mockCatalogBrandRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Contains(result, b => b.Brand == "Apple");
        Assert.Contains(result, b => b.Brand == "Samsung");
        Assert.Contains(result, b => b.Brand == "Sony");
    }

    #endregion

    #region Test 10: Récupérer tous les types

    [Fact]
    public async Task GetCatalogTypes_ShouldReturnAllTypes()
    {
        // Arrange
        var types = new List<CatalogType>
        {
            CatalogType.Create("Smartphones"),
            CatalogType.Create("Laptops"),
            CatalogType.Create("Tablets")
        };

        var query = new GetCatalogTypesQuery();

        _mockCatalogTypeRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(types);

        var handler = new GetCatalogTypesQueryHandler(_mockCatalogTypeRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Contains(result, t => t.Type == "Smartphones");
        Assert.Contains(result, t => t.Type == "Laptops");
        Assert.Contains(result, t => t.Type == "Tablets");
    }

    #endregion

    #region Test 11: Stock ne peut pas être négatif

    [Fact]
    public void UpdateStock_ShouldThrowException_WhenStockBecomeNegative()
    {
        // Arrange
        var product = CatalogItem.Create(
            "Test Product",
            "Test Description",
            99.99m,
            "test.jpg",
            "http://catalog.com/test.jpg",
            Guid.NewGuid(),
            Guid.NewGuid(),
            10, 5, 100);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => product.RemoveStock(20));
    }

    #endregion

    #region Test 12: Validation - Stock disponible ne peut pas être négatif

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateCatalogItem_ShouldThrowValidationException_WhenAvailableStockIsNegative(int stock)
    {
        // Arrange
        var command = new CreateCatalogItemCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            CatalogTypeId = Guid.NewGuid(),
            CatalogBrandId = Guid.NewGuid(),
            AvailableStock = stock,
            RestockThreshold = 10,
            MaxStockThreshold = 100
        };
        var validator = new CreateCatalogItemCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(CreateCatalogItemCommand.AvailableStock));
    }

    #endregion

    #region Test 13: Changement de prix déclenche un événement de domaine

    [Fact]
    public void UpdateDetails_ShouldRaisePriceChangedEvent_WhenPriceChanges()
    {
        // Arrange
        var product = CatalogItem.Create(
            "Test Product",
            "Test Description",
            99.99m,
            "test.jpg",
            "http://catalog.com/test.jpg",
            Guid.NewGuid(),
            Guid.NewGuid(),
            10, 5, 100);

        // Act
        product.UpdateDetails(
            "Test Product",
            "Test Description",
            149.99m, // Prix changé
            "test.jpg",
            "http://catalog.com/test.jpg",
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        Assert.Contains(product.DomainEvents, e => e.GetType().Name == "ProductPriceChangedDomainEvent");
    }

    #endregion

    #region Test 14: MaxStockThreshold doit être supérieur à RestockThreshold

    [Fact]
    public async Task CreateCatalogItem_ShouldThrowValidationException_WhenMaxStockLessThanRestockThreshold()
    {
        // Arrange
        var command = new CreateCatalogItemCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            CatalogTypeId = Guid.NewGuid(),
            CatalogBrandId = Guid.NewGuid(),
            AvailableStock = 50,
            RestockThreshold = 100,
            MaxStockThreshold = 50 // Inférieur à RestockThreshold
        };
        var validator = new CreateCatalogItemCommandValidator();

        // Act & Assert
        var validationResult = await validator.ValidateAsync(command);
        Assert.False(validationResult.IsValid);
    }

    #endregion
}
