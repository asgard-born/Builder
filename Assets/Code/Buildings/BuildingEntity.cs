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

        public struct Ctx
        {
            public BuildingState state;
            public Dictionary<int, BuildingLevel> upgrades;
            public BuildingPointView buildingPointView;
            public AnimationCurve upgradeCurve;
            public Camera camera;
            public ReactiveCommand<ResourceCount, BuingStatus?> trySpendResourcesForBuildingIteration;
            public ReactiveTrigger<ResourceCount> onSpendResourcesForBuildingIteration;
            public Dictionary<Resource, Sprite> resourcesSprites;
        }

        public BuildingEntity(Ctx ctx)
        {
            _onPlayerStay = AddUnsafe(new ReactiveTrigger());

            var buildingPointPmCtx = new BuildingPointPm.Ctx
            {
                state = ctx.state,
                upgrades = ctx.upgrades,
                upgradeCurve = ctx.upgradeCurve,
                onPlayerStay = _onPlayerStay,
                trySpendResourcesForBuildingIteration = ctx.trySpendResourcesForBuildingIteration,
                onSpendResourcesForBuildingIteration = ctx.onSpendResourcesForBuildingIteration,
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
    }
}