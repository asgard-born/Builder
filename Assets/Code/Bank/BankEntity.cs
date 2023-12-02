using Buildings;
using Framework;
using Framework.Reactive;
using Shared;
using UniRx;
using UnityEngine;

namespace Bank
{
    public class BankEntity : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveDictionary<Resource, int> playerResources;
            public RectTransform uiRoot;
            public BankView view;
            public ReactiveTrigger<BuildingState, ResourceCount> trySpendResourcesForBuildingIteration;
            public ReactiveTrigger<BuildingState, ResourceCount> onSpendResourcesForBuildingIteration;
        }

        public BankEntity(Ctx ctx)
        {
            var bankPmCtx = new BankPm.Ctx
            {
                playerResources = ctx.playerResources,
                onSpendResourcesForBuildingIteration = ctx.onSpendResourcesForBuildingIteration,
                trySpendResourcesForBuildingIteration = ctx.trySpendResourcesForBuildingIteration
            };

            AddUnsafe(new BankPm(bankPmCtx));

            BankView view = Object.Instantiate(ctx.view, ctx.uiRoot);

            var viewCtx = new BankView.Ctx
            {
                playerResources = ctx.playerResources
            };

            view.Initialize(viewCtx);
        }
    }
}