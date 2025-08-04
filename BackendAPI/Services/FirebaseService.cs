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

        public FirebaseService(IConfiguration configuration, ILogger<FirebaseService> logger, IHashService hashService)
        {
            _logger = logger;
            _hashService = hashService;

            try
            {
                var serviceAccountPath = configuration["Firebase:ServiceAccountPath"] ?? "firebase-service-account.json";
                var configuredDatabaseUrl = configuration["Firebase:DatabaseUrl"];
                
                string databaseUrl;
                
                // Priority 1: Use explicitly configured DatabaseUrl if provided
                if (!string.IsNullOrWhiteSpace(configuredDatabaseUrl))
                {
                    databaseUrl = configuredDatabaseUrl;
                    _logger.LogInformation("Using explicitly configured Firebase Database URL");
                }
                // Priority 2: Auto-derive from service account project_id
                else if (File.Exists(serviceAccountPath))
                {
                    var serviceAccountJson = File.ReadAllText(serviceAccountPath);
                    var serviceAccount = JsonSerializer.Deserialize<Dictionary<string, object>>(serviceAccountJson);
                    
                    if (serviceAccount?.TryGetValue("project_id", out var projectIdObj) == true)
                    {
                        var projectId = projectIdObj.ToString();
                        var region = configuration["Firebase:Region"] ?? "asia-southeast1";
                        
                        // Support different Firebase Realtime Database regions
                        if (region != "us-central1")
                        {
                            databaseUrl = $"https://{projectId}-default-rtdb.{region}.firebasedatabase.app/";
                        }
                        else
                        {
                            databaseUrl = $"https://{projectId}-default-rtdb.firebaseio.com/";
                        }
                        
                        _logger.LogInformation("Auto-derived Firebase Database URL for project: {ProjectId}, region: {Region}", 
                            projectId, region);
                    }
                    else
                    {
                        throw new InvalidOperationException("project_id not found in service account file");
                    }
                }
                // Priority 3: Environment variable fallback
                else
                {
                    databaseUrl = Environment.GetEnvironmentVariable("FIREBASE_DATABASE_URL") ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(databaseUrl))
                    {
                        throw new InvalidOperationException(
                            "Firebase Database URL not configured. Please either:\n" +
                            "1. Set Firebase:DatabaseUrl in appsettings.json, or\n" +
                            "2. Ensure firebase-service-account.json exists with valid project_id, or\n" +
                            "3. Set FIREBASE_DATABASE_URL environment variable");
                    }
                    _logger.LogInformation("Using Firebase Database URL from environment variable");
                }
                
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
