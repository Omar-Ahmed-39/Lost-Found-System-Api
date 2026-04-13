using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IHandoverService
{
    Task<bool> CreateHandoverAsync(Handover handover, int adminUserId);
}