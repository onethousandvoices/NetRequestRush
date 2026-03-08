using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Clicker
{
    public sealed class ClickerView : MonoBehaviour
    {
        public event Action Clicked;

        [SerializeField] private Button _clickButton;
        [SerializeField] private TMP_Text _currencyText;
        [SerializeField] private TMP_Text _energyText;
        [SerializeField] private RectTransform _buttonTransform;
        [SerializeField] private ParticleSystem _clickParticle;
        [SerializeField] private RectTransform _currencyFlyOrigin;
        [SerializeField] private RectTransform _currencyFlyTarget;

        public RectTransform CurrencyFlyOrigin => _currencyFlyOrigin;
        public RectTransform CurrencyFlyTarget => _currencyFlyTarget;

        private void Awake() => _clickButton.onClick.AddListener(OnClick);
        private void OnDestroy() => _clickButton.onClick.RemoveListener(OnClick);
        private void OnClick() => Clicked?.Invoke();
        public void Show() => gameObject.SetActiveSafe(true);
        public void Hide() => gameObject.SetActiveSafe(false);
        public void UpdateCurrency(int value) => _currencyText.SetText("{0}", value);
        public void UpdateEnergy(int value, int max) => _energyText.SetText("{0}/{1}", value, max);

        public void PlayButtonPunch(float scale, float duration)
        {
            _buttonTransform.DOKill();
            _buttonTransform.localScale = Vector3.one;
            _buttonTransform.DOPunchScale(Vector3.one * scale, duration, 1, 0.5f);
        }

        public void PlayClickParticle() => _clickParticle.Play();
    }
}
