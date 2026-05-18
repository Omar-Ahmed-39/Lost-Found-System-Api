namespace LostAndFound.Core.Interfaces;

public interface IPushNotificationService
{
    /// <summary>
    /// Sends a Firebase push notification.
    /// The optional <paramref name="data"/> dictionary is placed in the FCM Data payload
    /// so the mobile app can parse it natively (e.g., route to a match screen via matchId).
    /// </summary>
    Task<bool> SendPushNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null);
}