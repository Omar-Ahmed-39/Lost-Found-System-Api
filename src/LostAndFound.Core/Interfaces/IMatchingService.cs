namespace LostAndFound.Core.Interfaces;

public interface IMatchingService
{
    Task ProcessMatchesForReportAsync(int newReportId);
}