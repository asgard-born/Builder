using Buildings;
using Framework;
using Framework.Reactive;
using Shared;
using UniRx;

namespace Bank
{
    public class BankPm : BaseDisposable
    {
        private readonly ReactiveDictionary<Resource, int> _playerResources;
        private readonly ReactiveTrigger<ResourceCount, BuildingState> _onSpendResourcesForBuildingIteration;

        public struct Ctx
        {
            public ReactiveDictionary<Resource, int> playerResources;
            public ReactiveTrigger<ResourceCount, BuildingState> onSpendResourcesForBuildingIteration;
            public ReactiveTrigger<ResourceCount, BuildingState> trySpendResourcesForBuildingIteration;
        }

        public BankPm(Ctx ctx)
        {
            _playerResources = ctx.playerResources;
            _onSpendResourcesForBuildingIteration = ctx.onSpendResourcesForBuildingIteration;
            AddUnsafe(ctx.trySpendResourcesForBuildingIteration.Subscribe(TrySpendResourcesForBuildingIteration));
        }

        private void TrySpendResourcesForBuildingIteration(ResourceCount demandResource, BuildingState state)
        {
            int spendCount = TrySpendResource(demandResource);

            if (spendCount > 0)
            {
                _onSpendResourcesForBuildingIteration?.Notify(new ResourceCount(demandResource.resource, spendCount), state);
            }
        }

        private int TrySpendResource(ResourceCount demandResource)
        {
            if (_playerResources.TryGetValue(demandResource.resource, out var bankValueCount))
            {
                if (bankValueCount > 0)
                {
                    if (bankValueCount > demandResource.count)
                    {
                        bankValueCount -= demandResource.count;
                        _playerResources[demandResource.resource] = bankValueCount;

                        return demandResource.count;
                    }

                    bankValueCount = 0;
                    _playerResources[demandResource.resource] = 0;

                    return bankValueCount;
                }
            }

            return 0;
        }
    }
}