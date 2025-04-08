#nullable enable

using UnityEngine;
using PixelPunk.API.Services;
using PixelPunk.Core;
using PixelPunk.API.Models;

namespace PixelPunk.UI.Resources
{
    /// <summary>
    /// Manages the UI display of player resources in the PixelPunk game.
    /// </summary>
    /// <remarks>
    /// This component is responsible for:
    /// <list type="bullet">
    /// <item><description>Displaying current resource amounts (Gold, Silver, Coal)</description></item>
    /// <item><description>Updating resource values when they change</description></item>
    /// <item><description>Formatting resource numbers for display</description></item>
    /// </list>
    /// </remarks>
    public class ResourcesUI : MonoBehaviour
    {
        private IPlayerDataService? _playerDataService;

        private void Start()
        {
            _playerDataService = ServiceRegistry.Instance?.GetService<IPlayerDataService>();
            
            if (_playerDataService == null)
            {
                Debug.LogError("[ResourcesUI] Player data service not available");
                return;
            }

            // Subscribe to player data updates
            _playerDataService.OnPlayerDataUpdated += OnPlayerDataUpdated;

            // Log initial data if available
            if (_playerDataService.PlayerData != null)
            {
                LogResources(_playerDataService.PlayerData);
            }
        }

        private void OnDestroy()
        {
            if (_playerDataService != null)
            {
                _playerDataService.OnPlayerDataUpdated -= OnPlayerDataUpdated;
            }
        }

        /// <summary>
        /// Called when player data is updated.
        /// </summary>
        private void OnPlayerDataUpdated()
        {
            if (_playerDataService?.PlayerData == null)
            {
                Debug.LogWarning("[ResourcesUI] Received player data update but data is null");
                return;
            }

            LogResources(_playerDataService.PlayerData);
        }

        /// <summary>
        /// Logs the current resource values to the console.
        /// </summary>
        /// <param name="playerData">The player data containing resources</param>
        private void LogResources(PlayerData playerData)
        {
            if (playerData.resources == null || playerData.resources.Count == 0)
            {
                Debug.LogWarning("[ResourcesUI] Player data has no resources");
                return;
            }

            foreach (var resource in playerData.resources)
            {
                Debug.Log($"[ResourcesUI] {resource.resourceName}: {resource.amount} (Last updated: {resource.LastUpdatedAt})");
            }
        }
    }
}
