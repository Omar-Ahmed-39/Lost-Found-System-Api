using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.VisualBasic;

namespace LostAndFound.Core.Domain;

public class MatchingService : IMatchingService
{
    private readonly IUnitOfWork _unitOfWork;

    public MatchingService(IUnitOfWork unitOfWork, IMatchRepository matchRepository)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessMatchesForReportAsync(int newReportId)
    {
        var report = await _unitOfWork.ItemReports.GetAsync(x => x.Id == newReportId);
        if (report == null) return;

        // Determine the target report type to match against
        var targetType = report.ReportType == enReportType.Lost ? enReportType.Found : enReportType.Lost;

        var candidates = await _unitOfWork.ItemReports.GetAllAsync(
            predicate: t => t.ReportType == targetType
                        && t.CategoryId == report.CategoryId
                        && t.StatusType == enStatusType.Open,
            isTracking: false
        );

        var potentialMatches = new List<Match>();
        foreach (var candidate in candidates)
        {
            var matchScore = CalculateSimilarityScore(report, candidate);
            if (matchScore > 30.0f) // Only consider matches with a positive score
            {
                potentialMatches.Add(new Match
                {
                    LostId = report.ReportType == enReportType.Lost ? report.Id : candidate.Id,
                    FoundId = report.ReportType == enReportType.Found ? report.Id : candidate.Id,
                    MatchScore = matchScore,
                    Status = enMatchStatus.Pending,
                    MatchDate = DateTime.UtcNow
                });
            }

            var topMatches = potentialMatches
                .OrderByDescending(m => m.MatchScore)
                .Take(5)
                .ToList();

            if (topMatches.Any())
                await _unitOfWork.Matches.AddRangeAsync(topMatches);
            await _unitOfWork.SaveAsync();
        }
    }

    private float CalculateSimilarityScore(ItemReport report, ItemReport candidate)
    {
        if (report.CategoryId != candidate.CategoryId)
            return 0; // Different categories are not a match , Hard filter for category
        float score = 0;

        if (report.LocationId == candidate.LocationId)
            score += 50; // Location match is a strong indicator

        var daysDiff = Math.Abs((report.DateReported - candidate.DateReported).TotalDays);
        if (daysDiff <= 3)
            score += 30;
        else if (daysDiff <= 7)
            score += 15;

        if (string.IsNullOrWhiteSpace(report.Color) &&
            report.Color.Equals(candidate.Color, StringComparison.OrdinalIgnoreCase)
        )
        {
            score += 10; // Color match adds to the score
        }

        return Math.Min(score, 100); // Cap the score at 100
    }
}