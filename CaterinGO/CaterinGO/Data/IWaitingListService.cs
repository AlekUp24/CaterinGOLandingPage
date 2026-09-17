namespace CaterinGO.Data;

public interface IWaitingListService
{
    Task<WaitingListResult> AddOrRefreshAsync(string email, CancellationToken cancellationToken = default);
}
