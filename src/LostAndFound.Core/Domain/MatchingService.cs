using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace LostAndFound.Core.Domain;

public class MatchingService : IMatchingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPushNotificationService _pushNotification;
    private readonly ILogger<MatchingService> _logger;

    public MatchingService(
        IUnitOfWork unitOfWork,
        IPushNotificationService pushNotification,
        ILogger<MatchingService> logger)
    {
        _unitOfWork = unitOfWork;
        _pushNotification = pushNotification;
        _logger = logger;
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

        // Score each candidate
        var potentialMatches = new List<(Match Match, int CandidateId)>();
        foreach (var candidate in candidates)
        {
            var matchScore = CalculateSimilarityScore(report, candidate);
            if (matchScore > 30.0f)
            {
                var lostId = report.ReportType == enReportType.Lost ? report.Id : candidate.Id;
                var foundId = report.ReportType == enReportType.Found ? report.Id : candidate.Id;
                potentialMatches.Add((Match.Create(lostId, foundId, matchScore, matchedBy: null), candidate.Id));
            }
        }

        var topMatches = potentialMatches
            .OrderByDescending(m => m.Match.MatchScore)
            .Take(5)
            .ToList();

        if (!topMatches.Any())
            return;

        // ── Dedup guard: skip pairs that already have a Match record ──────────
        var existingMatches = await _unitOfWork.Matches.GetAllAsync(
            predicate: m => m.LostId == report.Id || m.FoundId == report.Id,
            isTracking: false
        );
        var existingPairs = existingMatches
            .Select(m => (m.LostId, m.FoundId))
            .ToHashSet();

        var newMatches = topMatches
            .Where(t => !existingPairs.Contains((t.Match.LostId, t.Match.FoundId)))
            .ToList();

        if (!newMatches.Any())
            return;
        // ─────────────────────────────────────────────────────────────────────

        await _unitOfWork.Matches.AddRangeAsync(newMatches.Select(m => m.Match));

        // In-app notifications
        var candidateById = candidates.ToDictionary(c => c.Id);
        var notifiedUsers = new HashSet<int> { report.UserId };
        foreach (var (_, candidateId) in newMatches)
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

        // ── FCM push notifications with MatchId in the data dict ──────────────
        // The mobile app reads data["matchId"] to navigate to GET /api/matches/{id}.
        foreach (var (match, candidateId) in newMatches)
        {
            // Notify the reporter of the NEW report (the one that triggered matching)
            await TrySendMatchNotificationAsync(report.UserId, match.Id);

            // Notify the owner of the matched candidate report
            if (candidateById.TryGetValue(candidateId, out var candidate))
                await TrySendMatchNotificationAsync(candidate.UserId, match.Id);
        }
        // ─────────────────────────────────────────────────────────────────────
    }

    /// <summary>
    /// Loads the user's FCM token from the DB and sends a push notification .
    /// </summary>
    private async Task TrySendMatchNotificationAsync(int userId, int matchId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);
            if (user is null || string.IsNullOrEmpty(user.FcmToken))
                return;

            await _pushNotification.SendPushNotificationAsync(
                deviceToken: user.FcmToken,
                title: "Potential Match Found!",
                body: "We found a potential match for your lost item. Tap to review.",
                data: new Dictionary<string, string>
                {
                    { "matchId", matchId.ToString() },
                    { "type",    "match_found" }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send FCM notification for match {MatchId} to user {UserId}", matchId, userId);
        }
    }

    private static float CalculateSimilarityScore(ItemReport report, ItemReport candidate)
    {

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