using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Notification = FirebaseAdmin.Messaging.Notification;

namespace LostAndFound.Infrastructure.Repository;

public class FirebaseNotificationService : IPushNotificationService
{
    private readonly ILogger<FirebaseNotificationService> _logger;

    public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendPushNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        if (string.IsNullOrEmpty(deviceToken))
            return false;

        var message = new Message
        {
            Token = deviceToken,
            // Notification = the OS visual banner (title + body in tray)
            Notification = new Notification
            {
                Title = title,
                Body  = body
            },
            // Data = key-value payload delivered to the app's onMessage handler.
            // The mobile app reads "matchId" here to navigate to the correct screen.
            Data = data ?? new Dictionary<string, string>()
        };

        try
        {
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Successfully sent FCM message: {MessageId}", response);
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning(
                    "Device token {DeviceToken} is unregistered. Remove it from the database.",
                    deviceToken);
            }
            else
            {
                _logger.LogError(
                    ex,
                    "Firebase FCM error sending to {DeviceToken}: {Message}",
                    deviceToken, ex.Message);
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending FCM notification to {DeviceToken}", deviceToken);
            return false;
        }
    }
}