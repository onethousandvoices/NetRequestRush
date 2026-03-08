using System;

namespace Features.Weather
{
    [Serializable]
    public class WeatherApiResponse
    {
        public WeatherProperties properties;
    }

    [Serializable]
    public class WeatherProperties
    {
        public WeatherPeriod[] periods;
    }

    [Serializable]
    public class WeatherPeriod
    {
        public int temperature;
        public string temperatureUnit;
        public string icon;
    }
}
