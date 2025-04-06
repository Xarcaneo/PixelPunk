#nullable enable

using UnityEngine;
using PixelPunk.API.Services;

namespace PixelPunk.Core
{
    /// <summary>
    /// Bootstrapper class responsible for initializing essential game services.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Method to perform initial setup before the first scene is loaded.
        /// This is automatically called by Unity during game initialization.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Setup()
        {
            // Initialize the service registry
            ServiceRegistry.Initialize();

            // Register core services
            ServiceRegistry.Instance?.RegisterService<IAuthService>(new AuthService());
        }
    }
}
