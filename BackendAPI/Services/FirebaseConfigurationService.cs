using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackendAPI.Services
{
    /// <summary>
    /// Centralized Firebase configuration service to handle all Firebase-related configuration logic
    /// </summary>
    public interface IFirebaseConfigurationService
    {
        /// <summary>
        /// Get Firebase Database URL with automatic configuration resolution
        /// </summary>
        Task<string> GetDatabaseUrlAsync();
        
        /// <summary>
        /// Get project ID from Firebase configuration
        /// </summary>
        Task<string> GetProjectIdAsync();
        
        /// <summary>
        /// Build Firebase Database URL for a given project ID
        /// </summary>
        string BuildDatabaseUrl(string projectId);
        
        /// <summary>
        /// Get service account configuration
        /// </summary>
        Task<Dictionary<string, object>?> GetServiceAccountAsync();
        
        /// <summary>
        /// Get Firebase configuration for client applications
        /// </summary>
        Task<FirebaseClientConfig> GetClientConfigAsync();
    }

    /// <summary>
    /// Firebase client configuration model
    /// </summary>
    public class FirebaseClientConfig
    {
        public string ProjectId { get; set; } = string.Empty;
        public string DatabaseURL { get; set; } = string.Empty;
    }

    /// <summary>
    /// Centralized Firebase configuration service implementation
    /// </summary>
    public class FirebaseConfigurationService : IFirebaseConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FirebaseConfigurationService> _logger;
        private string? _cachedDatabaseUrl;
        private string? _cachedProjectId;
        private Dictionary<string, object>? _cachedServiceAccount;

        public FirebaseConfigurationService(
            IConfiguration configuration, 
            ILogger<FirebaseConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get Firebase Database URL with caching and fallback logic
        /// </summary>
        public async Task<string> GetDatabaseUrlAsync()
        {
            if (!string.IsNullOrEmpty(_cachedDatabaseUrl))
            {
                return _cachedDatabaseUrl;
            }

            var configuredDatabaseUrl = _configuration["Firebase:DatabaseUrl"];
            
            // Priority 1: Use explicitly configured DatabaseUrl if provided
            if (!string.IsNullOrWhiteSpace(configuredDatabaseUrl))
            {
                _cachedDatabaseUrl = configuredDatabaseUrl;
                _logger.LogInformation("Using explicitly configured Firebase Database URL");
                return _cachedDatabaseUrl;
            }

            // Priority 2: Auto-derive from service account project_id
            var serviceAccount = await GetServiceAccountAsync();
            if (serviceAccount?.TryGetValue("project_id", out var projectIdObj) == true)
            {
                var projectId = projectIdObj.ToString()!;
                _cachedDatabaseUrl = BuildDatabaseUrl(projectId);
                _logger.LogInformation("Auto-derived Firebase Database URL for project: {ProjectId}", projectId);
                return _cachedDatabaseUrl;
            }

            // Priority 3: Environment variable fallback
            var envDatabaseUrl = Environment.GetEnvironmentVariable("FIREBASE_DATABASE_URL");
            if (!string.IsNullOrWhiteSpace(envDatabaseUrl))
            {
                _cachedDatabaseUrl = envDatabaseUrl;
                _logger.LogInformation("Using Firebase Database URL from environment variable");
                return _cachedDatabaseUrl;
            }

            throw new InvalidOperationException(
                "Firebase Database URL not configured. Please either:\n" +
                "1. Set Firebase:DatabaseUrl in appsettings.json, or\n" +
                "2. Ensure firebase-service-account.json exists with valid project_id, or\n" +
                "3. Set FIREBASE_DATABASE_URL environment variable");
        }

        /// <summary>
        /// Get project ID from Firebase configuration
        /// </summary>
        public async Task<string> GetProjectIdAsync()
        {
            if (!string.IsNullOrEmpty(_cachedProjectId))
            {
                return _cachedProjectId;
            }

            var serviceAccount = await GetServiceAccountAsync();
            if (serviceAccount?.TryGetValue("project_id", out var projectIdObj) == true)
            {
                _cachedProjectId = projectIdObj.ToString()!;
                return _cachedProjectId;
            }

            // Fallback: Extract from database URL
            var databaseUrl = await GetDatabaseUrlAsync();
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                var uri = new Uri(databaseUrl);
                var hostParts = uri.Host.Split('.');
                if (hostParts.Length > 0)
                {
                    _cachedProjectId = hostParts[0].Replace("-default-rtdb", "");
                    return _cachedProjectId;
                }
            }

            throw new InvalidOperationException("Could not determine Firebase project ID");
        }

        /// <summary>
        /// Build Firebase Database URL for a given project ID using region configuration
        /// </summary>
        public string BuildDatabaseUrl(string projectId)
        {
            var region = _configuration["Firebase:Region"] ?? "asia-southeast1";
            
            // Support different Firebase Realtime Database regions
            if (region != "us-central1")
            {
                return $"https://{projectId}-default-rtdb.{region}.firebasedatabase.app/";
            }
            else
            {
                return $"https://{projectId}-default-rtdb.firebaseio.com/";
            }
        }

        /// <summary>
        /// Get service account configuration with caching
        /// </summary>
        public async Task<Dictionary<string, object>?> GetServiceAccountAsync()
        {
            if (_cachedServiceAccount != null)
            {
                return _cachedServiceAccount;
            }

            var serviceAccountPath = _configuration["Firebase:ServiceAccountPath"] ?? "firebase-service-account.json";
            
            if (!File.Exists(serviceAccountPath))
            {
                _logger.LogWarning("Firebase service account file not found at: {Path}", serviceAccountPath);
                return null;
            }

            try
            {
                var serviceAccountJson = await File.ReadAllTextAsync(serviceAccountPath);
                _cachedServiceAccount = JsonSerializer.Deserialize<Dictionary<string, object>>(serviceAccountJson);
                _logger.LogInformation("Firebase service account loaded successfully");
                return _cachedServiceAccount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Firebase service account");
                return null;
            }
        }

        /// <summary>
        /// Get optimized Firebase configuration for client applications
        /// </summary>
        public async Task<FirebaseClientConfig> GetClientConfigAsync()
        {
            var projectId = await GetProjectIdAsync();
            var databaseUrl = await GetDatabaseUrlAsync();

            // Remove trailing slash for consistency
            if (databaseUrl.EndsWith('/'))
            {
                databaseUrl = databaseUrl.TrimEnd('/');
            }

            return new FirebaseClientConfig
            {
                ProjectId = projectId,
                DatabaseURL = databaseUrl
            };
        }
    }
}
