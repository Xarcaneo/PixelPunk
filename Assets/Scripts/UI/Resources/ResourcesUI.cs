#nullable enable

using UnityEngine;
using TMPro;
using PixelPunk.API.Services;
using PixelPunk.Core;
using PixelPunk.API.Models;
using PixelPunk.Game.Resources;

namespace PixelPunk.UI.Resources
{
    /// <summary>
    /// Manages the UI display of player resources in the PixelPunk game.
    /// </summary>
    public class ResourcesUI : MonoBehaviour
    {
        [Header("Resource Text Components")]
        [SerializeField] private TMP_Text goldText = null!;
        [SerializeField] private TMP_Text silverText = null!;
        [SerializeField] private TMP_Text coalText = null!;

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

            // Update UI with initial data if available
            if (_playerDataService.PlayerData != null)
            {
                UpdateResourceDisplay(_playerDataService.PlayerData);
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
        /// Handles updates to player data, including resource changes.
        /// </summary>
        private void OnPlayerDataUpdated()
        {
            var playerData = _playerDataService?.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[ResourcesUI] Received player data update but data is null");
                return;
            }

            UpdateResourceDisplay(playerData);
        }

        /// <summary>
        /// Updates the UI display with current resource values.
        /// </summary>
        /// <param name="playerData">The player data containing resource information</param>
        private void UpdateResourceDisplay(PlayerData playerData)
        {
            goldText.text = $"Gold: {playerData.GetResourceAmount(ResourceType.Gold)}";
            silverText.text = $"Silver: {playerData.GetResourceAmount(ResourceType.Silver)}";
            coalText.text = $"Coal: {playerData.GetResourceAmount(ResourceType.Coal)}";
        }
    }
}
