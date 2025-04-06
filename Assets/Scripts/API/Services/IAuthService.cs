#nullable enable

using System;
using System.Collections;
using PixelPunk.Core;
using PixelPunk.API.Models;

namespace PixelPunk.API.Services
{
    /// <summary>
    /// Interface for authentication-related services.
    /// Defines the contract for user authentication operations.
    /// </summary>
    public interface IAuthService : IGameService
    {
        /// <summary>
        /// Gets whether the user is currently authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Attempts to register a new user.
        /// </summary>
        /// <param name="request">Registration request data</param>
        /// <param name="onComplete">Callback for completion</param>
        /// <param name="onError">Callback for errors</param>
        /// <returns>IEnumerator for Unity coroutine</returns>
        IEnumerator Register(RegisterRequest request, Action<bool>? onComplete, Action<string>? onError);

        /// <summary>
        /// Attempts to log in an existing user.
        /// </summary>
        /// <param name="request">Login request data</param>
        /// <param name="onComplete">Callback for completion</param>
        /// <param name="onError">Callback for errors</param>
        /// <returns>IEnumerator for Unity coroutine</returns>
        IEnumerator Login(LoginRequest request, Action<bool>? onComplete, Action<string>? onError);

        /// <summary>
        /// Gets the current access token, if any.
        /// </summary>
        /// <returns>The current access token or null if not authenticated</returns>
        string? GetAccessToken();

        /// <summary>
        /// Clears all stored authentication tokens.
        /// </summary>
        void ClearTokens();
    }
}
