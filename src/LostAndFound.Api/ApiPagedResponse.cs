namespace LostAndFound.Api;

public class ApiPagedResponse<T> : ApiResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }

    public ApiPagedResponse(T data, int pageNumber, int pageSize, int totalRecords, string? message = null)
    {
        Succeeded = true;
        Data = data;
        Message = message ?? "Data Fetched Successfully .";
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        TotalRecords = totalRecords;
    }
}