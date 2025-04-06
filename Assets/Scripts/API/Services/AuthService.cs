#nullable enable

using System;
using System.Collections;
using UnityEngine;
using PixelPunk.API.Core;
using PixelPunk.API.Models;

namespace PixelPunk.API.Services
{
    /// <summary>
    /// Service for handling user authentication operations.
    /// Manages user login, registration, and token storage.
    /// </summary>
    public class AuthService : IAuthService
    {
        private const string AUTH_TOKEN_KEY = "auth_token";
        private const string REFRESH_TOKEN_KEY = "refresh_token";

        private string? _currentAccessToken;
        private string? _currentRefreshToken;

        public AuthService()
        {
            LoadTokens();
        }

        /// <summary>
        /// Gets whether the user is currently authenticated.
        /// </summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(_currentAccessToken);

        /// <summary>
        /// Loads authentication tokens from PlayerPrefs.
        /// </summary>
        private void LoadTokens()
        {
            _currentAccessToken = PlayerPrefs.GetString(AUTH_TOKEN_KEY, null);
            _currentRefreshToken = PlayerPrefs.GetString(REFRESH_TOKEN_KEY, null);
        }

        /// <summary>
        /// Saves authentication tokens to PlayerPrefs.
        /// </summary>
        /// <param name="accessToken">The access token to save</param>
        /// <param name="refreshToken">The refresh token to save</param>
        private void SaveTokens(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                Debug.LogError("[Auth] Received empty tokens! Access token is null: " + (accessToken == null) + ", Refresh token is null: " + (refreshToken == null));
                return;
            }

            _currentAccessToken = accessToken;
            _currentRefreshToken = refreshToken;
            PlayerPrefs.SetString(AUTH_TOKEN_KEY, accessToken);
            PlayerPrefs.SetString(REFRESH_TOKEN_KEY, refreshToken);
            PlayerPrefs.Save();
            Debug.Log($"[Auth] Tokens saved successfully. Access token length: {accessToken.Length}, Refresh token length: {refreshToken.Length}");
        }

        /// <summary>
        /// Clears all stored authentication tokens.
        /// </summary>
        public void ClearTokens()
        {
            _currentAccessToken = null;
            _currentRefreshToken = null;
            PlayerPrefs.DeleteKey(AUTH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(REFRESH_TOKEN_KEY);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">Registration request data</param>
        /// <param name="onComplete">Callback for completion</param>
        /// <param name="onError">Callback for errors</param>
        /// <returns>IEnumerator for Unity coroutine</returns>
        public IEnumerator Register(RegisterRequest request, Action<bool>? onComplete, Action<string>? onError)
        {
            return ApiClient.Instance?.SendRequest<RegisterRequest, EmptyResponse>(
                "auth/register",
                HttpMethod.POST,
                request,
                _ => onComplete?.Invoke(true),
                onError
            ) ?? throw new InvalidOperationException("ApiClient.Instance is null");
        }

        /// <summary>
        /// Logs in an existing user.
        /// </summary>
        /// <param name="request">Login request data</param>
        /// <param name="onComplete">Callback for completion</param>
        /// <param name="onError">Callback for errors</param>
        /// <returns>IEnumerator for Unity coroutine</returns>
        public IEnumerator Login(LoginRequest request, Action<bool>? onComplete, Action<string>? onError)
        {
            return ApiClient.Instance?.SendRequest<LoginRequest, TokenResponse>(
                "auth/login",
                HttpMethod.POST,
                request,
                response => {
                    SaveTokens(response.accessToken, response.refreshToken);
                    onComplete?.Invoke(true);
                },
                onError
            ) ?? throw new InvalidOperationException("ApiClient.Instance is null");
        }

        /// <summary>
        /// Checks if the user is currently logged in by verifying stored tokens.
        /// </summary>
        /// <returns>True if valid tokens exist, false otherwise.</returns>
        public bool IsLoggedIn()
        {
            LoadTokens();
            return !string.IsNullOrEmpty(_currentAccessToken) && !string.IsNullOrEmpty(_currentRefreshToken);
        }

        /// <summary>
        /// Gets the current access token, if any.
        /// </summary>
        /// <returns>The current access token or null if not authenticated</returns>
        public string? GetAccessToken() => _currentAccessToken;
    }
}
