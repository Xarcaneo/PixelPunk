#nullable enable

using System;
using System.Collections;
using UnityEngine;
using PixelPunk.API.Core;
using PixelPunk.API.Models;
using PixelPunk.Core;

namespace PixelPunk.API.Services
{
    public class PlayerDataService : IPlayerDataService
    {
        private PlayerData? _playerData;

        public PlayerData? PlayerData => _playerData;
        public event Action? OnPlayerDataUpdated;

        public IEnumerator FetchPlayerData(Action<PlayerData>? onComplete = null, Action<string>? onError = null)
        {
            var request = ApiClient.Instance?.SendRequest<PlayerData>(
                "Player/data",
                HttpMethod.GET,
                response => {
                    _playerData = response;
                    onComplete?.Invoke(response);
                    OnPlayerDataUpdated?.Invoke();
                },
                error => {
                    Debug.LogError($"[PlayerDataService] Error fetching player data: {error}");
                    onError?.Invoke(error);
                },
                requiresAuth: true
            );

            if (request == null)
            {
                throw new InvalidOperationException("ApiClient.Instance is null");
            }

            yield return request;
        }
    }
}
