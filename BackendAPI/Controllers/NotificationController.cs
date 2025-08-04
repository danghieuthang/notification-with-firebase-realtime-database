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
        private readonly IHashService _hashService;
        private readonly IFirebaseConfigurationService _firebaseConfig;
        private readonly INotificationFactory _notificationFactory;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            IHashService hashService,
            IFirebaseConfigurationService firebaseConfig,
            INotificationFactory notificationFactory,
            ILogger<NotificationController> logger)
        {
            _hashService = hashService;
            _firebaseConfig = firebaseConfig;
            _notificationFactory = notificationFactory;
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
        [ProducesResponseType(typeof(ApiResponses.RegisterResponse), 200)]
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

                // Send welcome notification using factory
                await _notificationFactory.CreateWelcomeNotificationAsync(request.UserId);

                // Get Firebase config and create listen URL
                var databaseUrl = await _firebaseConfig.GetDatabaseUrlAsync();
                var listenUrl = _hashService.CreateListenUrl(databaseUrl, request.UserId);

                // Return success with listen URL for client to display
                return Ok(ResponseFactory.CreateRegisterResponse(request.UserId, listenUrl));
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

                // Send random notification using factory
                await _notificationFactory.CreateRandomNotificationAsync(request.UserId);

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
        [ProducesResponseType(typeof(ApiResponses.ApiTestResponse), 200)]
        public IActionResult Test()
        {
            return Ok(ResponseFactory.CreateApiTestResponse());
        }
    }
}
