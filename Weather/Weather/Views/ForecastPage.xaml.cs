using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Weather.Models;
using Weather.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class ForecastPage : ContentPage
    {
        OpenWeatherService service;
        GroupedForecast groupedforecast;

        public ForecastPage()
        {
            InitializeComponent();

            service = new OpenWeatherService();
            groupedforecast = new GroupedForecast();

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            MainThread.BeginInvokeOnMainThread(async () => { await LoadForecast(); });
        }

        private async Task LoadForecast()
        {
            Forecast t1 = await service.GetForecastAsync(Title);

            var items = t1.Items;
            var groupedItems = items.OrderBy(d => d.DateTime.Date).GroupBy(d => d.DateTime.Date.ToString("dddd, MMMM d, yyyy"));

            GroupedDataList.ItemsSource = groupedItems;

            
        }

        private async void Refresh_Button(object sender, EventArgs e)
        {
            if (refreshButton.IsVisible == true)
            {
                await progressBar.ProgressTo(1, 2000, Easing.Linear);
                await LoadForecast();
                progressBar.IsVisible = true;
            }
            await progressBar.ProgressTo(0, 2000, Easing.Linear);

        }

    }
}