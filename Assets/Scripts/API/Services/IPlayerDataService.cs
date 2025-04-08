#nullable enable

using System;
using System.Collections;
using PixelPunk.API.Models;
using PixelPunk.Core;

namespace PixelPunk.API.Services
{
    public interface IPlayerDataService : IGameService
    {
        /// <summary>
        /// Gets the cached player data
        /// </summary>
        PlayerData? PlayerData { get; }

        /// <summary>
        /// Event triggered when player data is updated
        /// </summary>
        event Action? OnPlayerDataUpdated;

        /// <summary>
        /// Fetches the latest player data from the API
        /// </summary>
        IEnumerator FetchPlayerData(Action<PlayerData>? onComplete = null, Action<string>? onError = null);
    }
}
