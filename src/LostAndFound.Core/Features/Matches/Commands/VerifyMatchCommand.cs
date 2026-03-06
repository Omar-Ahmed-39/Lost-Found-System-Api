using MediatR;

namespace LostAndFound.Core.Features.Matches.Commands;

public record VerifyMatchCommand(
    int MatchId,
    int AdminId,
    bool IsApproved,
    string? RejectionReason = null
) : IRequest<bool>;
