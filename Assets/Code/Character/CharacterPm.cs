using Configs;
using Framework;
using Framework.Reactive;
using UnityEngine;

namespace Character
{
    public class CharacterPm : BaseDisposable
    {
        private readonly float _movementSpeed;
        private readonly ReactiveEvent<Vector3, Quaternion> _onMovementUpdated;
        private Vector3 _movement;

        public struct Ctx
        {
            public ContentProvider contentProvider;
            public ReactiveEvent<Vector3, Quaternion> onMovementUpdated;
            public float movementSpeed;
            public ReactiveEvent<Vector2> onInputUpdated;
        }

        public CharacterPm(Ctx ctx)
        {
            _onMovementUpdated = ctx.onMovementUpdated;
            _movementSpeed = ctx.movementSpeed;

            ctx.onInputUpdated.SubscribeWithSkip(UpdateMovement);
        }

        private void UpdateMovement(Vector2 playerInputData)
        {
            _movement = Vector3.zero;
            _movement += Vector3.forward * playerInputData.y;
            _movement += Vector3.right * playerInputData.x;
            _movement *= _movementSpeed * Time.deltaTime;

            var rotatingVector = new Vector3(playerInputData.x, 0, playerInputData.y);
            var rotation = Quaternion.LookRotation(rotatingVector, Vector3.up);

            _onMovementUpdated.Notify(_movement, rotation);
        }
    }
}