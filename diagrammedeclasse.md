```mermaid

classDiagram
    %% ========================================
    %% CATALOG MICROSERVICE - Clean Architecture
    %% ========================================
    
    namespace CatalogDomain {
        class CatalogItem {
            +Guid Id
            +string Name
            +string Description
            +decimal Price
            +string PictureUri
            +int CatalogTypeId
            +int CatalogBrandId
            +int AvailableStock
            +int RestockThreshold
            +int MaxStockThreshold
            +UpdateStock(quantity)
            +RemoveStock(quantity)
            +AddStock(quantity)
        }
        
        class CatalogType {
            +int Id
            +string Type
        }
        
        class CatalogBrand {
            +int Id
            +string Brand
        }
    }
    
    namespace CatalogApplication {
        class ICatalogService {
            <<interface>>
            +GetCatalogItems()
            +GetCatalogItemById(id)
            +CreateCatalogItem(item)
            +UpdateCatalogItem(item)
            +DeleteCatalogItem(id)
        }
        
        class CatalogService {
            -ICatalogRepository repository
            -IEventBus eventBus
            +GetCatalogItems()
            +GetCatalogItemById(id)
            +CreateCatalogItem(item)
        }
        
        class GetCatalogItemsQuery {
            +int PageSize
            +int PageIndex
        }
        
        class CreateCatalogItemCommand {
            +string Name
            +decimal Price
            +int CatalogTypeId
            +int CatalogBrandId
        }
    }
    
    namespace CatalogInfrastructure {
        class ICatalogRepository {
            <<interface>>
            +GetAsync(id)
            +GetAllAsync()
            +AddAsync(item)
            +UpdateAsync(item)
            +DeleteAsync(id)
        }
        
        class CatalogRepository {
            -CatalogContext context
            +GetAsync(id)
            +GetAllAsync()
            +AddAsync(item)
        }
        
        class CatalogContext {
            +DbSet~CatalogItem~ CatalogItems
            +DbSet~CatalogType~ CatalogTypes
            +DbSet~CatalogBrand~ CatalogBrands
        }
    }
    
    %% ========================================
    %% ORDERING MICROSERVICE - Clean Architecture
    %% ========================================
    
    namespace OrderingDomain {
        class Order {
            +Guid Id
            +DateTime OrderDate
            +string BuyerId
            +Address ShippingAddress
            +OrderStatus Status
            +List~OrderItem~ OrderItems
            +decimal Total
            +AddOrderItem(productId, quantity, price)
            +SetAwaitingValidationStatus()
            +SetStockConfirmedStatus()
            +SetPaidStatus()
            +SetShippedStatus()
        }
        
        class OrderItem {
            +Guid Id
            +string ProductName
            +string PictureUrl
            +decimal UnitPrice
            +int Units
            +Guid ProductId
        }
        
        class Address {
            +string Street
            +string City
            +string State
            +string Country
            +string ZipCode
        }
        
        class OrderStatus {
            <<enumeration>>
            Submitted
            AwaitingValidation
            StockConfirmed
            Paid
            Shipped
            Cancelled
        }
        
        class Buyer {
            +Guid Id
            +string IdentityGuid
            +string Name
            +List~PaymentMethod~ PaymentMethods
        }
        
        class PaymentMethod {
            +Guid Id
            +string Alias
            +string CardNumber
            +string SecurityNumber
            +string CardHolderName
            +DateTime Expiration
        }
    }
    
    namespace OrderingApplication {
        class IOrderService {
            <<interface>>
            +CreateOrder(order)
            +GetOrderById(id)
            +GetOrdersByUser(userId)
            +CancelOrder(orderId)
        }
        
        class CreateOrderCommand {
            +string UserId
            +string UserName
            +Address ShippingAddress
            +List~OrderItemDTO~ OrderItems
            +string CardNumber
            +string CardHolderName
        }
        
        class CreateOrderCommandHandler {
            -IOrderRepository orderRepository
            -IEventBus eventBus
            +Handle(command)
        }
        
        class OrderCreatedDomainEvent {
            +Guid OrderId
            +string BuyerId
            +List~OrderItem~ OrderItems
        }
    }
    
    namespace OrderingInfrastructure {
        class IOrderRepository {
            <<interface>>
            +GetAsync(orderId)
            +AddAsync(order)
            +UpdateAsync(order)
            +GetOrdersByBuyerIdAsync(buyerId)
        }
        
        class OrderRepository {
            -OrderingContext context
            +GetAsync(orderId)
            +AddAsync(order)
        }
        
        class OrderingContext {
            +DbSet~Order~ Orders
            +DbSet~OrderItem~ OrderItems
            +DbSet~Buyer~ Buyers
        }
    }
    
    %% ========================================
    %% BASKET MICROSERVICE - Clean Architecture
    %% ========================================
    
    namespace BasketDomain {
        class CustomerBasket {
            +string BuyerId
            +List~BasketItem~ Items
            +AddItem(productId, quantity, price)
            +UpdateQuantity(productId, quantity)
            +RemoveItem(productId)
        }
        
        class BasketItem {
            +string Id
            +string ProductId
            +string ProductName
            +decimal UnitPrice
            +int Quantity
            +string PictureUrl
        }
    }
    
    namespace BasketApplication {
        class IBasketService {
            <<interface>>
            +GetBasketAsync(customerId)
            +UpdateBasketAsync(basket)
            +DeleteBasketAsync(customerId)
            +CheckoutAsync(basketCheckout)
        }
        
        class UpdateBasketCommand {
            +string BuyerId
            +List~BasketItem~ Items
        }
        
        class BasketCheckoutCommand {
            +string BuyerId
            +string City
            +string Street
            +string State
            +string Country
            +string CardNumber
        }
    }
    
    namespace BasketInfrastructure {
        class IBasketRepository {
            <<interface>>
            +GetBasketAsync(customerId)
            +UpdateBasketAsync(basket)
            +DeleteBasketAsync(customerId)
        }
        
        class RedisBasketRepository {
            -IConnectionMultiplexer redis
            -IDatabase database
            +GetBasketAsync(customerId)
            +UpdateBasketAsync(basket)
        }
    }
    
    %% ========================================
    %% IDENTITY MICROSERVICE - Clean Architecture
    %% ========================================
    
    namespace IdentityDomain {
        class ApplicationUser {
            +string Id
            +string Email
            +string UserName
            +string Name
            +string LastName
            +string CardNumber
            +string SecurityNumber
            +string CardHolderName
            +DateTime CardExpiration
        }
    }
    
    namespace IdentityApplication {
        class IIdentityService {
            <<interface>>
            +RegisterUser(email, password)
            +LoginUser(email, password)
            +GetUserById(userId)
        }
        
        class RegisterCommand {
            +string Email
            +string Password
            +string Name
        }
        
        class LoginCommand {
            +string Email
            +string Password
        }
    }
    
    %% ========================================
    %% PAYMENT MICROSERVICE - Clean Architecture
    %% ========================================
    
    namespace PaymentDomain {
        class Payment {
            +Guid Id
            +Guid OrderId
            +DateTime PaymentDate
            +PaymentStatus Status
            +decimal Amount
            +string CardNumber
            +ProcessPayment()
            +ConfirmPayment()
            +RejectPayment()
        }
        
        class PaymentStatus {
            <<enumeration>>
            Pending
            Confirmed
            Rejected
        }
    }
    
    namespace PaymentApplication {
        class ProcessPaymentCommand {
            +Guid OrderId
            +decimal Amount
            +string CardNumber
            +string CardHolderName
        }
        
        class IPaymentService {
            <<interface>>
            +ProcessPayment(command)
        }
    }
    
    %% ========================================
    %% SHARED KERNEL
    %% ========================================
    
    namespace SharedKernel {
        class IEventBus {
            <<interface>>
            +Publish(event)
            +Subscribe(eventHandler)
        }
        
        class IntegrationEvent {
            +Guid Id
            +DateTime CreationDate
        }
        
        class IUnitOfWork {
            <<interface>>
            +SaveChangesAsync()
            +BeginTransactionAsync()
            +CommitTransactionAsync()
        }
        
        class Entity {
            <<abstract>>
            +Guid Id
            +List~DomainEvent~ DomainEvents
            +AddDomainEvent(event)
        }
        
        class DomainEvent {
            <<abstract>>
            +DateTime OccurredOn
        }
    }
    
    %% ========================================
    %% RELATIONSHIPS
    %% ========================================
    
    %% Catalog Relationships
    CatalogService ..|> ICatalogService
    CatalogService --> ICatalogRepository
    CatalogService --> IEventBus
    CatalogRepository ..|> ICatalogRepository
    CatalogRepository --> CatalogContext
    CatalogContext --> CatalogItem
    CatalogContext --> CatalogType
    CatalogContext --> CatalogBrand
    CatalogItem --> CatalogType
    CatalogItem --> CatalogBrand
    CatalogItem --|> Entity
    
    %% Ordering Relationships
    CreateOrderCommandHandler --> IOrderRepository
    CreateOrderCommandHandler --> IEventBus
    CreateOrderCommandHandler ..> CreateOrderCommand
    OrderRepository ..|> IOrderRepository
    OrderRepository --> OrderingContext
    OrderingContext --> Order
    OrderingContext --> OrderItem
    OrderingContext --> Buyer
    Order --> OrderItem
    Order --> Address
    Order --> OrderStatus
    Order --|> Entity
    Buyer --> PaymentMethod
    Buyer --|> Entity
    OrderCreatedDomainEvent --|> DomainEvent
    
    %% Basket Relationships
    RedisBasketRepository ..|> IBasketRepository
    CustomerBasket --> BasketItem
    
    %% Payment Relationships
    Payment --> PaymentStatus
    Payment --|> Entity
    
    %% Event Bus Relationships
    IntegrationEvent --|> DomainEvent