using System.Collections.Generic;
using System.Globalization;
using Buildings;
using Configs;
using Framework;
using UnityEngine;

namespace Root
{
    public class RootEntity : BaseDisposable
    {
        public struct Ctx
        {
            public ContentProvider contentProvider;
            public List<BuildingPointView> buildingPoints;
            public RectTransform uiRoot;
        }

        public RootEntity(Ctx ctx)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            CreateGameEntity(ctx);
        }
        
        private void CreateGameEntity(Ctx ctx)
        {
            var gameCtx = new GameEntity.Ctx
            {
                contentProvider = ctx.contentProvider,
                uiRoot = ctx.uiRoot,
                buildingPoints = ctx.buildingPoints,
            };

            AddUnsafe(new GameEntity(gameCtx));
        }
    }
}