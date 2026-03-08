using System;
using Services;
using UnityEngine;

namespace UI
{
    public sealed class TabBarView : MonoBehaviour
    {
        [SerializeField] private TabButtonView[] _tabs;

        public event Action<TabType> TabSelected;

        private void Awake()
        {
            for (var i = 0; i < _tabs.Length; i++)
                _tabs[i].Clicked += OnTabClicked;
        }

        private void OnDestroy()
        {
            for (var i = 0; i < _tabs.Length; i++)
                _tabs[i].Clicked -= OnTabClicked;
        }

        private void OnTabClicked(TabType tab) => TabSelected?.Invoke(tab);

        public void SetActiveTab(TabType tab)
        {
            for (var i = 0; i < _tabs.Length; i++)
                _tabs[i].SetActive(_tabs[i].TabType == tab);
        }
    }
}
