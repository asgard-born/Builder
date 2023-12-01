using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ResourcesViews
{
    public class ExpResource : ResourceBaseView
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _levelText;

        private int _maxValue;
        private int _currentLevel;

        public void OnLevelUp(int exp, int newMaxExp)
        {
            _currentLevel++;
            _currentValue = exp;
            _maxValue = newMaxExp;

            UpdateView();
        }

        protected override void UpdateView()
        {
            _slider.value = _currentValue / _maxValue;
            _levelText.text = _currentLevel.ToString();
            _valueText.text = $"Exp ({_currentValue}/{_maxValue})";
        }
    }
}