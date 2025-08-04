namespace BackendAPI.Models
{
    /// <summary>
    /// Centralized API response models for consistent response structure
    /// </summary>
    public static class ApiResponses
    {
        /// <summary>
        /// User registration response model
        /// </summary>
        public class RegisterResponse
        {
            public string Message { get; set; } = string.Empty;
            public string ListenUrl { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
        }





        /// <summary>
        /// API test endpoint response model
        /// </summary>
        public class ApiTestResponse
        {
            public string Message { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public string Version { get; set; } = string.Empty;
        }

        /// <summary>
        /// Generic error response model
        /// </summary>
        public class ErrorResponse
        {
            public string Error { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }

        /// <summary>
        /// Generic success response model
        /// </summary>
        public class SuccessResponse
        {
            public string Message { get; set; } = string.Empty;
            public bool Success { get; set; } = true;
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Centralized response factory for creating standardized API responses
    /// </summary>
    public static class ResponseFactory
    {
        /// <summary>
        /// Create a user registration response
        /// </summary>
        public static ApiResponses.RegisterResponse CreateRegisterResponse(string userId, string listenUrl, string message = "User registered successfully")
        {
            return new ApiResponses.RegisterResponse
            {
                Message = message,
                ListenUrl = listenUrl,
                UserId = userId
            };
        }





        /// <summary>
        /// Create an API test response
        /// </summary>
        public static ApiResponses.ApiTestResponse CreateApiTestResponse()
        {
            return new ApiResponses.ApiTestResponse
            {
                Message = "Notification API is working",
                Timestamp = DateTime.UtcNow,
                Version = "5.0 - Refactored & Optimized Firebase Notification System"
            };
        }

        /// <summary>
        /// Create an error response
        /// </summary>
        public static ApiResponses.ErrorResponse CreateErrorResponse(string error, string message)
        {
            return new ApiResponses.ErrorResponse
            {
                Error = error,
                Message = message
            };
        }

        /// <summary>
        /// Create a success response
        /// </summary>
        public static ApiResponses.SuccessResponse CreateSuccessResponse(string message)
        {
            return new ApiResponses.SuccessResponse
            {
                Message = message
            };
        }
    }
}
