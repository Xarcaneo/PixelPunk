#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PixelPunk.Game.Resources;

namespace PixelPunk.API.Models
{
    /// <summary>
    /// Represents player's game data including resources and buildings
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [SerializeField] private string createdAt = string.Empty;
        [SerializeField] private string lastUpdatedAt = string.Empty;
        [SerializeField] private List<ResourceData> resources = new();
        [SerializeField] private List<BuildingData> cityBuildings = new();  // Match backend field name

        /// <summary>
        /// Gets the time when this player data was created
        /// </summary>
        public DateTime CreatedAt => DateTime.Parse(createdAt);

        /// <summary>
        /// Gets the time when this player data was last updated
        /// </summary>
        public DateTime LastUpdatedAt => DateTime.Parse(lastUpdatedAt);

        /// <summary>
        /// Gets the list of player's resources
        /// </summary>
        public IReadOnlyList<ResourceData> Resources => resources;

        /// <summary>
        /// Gets the list of player's buildings
        /// </summary>
        public IReadOnlyList<BuildingData> Buildings => cityBuildings;

        /// <summary>
        /// Gets the amount of a specific resource type
        /// </summary>
        /// <param name="type">The type of resource to check</param>
        /// <returns>The amount of the specified resource</returns>
        public int GetResourceAmount(ResourceType type) => resources.GetAmount(type);

        /// <summary>
        /// Gets the last update time for a specific resource type
        /// </summary>
        /// <param name="type">The type of resource to check</param>
        /// <returns>The last time the resource was updated</returns>
        public DateTime GetResourceLastUpdate(ResourceType type) => resources.GetLastUpdateTime(type);

        /// <summary>
        /// Tries to get a specific resource by type
        /// </summary>
        /// <param name="type">The type of resource to find</param>
        /// <param name="resource">The found resource, if any</param>
        /// <returns>True if the resource was found, false otherwise</returns>
        public bool TryGetResource(ResourceType type, out ResourceData? resource) => resources.TryGetResource(type, out resource);
    }

    /// <summary>
    /// Represents a player's resource with its amount and metadata
    /// </summary>
    [Serializable]
    public class ResourceData
    {
        [SerializeField] private string resourceName = string.Empty;
        [SerializeField] private int amount;
        [SerializeField] private string lastUpdatedAt = string.Empty;
        
        /// <summary>
        /// Gets or sets the name of the resource
        /// </summary>
        public string ResourceName
        {
            get => resourceName;
            set => resourceName = value;
        }

        /// <summary>
        /// Gets or sets the amount of the resource
        /// </summary>
        public int Amount
        {
            get => amount;
            set => amount = value;
        }

        /// <summary>
        /// Gets the last time this resource was updated
        /// </summary>
        public DateTime LastUpdatedAt => DateTime.Parse(lastUpdatedAt);

        /// <summary>
        /// Gets the resource type parsed from the resource name
        /// </summary>
        public ResourceType Type => Enum.TryParse<ResourceType>(resourceName, true, out var type) ? type : default;
    }

    /// <summary>
    /// Represents a building in the player's city
    /// </summary>
    [Serializable]
    public class BuildingData
    {
        [SerializeField] private string buildingType = string.Empty;
        [SerializeField] private float x;
        [SerializeField] private float y;
        [SerializeField] private string lastUpdatedAt = string.Empty;

        /// <summary>
        /// Gets or sets the building type identifier
        /// </summary>
        public int BuildingId
        {
            get => int.TryParse(buildingType, out var id) ? id : 0;
            set => buildingType = value.ToString();
        }

        /// <summary>
        /// Gets or sets the X coordinate of the building
        /// </summary>
        public float PositionX
        {
            get => x;
            set => x = value;
        }

        /// <summary>
        /// Gets or sets the Y coordinate of the building
        /// </summary>
        public float PositionY
        {
            get => y;
            set => y = value;
        }

        /// <summary>
        /// Gets the last time this building was updated
        /// </summary>
        public DateTime LastUpdatedAt => DateTime.Parse(lastUpdatedAt);
    }
}
