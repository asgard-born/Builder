using CameraControls;
using Configs;
using Framework;
using Framework.Reactive;
using UnityEngine;

namespace Character
{
    public class CharacterEntity : BaseDisposable
    {
        public struct Ctx
        {
            public RectTransform uiRoot;
            public CharacterHud characterHud;
            public ContentProvider contentProvider;
            public ReactiveEvent<Vector2> onInputUpdated;
            public ReactiveEvent<Vector3, Quaternion> onMovementUpdated;
        }

        public CharacterEntity(Ctx ctx)
        {
            var characterPmCtx = new CharacterPm.Ctx
            {
                contentProvider = ctx.contentProvider,
                movementSpeed = ctx.contentProvider.initialStats.movementSpeed,
                onInputUpdated = ctx.onInputUpdated,
                onMovementUpdated = ctx.onMovementUpdated,
            };

            AddUnsafe(new CharacterPm(characterPmCtx));

            var playerStats = new PlayerStats
            {
                movementSpeed = ctx.contentProvider.initialStats.movementSpeed,
                rotationSpeed = ctx.contentProvider.initialStats.rotationSpeed,
                spawnPosition = ctx.contentProvider.initialStats.initialPosition,
                experienceCount = 0,
            };

            var characterViewCtx = new CharacterView.Ctx
            {
                stats = playerStats,
                onMovementUpdated = ctx.onMovementUpdated,
            };

            CharacterView characterView = Object.Instantiate(ctx.contentProvider.views.characterView);
            characterView.Initialize(characterViewCtx);
            characterView.transform.position = ctx.contentProvider.initialStats.initialPosition;

            var cameraViewCtx = new CameraView.Ctx
            {
                playerTransform = characterView.transform
            };

            CameraView cameraView = Object.Instantiate(ctx.contentProvider.views.cameraView);
            cameraView.Initialize(cameraViewCtx);
            
            CharacterHud characterHud = Object.Instantiate(ctx.contentProvider.uiViews.characterHud, ctx.uiRoot);
        }
    }
}