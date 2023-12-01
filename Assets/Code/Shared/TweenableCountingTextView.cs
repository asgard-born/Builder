using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Shared
{
    [Serializable]
    public class TweenableCountingTextView
    {
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private float _highScoreAnimationLength = 1.5f;

        private int _currentCount;
        private int _newCount;
        private float _t;

        public IEnumerator UpdateText(int newCount)
        {
            _newCount = newCount;

            var displayedScore = (int)Mathf.Lerp(_currentCount, _newCount, _t);
            _t = Mathf.MoveTowards(_t, 1.0f, Time.deltaTime / _highScoreAnimationLength);

            while (displayedScore < _newCount)
            {
                _t = Mathf.MoveTowards(_t, 1, Time.deltaTime / _highScoreAnimationLength);
                displayedScore = (int)Mathf.Lerp(_currentCount, _newCount, _t);
                _countText.text = displayedScore.ToString();

                if (displayedScore >= _newCount)
                {
                    _currentCount = _newCount;
                }

                yield return null;
            }
        }
    }
}