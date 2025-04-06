#nullable enable

using System;
using System.Collections.Generic;

namespace PixelPunk.Core
{
    /// <summary>
    /// A service locator for managing game service instances.
    /// Provides centralized access to all game services through a singleton pattern.
    /// </summary>
    public class ServiceRegistry
    {
        /// <summary>
        /// Dictionary to hold registered service instances.
        /// </summary>
        private readonly Dictionary<string, IGameService> _serviceDictionary = new Dictionary<string, IGameService>();

        /// <summary>
        /// Gets the singleton instance of the ServiceRegistry.
        /// </summary>
        public static ServiceRegistry? Instance { get; private set; }

        /// <summary>
        /// Initializes the ServiceRegistry singleton instance.
        /// Should be called during game initialization.
        /// </summary>
        public static void Initialize()
        {
            Instance = new ServiceRegistry();
        }

        /// <summary>
        /// Retrieves a service instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve</typeparam>
        /// <returns>The instance of the requested service</returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested service is not registered</exception>
        public T GetService<T>() where T : IGameService
        {
            string serviceName = typeof(T).Name;
            if (!_serviceDictionary.ContainsKey(serviceName))
            {
                throw new InvalidOperationException($"Service of type {serviceName} is not registered.");
            }

            return (T)_serviceDictionary[serviceName];
        }

        /// <summary>
        /// Registers a service instance with the service registry.
        /// </summary>
        /// <typeparam name="T">The type of service to register</typeparam>
        /// <param name="service">The service instance to register</param>
        /// <exception cref="InvalidOperationException">Thrown when a service of the same type is already registered</exception>
        public void RegisterService<T>(T service) where T : IGameService
        {
            string serviceName = typeof(T).Name;
            if (_serviceDictionary.ContainsKey(serviceName))
            {
                throw new InvalidOperationException($"Service of type {serviceName} is already registered.");
            }

            _serviceDictionary.Add(serviceName, service);
        }

        /// <summary>
        /// Unregisters a service instance from the service registry.
        /// </summary>
        /// <typeparam name="T">The type of service to unregister</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when the service to unregister is not registered</exception>
        public void UnregisterService<T>() where T : IGameService
        {
            string serviceName = typeof(T).Name;
            if (!_serviceDictionary.ContainsKey(serviceName))
            {
                throw new InvalidOperationException($"Service of type {serviceName} is not registered.");
            }

            _serviceDictionary.Remove(serviceName);
        }
    }
}
