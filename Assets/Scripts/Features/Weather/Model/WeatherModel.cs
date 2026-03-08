namespace Features.Weather
{
    public sealed class WeatherModel
    {
        public WeatherData? Current { get; private set; }

        public void Update(WeatherData data) => Current = data;
    }
}
