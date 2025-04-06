#nullable enable

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using PixelPunk.API.Services;
using PixelPunk.API.Models;
using PixelPunk.Core;

namespace PixelPunk.UI.Auth
{
    /// <summary>
    /// Manages the authentication user interface in the PixelPunk game.
    /// </summary>
    /// <remarks>
    /// This component handles:
    /// <list type="bullet">
    /// <item><description>User input for login and registration</description></item>
    /// <item><description>Form validation and error feedback</description></item>
    /// <item><description>Communication with the auth service</description></item>
    /// <item><description>Visual feedback during authentication operations</description></item>
    /// </list>
    /// 
    /// Dependencies:
    /// <list type="bullet">
    /// <item><description><see cref="IAuthService"/> - For authentication operations</description></item>
    /// <item><description><see cref="ServiceRegistry"/> - For service access</description></item>
    /// </list>
    /// 
    /// Usage:
    /// <code>
    /// // Attach this component to a GameObject with:
    /// // - TMP_InputField components for username/password
    /// // - Button components for login/register actions
    /// // - TextMeshProUGUI component for status display
    /// </code>
    /// </remarks>
    public class AuthUI : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField] 
        [Tooltip("Input field for username")]
        private TMP_InputField usernameInput = null!;

        [SerializeField]
        [Tooltip("Input field for password")]
        private TMP_InputField passwordInput = null!;

        [Header("Buttons")]
        [SerializeField]
        [Tooltip("Button to trigger login")]
        private Button loginButton = null!;

        [SerializeField]
        [Tooltip("Button to trigger registration")]
        private Button registerButton = null!;

        [Header("Status")]
        [SerializeField]
        [Tooltip("Text display for status messages")]
        private TextMeshProUGUI statusText = null!;

        /// <summary>
        /// Reference to the authentication service.
        /// Retrieved from <see cref="ServiceRegistry"/> during initialization.
        /// </summary>
        private IAuthService? _authService;

        /// <summary>
        /// Initializes the UI component by:
        /// <list type="number">
        /// <item><description>Retrieving the auth service from the registry</description></item>
        /// <item><description>Setting up button click handlers</description></item>
        /// <item><description>Initializing the status display</description></item>
        /// </list>
        /// </summary>
        private void Start()
        {
            _authService = ServiceRegistry.Instance?.GetService<IAuthService>();
            if (_authService == null)
            {
                Debug.LogError("AuthService not found in ServiceRegistry");
                return;
            }

            loginButton.onClick.AddListener(OnLoginClick);
            registerButton.onClick.AddListener(OnRegisterClick);
            UpdateStatus("Please log in or register");
        }

        /// <summary>
        /// Updates the status message displayed to the user.
        /// </summary>
        /// <param name="message">The message to display. Can be an error message, success message, or general status update.</param>
        /// <remarks>
        /// This method is used to provide real-time feedback about:
        /// <list type="bullet">
        /// <item><description>Form validation errors</description></item>
        /// <item><description>Authentication progress</description></item>
        /// <item><description>Success/failure of operations</description></item>
        /// </list>
        /// </remarks>
        private void UpdateStatus(string message)
        {
            statusText.text = message;
        }

        /// <summary>
        /// Handles the login button click event.
        /// </summary>
        /// <remarks>
        /// This method:
        /// <list type="number">
        /// <item><description>Validates the form input</description></item>
        /// <item><description>Creates a login request</description></item>
        /// <item><description>Disables UI during the request</description></item>
        /// <item><description>Handles success/failure responses</description></item>
        /// </list>
        /// 
        /// On success:
        /// <list type="bullet">
        /// <item><description>Updates status to show success</description></item>
        /// <item><description>TODO: Transitions to the game scene</description></item>
        /// </list>
        /// 
        /// On failure:
        /// <list type="bullet">
        /// <item><description>Displays the error message</description></item>
        /// <item><description>Re-enables the form</description></item>
        /// </list>
        /// </remarks>
        public void OnLoginClick()
        {
            if (_authService == null)
            {
                UpdateStatus("Auth service not available");
                return;
            }

            if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
            {
                UpdateStatus("Please fill in all fields");
                return;
            }

            var loginRequest = new LoginRequest
            {
                Username = usernameInput.text,
                Password = passwordInput.text
            };

            UpdateStatus("Logging in...");
            SetInteractable(false);

            StartCoroutine(_authService.Login(
                loginRequest,
                success => {
                    if (success)
                    {
                        UpdateStatus("Login successful!");
                        // TODO: Load your game scene here
                    }
                    SetInteractable(true);
                },
                error => {
                    UpdateStatus($"Login failed: {error}");
                    SetInteractable(true);
                }
            ));
        }

        /// <summary>
        /// Handles the register button click event.
        /// </summary>
        /// <remarks>
        /// This method:
        /// <list type="number">
        /// <item><description>Validates the form input</description></item>
        /// <item><description>Creates a registration request</description></item>
        /// <item><description>Disables UI during the request</description></item>
        /// <item><description>Handles success/failure responses</description></item>
        /// </list>
        /// 
        /// On success:
        /// <list type="bullet">
        /// <item><description>Updates status to show success</description></item>
        /// <item><description>Prompts user to log in</description></item>
        /// </list>
        /// 
        /// On failure:
        /// <list type="bullet">
        /// <item><description>Displays the error message</description></item>
        /// <item><description>Re-enables the form</description></item>
        /// </list>
        /// </remarks>
        public void OnRegisterClick()
        {
            if (_authService == null)
            {
                UpdateStatus("Auth service not available");
                return;
            }

            if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
            {
                UpdateStatus("Please fill in all fields");
                return;
            }

            var registerRequest = new RegisterRequest
            {
                Username = usernameInput.text,
                Password = passwordInput.text
            };

            UpdateStatus("Registering...");
            SetInteractable(false);

            StartCoroutine(_authService.Register(
                registerRequest,
                success => {
                    if (success)
                    {
                        UpdateStatus("Registration successful! You can now log in.");
                    }
                    SetInteractable(true);
                },
                error => {
                    UpdateStatus($"Registration failed: {error}");
                    SetInteractable(true);
                }
            ));
        }

        /// <summary>
        /// Controls the interactive state of all UI elements.
        /// </summary>
        /// <param name="interactable">Whether the UI elements should be interactable</param>
        /// <remarks>
        /// This method is used to:
        /// <list type="bullet">
        /// <item><description>Disable input during authentication operations</description></item>
        /// <item><description>Prevent multiple simultaneous requests</description></item>
        /// <item><description>Re-enable input after operations complete</description></item>
        /// </list>
        /// 
        /// Affected elements:
        /// <list type="bullet">
        /// <item><description>Username input field</description></item>
        /// <item><description>Password input field</description></item>
        /// <item><description>Login button</description></item>
        /// <item><description>Register button</description></item>
        /// </list>
        /// </remarks>
        private void SetInteractable(bool interactable)
        {
            usernameInput.interactable = interactable;
            passwordInput.interactable = interactable;
            loginButton.interactable = interactable;
            registerButton.interactable = interactable;
        }
    }
}
