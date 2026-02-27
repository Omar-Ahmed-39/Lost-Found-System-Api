namespace LostAndFound.Infrastructure.Repository;

public class FirebaseNotificationService : IPushNotificationService
{
    public Task<bool> SendAsync(string deviceToken, string title, string body)
    {
        throw new NotImplementedException();
    }
}