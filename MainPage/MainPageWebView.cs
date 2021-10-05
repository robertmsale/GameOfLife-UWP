using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GameOfLife_UWP
{
    public sealed partial class MainPage
    {
        #region WebView Events
        private async void WebView_NavigationStarting(object sender, WebViewNavigationStartingEventArgs args)
        {
            string host = args.Uri.Host;
            if (host == "playgameoflife.com")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("https://playgameoflife.com/lexicon/data/");
                sb.Append(args.Uri.ToString().Split('/').Last());
                sb.Append(".json");
                try
                {
                    string responseBody = await client.GetStringAsync(new Uri(sb.ToString()));
                    JsonObject jo = JsonObject.Parse(responseBody);
                    string pattern = jo["pattern"].GetString();
                    string[] rows = pattern.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    int xlen = rows[0].Length;
                    int ylen = rows.Length;
                    for (int y = 0; y < ylen; y++)
                    {
                        for (int x = 0; x < xlen; x++)
                        {
                            if (rows[y][x] == 'O') vm.universe[x + (int)NumberBoxXPos.Value, y + (int)NumberBoxYPos.Value] = true;
                        }
                    }
                    canvas.Invalidate();
                    WebGrid.Visibility = Visibility.Collapsed;
                }
                catch (HttpRequestException e)
                {

                }
            }
            else if (host == "bitstorm.org")
            {
                return;
            }
            args.Cancel = true;
        }
        private void CloseWebview(object sender, RoutedEventArgs e)
        {
            WebGrid.Visibility = Visibility.Collapsed;
        }
        private void OpenWebView(object sender, RoutedEventArgs e)
        {
            WebView.Navigate(new Uri("https://bitstorm.org/gameoflife/lexicon/"));
            WebGrid.Visibility = Visibility.Visible;
        }
        #endregion
    }
}