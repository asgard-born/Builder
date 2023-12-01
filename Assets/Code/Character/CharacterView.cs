using Framework.Reactive;
using UnityEngine;

namespace Character
{
    public class CharacterView : MonoBehaviour
    {
        [SerializeField] private GameObject _characterModel;
        [SerializeField] private CharacterController controller;
        [SerializeField] private Animator animator;

        private PlayerStats _playerStats;
        private bool _isMoving;

        private static readonly int _isMovingHash = Animator.StringToHash("isMoving");

        public bool isMoving => _isMoving;

        public struct Ctx
        {
            public PlayerStats stats;
            public ReactiveEvent<Vector3, Quaternion> onMovementUpdated;
        }

        public void Initialize(Ctx ctx)
        {
            _playerStats = ctx.stats;
            ctx.onMovementUpdated.SubscribeWithSkip(Move);
        }

        private void Move(Vector3 movement, Quaternion newRotation)
        {
            if (movement == Vector3.zero)
            {
                Stop();

                return;
            }

            _isMoving = true;
            controller.Move(movement);
            animator.SetBool(_isMovingHash, true);

            _characterModel.transform.rotation =
                Quaternion.RotateTowards(
                    _characterModel.transform.rotation, newRotation,
                    _playerStats.rotationSpeed * Time.deltaTime);
        }

        private void Stop()
        {
            _isMoving = false;
            animator.SetBool(_isMovingHash, false);
        }
    }
}