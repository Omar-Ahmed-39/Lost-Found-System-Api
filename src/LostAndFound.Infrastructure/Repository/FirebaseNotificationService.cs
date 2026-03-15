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
    public async Task<bool> SendPushNotificationAsync(string deviceToken, string title, string body)
    {
        if (string.IsNullOrEmpty(deviceToken))
            return false;

        var message = new Message()
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        try
        {
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation($"Successfully sent message: {response}");
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning($"Device token {deviceToken} is unregistered. Removing from database.");
            }
            else
            {
                _logger.LogError($"Firebase Error: {ex.Message}");
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex.Message}");
            return false;
        }
    }
}