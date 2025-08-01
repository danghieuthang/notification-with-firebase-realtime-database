using Firebase.Database;
using Firebase.Database.Query;
using System.Text.Json;

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

        public FirebaseService(ILogger<FirebaseService> logger)
        {
            _logger = logger;

            try
            {
                // Initialize Firebase Database với service account
                var firebaseUrl = "https://fir-notification-b6ff2-default-rtdb.asia-southeast1.firebasedatabase.app/"; // URL thực tế từ project của bạn

                // Đọc service account key file
                var serviceAccountPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-service-account.json");

                if (File.Exists(serviceAccountPath))
                {
                    var serviceAccountJson = File.ReadAllText(serviceAccountPath);
                    var serviceAccountData = JsonSerializer.Deserialize<Dictionary<string, object>>(serviceAccountJson);

                    // Tạo FirebaseClient với auth token (optional - cho production)
                    _firebaseClient = new FirebaseClient(firebaseUrl);
                    _logger.LogInformation("Firebase Database initialized successfully with service account");
                }
                else
                {
                    // Fallback cho development
                    _firebaseClient = new FirebaseClient(firebaseUrl);
                    _logger.LogInformation("Firebase Database initialized successfully (development mode)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Firebase Database initialization failed: {Message}. Using mock mode.", ex.Message);
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
                    // Save notification to Firebase: notifications/{userId}/{notificationId}
                    await _firebaseClient
                        .Child("notifications")
                        .Child(userId)
                        .Child(notificationId)
                        .PutAsync(notification);

                    _logger.LogInformation("Successfully saved notification to Firebase for user: {UserId}", userId);
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
