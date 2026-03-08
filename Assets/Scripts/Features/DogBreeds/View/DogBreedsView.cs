using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Zenject;

namespace Features.DogBreeds
{
    public sealed class DogBreedsView : MonoBehaviour
    {
        public event Action<int> BreedClicked;
        public event Action PopupClosed;

        [SerializeField] private Transform _listContainer;
        [SerializeField] private LoadingIndicator _loadingIndicator;
        [SerializeField] private BreedPopupView _popup;

        private readonly List<BreedListItemView> _activeItems = new();
        private BreedListItemView.Pool _pool;

        public Transform ListContainer => _listContainer;

        [Inject] public void Construct(BreedListItemView.Pool pool) => _pool = pool;

        private void Awake() => _popup.CloseClicked += OnPopupClose;
        public void Show() => gameObject.SetActiveSafe(true);

        public void Hide()
        {
            HideLoading();
            ResetItemLoading();
            gameObject.SetActiveSafe(false);
        }

        public void ShowLoading() => _loadingIndicator.Show();
        public void HideLoading() => _loadingIndicator.Hide();

        public void ResetItemLoading()
        {
            for (var i = 0; i < _activeItems.Count; i++)
                _activeItems[i].HideLoading();
        }

        public void PopulateList(IReadOnlyList<BreedData> breeds)
        {
            ClearList();

            for (var i = 0; i < breeds.Count; i++)
            {
                var item = _pool.Spawn();
                item.Setup(i, breeds[i].Name);
                item.transform.SetSiblingIndex(i);
                item.Clicked += OnItemClicked;
                _activeItems.Add(item);
            }
        }

        public void ShowItemLoading(int index)
        {
            if (index >= 0 && index < _activeItems.Count)
                _activeItems[index].ShowLoading();
        }

        public void HideItemLoading(int index)
        {
            if (index >= 0 && index < _activeItems.Count)
                _activeItems[index].HideLoading();
        }

        public void ShowPopup(string breedName, string description) => _popup.Show(breedName, description);
        public void HidePopup() => _popup.Hide();
        private void OnItemClicked(int index) => BreedClicked?.Invoke(index);
        private void OnPopupClose() => PopupClosed?.Invoke();

        private void ClearList()
        {
            for (var i = 0; i < _activeItems.Count; i++)
            {
                _activeItems[i].Clicked -= OnItemClicked;
                _activeItems[i].HideLoading();
                _pool.Despawn(_activeItems[i]);
            }

            _activeItems.Clear();
        }

        private void OnDestroy()
        {
            _popup.CloseClicked -= OnPopupClose;
            ClearList();
        }
    }
}
