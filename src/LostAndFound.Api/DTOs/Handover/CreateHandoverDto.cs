using LostAndFound.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace LostAndFound.Api.DTOs.Handovers;

public class CreateHandoverDto
{
    public enIdType IdType { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public IFormFile? IdPhoto { get; set; }
    public IFormFile? SignatureImage { get; set; }
    public DateTime HandoverDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public int ReceiverUserId { get; set; }
    public int ClaimId { get; set; }
}