using System;
using System.Collections.Generic;
using Core;
using Zenject;

namespace Services
{
    public sealed class NavigationService : INavigationService, IInitializable
    {
        public event Action<TabType> TabChanged;

        private readonly Dictionary<TabType, IPresenter> _presenters = new();

        public TabType CurrentTab { get; private set; } = (TabType)byte.MaxValue;

        public void Register(TabType tab, IPresenter presenter) => _presenters[tab] = presenter;

        public void Initialize() => SwitchTo(TabType.Clicker);

        public void SwitchTo(TabType tab)
        {
            if (CurrentTab == tab)
                return;

            if (_presenters.TryGetValue(CurrentTab, out var previous))
                previous.Deactivate();

            CurrentTab = tab;

            if (_presenters.TryGetValue(tab, out var next))
                next.Activate();

            TabChanged?.Invoke(tab);
        }
    }
}
