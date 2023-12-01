using Shared;
using UniRx;
using UnityEngine;

namespace Character
{
    public struct PlayerStats
    {
        public int movementSpeed;
        public int rotationSpeed;
        public Vector3 spawnPosition;
        public int experienceCount;
        public ReactiveDictionary<Resource, int> resourcesCount;
    }
}