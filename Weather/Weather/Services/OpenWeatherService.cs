using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json; //Requires nuget package System.Net.Http.Json
using System.Threading.Tasks;

using Weather.Models;

namespace Weather.Services
{

    public class WeatherCache
    {
        public DateTime LatestFetched { get; set; }
        public Forecast Forecast { get; set; }
        public Action OnCacheTrigger { get; }
        public Action OnFetchTrigger { get; }
        public Func<Task<Forecast>> FetchData { get; }


        public WeatherCache(
            Action onCacheTrigger,
            Action onFetchTrigger,
            Func<Task<Forecast>> fetchData)
        {
            OnCacheTrigger = onCacheTrigger;
            OnFetchTrigger = onFetchTrigger;
            FetchData = fetchData;

        }

        public async Task<Forecast> GetForecast()
        {
            if (DateTime.Now > LatestFetched.AddMinutes(1))
            {
                Forecast = await FetchData.Invoke();
                LatestFetched = DateTime.Now;
                OnFetchTrigger();
            }
            else
            {
                OnCacheTrigger();
            }

            return Forecast;
        }
    }


    public class OpenWeatherService
    {

        HttpClient httpClient = new HttpClient();
        readonly string apiKey = "2badc0d0b2c833da44b9966864769599"; // Your API Key

        //En delegate av typen string 
        public event EventHandler<string> WeatherForecastAvailable;

        ConcurrentDictionary<string, WeatherCache> getWeatherByCity = new ConcurrentDictionary<string, WeatherCache>();

        ConcurrentDictionary<(double, double), WeatherCache> getWeatherByLatLong = new ConcurrentDictionary<(double, double), WeatherCache>();


        public async Task<Forecast> GetForecastAsync(string city)
        {

            if (!getWeatherByCity.TryGetValue(city, out var weatherCache))
            {
                var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&units=metric&lang={language}&appid={apiKey}";

                var cacheMessage = $"Cached weather forecast for {city}";
                var fetchMessage = $"New weather forecast for {city}";

                weatherCache = new WeatherCache(
                    onCacheTrigger: () => WeatherForecastAvailable?.Invoke(this, cacheMessage),
                    onFetchTrigger: () => WeatherForecastAvailable?.Invoke(this, fetchMessage),
                    fetchData: () => ReadWebApiAsync(uri)
                );

                getWeatherByCity.TryAdd(city, weatherCache);

            }
            return await weatherCache.GetForecast();


        }
        public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
        {

            if (!getWeatherByLatLong.TryGetValue((latitude, longitude), out var weatherCache))
            {
                var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={apiKey}";

                var cacheMessage = $"Cached weather forecast for ({latitude}, {longitude})";
                var fetchMessage = $"New weather forecast for ({latitude}, {longitude})";

                weatherCache = new WeatherCache(
                    onCacheTrigger: () => WeatherForecastAvailable?.Invoke(this, cacheMessage),
                    onFetchTrigger: () => WeatherForecastAvailable?.Invoke(this, fetchMessage),
                    fetchData: () => ReadWebApiAsync(uri)
                );

                getWeatherByLatLong.TryAdd((latitude, longitude), weatherCache);

            }
            return await weatherCache.GetForecast();

        }
        private async Task<Forecast> ReadWebApiAsync(string uri)
        {

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            WeatherApiData wd = await response.Content.ReadFromJsonAsync<WeatherApiData>();

            Forecast forecast = new Forecast();

            try
            {
                forecast.City = wd.city.name;
                forecast.Items = wd.list.Select(item => new ForecastItem
                {
                    DateTime = UnixTimeStampToDateTime(item.dt),
                    Temperature = item.main.temp,
                    WindSpeed = item.wind.speed,
                    Description = item.weather.Select(desc => desc.description).FirstOrDefault().ToString(),
                    Icon = item.weather.Select(ic => $"{ic.icon}.jpg").FirstOrDefault()
                }).ToList();
            }
            catch (Exception)
            {

                throw;
            }

            return forecast;

        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}