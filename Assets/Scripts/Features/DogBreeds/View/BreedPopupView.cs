using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.DogBreeds
{
    public sealed class BreedPopupView : MonoBehaviour
    {
        public event Action CloseClicked;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private float _animDuration = 0.25f;

        private Sequence _currentSequence;
        private ScrollRect _scrollRect;

        private void Awake()
        {
            _scrollRect = GetComponentInChildren<ScrollRect>(true);
            _titleText.enableAutoSizing = false;
            _descriptionText.enableAutoSizing = false;
            _closeButton.onClick.AddListener(OnClose);
        }

        private void OnClose() => CloseClicked?.Invoke();

        public void Show(string breedName, string description)
        {
            KillSequence();
            _titleText.SetText(breedName);
            _descriptionText.SetText(description);
            gameObject.SetActiveSafe(true);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _panel.localScale = Vector3.one * 0.8f;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);
            if (_scrollRect)
                _scrollRect.verticalNormalizedPosition = 1f;

            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(_canvasGroup.DOFade(1f, _animDuration));
            _currentSequence.Join(_panel.DOScale(Vector3.one, _animDuration).SetEase(Ease.OutBack));
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            KillSequence();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(_canvasGroup.DOFade(0f, _animDuration));
            _currentSequence.Join(_panel.DOScale(Vector3.one * 0.8f, _animDuration).SetEase(Ease.InBack));
            _currentSequence.OnComplete(() => gameObject.SetActiveSafe(false));
        }

        private void KillSequence()
        {
            if (_currentSequence != null && _currentSequence.IsActive())
                _currentSequence.Kill();

            _currentSequence = null;
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(OnClose);
            KillSequence();
        }
    }
}
