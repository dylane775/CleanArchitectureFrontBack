namespace Ordering.Application.DTOs.Input
{
    public record UpdateBillingAddressDto
    {
        public string BillingAddress { get; init; } = string.Empty;
    }
}
