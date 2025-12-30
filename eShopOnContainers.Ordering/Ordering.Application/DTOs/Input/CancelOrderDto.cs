namespace Ordering.Application.DTOs.Input
{
    public record CancelOrderDto
    {
        public string? Reason { get; init; }
    }
}
