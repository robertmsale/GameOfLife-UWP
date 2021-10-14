using System;
using Windows.UI.Xaml.Controls;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GameOfLife_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            // Set data context here so that the XAML elements have binding access to the properties
            // defined in this class
            DataContext = this;
            // The View model field, which contains most of the application state
            vm = new ViewModel();
            vm.universe.Randomize();
            vm.GridWidth = vm.CellSize * vm.universe.XLen;
            vm.GridHeight = vm.CellSize * vm.universe.YLen;
            // Calls the underlying WinRT initialization of XAML elements
            InitializeComponent();
            // Set font size and canvas size immediately
            CanvasFontSize = canvas.FontSize;
            canvas.Width = vm.GridWidth;
            canvas.Height = vm.GridHeight;
            // Set color modal's values to their default
            colorModal.Living = Colors.Chartreuse;
            colorModal.Dead = Colors.Black;
            colorModal.Grid = Colors.Azure;
            // Initialize timer to its default value and set speed slider text
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1000); // milliseconds
            timer.Tick += Timer_Tick;
            CurrentSpeedItem.Text = "Current Speed: 1000ms";
            SpeedSlider.Value = (double)1000;
            // Manually add navigation event handler to webview for import magic
            WebView.NavigationStarting += WebView_NavigationStarting;
            // Set view settings to their defaults defined by the view model
            TC.IsChecked = vm.CurrentGenShown;
            TT.IsChecked = vm.TotalGensShown;
            TL.IsChecked = vm.LivingCellsShown;

            // Very special, hand crafted menu flyout that has to be initialized here
            // to enable cut copy paste on the canvas
            MenuFlyoutItem cut = new MenuFlyoutItem { Text = "Cut", Tag = "CanvasCut" };
            MenuFlyoutItem copy = new MenuFlyoutItem { Text = "Copy", Tag = "CanvasCopy" };
            MenuFlyoutItem paste = new MenuFlyoutItem { Text = "Paste", Tag = "CanvasPaste" };

            cut.Click += CanvasCutClicked;
            copy.Click += CanvasCopyClicked;
            paste.Click += CanvasPasteClicked;

            CanvasFlyout.Items.Add(cut);
            CanvasFlyout.Items.Add(copy);
            CanvasFlyout.Items.Add(paste);
        }

    }
}
