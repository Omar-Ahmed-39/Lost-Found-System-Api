using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Infrastructure.Repository;

public class HandoverRepository : GenericRepository<Handover>, IHandoverRepository
{
    private readonly ApplicationDbContext _context;

    public HandoverRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Handover?> GetDetailsAsync(int id)
    {
        return await _context.Handovers
            .Include(h => h.Location)
            .Include(h => h.ReceiverUser)
            .Include(h => h.HandedByUser)
            .Include(h => h.Claim)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<Handover?> GetByClaimIdAsync(int claimId)
    {
        return await _context.Handovers
            .Include(h => h.Location)
            .Include(h => h.ReceiverUser)
            .Include(h => h.HandedByUser)
            .Include(h => h.Claim)
            .FirstOrDefaultAsync(h => h.ClaimId == claimId);
    }
}