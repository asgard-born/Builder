using System.Collections.Generic;
using Framework;
using Framework.Reactive;
using Shared;
using UniRx;
using UnityEngine;

namespace Buildings
{
    public class BuildingEntity : BaseDisposable
    {
        private readonly ReactiveTrigger _onPlayerStay;
        private readonly Ctx _ctx;

        public struct Ctx
        {
            public BuildingState state;
            public Dictionary<int, BuildingLevel> upgrades;
            public BuildingPointView buildingPointView;
            public AnimationCurve upgradeCurve;
            public Camera camera;
            public ReactiveTrigger<ResourceCount> trySpendResourcesForBuildingIteration;
            public ReactiveTrigger<ResourceCount> onSpendResourcesForBuildingIteration;
            public Dictionary<Resource, Sprite> resourcesSprites;
        }

        public BuildingEntity(Ctx ctx)
        {
            _ctx = ctx;

            AddUnsafe(ctx.state.level.Subscribe(OnChangeLevel));
            AddUnsafe(ctx.onSpendResourcesForBuildingIteration.Subscribe(OnSpendResourcesForBuildingIteration));

            _onPlayerStay = AddUnsafe(new ReactiveTrigger());

            var buildingPointPmCtx = new BuildingPointPm.Ctx
            {
                state = ctx.state,
                upgrades = ctx.upgrades,
                upgradeCurve = ctx.upgradeCurve,
                onPlayerStay = _onPlayerStay,
                trySpendResourcesForBuildingIteration = ctx.trySpendResourcesForBuildingIteration,
            };

            AddUnsafe(new BuildingPointPm(buildingPointPmCtx));

            InitializeView(ctx);
        }

        private void InitializeView(Ctx ctx)
        {
            var buildingViewCtx = new BuildingPointView.Ctx
            {
                state = ctx.state,
                resourcesSprites = ctx.resourcesSprites,
                camera = ctx.camera,
                onPlayerStay = _onPlayerStay,
            };

            ctx.buildingPointView.Initialize(buildingViewCtx);
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
        }

        private void LevelUp(BuildingState state)
        {
            state.level.Value++;
            state.addedResources.Clear();
            state.inProcessByingTime = 0;
        }

        private void OnChangeLevel(int newLevel)
        {
            if (_ctx.buildingPointView.upgradeDictionary.TryGetValue(newLevel, out BuildingLevel buildingLevel))
            {
                _ctx.state.requiredResourcesForUpgrade = buildingLevel.resources;
            }
        }
    }
}