using System;
using Core;

namespace Services
{
    public enum TabType : byte
    {
        Clicker,
        Weather,
        DogBreeds
    }

    public interface INavigationService
    {
        event Action<TabType> TabChanged;
        TabType CurrentTab { get; }
        void SwitchTo(TabType tab);
        void Register(TabType tab, IPresenter presenter);
    }
}
