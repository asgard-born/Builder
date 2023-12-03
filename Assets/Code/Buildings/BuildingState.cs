using System;
using System.Collections.Generic;
using Framework;
using Shared;
using UniRx;

namespace Buildings
{
    [Serializable]
    public class BuildingState : BaseDisposable
    {
        public int id;
        public float inProcessByingTime;
        public ReactiveProperty<int> level = new();
        public Dictionary<Resource, int> requiredResourcesForUpgrade;
        public ReactiveDictionary<Resource, int> addedResources = new();
        // BuingStatus: waiting, failed, success 
    }
}