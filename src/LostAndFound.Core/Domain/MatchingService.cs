using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;

namespace LostAndFound.Core.Domain;

public class MatchingService : IMatchingService
{
    private readonly IUnitOfWork _unitOfWork;

    public MatchingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessMatchesForReportAsync(int newReportId)
    {
        var report = await _unitOfWork.ItemReports.GetAsync(x => x.Id == newReportId);
        if (report == null) return;

        var targetType = report.ReportType == enReportType.Lost ? enReportType.Found : enReportType.Lost;

        var minDate = report.DateReported.AddDays(-30);
        var maxDate = report.DateReported.AddDays(30);

        var candidates = await _unitOfWork.ItemReports.GetAllAsync(
            predicate: t => t.ReportType == targetType
                        && t.CategoryId == report.CategoryId
                        && t.StatusType == enStatusType.Open
                        && t.DateReported >= minDate
                        && t.DateReported <= maxDate,
            isTracking: false
        );

        // Track candidate id alongside each match so we can notify only matched candidates
        var potentialMatches = new List<(Match Match, int CandidateId)>();
        foreach (var candidate in candidates)
        {
            var matchScore = CalculateSimilarityScore(report, candidate);
            if (matchScore > 30.0f)
            {
                var lostId  = report.ReportType == enReportType.Lost  ? report.Id : candidate.Id;
                var foundId = report.ReportType == enReportType.Found ? report.Id : candidate.Id;
                potentialMatches.Add((Match.Create(lostId, foundId, matchScore, matchedBy: 0), candidate.Id));
            }
        }

        var topMatches = potentialMatches
            .OrderByDescending(m => m.Match.MatchScore)
            .Take(5)
            .ToList();

        if (topMatches.Any())
        {
            await _unitOfWork.Matches.AddRangeAsync(topMatches.Select(m => m.Match));

            var candidateById = candidates.ToDictionary(c => c.Id);
            var notifiedUsers = new HashSet<int> { report.UserId };
            foreach (var (_, candidateId) in topMatches)
            {
                if (candidateById.TryGetValue(candidateId, out var matchedCandidate))
                    notifiedUsers.Add(matchedCandidate.UserId);
            }

            var notifications = notifiedUsers.Select(uid => new Notification
            {
                UserId = uid,
                Title = "Potential Match Found",
                Message = "A potential match was found for your item!",
            }).ToList();

            await _unitOfWork.Notifications.AddRangeAsync(notifications);
            await _unitOfWork.SaveAsync();
        }
    }

    private static float CalculateSimilarityScore(ItemReport report, ItemReport candidate)
    {
        if (report.CategoryId != candidate.CategoryId)
            return 0; 

        float score = 0;

        if (report.LocationId == candidate.LocationId)
            score += 50;

        var daysDiff = Math.Abs((report.DateReported - candidate.DateReported).TotalDays);
        if (daysDiff <= 3)
            score += 30;
        else if (daysDiff <= 7)
            score += 15;

        
        if (!string.IsNullOrWhiteSpace(report.Color) &&
            report.Color.Equals(candidate.Color, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
        }

        return Math.Min(score, 100);
    }
}