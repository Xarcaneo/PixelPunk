#nullable enable

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using PixelPunk.API.Services;
using PixelPunk.API.Models;

namespace PixelPunk.UI.Auth
{
    /// <summary>
    /// Handles the user interface for authentication operations.
    /// Provides login and registration functionality with status feedback.
    /// </summary>
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

        private void Start()
        {
            loginButton.onClick.AddListener(OnLoginClick);
            registerButton.onClick.AddListener(OnRegisterClick);
            UpdateStatus("Please log in or register");
        }

        /// <summary>
        /// Updates the status message displayed to the user.
        /// </summary>
        /// <param name="message">The message to display</param>
        private void UpdateStatus(string message)
        {
            statusText.text = message;
        }

        /// <summary>
        /// Handles the login button click.
        /// Validates input and attempts to log in the user.
        /// </summary>
        public void OnLoginClick()
        {
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

            StartCoroutine(AuthService.Instance?.Login(
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
        /// Handles the register button click.
        /// Validates input and attempts to register a new user.
        /// </summary>
        public void OnRegisterClick()
        {
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

            StartCoroutine(AuthService.Instance?.Register(
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
        /// Sets the interactable state of all UI elements.
        /// Used to disable input during API operations.
        /// </summary>
        /// <param name="interactable">Whether the UI elements should be interactable</param>
        private void SetInteractable(bool interactable)
        {
            usernameInput.interactable = interactable;
            passwordInput.interactable = interactable;
            loginButton.interactable = interactable;
            registerButton.interactable = interactable;
        }
    }
}
