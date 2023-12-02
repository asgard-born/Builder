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
        private readonly Dictionary<int, BuildingLevel> _upgrades;
        private readonly AnimationCurve _upgradeCurve;
        private readonly float _lastKeyTime;
        private readonly BuildingState _state;

        private readonly ReactiveTrigger<ResourceCount> _trySpendResourcesForBuildingIteration;
        private int _byingSteps = 12;
        private float _stepByingTime;
        private readonly Ctx _ctx;

        public struct Ctx
        {
            public BuildingState state;
            public Dictionary<int, BuildingLevel> upgrades;
            public AnimationCurve upgradeCurve;
            public ReactiveTrigger onPlayerStay;
            public ReactiveTrigger<ResourceCount> trySpendResourcesForBuildingIteration;
        }

        public BuildingPointPm(Ctx ctx)
        {
            _ctx = ctx;
            _state = ctx.state;
            _upgrades = ctx.upgrades;
            _upgradeCurve = ctx.upgradeCurve;
            _lastKeyTime = ctx.upgradeCurve.keys[ctx.upgradeCurve.keys.Length - 1].time;
            _trySpendResourcesForBuildingIteration = ctx.trySpendResourcesForBuildingIteration;
            _stepByingTime = _lastKeyTime / _byingSteps;

            AddUnsafe(ctx.onPlayerStay.Subscribe(OnPlayerStay));
        }

        private void OnPlayerStay()
        {
            Dictionary<Resource, int> resourcesRequired;

            if (_upgrades.TryGetValue(_state.level.Value + 1, out var buildingLevel))
            {
                resourcesRequired = buildingLevel.resources;
            }
            else
            {
                return;
            }

            ReactiveDictionary<Resource, int> resourcesAdded = _state.addedResources;
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

            var prevByingStep = (int)Math.Floor(_state.inProcessByingTime / _stepByingTime);
            _state.inProcessByingTime += Time.deltaTime;
            var nextByingStep = (int)Math.Floor(_state.inProcessByingTime / _stepByingTime);

            if (nextByingStep > prevByingStep)
            {
                float percentToSpend;

                if (_state.inProcessByingTime >= _lastKeyTime)
                {
                    percentToSpend = _upgradeCurve.Evaluate(_lastKeyTime);
                    _state.inProcessByingTime = _lastKeyTime;
                }
                else
                {
                    percentToSpend = _upgradeCurve.Evaluate(_state.inProcessByingTime);
                }

                percentToSpend = Mathf.Clamp01(percentToSpend);

                int totalResourcesSpendCheckpoint = Mathf.Max(1, (int)Math.Floor(percentToSpend * requiredResource.Value));
                int resourceToSpend = totalResourcesSpendCheckpoint - spendResource;

                _trySpendResourcesForBuildingIteration?.Notify(new ResourceCount(requiredResource.Key, resourceToSpend));
            }
        }
    }
}