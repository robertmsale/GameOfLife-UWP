using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.Data.Json;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.ComponentModel;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GameOfLife_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public ViewModel ViewModel;
        HttpClient client = new HttpClient();
        public double GridWidth { get; set; }
        public double GridHeight{ get; set; }
        public bool timerRunning = false;
        public string uName = "New Universe";
        public string uDescription = "New Universe";
        public int Speed = 100000;
        public string FilePath = "";
        public SolidColorBrush modalTop 
        { 
            get
            {
                Color c = new Color { R=126, G=126, B=126, A=50 };
                return new SolidColorBrush(c);
            } 
        }
        public string PlayPause
        {
            get => timerRunning ? "Pause" : "Play";
        }
        DispatcherTimer timer = new DispatcherTimer();
        public MainPage()
        {
            this.ViewModel = new ViewModel();
            this.ViewModel.universe.Randomize();
            GridWidth = this.ViewModel.CellSize * this.ViewModel.universe.XLen;
            GridHeight = this.ViewModel.CellSize * this.ViewModel.universe.YLen;
            this.InitializeComponent();
            this.canvas.Width = GridWidth;
            this.canvas.Height = GridHeight;
            timer.Interval = new TimeSpan(100000); // milliseconds
            timer.Tick += Timer_Tick;
            this.CurrentSpeedItem.Text = "Current Speed: 100ms";
            this.WebView.NavigationStarting += WebView_NavigationStarting;
            this.NameChangeModal.PointerReleased += CloseModal;
        }
        private void Timer_Tick(object sender, object e)
        {
            this.ViewModel.universe.CalculateNextGeneration();
            this.canvas.Invalidate();
        }
        private ICanvasBrush LiveCell(CanvasControl c)
        {
            Color col = Colors.Chartreuse;
            return new CanvasSolidColorBrush(c, col);
        }
        private ICanvasBrush DeadCell(CanvasControl c)
        {
            Color col = Colors.Black;
            return new CanvasSolidColorBrush(c, col);
        }
        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            Color c = new Color();
            c.A = 0;
            args.DrawingSession.Clear(c);
            for (int x = 0; x < this.ViewModel.universe.XLen; x++)
            {
                for (int y = 0; y < this.ViewModel.universe.YLen; y++)
                {
                    Color gridcolor = Colors.Azure;
                    ICanvasBrush cellColor;
                    if (this.ViewModel.universe[x, y]) cellColor = LiveCell(sender);
                    else cellColor = DeadCell(sender);
                    args.DrawingSession.DrawRectangle(new Rect(new Point(x * this.ViewModel.CellSize, y * this.ViewModel.CellSize), new Size(this.ViewModel.CellSize, this.ViewModel.CellSize)), gridcolor, 1.0f);
                    args.DrawingSession.FillRectangle(new Rect(new Point(x * this.ViewModel.CellSize, y * this.ViewModel.CellSize), new Size(this.ViewModel.CellSize, this.ViewModel.CellSize)), cellColor);
                }
            }
            this.StatusBar.Text = this.ViewModel.StatusBar;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            this.canvas.RemoveFromVisualTree();
            this.canvas = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.universe.Randomize();
            this.canvas.Invalidate();
        }

        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Point p = e.GetCurrentPoint(this.canvas).Position;
            this.ViewModel.universe.ClickCell(
                (int)(p.X / this.ViewModel.CellSize),
                (int)(p.Y / this.ViewModel.CellSize)
                );
            this.canvas.Invalidate();
        }

        private void NewUniverse(object sender, RoutedEventArgs e)
        {
            this.ViewModel.universe = new Universe((int)this.NumberBoxWidth.Value, (int)this.NumberBoxHeight.Value, this.IsToroidalSwitch.IsOn);
            ResizeGrid();
            this.canvas.Invalidate();
        }

        private void CloseWebview(object sender, RoutedEventArgs e)
        {
            this.WebGrid.Visibility = Visibility.Collapsed;
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (timerRunning)
            {
                timer.Stop();
                this.PPBtn.Icon = new SymbolIcon(Symbol.Play);
                this.PPBtn.Label = "Play";
            }
            else
            {
                this.PPBtn.Icon = new SymbolIcon(Symbol.Pause);
                this.PPBtn.Label = "Pause";
                timer.Start();
            }
            timerRunning = !timerRunning;
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.universe.GoTo(this.ViewModel.universe.Current - 1);
            this.canvas.Invalidate();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.universe.Current == this.ViewModel.universe.TotalGenerations - 1)
                this.ViewModel.universe.CalculateNextGeneration();
            this.ViewModel.universe.GoTo(this.ViewModel.universe.Current + 1);
            this.canvas.Invalidate();
        }
        private void ResizeGrid()
        {
            GridWidth = this.ViewModel.CellSize * this.ViewModel.universe.XLen;
            GridHeight = this.ViewModel.CellSize * this.ViewModel.universe.YLen;
            this.canvas.Width = GridWidth;
            this.canvas.Height = GridHeight;
            this.canvas.Invalidate();
        }
        private void NewToroidal(object sender, RoutedEventArgs e)
        {
            this.ViewModel.universe = new Universe((int)this.NumberBoxWidth.Value, (int)this.NumberBoxHeight.Value, true);
            ResizeGrid();
            this.canvas.Invalidate();
        }

        private void NewFinite(object sender, RoutedEventArgs e)
        {
            this.ViewModel.universe = new Universe((int)this.NumberBoxWidth.Value, (int)this.NumberBoxHeight.Value, true);
            ResizeGrid();
            this.canvas.Invalidate();
        }

        private void ZoomIn(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Zoom += 0.1;
            ResizeGrid();
            this.canvas.Invalidate();
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Zoom -= 0.1;
            ResizeGrid();
            this.canvas.Invalidate();
        }

        private void Randomize(object sender, RoutedEventArgs e)
        {
            this.ViewModel.universe.Randomize();
            this.canvas.Invalidate();
        }

        private void ClearHistory(object sender, RoutedEventArgs e)
        {
            this.ViewModel.universe.ClearDiffMap();
            this.canvas.Invalidate();
        }

        private void CloseModal(object sender, RoutedEventArgs e)
        {
            this.NameChangeModal.Visibility = Visibility.Collapsed;
            this.UniverseNameBox.Text = uName;
            this.UniverseDBox.Text = uDescription;
        }
        private void SaveModal(object sender, RoutedEventArgs e)
        {
            this.NameChangeModal.Visibility = Visibility.Collapsed;
            uName = this.UniverseNameBox.Text;
            uDescription = this.UniverseDBox.Text;
        }
        private void OpenModal(object sender, RoutedEventArgs e)
        {
            CloseModal(sender, e);
            this.NameChangeModal.Visibility = Visibility.Visible;
        }

        private void OpenWebView(object sender, RoutedEventArgs e)
        {
            this.WebView.Navigate(new Uri("https://bitstorm.org/gameoflife/lexicon/"));
            this.WebGrid.Visibility = Visibility.Visible;
        }
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
                    string responseBody = await client.GetStringAsync(sb.ToString());
                    JsonObject jo = JsonObject.Parse(responseBody);
                    string pattern = jo["pattern"].GetString();
                    string[] rows = pattern.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    int xlen = rows[0].Length;
                    int ylen = rows.Length;
                    bool[,] import = new bool[xlen, ylen];
                    for (int y = 0; y < ylen; y++)
                    {
                        for (int x = 0; x < xlen; x++)
                        {
                            if (rows[y][x] == 'O') import[x, y] = true;
                            else import[x, y] = false;
                        }
                    }
                    for (int y = 0; y < ylen; y++)
                    {
                        for (int x = 0; x < xlen; x++)
                        {
                            if (import[x, y]) this.ViewModel.universe[x + (int)this.NumberBoxXPos.Value, y + (int)this.NumberBoxYPos.Value] = true;
                        }
                    }
                    this.ViewModel.universe.ClearDiffMap();
                    this.canvas.Invalidate();
                    this.WebGrid.Visibility = Visibility.Collapsed;
                }
                catch (HttpRequestException e)
                {

                }
            } else if (host == "bitstorm.org")
            {
                return;
            }
            args.Cancel = true;
        }

        private void AddSpeed(object sender, RoutedEventArgs e)
        {
            Speed = Math.Min(4000000, Speed + 100000);
            timer.Interval = new TimeSpan(Speed);
            this.CurrentSpeedItem.Text = "Current Speed: " + Speed / 1000 + "ms";
        }

        private void SubSpeed(object sender, RoutedEventArgs e)
        {
            Speed = Math.Max(400000, Speed - 100000);
            timer.Interval = new TimeSpan(Speed);
            this.CurrentSpeedItem.Text = "Current Speed: " + Speed / 1000 + "ms";
        }

        private void FileSave(object sender, RoutedEventArgs e)
        {
            var SavePicker = new Windows.Storage.Pickers.FileSavePicker();
            SavePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            SavePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt", ".cells" });
            SavePicker.SuggestedFileName = "Universe";
            this.ViewModel.universe.SaveToPlainText(SavePicker, uName, uDescription);
        }

        private async void FileLoad(object sender, RoutedEventArgs e)
        {
            var OpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            OpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            OpenPicker.FileTypeFilter.Add(".txt");
            OpenPicker.FileTypeFilter.Add(".cells");
            List<string> lines = new List<string>();
            Windows.Storage.StorageFile file = await OpenPicker.PickSingleFileAsync();
            if (file == null) return;
            Windows.Storage.CachedFileManager.DeferUpdates(file);
            foreach (string s in await Windows.Storage.FileIO.ReadLinesAsync(file))
            {
                lines.Add(s);
            }
            await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
            var res = this.ViewModel.universe.LoadFromFile(lines, false);
            if (res != null)
            {
                uName = res.Item1;
                uDescription = res.Item2;
            }
            this.canvas.Invalidate();
        }
        private async void FileImport(object sender, RoutedEventArgs e)
        {
            var OpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            OpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            OpenPicker.FileTypeFilter.Add(".txt");
            OpenPicker.FileTypeFilter.Add(".cells");
            List<string> lines = new List<string>();
            Windows.Storage.StorageFile file = await OpenPicker.PickSingleFileAsync();
            if (file == null) return;
            Windows.Storage.CachedFileManager.DeferUpdates(file);
            foreach (string s in await Windows.Storage.FileIO.ReadLinesAsync(file))
            {
                lines.Add(s);
            }
            await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
            this.ViewModel.universe.LoadFromFile(lines, true);
            this.canvas.Invalidate();
        }

        private void OpenAboutModal(object sender, RoutedEventArgs e)
        {
            this.AboutModal.Visibility = Visibility.Visible;
        }
        private void CloseAboutModal(object sender, RoutedEventArgs e)
        {
            this.AboutModal.Visibility = Visibility.Collapsed;
        }

        private void Quit(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.Core.CoreApplication.Exit();
        }
    }
    public class Prop<T>: INotifyPropertyChanged
    {
        private T _value;
        public T Value
        {
            get => _value;
            set { _value = value; NotifyPropertyChanged(nameof(_value)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
