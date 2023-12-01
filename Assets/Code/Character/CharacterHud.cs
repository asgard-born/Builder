using UI.ResourcesViews;
using UnityEngine;

namespace Character
{
    public class CharacterHud : MonoBehaviour
    {
        [SerializeField] private ExpResource _experience;
        [SerializeField] private HealthResource _health;
        
        private int maxHealth;

        public void ChangeHealthValue(int value)
        {
            _health.ChangeValue(value);
        }

        public void ChangeExpValue(int newValue)
        {
            _experience.ChangeValue(newValue);
        }
    }
}