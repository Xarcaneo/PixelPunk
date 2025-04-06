#nullable enable

using System;

namespace PixelPunk.API.Models
{
    /// <summary>
    /// Request model for user login.
    /// </summary>
    [Serializable]
    public class LoginRequest
    {
        /// <summary>
        /// Gets or sets the username for login.
        /// </summary>
        public string Username = string.Empty;

        /// <summary>
        /// Gets or sets the password for login.
        /// </summary>
        public string Password = string.Empty;
    }

    /// <summary>
    /// Request model for user registration.
    /// </summary>
    [Serializable]
    public class RegisterRequest
    {
        /// <summary>
        /// Gets or sets the username for registration.
        /// </summary>
        public string Username = string.Empty;

        /// <summary>
        /// Gets or sets the password for registration.
        /// </summary>
        public string Password = string.Empty;
    }

    /// <summary>
    /// Response model containing authentication tokens.
    /// </summary>
    [Serializable]
    public class TokenResponse
    {
        /// <summary>
        /// Gets or sets the access token for API authentication.
        /// </summary>
        public string AccessToken = string.Empty;

        /// <summary>
        /// Gets or sets the refresh token for obtaining new access tokens.
        /// </summary>
        public string RefreshToken = string.Empty;
    }
}
