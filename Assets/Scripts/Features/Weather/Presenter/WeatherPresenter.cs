using System;
using System.Threading;
using Core;
using Services;
using UnityEngine;
using Zenject;

namespace Features.Weather
{
    public sealed class WeatherPresenter : IPresenter, IInitializable, IDisposable
    {
        [Inject] private WeatherView _view;
        [Inject] private WeatherModel _model;
        [Inject] private WeatherConfig _config;
        [Inject] private IRequestQueueService _requestQueue;
        [Inject] private INavigationService _navigation;

        private QueuedRequestFlow _forecastFlow;
        private QueuedRequestFlow _iconFlow;
        private string _appliedIconUrl;
        private bool _disposed;

        public void Initialize()
        {
            _forecastFlow = new(CancelForecastRequest, _view.ShowLoading, _view.HideLoading);
            _iconFlow = new(CancelIconRequest);
            _navigation.Register(TabType.Weather, this);
        }

        public void Dispose()
        {
            _disposed = true;
            StopPolling(false);
        }

        public void Activate()
        {
            _view.Show();
            if (_model.Current.HasValue)
                _view.UpdateWeather(_model.Current.Value);

            StartPolling();
        }

        public void Deactivate()
        {
            if (_disposed)
                return;

            StopPolling(true);
            _view.Hide();
        }

        private void StartPolling()
        {
            _forecastFlow.StartNewSession(out var token, out var version);
            _iconFlow.Cancel(false);
            EnqueueForecastRequest(token, version).Forget();
            PollWeatherLoop(token, version).Forget();
        }

        private void StopPolling(bool updateView)
        {
            _appliedIconUrl = null;
            _forecastFlow.Cancel(updateView);
            _iconFlow.Cancel(false);
        }

        private async Awaitable PollWeatherLoop(CancellationToken ct, int version)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Awaitable.WaitForSecondsAsync(_config.PollIntervalSec, ct);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (!_forecastFlow.IsCurrent(ct, version))
                    return;

                EnqueueForecastRequest(ct, version).Forget();
            }
        }

        private async Awaitable EnqueueForecastRequest(CancellationToken ct, int version)
        {
            _forecastFlow.BeginTrackedRequest();

            try
            {
                var result = await _requestQueue.EnqueueText(_config.ForecastUrl, _config.ForecastRequestTag, ct);
                if (!_forecastFlow.IsCurrent(ct, version))
                    return;
                if (!result.IsSuccess || string.IsNullOrEmpty(result.Data))
                    return;

                ProcessWeatherResponse(result.Data);
            }
            finally
            {
                _forecastFlow.EndTrackedRequest(version);
            }
        }

        private void ProcessWeatherResponse(string json)
        {
            try
            {
                var response = JsonUtility.FromJson<WeatherApiResponse>(json);
                if (response?.properties?.periods is not { Length: > 0 })
                    return;

                var period = response.properties.periods[0];
                var data = new WeatherData(period.temperature, period.temperatureUnit);
                _model.Update(data);
                _view.UpdateWeather(data);
                if (string.IsNullOrEmpty(period.icon))
                {
                    _appliedIconUrl = null;
                    _view.ClearIcon();
                    return;
                }
                if (period.icon == _appliedIconUrl)
                    return;

                _iconFlow.StartNewSession(out var token, out var version);
                LoadIcon(period.icon, token, version).Forget();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Awaitable LoadIcon(string url, CancellationToken ct, int version)
        {
            var result = await _requestQueue.EnqueueBytes(url, _config.IconRequestTag, ct);

            if (!_iconFlow.IsCurrent(ct, version))
                return;
            if (!result.IsSuccess || result.Data == null)
            {
                _appliedIconUrl = null;
                _view.ClearIcon();
                return;
            }
            if (!_view.SetIcon(result.Data))
            {
                _appliedIconUrl = null;
                _view.ClearIcon();
                return;
            }

            _appliedIconUrl = url;
        }

        private void CancelForecastRequest() => _requestQueue.Cancel(_config.ForecastRequestTag);
        private void CancelIconRequest() => _requestQueue.Cancel(_config.IconRequestTag);
    }
}
