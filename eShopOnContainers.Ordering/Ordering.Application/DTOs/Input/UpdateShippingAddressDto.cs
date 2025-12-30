namespace Ordering.Application.DTOs.Input
{
    public record UpdateShippingAddressDto
    {
        public string ShippingAddress { get; init; } = string.Empty;
    }
}
