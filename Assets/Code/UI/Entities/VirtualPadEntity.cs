using Framework;
using Framework.Reactive;
using UI.Views;
using UnityEngine;

namespace UI.Entities
{
    public class VirtualPadEntity : BaseDisposable
    {
        public struct Ctx
        {
            public RectTransform uiRoot;
            public VirtualPadView virtualPadView;
            public ReactiveEvent<Vector2> onInputUpdated;
        }

        public VirtualPadEntity(Ctx ctx)
        {
            var virtualPadCtx = new VirtualPadView.Ctx
            {
                onInputUpdated = ctx.onInputUpdated
            };

            VirtualPadView virtualPadView = Object.Instantiate(ctx.virtualPadView, ctx.uiRoot);
            virtualPadView.SetCtx(virtualPadCtx);
        }
    }
}