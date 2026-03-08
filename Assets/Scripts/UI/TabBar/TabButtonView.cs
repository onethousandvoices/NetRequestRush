using System;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class TabButtonView : MonoBehaviour
    {
        public event Action<TabType> Clicked;

        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TabType _tabType;
        [SerializeField] private Color _activeColor = Color.white;
        [SerializeField] private Color _inactiveColor = Color.gray;

        public TabType TabType => _tabType;

        private void Awake() => _button.onClick.AddListener(OnClick);
        private void OnDestroy() => _button.onClick.RemoveListener(OnClick);
        private void OnClick() => Clicked?.Invoke(_tabType);

        public void SetActive(bool isActive)
        {
            var color = isActive ? _activeColor : _inactiveColor;
            _icon.color = color;
        }
    }
}
