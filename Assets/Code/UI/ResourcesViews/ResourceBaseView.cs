using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ResourcesViews
{
    public class ResourceBaseView : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI _valueText;
        [SerializeField] protected bool _withButton;
        [SerializeField, ShowIf("_withButton")] protected Button button;

        protected float _currentValue;

        public void InitializeButton(Action onClick)
        {
            if (!_withButton) return;

            button.onClick.AddListener(() => onClick?.Invoke());
        }

        public void ChangeValue(float newValue)
        {
            _currentValue = newValue;
            UpdateView();
        }

        protected virtual void UpdateView()
        {
            float newValue;
            string newValueText;

            if (_currentValue > 999999999999999999)
            {
                newValue = _currentValue / 1000000000000000;
                newValue = (float)Math.Round(newValue, 1);

                newValueText = $"{newValue}E";
            }
            else if (_currentValue > 999999999999999)
            {
                newValue = _currentValue / 1000000000000;
                newValue = (float)Math.Round(newValue, 1);

                newValueText = $"{newValue}D";
            }

            else if (_currentValue > 999999999999)
            {
                newValue = _currentValue / 1000000000;
                newValue = (float)Math.Round(newValue, 1);

                newValueText = $"{newValue}C";
            }
            else if (_currentValue > 999999999999)
            {
                newValue = _currentValue / 1000000000;
                newValue = (float)Math.Round(newValue, 1);

                newValueText = $"{newValue}B";
            }
            else if (_currentValue > 999999999)
            {
                newValue = _currentValue / 1000000;
                newValue = (float)Math.Round(newValue, 1);

                newValueText = $"{newValue}A";
            }
            else if (_currentValue > 999999)
            {
                newValue = _currentValue / 1000000;
                newValue = (float)Math.Round(newValue, 1);

                newValueText = $"{newValue}M";
            }
            else if (_currentValue > 999)
            {
                newValue = _currentValue / 1000;
                newValue = (float)Math.Round(newValue, 2);

                newValueText = $"{newValue}K";
            }
            else
            {
                newValueText = _currentValue.ToString();
            }

            _valueText.text = newValueText;
        }
    }
}