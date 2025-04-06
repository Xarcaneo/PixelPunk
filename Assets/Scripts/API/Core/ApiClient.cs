#nullable enable

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using PixelPunk.API.Services;
using PixelPunk.Core;

namespace PixelPunk.API.Core
{
    /// <summary>
    /// Core client for handling HTTP API requests in the PixelPunk game.
    /// Implements a singleton pattern for global access.
    /// </summary>
    public class ApiClient : MonoBehaviour
    {
        [SerializeField] 
        [Tooltip("Base URL for the API server")]
        private string baseUrl = "https://localhost:7777";  // Default value, should be configured in inspector
        
        /// <summary>
        /// Gets the singleton instance of the ApiClient.
        /// </summary>
        public static ApiClient? Instance { get; private set; }
        
        /// <summary>
        /// Gets the base URL for API requests.
        /// </summary>
        public string BaseUrl => baseUrl;

        private IAuthService? _authService;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _authService = ServiceRegistry.Instance?.GetService<IAuthService>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Sends a typed HTTP request to the API server.
        /// </summary>
        /// <typeparam name="TRequest">Type of the request data</typeparam>
        /// <typeparam name="TResponse">Type of the expected response</typeparam>
        /// <param name="endpoint">API endpoint path</param>
        /// <param name="method">HTTP method to use</param>
        /// <param name="requestData">Data to send in the request</param>
        /// <param name="onSuccess">Callback for successful response</param>
        /// <param name="onError">Callback for error response</param>
        /// <param name="requiresAuth">Whether the request requires authentication</param>
        /// <returns>IEnumerator for Unity coroutine</returns>
        public IEnumerator SendRequest<TRequest, TResponse>(
            string endpoint,
            HttpMethod method,
            TRequest requestData,
            Action<TResponse>? onSuccess,
            Action<string>? onError,
            bool requiresAuth = false) where TResponse : class, new()
        {
            string url = $"{baseUrl}/{endpoint.TrimStart('/')}";
            string? json = requestData != null ? JsonUtility.ToJson(requestData) : null;

            using var request = new UnityWebRequest(url, method.ToString());
            
            if (json != null)
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Add auth header if required
            if (requiresAuth)
            {
                string? token = _authService?.GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {token}");
                }
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (typeof(TResponse) == typeof(EmptyResponse))
                {
                    onSuccess?.Invoke(new EmptyResponse() as TResponse ?? new TResponse());
                }
                else if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    var response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                    if (response != null)
                    {
                        onSuccess?.Invoke(response);
                    }
                    else
                    {
                        onError?.Invoke("Failed to parse response");
                    }
                }
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    /// <summary>
    /// Enumeration of supported HTTP methods.
    /// </summary>
    public enum HttpMethod
    {
        /// <summary>HTTP GET method</summary>
        GET,
        /// <summary>HTTP POST method</summary>
        POST,
        /// <summary>HTTP PUT method</summary>
        PUT,
        /// <summary>HTTP DELETE method</summary>
        DELETE,
        /// <summary>HTTP PATCH method</summary>
        PATCH
    }

    /// <summary>
    /// Empty response class for endpoints that don't return data.
    /// </summary>
    public class EmptyResponse { }
}
