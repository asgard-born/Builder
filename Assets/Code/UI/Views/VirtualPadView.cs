using Framework;
using Framework.Reactive;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Views
{
    public class VirtualPadView : BaseMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private float _minDistance;
        [SerializeField] private float _maxDistance;

        private ReactiveEvent<Vector2> _onInputUpdated;

        private Vector2 _pointerDownPosition;
        private Vector2 _pointerCurrentPosition;
        private Vector2 _playerInputData;

        public struct Ctx
        {
            public ReactiveEvent<Vector2> onInputUpdated;
        }

        public void SetCtx(Ctx ctx)
        {
            _onInputUpdated = ctx.onInputUpdated;
            Observable.EveryUpdate().Subscribe(UpdateInput).AddTo(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownPosition = eventData.position;
            _pointerCurrentPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pointerDownPosition = Vector2.zero;
            _pointerCurrentPosition = Vector2.zero;
            _onInputUpdated.Notify(Vector2.zero);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _pointerCurrentPosition = eventData.position;
        }

        private void UpdateInput(long _)
        {
            var distance = Vector3.Distance(_pointerCurrentPosition, _pointerDownPosition);

            if (distance == 0) return;

            if (distance > _maxDistance)
            {
                _pointerDownPosition += (_pointerCurrentPosition - _pointerDownPosition).normalized * (distance - _maxDistance);
            }

            _playerInputData = (_pointerCurrentPosition - _pointerDownPosition).normalized;

            _onInputUpdated.Notify(_playerInputData);
        }
    }
}