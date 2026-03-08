using UnityEngine;

namespace Features.Weather
{
    [CreateAssetMenu(fileName = "WeatherConfig", menuName = "Configs/WeatherConfig")]
    public sealed class WeatherConfig : ScriptableObject
    {
        [SerializeField] private string _apiUrl = "https://api.weather.gov/gridpoints/TOP/32,81/forecast";
        [SerializeField] private float _pollIntervalSec = 5f;

        public string ForecastRequestTag => "weather";
        public string IconRequestTag => "weather_icon";
        public string ForecastUrl => _apiUrl;
        public float PollIntervalSec => _pollIntervalSec;
    }
}
