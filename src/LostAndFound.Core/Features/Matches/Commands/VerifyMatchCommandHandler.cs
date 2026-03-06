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
        var match = await _unitOfWork.Matches.FindAsync(command.MatchId);

        if (match is null)
            return false;

        bool success = command.IsApproved
            ? match.Approve(command.AdminId)
            : match.Reject(command.AdminId, command.RejectionReason ?? string.Empty);

        if (!success)
            return false;

        await _unitOfWork.SaveAsync();
        return true;
    }
}
