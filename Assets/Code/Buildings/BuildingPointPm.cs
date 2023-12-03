using System;
using System.Collections.Generic;
using Framework;
using Framework.Reactive;
using Shared;
using UniRx;
using UnityEngine;

namespace Buildings
{
    public class BuildingPointPm : BaseDisposable
    {
        private readonly float _lastKeyTime;
        private readonly ReactiveCommand<ResourceCount, BuingStatus?> _trySpendResourcesForBuildingIteration;
        private readonly int _byingSteps = 12;
        private readonly float _stepByingTime;
        private readonly Ctx _ctx;
        
        private BuingStatus? _status;

        public struct Ctx
        {
            public BuildingState state;
            public Dictionary<int, BuildingLevel> upgrades;
            public AnimationCurve upgradeCurve;
            public ReactiveTrigger onPlayerStay;
            public ReactiveCommand<ResourceCount, BuingStatus?> trySpendResourcesForBuildingIteration;
            public ReactiveTrigger<ResourceCount> onSpendResourcesForBuildingIteration;
        }

        public BuildingPointPm(Ctx ctx)
        {
            _ctx = ctx;
            _lastKeyTime = ctx.upgradeCurve.keys[ctx.upgradeCurve.keys.Length - 1].time;
            _trySpendResourcesForBuildingIteration = ctx.trySpendResourcesForBuildingIteration;
            _stepByingTime = _lastKeyTime / _byingSteps;

            AddUnsafe(ctx.onPlayerStay.Subscribe(OnPlayerStay));
            AddUnsafe(ctx.onSpendResourcesForBuildingIteration.Subscribe(OnSpendResourcesForBuildingIteration));
        }
        
        private void OnSpendResourcesForBuildingIteration(ResourceCount spendedResource)
        {
            if (_ctx.state.addedResources.TryGetValue(spendedResource.resource, out int value))
            {
                _ctx.state.addedResources[spendedResource.resource] = value + spendedResource.count;
            }
            else
            {
                _ctx.state.addedResources[spendedResource.resource] = spendedResource.count;
            }

            OnResourceChanged(spendedResource.resource, _ctx.state);
        }

        private void OnResourceChanged(Resource resKey, BuildingState state)
        {
            var allIsAdded = true;

            foreach (var required in state.requiredResourcesForUpgrade)
            {
                if (state.addedResources.TryGetValue(required.Key, out var addedCount))
                {
                    if (addedCount < required.Value)
                    {
                        allIsAdded = false;
                    }
                    else
                    {
                        if (resKey == required.Key)
                        {
                            state.inProcessByingTime = 0;
                        }
                    }
                }
                else
                {
                    allIsAdded = false;
                }
            }

            if (allIsAdded)
            {
                LevelUp(state);
            }

            _status = BuingStatus.Completed;
        }

        private void LevelUp(BuildingState state)
        {
            state.level.Value += 1;
            state.addedResources.Clear();
            state.inProcessByingTime = 0;
            
            OnChangeLevel();
        }

        private void OnChangeLevel()
        {
            if (_ctx.upgrades.TryGetValue(_ctx.state.level.Value + 1, out BuildingLevel buildingLevel))
            {
                _ctx.state.requiredResourcesForUpgrade = buildingLevel.resources;
            }
        }

        private void OnPlayerStay()
        {
            if (_status == BuingStatus.Waiting) return;
            
            Dictionary<Resource, int> resourcesRequired;

            if (_ctx.upgrades.TryGetValue(_ctx.state.level.Value + 1, out var buildingLevel))
            {
                resourcesRequired = buildingLevel.resources;
            }
            else
            {
                return;
            }

            ReactiveDictionary<Resource, int> resourcesAdded = _ctx.state.addedResources;
            KeyValuePair<Resource, int> requiredResource = new KeyValuePair<Resource, int>();

            int spendResource = 0;
            bool isAllAdded = true;

            foreach (var required in resourcesRequired)
            {
                var requiredCount = resourcesRequired[required.Key];

                if (resourcesAdded.TryGetValue(required.Key, out var addedCount))
                {
                    if (addedCount < requiredCount)
                    {
                        spendResource = addedCount;
                        requiredResource = required;
                        isAllAdded = false;

                        break;
                    }
                }
                else
                {
                    spendResource = addedCount;
                    requiredResource = required;
                    isAllAdded = false;

                    break;
                }
            }

            if (isAllAdded) return;

            var prevByingStep = (int)Math.Floor(_ctx.state.inProcessByingTime / _stepByingTime);
            _ctx.state.inProcessByingTime += Time.deltaTime;
            var nextByingStep = (int)Math.Floor(_ctx.state.inProcessByingTime / _stepByingTime);

            if (nextByingStep > prevByingStep)
            {
                float percentToSpend;

                if (_ctx.state.inProcessByingTime >= _lastKeyTime)
                {
                    percentToSpend = _ctx.upgradeCurve.Evaluate(_lastKeyTime);
                    _ctx.state.inProcessByingTime = _lastKeyTime;
                }
                else
                {
                    percentToSpend = _ctx.upgradeCurve.Evaluate(_ctx.state.inProcessByingTime);
                }

                percentToSpend = Mathf.Clamp01(percentToSpend);

                int totalResourcesSpendCheckpoint = Mathf.Max(1, (int)Math.Floor(percentToSpend * requiredResource.Value));
                int resourceToSpend = totalResourcesSpendCheckpoint - spendResource;

                _status = BuingStatus.Waiting;
                _status = _trySpendResourcesForBuildingIteration?.Execute(new ResourceCount(requiredResource.Key, resourceToSpend));
            }
        }
    }
}