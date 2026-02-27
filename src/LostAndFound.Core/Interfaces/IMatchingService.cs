namespace LostAndFound.Core.Interfaces;

public interface IMatchingService
{
    Task ProcessMatchesForRepoertAsync(int newReportId);
}