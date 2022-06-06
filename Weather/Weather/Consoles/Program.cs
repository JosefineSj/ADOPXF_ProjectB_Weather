using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weather.Models;
using Weather.Services;

namespace Weather.Consoles
{
    class Program
    {

        Views.ConsolePage theConsole;
        StringBuilder theConsoleString;

        public Program(Views.ConsolePage myConsole)
        {
            //used for the Console
            #region used by the Console

            theConsole = myConsole;
            theConsoleString = new StringBuilder();


            #endregion
        }

        public async Task MyMain()
        {

            OpenWeatherService service = new OpenWeatherService();
            service.WeatherForecastAvailable += ReportWeatherDataAvailable;

            Task<Forecast> t1 = null, t2 = null, t3 = null, t4 = null;
            Exception exception = null;
            try
            {
                double latitude = 59.5086798659495;
                double longitude = 18.2654625932976;
                string city = "Österåker";

                await service.GetForecastAsync(latitude, longitude);
                await service.GetForecastAsync(city);

                t1 = service.GetForecastAsync(latitude, longitude);
                t2 = service.GetForecastAsync(city);
                Task.WaitAll(t1, t2);

                t3 = service.GetForecastAsync(latitude, longitude);
                t4 = service.GetForecastAsync(city);
                Task.WaitAll(t3, t4);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            theConsole.WriteLine("-----------------");
            if (t1?.Status == TaskStatus.RanToCompletion)
            {
                Forecast forecast = t1.Result;
                theConsoleString.Append($"Weather forecast for {forecast.City}");
                var GroupedList = forecast.Items.GroupBy(item => item.DateTime.Date);
                foreach (var group in GroupedList)
                {
                    theConsoleString.Append(group.Key.Date.ToShortDateString());
                    foreach (var item in group)
                    {
                        theConsoleString.AppendLine($"   -{item.DateTime.ToShortTimeString()}: {item.Description}, Temperature: {item.Temperature} degC, Wind: {item.WindSpeed} m/s");
                    }
                }
                theConsole.WriteLine(theConsoleString.ToString());
                await Task.Delay(2000);

            }
            else
            {
                theConsoleString.AppendLine($"Geolocation weather service error.");
                theConsoleString.AppendLine($"Error: {exception.Message}");
            }
            theConsoleString.Clear();

            theConsole.WriteLine("-----------------");
            if (t2?.Status == TaskStatus.RanToCompletion)
            {
                Forecast forecast = t2.Result;
                theConsoleString.Append($"Weather forecast for {forecast.City}");
                var GroupedList = forecast.Items.GroupBy(item => item.DateTime.Date);
                foreach (var group in GroupedList)
                {
                    theConsoleString.Append(group.Key.Date.ToShortDateString());
                    foreach (var item in group)
                    {
                        theConsoleString.AppendLine($"   -{item.DateTime.ToShortTimeString()}: {item.Description}, Temperature: {item.Temperature} degC, Wind: {item.WindSpeed} m/s");
                    }
                }
                theConsole.WriteLine(theConsoleString.ToString());
                await Task.Delay(2000);


            }
            else
            {
                theConsole.WriteLine($"City weather service error");
                theConsole.WriteLine($"Error: {exception.Message}");
            }
            theConsoleString.Clear();


            void ReportWeatherDataAvailable(object sender, string message)
            {
                theConsole.WriteLine($"Event message: {message}"); //theConsole is a Captured Variable, don't use myConsoleString here
            }

        }

    }

}

