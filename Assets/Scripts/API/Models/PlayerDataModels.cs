using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelPunk.API.Models
{
    [Serializable]
    public class PlayerData
    {
        [SerializeField] private string createdAt;
        [SerializeField] private string lastUpdatedAt;
        public List<ResourceData> resources = new();

        public DateTime CreatedAt => DateTime.Parse(createdAt);
        public DateTime LastUpdatedAt => DateTime.Parse(lastUpdatedAt);
    }

    [Serializable]
    public class ResourceData
    {
        public string resourceName;
        public int amount;
        [SerializeField] private string lastUpdatedAt;
        
        public DateTime LastUpdatedAt => DateTime.Parse(lastUpdatedAt);
    }
}
