using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Common.Interfaces
{
    /// <summary>
    /// Database context interface for Identity bounded context
    /// </summary>
    public interface IIdentityDbContext
    {
        /// <summary>
        /// Users DbSet
        /// </summary>
        DbSet<User> Users { get; }

        /// <summary>
        /// Roles DbSet
        /// </summary>
        DbSet<Role> Roles { get; }

        /// <summary>
        /// Refresh tokens DbSet
        /// </summary>
        DbSet<RefreshToken> RefreshTokens { get; }

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of affected rows</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
