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
using Windows.Storage;

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
            DataContext = this;
            vm = new ViewModel();
            vm.universe.Randomize();
            vm.GridWidth = vm.CellSize * vm.universe.XLen;
            vm.GridHeight = vm.CellSize * vm.universe.YLen;
            InitializeComponent();
            canvas.Width = vm.GridWidth;
            canvas.Height = vm.GridHeight;
            timer.Interval = new TimeSpan(10000000); // milliseconds
            timer.Tick += Timer_Tick;
            CurrentSpeedItem.Text = "Current Speed: 1000ms";
            SpeedSlider.Value = (double)10000000;
            WebView.NavigationStarting += WebView_NavigationStarting;
            TC.IsChecked = vm.CurrentGenShown;
            TT.IsChecked = vm.TotalGensShown;
            TL.IsChecked = vm.LivingCellsShown;
        }
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
            canvas.Width = vm.GridWidth;
            canvas.Height = vm.GridHeight;
            UniverseWidthSlider.Value = vm.universe.XLen;
            UniverseHeightSlider.Value = vm.universe.YLen;
            timer.Interval = new TimeSpan(vm.Speed); // milliseconds
            CurrentSpeedItem.Text = "Current Speed: "+(vm.Speed/10000)+"ms";
            SpeedSlider.Value = (double)vm.Speed/100;
            TC.IsChecked = vm.CurrentGenShown;
            TT.IsChecked = vm.TotalGensShown;
            TL.IsChecked = vm.LivingCellsShown;
            ShowNeighborsToggle.IsChecked = vm.NeighborsShown;
            ShowGridToggle.IsChecked = vm.GridShown;
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
        
        
        #region Universe Details Modal Events and Methods
        private async void OpenModal(object sender, RoutedEventArgs e)
        {
            var result = await editUniverseModal.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                vm.uName = editUniverseModal.Name;
                vm.uDescription = editUniverseModal.Description;
            } else
            {
                editUniverseModal.Name = vm.uName;
                editUniverseModal.Description = vm.uDescription;
            }
        }
        #endregion
        #region About Modal Events and Methods
        private async void OpenAboutModal(object sender, RoutedEventArgs e)
        {
            await aboutModal.ShowAsync();
        }
        #endregion

        private void SpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            vm.Speed = (int)e.NewValue * 10000;
            CurrentSpeedItem.Text = "Current Speed: "+ (int)e.NewValue + "ms";
            timer.Interval = new TimeSpan(vm.Speed);
            if (timer.IsEnabled)
            {
                timer.Stop();
                timer.Start();
            }
        }

        private void ShowNeighborsToggle_Click(object sender, RoutedEventArgs e)
        {
            canvas.Invalidate();
        }

        private void ShowGridToggle_Click(object sender, RoutedEventArgs e)
        {
            canvas.Invalidate();
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await colorModal.ShowAsync();
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
