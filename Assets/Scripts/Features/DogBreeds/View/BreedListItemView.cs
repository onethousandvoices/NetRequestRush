using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Features.DogBreeds
{
    public sealed class BreedListItemView : MonoBehaviour
    {
        public event Action<int> Clicked;

        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Button _button;
        [SerializeField] private LoadingIndicator _loadingIndicator;

        private int _index;

        private void Awake() => _button.onClick.AddListener(OnClick);
        private void OnDestroy() => _button.onClick.RemoveListener(OnClick);
        private void OnClick() => Clicked?.Invoke(_index);

        public void Setup(int index, string breedName)
        {
            _index = index;
            _nameText.text = $"{index + 1} - {breedName}";
            HideLoading();
            gameObject.SetActiveSafe(true);
        }

        public void ShowLoading() => _loadingIndicator.Show();
        public void HideLoading() => _loadingIndicator.Hide();

        private void ResetState()
        {
            HideLoading();
            gameObject.SetActiveSafe(false);
        }

        public sealed class Pool : MemoryPool<BreedListItemView>
        {
            protected override void OnCreated(BreedListItemView item) => item.gameObject.SetActiveSafe(false);
            protected override void OnDespawned(BreedListItemView item) => item.ResetState();
        }
    }
}
