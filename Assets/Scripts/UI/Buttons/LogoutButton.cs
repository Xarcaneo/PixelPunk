#nullable enable

using UnityEngine;
using MenuFlow.Components.Navigation;
using MenuFlow.Core;
using PixelPunk.API.Services;
using PixelPunk.Core;

namespace PixelPunk.UI.Buttons
{
    /// <summary>
    /// Button that handles user logout functionality and transitions to the login menu
    /// </summary>
    public class LogoutButton : MenuButton
    {
        /// <summary>
        /// Reference to the authentication service.
        /// Retrieved from <see cref="ServiceRegistry"/> during initialization.
        /// </summary>
        private IAuthService? _authService;

        protected override void Awake()
        {
            base.Awake();
            _authService = ServiceRegistry.Instance?.GetService<IAuthService>();
        }

        protected override async void OnClick()
        {
            // Clear authentication using the AuthService
            _authService?.ClearTokens();
            
            // Transition to login menu
            await MenuManager.Instance.TransitionTo(MenuFlow.MenuFlowConstants.Menus.LoginMenu);
        }
    }
}
