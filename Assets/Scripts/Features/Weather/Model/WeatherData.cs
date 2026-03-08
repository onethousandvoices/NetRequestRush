namespace Features.Weather
{
    public readonly record struct WeatherData(
        int Temperature,
        string TemperatureUnit
    );
}
