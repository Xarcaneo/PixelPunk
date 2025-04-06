#nullable enable

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using PixelPunk.API.Services;
using PixelPunk.API.Models;
using PixelPunk.Core;
using MenuFlow.Core;

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
    /// </remarks>
    public class AuthUI : MonoBehaviour
    {
        [Header("Login Fields")]
        [SerializeField] 
        [Tooltip("Input field for login username")]
        private TMP_InputField loginUsernameInput = null!;

        [SerializeField]
        [Tooltip("Input field for login password")]
        private TMP_InputField loginPasswordInput = null!;

        [SerializeField]
        [Tooltip("Button to trigger login")]
        private Button loginButton = null!;

        [SerializeField]
        [Tooltip("Status text for login operations")]
        private TextMeshProUGUI loginStatusText = null!;

        [Header("Register Fields")]
        [SerializeField] 
        [Tooltip("Input field for register username")]
        private TMP_InputField registerUsernameInput = null!;

        [SerializeField]
        [Tooltip("Input field for register password")]
        private TMP_InputField registerPasswordInput = null!;

        [SerializeField]
        [Tooltip("Input field to confirm register password")]
        private TMP_InputField registerConfirmPasswordInput = null!;

        [SerializeField]
        [Tooltip("Button to trigger registration")]
        private Button registerButton = null!;

        [SerializeField]
        [Tooltip("Status text for registration operations")]
        private TextMeshProUGUI registerStatusText = null!;

        /// <summary>
        /// Reference to the authentication service.
        /// Retrieved from <see cref="ServiceRegistry"/> during initialization.
        /// </summary>
        private IAuthService? _authService;

        private void Start()
        {
            _authService = ServiceRegistry.Instance?.GetService<IAuthService>();
            if (_authService == null)
            {
                UpdateLoginStatus("Auth service not available");
                UpdateRegisterStatus("Auth service not available");
                return;
            }

            loginButton.onClick.AddListener(OnLoginClick);
            registerButton.onClick.AddListener(OnRegisterClick);
            
            UpdateLoginStatus("Please log in");
            UpdateRegisterStatus("Create a new account");
        }

        /// <summary>
        /// Updates the login status message.
        /// </summary>
        /// <param name="message">Status message for login operations</param>
        private void UpdateLoginStatus(string message)
        {
            loginStatusText.text = message;
        }

        /// <summary>
        /// Updates the registration status message.
        /// </summary>
        /// <param name="message">Status message for registration operations</param>
        private void UpdateRegisterStatus(string message)
        {
            registerStatusText.text = message;
        }

        /// <summary>
        /// Handles the login button click event.
        /// </summary>
        public void OnLoginClick()
        {
            if (_authService == null)
            {
                UpdateLoginStatus("Auth service not available");
                return;
            }

            if (string.IsNullOrEmpty(loginUsernameInput.text) || string.IsNullOrEmpty(loginPasswordInput.text))
            {
                UpdateLoginStatus("Please fill in all login fields");
                return;
            }

            var loginRequest = new LoginRequest
            {
                username = loginUsernameInput.text,
                password = loginPasswordInput.text
            };

            UpdateLoginStatus("Logging in...");
            SetLoginInteractable(false);

            StartCoroutine(_authService.Login(
                loginRequest,
                async success => {
                    if (success)
                    {
                        UpdateLoginStatus("Login successful!");
                        await MenuManager.Instance.TransitionTo(MenuFlow.MenuFlowConstants.Menus.GameOverlayMenu);
                    }
                    SetLoginInteractable(true);
                },
                error => {
                    UpdateLoginStatus($"Login failed: {error}");
                    SetLoginInteractable(true);
                }
            ));
        }

        /// <summary>
        /// Handles the register button click event.
        /// </summary>
        public void OnRegisterClick()
        {
            if (_authService == null)
            {
                UpdateRegisterStatus("Auth service not available");
                return;
            }

            if (string.IsNullOrEmpty(registerUsernameInput.text) || 
                string.IsNullOrEmpty(registerPasswordInput.text) ||
                string.IsNullOrEmpty(registerConfirmPasswordInput.text))
            {
                UpdateRegisterStatus("Please fill in all registration fields");
                return;
            }

            if (registerPasswordInput.text != registerConfirmPasswordInput.text)
            {
                UpdateRegisterStatus("Passwords do not match");
                return;
            }

            var registerRequest = new RegisterRequest
            {
                username = registerUsernameInput.text,
                password = registerPasswordInput.text
            };

            UpdateRegisterStatus("Registering...");
            SetRegisterInteractable(false);

            StartCoroutine(_authService.Register(
                registerRequest,
                success => {
                    if (success)
                    {
                        UpdateRegisterStatus("Registration successful! You can now log in.");
                        ClearRegisterFields();
                    }
                    SetRegisterInteractable(true);
                },
                error => {
                    UpdateRegisterStatus($"Registration failed: {error}");
                    SetRegisterInteractable(true);
                }
            ));
        }

        /// <summary>
        /// Controls the interactive state of login UI elements.
        /// </summary>
        private void SetLoginInteractable(bool interactable)
        {
            // Check if the UI elements still exist before accessing them
            if (this == null || !gameObject || loginUsernameInput == null || loginPasswordInput == null || loginButton == null)
                return;

            loginUsernameInput.interactable = interactable;
            loginPasswordInput.interactable = interactable;
            loginButton.interactable = interactable;
        }

        /// <summary>
        /// Controls the interactive state of registration UI elements.
        /// </summary>
        private void SetRegisterInteractable(bool interactable)
        {
            // Check if the UI elements still exist before accessing them
            if (this == null || !gameObject || registerUsernameInput == null || registerPasswordInput == null || 
                registerConfirmPasswordInput == null || registerButton == null)
                return;

            registerUsernameInput.interactable = interactable;
            registerPasswordInput.interactable = interactable;
            registerConfirmPasswordInput.interactable = interactable;
            registerButton.interactable = interactable;
        }

        /// <summary>
        /// Clears all registration input fields.
        /// </summary>
        private void ClearRegisterFields()
        {
            // Check if the UI elements still exist before accessing them
            if (this == null || !gameObject || registerUsernameInput == null || registerPasswordInput == null || 
                registerConfirmPasswordInput == null)
                return;

            registerUsernameInput.text = string.Empty;
            registerPasswordInput.text = string.Empty;
            registerConfirmPasswordInput.text = string.Empty;
        }
    }
}
