using System;
using MediatR;

namespace Catalog.Application.Commands.Reviews
{
    public record DeleteReviewCommand(Guid ReviewId, string UserId) : IRequest<bool>;
}
