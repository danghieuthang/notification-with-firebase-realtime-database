using BackendAPI.Models;
using BackendAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BackendAPI.Controllers
{
    /// <summary>
    /// Firebase Realtime Database Notification API Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            IFirebaseService firebaseService,
            ILogger<NotificationController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user and send welcome notification
        /// </summary>
        /// <param name="request">User registration request</param>
        /// <returns>Success status only</returns>
        /// <response code="200">User registered and notification sent to Firebase</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("register")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest("UserId is required");
                }

                // Send welcome notification
                var welcomeTitle = "Welcome!";
                var welcomeBody = "You have successfully registered to our notification system.";
                var data = new Dictionary<string, string>
                {
                    { "type", "welcome" }
                };

                await _firebaseService.SendNotificationAsync(
                    request.UserId,
                    welcomeTitle,
                    welcomeBody,
                    data);

                // Return 200 OK with no content - Angular will listen to Firebase for the actual notification
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return StatusCode(500, "Failed to register user");
            }
        }

        /// <summary>
        /// Send a random notification to a user for testing
        /// </summary>
        /// <param name="request">Random notification request</param>
        /// <returns>Success status only</returns>
        /// <response code="200">Notification sent to Firebase successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("send-random")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SendRandomNotification([FromBody] SendRandomRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest("UserId is required");
                }

                // Random notification data
                var random = new Random();
                var titles = new[] { "Breaking News!", "Important Update", "New Message", "Alert!", "Information" };
                var messages = new[]
                {
                    "You have received a new message from the system.",
                    "This is an important update for your account.",
                    "A new feature has been added to your dashboard.",
                    "Your request has been processed successfully.",
                    "Please check your account for important information."
                };

                var randomTitle = titles[random.Next(titles.Length)];
                var randomBody = messages[random.Next(messages.Length)];
                var randomData = new Dictionary<string, string>
                {
                    { "type", "random" },
                    { "priority", random.Next(1, 4).ToString() },
                    { "category", new[] { "info", "warning", "success" }[random.Next(3)] },
                    { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                // Send notification to Firebase only - don't return details
                await _firebaseService.SendNotificationAsync(
                    request.UserId,
                    randomTitle,
                    randomBody,
                    randomData);

                // Return 200 OK with no content - Angular will listen to Firebase for the actual notification
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending random notification");
                return StatusCode(500, "Failed to send notification");
            }
        }

        /// <summary>
        /// Test endpoint to verify API is working
        /// </summary>
        /// <returns>API status message</returns>
        /// <response code="200">API is working correctly</response>
        [HttpGet("test")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult Test()
        {
            return Ok(new
            {
                Message = "Notification API is working",
                Timestamp = DateTime.UtcNow,
                Version = "4.0 - Simplified Firebase Notification System"
            });
        }

        /// <summary>
        /// Get Firebase configuration for client apps
        /// </summary>
        /// <returns>Firebase client configuration</returns>
        /// <response code="200">Firebase config retrieved successfully</response>
        [HttpGet("firebase-config")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult GetFirebaseConfig()
        {
            try
            {
                // Đọc service account để lấy thông tin project
                var serviceAccountPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-service-account.json");

                if (!System.IO.File.Exists(serviceAccountPath))
                {
                    return NotFound("Firebase service account file not found");
                }

                var serviceAccountJson = System.IO.File.ReadAllText(serviceAccountPath);
                var serviceAccount = JsonSerializer.Deserialize<JsonElement>(serviceAccountJson);

                var projectId = serviceAccount.GetProperty("project_id").GetString();

                // Trả về client config (không bao gồm private key)
                return Ok(new
                {
                    projectId = projectId,
                    databaseURL = $"https://{projectId}-default-rtdb.asia-southeast1.firebasedatabase.app",
                    authDomain = $"{projectId}.firebaseapp.com",
                    storageBucket = $"{projectId}.firebasestorage.app",
                    // Note: Không trả về API key và các thông tin nhạy cảm
                    // Client sẽ cần sử dụng Firebase Auth hoặc Anonymous Auth
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Firebase config");
                return StatusCode(500, "Failed to get Firebase config");
            }
        }
    }
}
