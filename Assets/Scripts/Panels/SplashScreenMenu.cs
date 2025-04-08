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
    private IPlayerDataService? _playerDataService;
    private TaskCompletionSource<bool>? _fetchDataTask;

    private void Start()
    {
        _authService = ServiceRegistry.Instance?.GetService<IAuthService>();
        _playerDataService = ServiceRegistry.Instance?.GetService<IPlayerDataService>();
    }

    private async void OnTransitionComplete()
    {
        if (_authService != null && _authService.IsLoggedIn())
        {
            // Try to fetch player data first
            if (_playerDataService != null)
            {
                _fetchDataTask = new TaskCompletionSource<bool>();
                
                // Start coroutine to fetch data
                StartCoroutine(_playerDataService.FetchPlayerData(
                    onComplete: playerData => {
                        _fetchDataTask?.SetResult(true);
                    },
                    onError: error => {
                        Debug.LogError($"[SplashScreen] Failed to fetch player data: {error}");
                        _fetchDataTask?.SetResult(false);
                    }
                ));

                // Wait for fetch to complete
                try
                {
                    bool success = await _fetchDataTask.Task;
                    if (!success)
                    {
                        Debug.LogWarning("[SplashScreen] Could not fetch player data, redirecting to login");
                        await MenuManager.Instance.TransitionTo(MenuFlow.MenuFlowConstants.Menus.LoginMenu);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SplashScreen] Error waiting for player data: {ex}");
                    await MenuManager.Instance.TransitionTo(MenuFlow.MenuFlowConstants.Menus.LoginMenu);
                    return;
                }
            }

            await MenuManager.Instance.TransitionTo(MenuFlow.MenuFlowConstants.Menus.GameOverlayMenu);
        }
        else
        {
            await MenuManager.Instance.TransitionTo(MenuFlow.MenuFlowConstants.Menus.LoginMenu);
        }
    }

    public override async Task Show()
    {
        await base.Show();
        MenuManager.Instance.onTransitionComplete.AddListener(OnTransitionComplete);
        await Task.Delay(4000); 
    }

    public override async Task Exit()
    {
        MenuManager.Instance.onTransitionComplete.RemoveListener(OnTransitionComplete);
        await base.Exit();
    }
}
