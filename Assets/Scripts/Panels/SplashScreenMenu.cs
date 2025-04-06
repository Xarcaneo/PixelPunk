#nullable enable

using UnityEngine;
using System;
using System.Threading.Tasks;
using PixelPunk.API.Services;
using PixelPunk.Core;
using MenuFlow.Core;

public class SplashScreenMenu : MenuFlow.Components.MenuPanel
{
    private IAuthService? _authService;

    private void Start()
    {
        _authService = ServiceRegistry.Instance?.GetService<IAuthService>();
    }

    private async void OnTransitionComplete()
    {
        //Check authentication and redirect
        if (_authService != null && _authService.IsLoggedIn())
        {
            await MenuManager.Instance.TransitionTo(MenuFlow.MenuFlowConstants.Menus.GameOverlayMenu);
        }
        else
        {
            await MenuManager.Instance.TransitionTo(MenuFlow.MenuFlowConstants.Menus.LoginMenu);
        }
    }

    public override async Task Show()
    {
        Debug.Log("SHOW");
        await base.Show();
        MenuManager.Instance.onTransitionComplete.AddListener(OnTransitionComplete);
        await Task.Delay(4000); 
    }

    public override async Task Exit()
    {
        Debug.Log("HIDE");
        MenuManager.Instance.onTransitionComplete.RemoveListener(OnTransitionComplete);
        await base.Exit();
    }
}
