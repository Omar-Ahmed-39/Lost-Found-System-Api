namespace LostAndFound.Core.Interfaces;

public interface IPushNotificationService
{
    Task<bool> SendPushNotificationAsync(string deviceToken, string title, string body);
}