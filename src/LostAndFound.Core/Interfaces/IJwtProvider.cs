using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user, IList<string> roles);
}