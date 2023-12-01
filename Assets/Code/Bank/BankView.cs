using System.Collections.Generic;
using Shared;
using Sirenix.OdinInspector;
using UI.ResourcesViews;
using UniRx;
using UnityEngine;

namespace Bank
{
    public class BankView : SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<Resource, ResourceBaseView> _resources;

        private readonly ReactiveDictionary<Resource, int> _playerResources;

        public struct Ctx
        {
            public ReactiveDictionary<Resource, int> playerResources;
        }

        public void Initialize(Ctx ctx)
        {
            ctx.playerResources.ObserveAdd().Subscribe(OnResourceAdded);
            ctx.playerResources.ObserveReplace().Subscribe(OnResourceUpdate);

            foreach (var resource in ctx.playerResources)
            {
                _resources[resource.Key].ChangeValue(resource.Value);
            }
        }

        private void OnResourceUpdate(DictionaryReplaceEvent<Resource, int> replaceEvent)
        {
            if (_resources.TryGetValue(replaceEvent.Key, out _))
            {
                _resources[replaceEvent.Key].ChangeValue(replaceEvent.NewValue);
            }
        }

        private void OnResourceAdded(DictionaryAddEvent<Resource, int> addEvent)
        {
            if (_resources.TryGetValue(addEvent.Key, out var oldValue))
            {
                _resources[addEvent.Key].ChangeValue(addEvent.Value);
            }
        }
    }
}