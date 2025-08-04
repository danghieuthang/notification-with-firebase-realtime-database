using BackendAPI.Services;

namespace BackendAPI.Services
{
    /// <summary>
    /// Centralized notification factory service for creating standardized notifications
    /// </summary>
    public interface INotificationFactory
    {
        /// <summary>
        /// Create a welcome notification for new user registration
        /// </summary>
        Task<string> CreateWelcomeNotificationAsync(string userId);
        
        /// <summary>
        /// Create a random notification for testing purposes
        /// </summary>
        Task<string> CreateRandomNotificationAsync(string userId);
        
        /// <summary>
        /// Create a custom notification with specified data
        /// </summary>
        Task<string> CreateCustomNotificationAsync(string userId, string title, string body, Dictionary<string, string>? data = null);
    }

    /// <summary>
    /// Notification factory implementation
    /// </summary>
    public class NotificationFactory : INotificationFactory
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<NotificationFactory> _logger;
        
        // Predefined notification templates
        private static readonly string[] RandomTitles = {
            "Breaking News!", "Important Update", "New Message", "Alert!", "Information"
        };
        
        private static readonly string[] RandomMessages = {
            "You have received a new message from the system.",
            "This is an important update for your account.",
            "A new feature has been added to your dashboard.",
            "Your request has been processed successfully.",
            "Please check your account for important information."
        };
        
        private static readonly string[] RandomCategories = { "info", "warning", "success" };

        public NotificationFactory(IFirebaseService firebaseService, ILogger<NotificationFactory> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        /// <summary>
        /// Create a welcome notification for new user registration
        /// </summary>
        public async Task<string> CreateWelcomeNotificationAsync(string userId)
        {
            var welcomeTitle = "Welcome!";
            var welcomeBody = "You have successfully registered to our notification system.";
            var data = new Dictionary<string, string>
            {
                { "type", "welcome" },
                { "category", "success" },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            var notificationId = await _firebaseService.SendNotificationAsync(userId, welcomeTitle, welcomeBody, data);
            _logger.LogInformation("Welcome notification created for user: {UserId}", userId);
            
            return notificationId;
        }

        /// <summary>
        /// Create a random notification for testing purposes
        /// </summary>
        public async Task<string> CreateRandomNotificationAsync(string userId)
        {
            var random = new Random();
            var randomTitle = RandomTitles[random.Next(RandomTitles.Length)];
            var randomBody = RandomMessages[random.Next(RandomMessages.Length)];
            var randomData = new Dictionary<string, string>
            {
                { "type", "random" },
                { "priority", random.Next(1, 4).ToString() },
                { "category", RandomCategories[random.Next(RandomCategories.Length)] },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            var notificationId = await _firebaseService.SendNotificationAsync(userId, randomTitle, randomBody, randomData);
            _logger.LogInformation("Random notification created for user: {UserId} with title: {Title}", userId, randomTitle);
            
            return notificationId;
        }

        /// <summary>
        /// Create a custom notification with specified data
        /// </summary>
        public async Task<string> CreateCustomNotificationAsync(string userId, string title, string body, Dictionary<string, string>? data = null)
        {
            // Ensure timestamp is always included
            data ??= new Dictionary<string, string>();
            data.TryAdd("timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            data.TryAdd("type", "custom");

            var notificationId = await _firebaseService.SendNotificationAsync(userId, title, body, data);
            _logger.LogInformation("Custom notification created for user: {UserId} with title: {Title}", userId, title);
            
            return notificationId;
        }
    }
}
