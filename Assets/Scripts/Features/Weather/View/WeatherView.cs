using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Weather
{
    public sealed class WeatherView : MonoBehaviour
    {
        [SerializeField] private RawImage _weatherIcon;
        [SerializeField] private TMP_Text _forecastText;
        [SerializeField] private LoadingIndicator _loadingIndicator;

        private Texture2D _iconTexture;

        public void Show() => gameObject.SetActiveSafe(true);

        public void Hide()
        {
            HideLoading();
            gameObject.SetActiveSafe(false);
        }

        public void ShowLoading() => _loadingIndicator.Show();
        public void HideLoading() => _loadingIndicator.Hide();
        public void UpdateWeather(WeatherData data) => _forecastText.text = $"Today: {data.Temperature}{data.TemperatureUnit}";

        public bool SetIcon(byte[] data)
        {
            _iconTexture ??= new(2, 2, TextureFormat.RGBA32, false);
            if (!_iconTexture.LoadImage(data))
                return false;

            _weatherIcon.texture = _iconTexture;
            return true;
        }

        public void ClearIcon() => _weatherIcon.texture = null;

        private void OnDestroy()
        {
            ClearIcon();
            if (_iconTexture)
                Destroy(_iconTexture);
        }
    }
}
