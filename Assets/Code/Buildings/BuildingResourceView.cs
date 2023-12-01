using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings
{
    public class BuildingResourceView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _text;

        public void Initialize(Sprite sprite, int count)
        {
            _image.sprite = sprite;
            UpdateText(count);
        }

        public void UpdateText(int newCount)
        {
            _text.text = newCount.ToString();
        }
    }
}