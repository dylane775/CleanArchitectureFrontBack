using System;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Commands.Reviews
{
    public record VoteReviewCommand(Guid ReviewId, bool IsHelpful) : IRequest<ProductReviewDto>;
}
