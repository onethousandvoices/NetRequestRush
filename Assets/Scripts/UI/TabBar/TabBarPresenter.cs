using System;
using Services;
using Zenject;

namespace UI
{
    public sealed class TabBarPresenter : IInitializable, IDisposable
    {
        [Inject] private TabBarView _view;
        [Inject] private INavigationService _navigation;
        [Inject] private IAudioService _audio;

        public void Initialize()
        {
            _view.TabSelected += OnTabSelected;
            _navigation.TabChanged += OnTabChanged;
        }

        public void Dispose()
        {
            if (_view)
                _view.TabSelected -= OnTabSelected;

            _navigation.TabChanged -= OnTabChanged;
        }

        private void OnTabSelected(TabType tab)
        {
            _navigation.SwitchTo(tab);
            _audio.Play(SoundType.TabSwitch);
        }
        private void OnTabChanged(TabType tab) => _view.SetActiveTab(tab);
    }
}
