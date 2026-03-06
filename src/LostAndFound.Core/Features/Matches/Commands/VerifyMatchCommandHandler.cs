using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using MediatR;

namespace LostAndFound.Core.Features.Matches.Commands;

public class VerifyMatchCommandHandler : IRequestHandler<VerifyMatchCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public VerifyMatchCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(VerifyMatchCommand command, CancellationToken cancellationToken)
    {
        var match = await _unitOfWork.Matches.GetAsync(
            predicate: m => m.Id == command.MatchId,
            isTracking: true,
            includes: [m => m.LostItem, m => m.FoundItem]
        );

        if (match is null)
            return false;

        bool success = command.IsApproved
            ? match.Approve(command.AdminId)
            : match.Reject(command.AdminId, command.RejectionReason ?? string.Empty);

        if (!success)
            return false;

        await _unitOfWork.SaveAsync();

        if (command.IsApproved)
        {
            var notifications = new List<Notification>
            {
                new Notification
                {
                    UserId = match.LostItem.UserId,
                    Title = "Match Confirmed",
                    Message = "Your match was confirmed by administration!",
                },
                new Notification
                {
                    UserId = match.FoundItem.UserId,
                    Title = "Match Confirmed",
                    Message = "Your match was confirmed by administration!",
                },
            };

            await _unitOfWork.Notifications.AddRangeAsync(notifications);
            await _unitOfWork.SaveAsync();
        }

        return true;
    }
}
