using UniRx;
using UnityEngine;

namespace CameraControls
{
    public class CameraView : MonoBehaviour
    {
        [SerializeField] private float _slowFactor = 15f;

        private Ctx _ctx;

        public struct Ctx
        {
            public Transform playerTransform;
        }

        public void Initialize(Ctx ctx)
        {
            _ctx = ctx;

            Observable.EveryLateUpdate().Subscribe(_ => UpdatePosition());
        }

        private void UpdatePosition()
        {
            transform.position = Vector3.Slerp(transform.position, _ctx.playerTransform.position, _slowFactor * Time.deltaTime);
        }
    }
}