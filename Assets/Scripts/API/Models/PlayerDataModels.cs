#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using PixelPunk.Game.Resources;

namespace PixelPunk.API.Models
{
    [Serializable]
    public class PlayerData
    {
        [SerializeField] private string createdAt = string.Empty;
        [SerializeField] private string lastUpdatedAt = string.Empty;
        public List<ResourceData> resources = new();

        public DateTime CreatedAt => DateTime.Parse(createdAt);
        public DateTime LastUpdatedAt => DateTime.Parse(lastUpdatedAt);

        // Helper methods for easy resource access
        public int GetResourceAmount(ResourceType type) => resources.GetAmount(type);
        public DateTime GetResourceLastUpdate(ResourceType type) => resources.GetLastUpdateTime(type);
        public bool TryGetResource(ResourceType type, out ResourceData? resource) => resources.TryGetResource(type, out resource);
    }

    [Serializable]
    public class ResourceData
    {
        public string resourceName = string.Empty;
        public int amount;
        [SerializeField] private string lastUpdatedAt = string.Empty;
        
        public DateTime LastUpdatedAt => DateTime.Parse(lastUpdatedAt);
        public ResourceType Type => Enum.TryParse<ResourceType>(resourceName, true, out var type) ? type : default;
    }
}
