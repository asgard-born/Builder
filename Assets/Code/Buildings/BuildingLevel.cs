using System;
using System.Collections.Generic;
using Shared;
using UnityEngine;

namespace Buildings
{
    [Serializable]
    public class BuildingLevel
    {
        [SerializeField] private Dictionary<Resource, int> _resources;
        [SerializeField] private GameObject _model;
        
        public Dictionary<Resource, int> resources => _resources;
        public GameObject model => _model;
    }
}