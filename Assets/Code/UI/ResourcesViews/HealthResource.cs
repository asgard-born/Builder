using UnityEngine;
using UnityEngine.UI;

namespace UI.ResourcesViews
{
    public class HealthResource : ResourceBaseView
    {
        [SerializeField] private Slider _slider;

        private int _maxValue;

        protected override void UpdateView()
        {
            _slider.value = _currentValue / _maxValue;
            _valueText.text = $"HP ({_currentValue}/{_maxValue})";
        }
    }
}