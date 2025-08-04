using Firebase.Database;
using Firebase.Database.Query;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackendAPI.Services
{
    public interface IFirebaseService
    {
        Task<string> SendNotificationAsync(string userId, string title, string body, Dictionary<string, string>? data = null);
    }

    public class NotificationData
    {
        public string title { get; set; } = string.Empty;
        public string body { get; set; } = string.Empty;
        public string timestamp { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
    }

    public class FirebaseService : IFirebaseService
    {
        private readonly ILogger<FirebaseService> _logger;
        private readonly FirebaseClient? _firebaseClient;
        private readonly IHashService _hashService;

        public FirebaseService(
            IFirebaseConfigurationService firebaseConfig, 
            ILogger<FirebaseService> logger, 
            IHashService hashService)
        {
            _logger = logger;
            _hashService = hashService;

            try
            {
                var databaseUrl = firebaseConfig.GetDatabaseUrlAsync().GetAwaiter().GetResult();
                _firebaseClient = new FirebaseClient(databaseUrl);
                _logger.LogInformation("Firebase client successfully initialized with URL: {DatabaseUrl}", databaseUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase service");
                throw;
            }
        }

        public async Task<string> SendNotificationAsync(string userId, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var notificationId = Guid.NewGuid().ToString();
                var notification = new NotificationData
                {
                    title = title,
                    body = body,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    type = data?.GetValueOrDefault("type", "notification") ?? "notification"
                };

                if (_firebaseClient != null)
                {
                    // Hash userId for privacy and use it in Firebase path
                    var hashedUserId = _hashService.HashUserId(userId);
                    
                    // Save notification to Firebase: notifications/{hashedUserId}/{notificationId}
                    await _firebaseClient
                        .Child("notifications")
                        .Child(hashedUserId)
                        .Child(notificationId)
                        .PutAsync(notification);

                    _logger.LogInformation("Notification sent to Firebase for user {UserId} (hashed: {HashedUserId}): {Title}", 
                        userId, hashedUserId, title);
                    return notificationId;
                }
                else
                {
                    // Mock response for demo
                    _logger.LogInformation("Mock notification saved: {Title} - {Body} for user: {UserId}", title, body, userId);
                    return notificationId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                throw;
            }
        }
    }
}
