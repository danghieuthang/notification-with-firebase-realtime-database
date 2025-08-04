using System.Security.Cryptography;
using System.Text;

namespace BackendAPI.Services
{
    public interface IHashService
    {
        string HashUserId(string userId);
        string CreateNotificationPath(string userId);
        string CreateListenUrl(string baseUrl, string userId);
    }

    public class HashService : IHashService
    {
        private readonly ILogger<HashService> _logger;

        public HashService(ILogger<HashService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Hash userId using SHA256 and return first 12 characters for privacy and shorter URLs
        /// </summary>
        /// <param name="userId">Original user ID</param>
        /// <returns>Hashed user ID (12 characters)</returns>
        public string HashUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
            }

            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var inputBytes = Encoding.UTF8.GetBytes(userId.Trim().ToLowerInvariant());
                    var hashBytes = sha256.ComputeHash(inputBytes);
                    var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();
                    
                    // Return first 12 characters for shorter, readable URLs
                    var hashedUserId = hashString[..12];
                    
                    _logger.LogDebug("Hashed userId '{UserId}' to '{HashedUserId}'", userId, hashedUserId);
                    return hashedUserId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to hash userId: {UserId}", userId);
                throw new InvalidOperationException("Failed to hash user ID", ex);
            }
        }

        /// <summary>
        /// Create Firebase notification path with hashed user ID
        /// </summary>
        /// <param name="userId">Original user ID</param>
        /// <returns>Firebase path: notifications/{hashedUserId}</returns>
        public string CreateNotificationPath(string userId)
        {
            var hashedUserId = HashUserId(userId);
            return $"notifications/{hashedUserId}";
        }

        /// <summary>
        /// Create complete Firebase listen URL with hashed user ID
        /// </summary>
        /// <param name="baseUrl">Firebase database base URL</param>
        /// <param name="userId">Original user ID</param>
        /// <returns>Complete Firebase listen URL</returns>
        public string CreateListenUrl(string baseUrl, string userId)
        {
            var notificationPath = CreateNotificationPath(userId);
            return $"{baseUrl.TrimEnd('/')}/{notificationPath}";
        }
    }
}
