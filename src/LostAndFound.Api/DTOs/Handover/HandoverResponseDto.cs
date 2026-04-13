using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.Handovers;

public class HandoverResponseDto
{
    public int Id { get; set; }
    public enIdType IdType { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public DateTime HandoverDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string HandedByName { get; set; } = string.Empty;
    public int ClaimId { get; set; }
}