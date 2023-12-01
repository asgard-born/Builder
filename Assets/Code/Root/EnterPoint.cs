using System.Collections.Generic;
using Buildings;
using Configs;
using UnityEngine;

namespace Root
{
    public class EnterPoint : MonoBehaviour
    {
        [SerializeField] private ContentProvider _contentProvider;
        [SerializeField] private List<BuildingPointView> _buildingPoints;
        [SerializeField] private RectTransform _uiRoot;

        private RootEntity _root;

        private void Start()
        {
            var rootCtx = new RootEntity.Ctx
            {
                contentProvider = _contentProvider,
                uiRoot = _uiRoot,
                buildingPoints = _buildingPoints,
            };

            _root = new RootEntity(rootCtx);
        }

        private void OnDestroy()
        {
            _root.Dispose();
        }
    }
}