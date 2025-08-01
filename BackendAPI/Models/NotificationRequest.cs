namespace BackendAPI.Models
{
    public class RegisterRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string AccessUrl { get; set; } = string.Empty;
        public string NotificationId { get; set; } = string.Empty;
        public string FirebaseUrl { get; set; } = string.Empty;
    }

    public class SendRandomRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class SendNotificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NotificationId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    // Keep existing models for backward compatibility
    public class NotificationRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string>? Data { get; set; }
    }

    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NotificationId { get; set; } = string.Empty;
    }

    public class GenerateUrlRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class GenerateUrlResponse
    {
        public string AccessUrl { get; set; } = string.Empty;
        public string NotificationId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }
}
