using Microsoft.EntityFrameworkCore;

namespace CaterinGO.Data;

public sealed class WaitingListService(
    IDbContextFactory<CaterinGoDbContext> dbContextFactory,
    ILogger<WaitingListService> logger) : IWaitingListService
{
    public async Task<WaitingListResult> AddOrRefreshAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var existingEntry = await dbContext.WaitingListEntries
            .SingleOrDefaultAsync(entry => entry.Email == normalizedEmail, cancellationToken);

        if (existingEntry is not null)
        {
            existingEntry.DateOfEntry = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return WaitingListResult.Refreshed;
        }

        dbContext.WaitingListEntries.Add(new WaitingListEntry
        {
            Email = normalizedEmail,
            DateOfEntry = DateTime.UtcNow
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return WaitingListResult.Added;
        }
        catch (DbUpdateException)
        {
            logger.LogWarning("Wait-list insert for {Email} conflicted or failed; checking for a concurrent entry.", normalizedEmail);
            await using var retryContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var concurrentEntry = await retryContext.WaitingListEntries
                .SingleOrDefaultAsync(entry => entry.Email == normalizedEmail, cancellationToken);

            if (concurrentEntry is null)
            {
                logger.LogError("Wait-list entry for {Email} could not be saved because no concurrent entry was found.", normalizedEmail);
                throw;
            }

            concurrentEntry.DateOfEntry = DateTime.UtcNow;
            await retryContext.SaveChangesAsync(cancellationToken);
            return WaitingListResult.Refreshed;
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
