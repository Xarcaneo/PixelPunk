#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using PixelPunk.API.Models;

namespace PixelPunk.Game.Resources
{
    /// <summary>
    /// Extension methods for working with game resources.
    /// </summary>
    public static class ResourceExtensions
    {
        /// <summary>
        /// Gets the amount of a specific resource type from a collection of resources.
        /// </summary>
        /// <param name="resources">The collection of resources to search</param>
        /// <param name="resourceType">The type of resource to find</param>
        /// <returns>The amount of the resource, or 0 if not found</returns>
        public static int GetAmount(this IEnumerable<ResourceData> resources, ResourceType resourceType)
        {
            return resources?
                .FirstOrDefault(r => string.Equals(r.resourceName, resourceType.ToString(), StringComparison.OrdinalIgnoreCase))
                ?.amount ?? 0;
        }

        /// <summary>
        /// Gets the last update time for a specific resource type.
        /// </summary>
        /// <param name="resources">The collection of resources to search</param>
        /// <param name="resourceType">The type of resource to find</param>
        /// <returns>The last update time of the resource, or DateTime.MinValue if not found</returns>
        public static DateTime GetLastUpdateTime(this IEnumerable<ResourceData> resources, ResourceType resourceType)
        {
            return resources?
                .FirstOrDefault(r => string.Equals(r.resourceName, resourceType.ToString(), StringComparison.OrdinalIgnoreCase))
                ?.LastUpdatedAt ?? DateTime.MinValue;
        }

        /// <summary>
        /// Tries to get a specific resource from a collection of resources.
        /// </summary>
        /// <param name="resources">The collection of resources to search</param>
        /// <param name="resourceType">The type of resource to find</param>
        /// <param name="resourceData">When this method returns, contains the resource data if found, or null if not found</param>
        /// <returns>True if the resource was found, false otherwise</returns>
        public static bool TryGetResource(this IEnumerable<ResourceData> resources, ResourceType resourceType, out ResourceData? resourceData)
        {
            resourceData = resources?
                .FirstOrDefault(r => string.Equals(r.resourceName, resourceType.ToString(), StringComparison.OrdinalIgnoreCase));
            return resourceData != null;
        }
    }
}
