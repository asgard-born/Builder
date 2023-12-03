using System.Collections.Generic;
using System.Linq;
using Bank;
using Buildings;
using Character;
using Configs;
using Framework;
using Framework.Reactive;
using Shared;
using UI.Entities;
using UniRx;
using UnityEngine;

namespace Root
{
    public class GameEntity : BaseDisposable
    {
        private ReactiveEvent<Vector3, Quaternion> _onMovementUpdated;
        private ReactiveEvent<Vector2> _onInputUpdated;
        private ReactiveCommand<(BuildingState, ResourceCount), BuingStatus> _trySpendResourcesForBuildingIteration;
        private ReactiveTrigger<BuildingState, ResourceCount> _onSpendResourcesForBuildingIteration;

        public struct Ctx
        {
            public ContentProvider contentProvider;
            public List<BuildingPointView> buildingPoints;
            public RectTransform uiRoot;
        }

        public GameEntity(Ctx ctx)
        {
            InitializeRx();
            InitializeInput(ctx);
            InitializeCharacter(ctx);
            InitializeBuildings(ctx);
            InitializeBank(ctx);
        }

        private void InitializeRx()
        {
            _onInputUpdated = AddUnsafe(new ReactiveEvent<Vector2>());
            _onMovementUpdated = AddUnsafe(new ReactiveEvent<Vector3, Quaternion>());
            _trySpendResourcesForBuildingIteration = AddUnsafe(new ReactiveCommand<(BuildingState, ResourceCount), BuingStatus>());
            _onSpendResourcesForBuildingIteration = AddUnsafe(new ReactiveTrigger<BuildingState, ResourceCount>());
        }

        private void InitializeBuildings(Ctx ctx)
        {
            Dictionary<int, BuildingState> states = ctx.buildingPoints.Select(point => point.state).ToDictionary(state => state.id, state => state);

            foreach (var buildingPointView in ctx.buildingPoints)
            {
                if (!states.TryGetValue(buildingPointView.id, out var state)) continue;

                //TODO for demo, after - Deserialization from Json
                state.requiredResourcesForUpgrade = buildingPointView.upgradeDictionary[buildingPointView.state.level.Value + 1].resources;

                var onSpendResourcesForBuildingIteration = AddUnsafe(new ReactiveTrigger<ResourceCount>());
                var trySpendResourcesForBuildingIteration = AddUnsafe(new ReactiveCommand<ResourceCount, BuingStatus?>());

                trySpendResourcesForBuildingIteration.Subscribe(resourceCount =>
                {
                    return _trySpendResourcesForBuildingIteration?.Execute((state, resourceCount));
                });
                
                _onSpendResourcesForBuildingIteration.Subscribe((buildingState, count) =>
                {
                    if (buildingState == state)
                    {
                        onSpendResourcesForBuildingIteration.Notify(count);
                    }
                });

                var buildingEntityCtx = new BuildingEntity.Ctx
                {
                    upgrades = buildingPointView.upgradeDictionary,
                    state = state,
                    upgradeCurve = ctx.contentProvider.initialStats.buildingUpgradeCurve,
                    resourcesSprites = ctx.contentProvider.resourcesSprites,
                    buildingPointView = buildingPointView,
                    camera = Camera.main,
                    trySpendResourcesForBuildingIteration = trySpendResourcesForBuildingIteration,
                    onSpendResourcesForBuildingIteration = onSpendResourcesForBuildingIteration
                };

                AddUnsafe(new BuildingEntity(buildingEntityCtx));
            }
        }

        private void InitializeInput(Ctx ctx)
        {
            var virtualPadEntityCtx = new VirtualPadEntity.Ctx
            {
                uiRoot = ctx.uiRoot,
                virtualPadView = ctx.contentProvider.uiViews.virtualPadView,
                onInputUpdated = _onInputUpdated
            };

            AddUnsafe(new VirtualPadEntity(virtualPadEntityCtx));
        }

        private void InitializeCharacter(Ctx ctx)
        {
            var characterCtx = new CharacterEntity.Ctx
            {
                uiRoot = ctx.uiRoot,
                contentProvider = ctx.contentProvider,
                onInputUpdated = _onInputUpdated,
                onMovementUpdated = _onMovementUpdated
            };

            AddUnsafe(new CharacterEntity(characterCtx));
        }

        private void InitializeBank(Ctx ctx)
        {
            var bankCtx = new BankEntity.Ctx
            {
                playerResources = ctx.contentProvider.playerResources,
                uiRoot = ctx.uiRoot,
                view = ctx.contentProvider.uiViews.BankView,
                trySpendResourcesForBuildingIteration = _trySpendResourcesForBuildingIteration,
                onSpendResourcesForBuildingIteration = _onSpendResourcesForBuildingIteration
            };

            AddUnsafe(new BankEntity(bankCtx));
        }
    }
}