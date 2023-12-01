using System;
using System.Collections.Generic;
using Bank;
using CameraControls;
using Character;
using Shared;
using Sirenix.OdinInspector;
using UI.Views;
using UniRx;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "ContentProvider", menuName = "Configs/Content Provider")]
    public class ContentProvider : SerializedScriptableObject
    {
        public ViewsContent views => _views;
        public UIViewsContent uiViews => _uiViews;
        public InitialStats initialStats => _initialStats;

        [Space] [SerializeField] private ViewsContent _views;
        [SerializeField] private UIViewsContent _uiViews;

        [SerializeField] private Dictionary<Resource, Sprite> _resourcesSprites;
        [SerializeField] private List<LevelExperience> _levelsExperiences;

        [Space] [SerializeField] private InitialStats _initialStats;

        public Dictionary<Resource, Sprite> resourcesSprites => _resourcesSprites;
        public List<LevelExperience> levelsExperiences => _levelsExperiences;

        public ReactiveDictionary<Resource, int> playerResources => _playerResources;

        private ReactiveDictionary<Resource, int> _playerResources;

        private void OnEnable()
        {
            _playerResources = new ReactiveDictionary<Resource, int>();
            _playerResources.Add(Resource.Wood, 100000);
            _playerResources.Add(Resource.Gold, 5000);
            _playerResources.Add(Resource.Crystals, 5);
        }

        [Serializable]
        public class ViewsContent
        {
            public CharacterView characterView => _characterView;
            public CameraView cameraView => _cameraView;

            [SerializeField] private CharacterView _characterView;
            [SerializeField] private CameraView _cameraView;
        }

        [Serializable]
        public class UIViewsContent
        {
            public BankView BankView => _bankView;
            public CharacterHud characterHud => _characterHud;
            public VirtualPadView virtualPadView => _virtualPadView;

            [SerializeField] private BankView _bankView;
            [SerializeField] private CharacterHud _characterHud;
            [SerializeField] private VirtualPadView _virtualPadView;
        }

        [Serializable]
        public class InitialStats
        {
            public Vector3 initialPosition => _initialPosition;
            public int movementSpeed => _movementSpeed;
            public int rotationSpeed => _rotationSpeed;
            public AnimationCurve buildingUpgradeCurve => _buildingUpgradeCurve;

            [SerializeField] private AnimationCurve _buildingUpgradeCurve;
            [SerializeField] private Vector3 _initialPosition = new(21.4f, -20.7f, -973.8f);
            [SerializeField] private int _movementSpeed = 15;
            [SerializeField] private int _rotationSpeed = 350;
        }
    }
}