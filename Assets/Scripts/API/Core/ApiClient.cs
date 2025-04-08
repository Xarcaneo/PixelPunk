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
        /// Handles the response from an HTTP request and processes it into the expected type.
        /// </summary>
        /// <typeparam name="TResponse">Type of response expected from the API</typeparam>
        /// <param name="request">The UnityWebRequest that was sent</param>
        /// <param name="endpoint">The API endpoint that was called</param>
        /// <param name="onSuccess">Callback to invoke on successful response parsing</param>
        /// <param name="onError">Callback to invoke when an error occurs</param>
        /// <returns>IEnumerator for Unity coroutine</returns>
        private IEnumerator HandleResponse<TResponse>(UnityWebRequest request, string endpoint, Action<TResponse>? onSuccess, Action<string>? onError) where TResponse : class, new()
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Special case for EmptyResponse - don't try to parse the response body
                    if (typeof(TResponse) == typeof(EmptyResponse))
                    {
                        onSuccess?.Invoke(new TResponse());
                        yield break;
                    }

                    // For non-empty responses, parse the JSON
                    if (!string.IsNullOrEmpty(request.downloadHandler.text))
                    {
                        var response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                        if (response != null)
                        {
                            onSuccess?.Invoke(response);
                            yield break;
                        }
                    }
                    
                    onError?.Invoke("Failed to parse response");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[API] Error parsing response: {ex.Message}");
                    onError?.Invoke($"Failed to parse response: {ex.Message}");
                }
            }
            else
            {
                string errorMessage = request.error ?? "Unknown error";
                Debug.LogError($"[API] Request failed: {errorMessage}");
                onError?.Invoke(errorMessage);
            }
            yield break;
        }

        /// <summary>
        /// Sends a typed HTTP request to the API server without a request body.
        /// Ideal for GET and DELETE requests.
        /// </summary>
        /// <typeparam name="TResponse">Type of the expected response</typeparam>
        /// <param name="endpoint">API endpoint path</param>
        /// <param name="method">HTTP method to use</param>
        /// <param name="onSuccess">Callback for successful response</param>
        /// <param name="onError">Callback for error response</param>
        /// <param name="requiresAuth">Whether the request requires authentication</param>
        /// <returns>IEnumerator for Unity coroutine</returns>
        /// <remarks>
        /// This overload is optimized for requests that don't need a request body,
        /// such as GET requests. It automatically handles authentication if required.
        /// </remarks>
        public IEnumerator SendRequest<TResponse>(
            string endpoint,
            HttpMethod method,
            Action<TResponse>? onSuccess,
            Action<string>? onError,
            bool requiresAuth = false) where TResponse : class, new()
        {
            string url = $"{baseUrl}/{endpoint.TrimStart('/')}";

            using var request = new UnityWebRequest(url, method.ToString());
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
            yield return HandleResponse(request, endpoint, onSuccess, onError);
        }

        /// <summary>
        /// Sends a typed HTTP request to the API server with a request body.
        /// Ideal for POST, PUT, and PATCH requests.
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
        /// <remarks>
        /// This overload handles requests that need to send data in the request body,
        /// such as POST or PUT requests. The request data is automatically serialized
        /// to JSON and proper headers are set.
        /// </remarks>
        public IEnumerator SendRequest<TRequest, TResponse>(
            string endpoint,
            HttpMethod method,
            TRequest requestData,
            Action<TResponse>? onSuccess,
            Action<string>? onError,
            bool requiresAuth = false) where TResponse : class, new()
        {
            string url = $"{baseUrl}/{endpoint.TrimStart('/')}";

            using var request = new UnityWebRequest(url, method.ToString());
            
            if (requestData != null)
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestData));
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
            yield return HandleResponse(request, endpoint, onSuccess, onError);
        }
    }

    /// <summary>
    /// Enumeration of supported HTTP methods.
    /// </summary>
    public enum HttpMethod
    {
        /// <summary>
        /// HTTP GET method for retrieving resources
        /// </summary>
        GET,
        
        /// <summary>
        /// HTTP POST method for creating new resources
        /// </summary>
        POST,
        
        /// <summary>
        /// HTTP PUT method for updating existing resources
        /// </summary>
        PUT,
        
        /// <summary>
        /// HTTP DELETE method for removing resources
        /// </summary>
        DELETE,
        
        /// <summary>
        /// HTTP PATCH method for partial updates to resources
        /// </summary>
        PATCH
    }

    /// <summary>
    /// Empty response class for endpoints that don't return data.
    /// Used as a type parameter for requests that expect no response body.
    /// </summary>
    /// <remarks>
    /// This class is used to maintain type safety when making requests to
    /// endpoints that return no content (HTTP 204) or empty responses.
    /// </remarks>
    public class EmptyResponse { }
}
