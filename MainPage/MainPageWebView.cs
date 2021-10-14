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
        /// <summary>
        /// Web View Navigation event
        /// </summary>
        private async void WebView_NavigationStarting(object sender, WebViewNavigationStartingEventArgs args)
        {
            string host = args.Uri.Host;
            // If the navigation takes you to the WASM online game, capture that navigation
            if (host == "playgameoflife.com")
            {
                // Begin building HTTP request using captured URI
                StringBuilder sb = new StringBuilder();
                sb.Append("https://playgameoflife.com/lexicon/data/");
                sb.Append(args.Uri.ToString().Split('/').Last());
                sb.Append(".json");
                try
                {
                    // Send HTTP Request and get the payload as a plain ol' string
                    string responseBody = await client.GetStringAsync(new Uri(sb.ToString()));
                    // Use Windows JSON Library to parse the payload
                    JsonObject jo = JsonObject.Parse(responseBody);
                    // Get pattern 
                    string pattern = jo["pattern"].GetString();
                    // Split rows
                    string[] rows = pattern.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    // Get X and Y box size from pattern
                    int xlen = rows[0].Length;
                    int ylen = rows.Length;
                    // Begin import, placing cells at exact position indicated by number boxes
                    for (int y = 0; y < ylen; y++)
                    {
                        for (int x = 0; x < xlen; x++)
                        {
                            if (rows[y][x] == 'O') vm.universe[x + (int)NumberBoxXPos.Value, y + (int)NumberBoxYPos.Value] = true;
                        }
                    }
                    canvas.Invalidate();
                    // Close Webview
                    WebGrid.Visibility = Visibility.Collapsed;
                }
                catch (HttpRequestException e) // If failure occurs, pretend nothing happened
                {
                    // This is a feature not a bug!
                }
            }
            // If we're still in the lexicon, let navigation happen as usual
            else if (host == "bitstorm.org")
            {
                return;
            }
            // If navigating outside of the lexicon, prevent navigation
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