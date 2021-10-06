
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GameOfLife_UWP
{
    public sealed partial class MainPage
    {
        #region Button Events
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            TimerRunning = !TimerRunning;
            PPBtn.Icon = PlayPauseIcon;
            PPBtn.Label = PlayPauseLabel;
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (vm.universe.Current == 0) return;
            vm.universe.GoTo(vm.universe.Current - 1);
            canvas.Invalidate();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            vm.universe.GoTo(vm.universe.Current + 1);
            canvas.Invalidate();
        }
        private void NewToroidal(object sender, RoutedEventArgs e)
        {
            vm.universe = new Universe(50, true);
            ResizeGrid();
            canvas.Invalidate();
        }

        private void NewFinite(object sender, RoutedEventArgs e)
        {
            vm.universe = new Universe(50, false);
            ResizeGrid();
            canvas.Invalidate();
        }
        private void NewUniverse(object sender, RoutedEventArgs e)
        {
            int w = (int)UniverseWidthSlider.Value;
            int h = (int)UniverseHeightSlider.Value;
            bool t = IsToroidalSwitch.IsOn;
            bool[,] nu = new bool[w, h];
            for (int y = 0; y < Math.Min(h, vm.universe.YLen); y++)
            {
                for (int x = 0; x < Math.Min(w, vm.universe.XLen); x++)
                {
                    nu[x, y] = vm.universe[x, y];
                }
            }
            vm.universe = new Universe(w, h, t);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    vm.universe[x, y] = nu[x, y];
                }
            }
            ResizeGrid();
            canvas.Invalidate();
        }
        private void ZoomIn(object sender, RoutedEventArgs e)
        {
            vm.Zoom += 0.1;
            ResizeGrid();
            canvas.Invalidate();
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            vm.Zoom -= 0.1;
            ResizeGrid();
            canvas.Invalidate();
        }

        private void Randomize(object sender, RoutedEventArgs e)
        {
            vm.universe.Randomize();
            canvas.Invalidate();
        }
        private void ClearHistory(object sender, RoutedEventArgs e)
        {
            vm.universe.ClearDiffMap();
            canvas.Invalidate();
        }

        private void FileSave(object sender, RoutedEventArgs e)
        {
            var SavePicker = new Windows.Storage.Pickers.FileSavePicker();
            SavePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            SavePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt", ".cells" });
            SavePicker.SuggestedFileName = "Universe";
            vm.universe.SaveToPlainText(SavePicker, vm.uName, vm.uDescription);
        }
        private async Task<List<string>> FileLoader()
        {
            var OpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            OpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            OpenPicker.FileTypeFilter.Add(".txt");
            OpenPicker.FileTypeFilter.Add(".cells");
            List<string> lines = new List<string>();
            StorageFile file = await OpenPicker.PickSingleFileAsync();
            if (file == null) return new List<string>();
            CachedFileManager.DeferUpdates(file);
            foreach (string s in await FileIO.ReadLinesAsync(file))
            {
                lines.Add(s);
            }
            await CachedFileManager.CompleteUpdatesAsync(file);
            return lines;
        }
        private async void FileLoad(object sender, RoutedEventArgs e)
        {
            List<string> lines = await FileLoader();
            if (lines.Count == 0) return;
            var res = vm.universe.LoadFromFile(lines, false);
            if (res != null)
            {
                vm.uName = res.Item1;
                vm.uDescription = res.Item2;
            }
            canvas.Invalidate();
        }
        private async void FileImport(object sender, RoutedEventArgs e)
        {
            List<string> lines = await FileLoader();
            if (lines.Count == 0) return;
            vm.universe.LoadFromFile(lines, true);
            canvas.Invalidate();
        }
        private void ToggleCurrent(object sender, RoutedEventArgs e)
        {
            vm.CurrentGenShown = !vm.CurrentGenShown;
            (sender as ToggleMenuFlyoutItem).IsChecked = vm.CurrentGenShown;
            StatusBar.Text = vm.StatusBarText;
        }
        private void ToggleTotal(object sender, RoutedEventArgs e)
        {
            vm.TotalGensShown = !vm.TotalGensShown;
            (sender as ToggleMenuFlyoutItem).IsChecked = vm.TotalGensShown;
            StatusBar.Text = vm.StatusBarText;
        }
        private void ToggleLiving(object sender, RoutedEventArgs e)
        {
            vm.LivingCellsShown = !vm.LivingCellsShown;
            (sender as ToggleMenuFlyoutItem).IsChecked = vm.LivingCellsShown;
            StatusBar.Text = vm.StatusBarText;
        }
        #endregion
    }
}