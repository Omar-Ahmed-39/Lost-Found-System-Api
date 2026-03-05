namespace LostAndFound.Core.Enums;

public enum enStatusType
{
    Open = 1,       
    UnderReview = 2,
    Matched = 3,    
    Approved = 4,   
    Rejected = 5,   
    Returned = 6,
    Canceled = 7, // if the user want to cancel his report, or if the admin want to cancel it for any reason, 
    Closed = 8    
}

