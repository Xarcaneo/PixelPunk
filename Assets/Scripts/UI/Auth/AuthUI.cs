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

        /// <summary>
        /// Reference to the player data service.
        /// Retrieved from <see cref="ServiceRegistry"/> during initialization.
        /// </summary>
        private IPlayerDataService? _playerDataService;

        private void Start()
        {
            _authService = ServiceRegistry.Instance?.GetService<IAuthService>();
            _playerDataService = ServiceRegistry.Instance?.GetService<IPlayerDataService>();

            if (_authService == null)
            {
                UpdateLoginStatus("Auth service not available");
                UpdateRegisterStatus("Auth service not available");
                return;
            }

            if (_playerDataService == null)
            {
                Debug.LogWarning("Player data service not available");
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
            if (this == null || !gameObject) return;
            
            loginStatusText.text = message;
        }

        /// <summary>
        /// Updates the registration status message.
        /// </summary>
        /// <param name="message">Status message for registration operations</param>
        private void UpdateRegisterStatus(string message)
        {
            if (this == null || !gameObject) return;
            
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
                        // Don't re-enable UI elements since we're transitioning away
                        await FetchPlayerDataAndTransition();
                    }
                    else
                    {
                        SetLoginInteractable(true);
                    }
                },
                error => {
                    UpdateLoginStatus($"Login failed: {error}");
                    SetLoginInteractable(true);
                }
            ));
        }

        /// <summary>
        /// Fetches player data and transitions to the game overlay menu.
        /// </summary>
        private async Task FetchPlayerDataAndTransition()
        {
            if (_playerDataService != null)
            {
                var tcs = new TaskCompletionSource<bool>();

                StartCoroutine(_playerDataService.FetchPlayerData(
                    success => {
                        tcs.SetResult(true);
                    },
                    error => {
                        Debug.LogError($"Failed to fetch player data: {error}");
                        tcs.SetResult(false);
                    }
                ));

                await tcs.Task;
            }

            await MenuManager.Instance.TransitionTo(MenuFlow.MenuFlowConstants.Menus.GameOverlayMenu);
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
                        // After successful registration, attempt to log in automatically
                        var loginRequest = new LoginRequest
                        {
                            username = registerUsernameInput.text,
                            password = registerPasswordInput.text
                        };

                        StartCoroutine(_authService.Login(
                            loginRequest,
                            async loginSuccess => {
                                if (loginSuccess)
                                {
                                    UpdateRegisterStatus("Registration successful! Logging you in...");
                                    // Don't re-enable UI elements since we're transitioning away
                                    await FetchPlayerDataAndTransition();
                                }
                                else
                                {
                                    UpdateRegisterStatus("Registration successful! Please log in.");
                                    ClearRegisterFields();
                                    SetRegisterInteractable(true);
                                }
                            },
                            error => {
                                UpdateRegisterStatus("Registration successful! Please log in.");
                                ClearRegisterFields();
                                SetRegisterInteractable(true);
                            }
                        ));
                    }
                    else
                    {
                        SetRegisterInteractable(true);
                    }
                },
                error => {
                    UpdateRegisterStatus($"Registration failed: {error}");
                    SetRegisterInteractable(true);
                }
            ));
        }

        /// <summary>
        /// Clears all registration input fields.
        /// </summary>
        private void ClearRegisterFields()
        {
            if (this == null || !gameObject) return;
            
            registerUsernameInput.text = "";
            registerPasswordInput.text = "";
            registerConfirmPasswordInput.text = "";
        }

        /// <summary>
        /// Sets the interactability of login-related UI elements.
        /// </summary>
        /// <param name="interactable">Whether the elements should be interactable</param>
        private void SetLoginInteractable(bool interactable)
        {
            if (this == null || !gameObject) return;
            
            loginButton.interactable = interactable;
            loginUsernameInput.interactable = interactable;
            loginPasswordInput.interactable = interactable;
        }

        /// <summary>
        /// Sets the interactability of registration-related UI elements.
        /// </summary>
        /// <param name="interactable">Whether the elements should be interactable</param>
        private void SetRegisterInteractable(bool interactable)
        {
            if (this == null || !gameObject) return;
            
            registerButton.interactable = interactable;
            registerUsernameInput.interactable = interactable;
            registerPasswordInput.interactable = interactable;
            registerConfirmPasswordInput.interactable = interactable;
        }
    }
}
