using System;
using Windows.UI.Xaml;

namespace GameOfLife_UWP
{
    public partial class MainPage
    {
        #region Window and Async Events
        /// <summary>
        /// Event handler for when the window loads. Sets the data context to this whole class so properties are easily accessible in XAML
        /// </summary>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            vm = await ViewModel.Factory();
            vm.GridWidth = vm.CellSize * vm.universe.XLen;
            vm.GridHeight = vm.CellSize * vm.universe.YLen;
            InitializeComponent();
            colorModal.Living = vm.LiveCell;
            colorModal.Dead = vm.DeadCell;
            colorModal.Grid = vm.GridColor;
            canvas.Width = vm.GridWidth;
            canvas.Height = vm.GridHeight;
            UniverseWidthSlider.Value = vm.universe.XLen;
            UniverseHeightSlider.Value = vm.universe.YLen;
            timer.Interval = new TimeSpan(0, 0, 0, 0, vm.Speed);
            CurrentSpeedItem.Text = "Current Speed: " + vm.Speed + "ms";
            SpeedSlider.Value = vm.Speed;
            TC.IsChecked = vm.CurrentGenShown;
            TT.IsChecked = vm.TotalGensShown;
            TL.IsChecked = vm.LivingCellsShown;
            ShowNeighborsToggle.IsChecked = vm.NeighborsShown;
            ShowGridToggle.IsChecked = vm.GridShown;
            IsToroidalSwitch.IsOn = vm.universe.IsToroidal;
            canvas.Invalidate();
        }
        /// <summary>
        /// Gracefully unloads page by releasing Direct2D Canvas resources and removing from page
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }
        /// <summary>
        /// This event calculates the next generation and redraws the canvas
        /// </summary>
        private void Timer_Tick(object sender, object e)
        {
            vm.universe.CalculateNextGeneration();
            canvas.Invalidate();
        }
        /// <summary>
        /// Handles gracefully quitting the UWP application
        /// </summary>
        private void Quit(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.Core.CoreApplication.Exit();
        }
        #endregion
    }
}