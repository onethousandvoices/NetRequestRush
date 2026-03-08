using System;
using Core;
using Services;
using UnityEngine;
using Zenject;

namespace Features.Clicker
{
    public sealed class ClickerPresenter : IPresenter, ITickable, IInitializable, IDisposable
    {
        [Inject] private ClickerView _view;
        [Inject] private ClickerModel _model;
        [Inject] private ClickerConfig _config;
        [Inject] private IAudioService _audio;
        [Inject] private INavigationService _navigation;
        [Inject] private CurrencyFlyEffect.Pool _flyPool;

        private float _autoCollectTimer;
        private float _energyRegenTimer;
        private bool _isActive;
        private bool _disposed;

        public void Initialize()
        {
            _navigation.Register(TabType.Clicker, this);
            _model.CurrencyChanged += OnCurrencyChanged;
            _model.EnergyChanged += OnEnergyChanged;
            _view.Clicked += OnClicked;
            _view.UpdateCurrency(_model.Currency);
            _view.UpdateEnergy(_model.Energy, _model.MaxEnergy);
        }

        public void Dispose()
        {
            _disposed = true;
            _isActive = false;
            if (_view)
                _view.Clicked -= OnClicked;

            _model.CurrencyChanged -= OnCurrencyChanged;
            _model.EnergyChanged -= OnEnergyChanged;
        }

        public void Activate()
        {
            _isActive = true;
            _view.Show();
        }

        public void Deactivate()
        {
            if (!_isActive || _disposed)
                return;

            _isActive = false;
            _view.Hide();
        }

        public void Tick()
        {
            _autoCollectTimer += Time.deltaTime;
            while (_autoCollectTimer >= _config.AutoCollectInterval)
            {
                _autoCollectTimer -= _config.AutoCollectInterval;
                TryCollect(SoundType.AutoCollect, _isActive);
            }

            _energyRegenTimer += Time.deltaTime;
            while (_energyRegenTimer >= _config.EnergyRegenInterval)
            {
                _energyRegenTimer -= _config.EnergyRegenInterval;
                _model.AddEnergy(_config.EnergyRegenAmount);
            }
        }

        private void OnClicked() => TryCollect(SoundType.Click, true);

        private void TryCollect(SoundType sound, bool playFeedback)
        {
            if (!_model.TrySpendEnergy(_config.EnergyPerClick))
                return;

            _model.AddCurrency(_config.CurrencyPerClick);
            if (!playFeedback)
                return;

            _view.PlayButtonPunch(_config.ButtonPunchScale, _config.ButtonPunchDuration);
            _view.PlayClickParticle();
            _audio.Play(sound);

            var fly = _flyPool.Spawn();
            fly.Init(_view.CurrencyFlyOrigin.position, _view.CurrencyFlyTarget.position, _config.CurrencyFlyDuration, _flyPool);
        }

        private void OnCurrencyChanged(int value) => _view.UpdateCurrency(value);
        private void OnEnergyChanged(int value) => _view.UpdateEnergy(value, _model.MaxEnergy);
    }
}
