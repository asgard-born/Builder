using System.Collections.Generic;
using System.Linq;
using Character;
using Framework.Reactive;
using Shared;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings
{
    public class BuildingPointView : SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<Resource, BuildingResourceView> _costResourcesViews = new();
        [SerializeField] private BuildingResourceView _buildingResourceViewPrefab;
        [SerializeField] private int _id;
        [SerializeField] private Dictionary<int, BuildingLevel> _upgradeDictionary;
        [SerializeField] private Dictionary<BuildCondition, int> _openConditions;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;

        //TODO for demo, after - Deserialization from Json
        [SerializeField] private BuildingState _state;

        private ReactiveTrigger _onPlayerStay;

        private CharacterView _characterView;
        private GameObject _spawnedObject;
        private Camera _camera;
        private int _cost;
        private float _stayedTime;
        private bool _isCharacterStaying;

        public BuildingState state => _state;
        private Dictionary<Resource, Sprite> _resourcesSprites;

        public Dictionary<int, BuildingLevel> upgradeDictionary => _upgradeDictionary;

        public int id => _id;

        public struct Ctx
        {
            public BuildingState state;
            public Dictionary<Resource, Sprite> resourcesSprites;
            public ReactiveTrigger onPlayerStay;
            public Camera camera;
        }

        private void Awake()
        {
            _state.id = _id;
        }

        public void Initialize(Ctx ctx)
        {
            _camera = ctx.camera;
            _state = ctx.state;
            _onPlayerStay = ctx.onPlayerStay;
            _resourcesSprites = ctx.resourcesSprites;

            RxSubscribe();
            RebuildCostViews();
            _layoutGroup.transform.LookAt(_layoutGroup.transform.position + _camera.transform.rotation * Vector3.forward);
        }

        private void RxSubscribe()
        {
            _state.addedResources.ObserveAdd().Subscribe(OnResourceAdded);
            _state.addedResources.ObserveReplace().Subscribe(OnResourceUpdate);
            _state.level.Subscribe(OnLevelUp);
        }

        private void RebuildCostViews()
        {
            if (_costResourcesViews.Any())
            {
                foreach (var view in _costResourcesViews)
                {
                    Destroy(view.Value.gameObject);
                }

                _costResourcesViews.Clear();
            }

            var nextLevelToUpgrade = _state.level.Value + 1;

            if (_upgradeDictionary.TryGetValue(nextLevelToUpgrade, out BuildingLevel buildingLevel))
            {
                foreach (var resourceCount in buildingLevel.resources)
                {
                    BuildingResourceView resourceView = Instantiate(_buildingResourceViewPrefab, _layoutGroup.transform);
                    Sprite sprite = _resourcesSprites[resourceCount.Key];
                    resourceView.Initialize(sprite, resourceCount.Value);
                    _costResourcesViews.Add(resourceCount.Key, resourceView);
                }
            }
        }

        private void OnResourceUpdate(DictionaryReplaceEvent<Resource, int> replaceEvent)
        {
            var remainResource = _state.requiredResourcesForUpgrade[replaceEvent.Key] - replaceEvent.NewValue;
            ArrangeResourceView(replaceEvent.Key, remainResource);
        }

        private void OnResourceAdded(DictionaryAddEvent<Resource, int> addEvent)
        {
            var remainResource = _state.requiredResourcesForUpgrade[addEvent.Key] - addEvent.Value;
            ArrangeResourceView(addEvent.Key, remainResource);
        }

        private void ArrangeResourceView(Resource res, int remainResource)
        {
            if (_costResourcesViews == null || !_costResourcesViews.Any()) return;

            if (!_costResourcesViews.TryGetValue(res, out BuildingResourceView buildingResourceView)) return;

            if (remainResource > 0)
            {
                Debug.Log($"________________AAAAA {gameObject.name} {res.ToString()}: {remainResource}");

                _costResourcesViews[res].UpdateText(remainResource);
            }
            else
            {
                Destroy(buildingResourceView.gameObject);
                _costResourcesViews.Remove(res);
            }
        }

        private void OnLevelUp(int newLevel)
        {
            if (newLevel == 0) return;

            if (_spawnedObject != null)
            {
                Destroy(_spawnedObject);
                _spawnedObject = null;
            }

            BuildingLevel buildingLevel = _upgradeDictionary[newLevel];
            _spawnedObject = Instantiate(buildingLevel.model, _spawnPoint);

            RebuildCostViews();
        }

        private void OnTriggerEnter(Collider other)
        {
            _characterView = other.GetComponent<CharacterView>();

            if (_characterView != null)
            {
                _isCharacterStaying = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _isCharacterStaying = false;
        }

        private void Update()
        {
            if (_isCharacterStaying && !_characterView.isMoving)
            {
                _onPlayerStay?.Notify();
            }
        }
    }
}