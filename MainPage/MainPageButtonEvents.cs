
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace GameOfLife_UWP
{
    public sealed partial class MainPage
    {
        #region Button Events
        /// <summary>
        /// Event handler for when the play/pause button is clicked
        /// </summary>
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            // Toggle timer
            TimerRunning = !TimerRunning;
            // After timer is toggled, get the icon (which is a property that depends on TimerRunning)
            PPBtn.Icon = PlayPauseIcon;
            // Same as above but for the label
            PPBtn.Label = PlayPauseLabel;
        }

        /// <summary>
        /// Navigates backwards in the universe array
        /// </summary>
        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            // Don't go back past zero (avoid index out of range error!)
            if (vm.universe.Current == 0) return;
            vm.universe.GoTo(vm.universe.Current - 1);
            canvas.Invalidate();
        }
        /// <summary>
        /// Navigate forward in the universe array, calculating next if necessary.
        /// </summary>
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            vm.universe.GoTo(vm.universe.Current + 1);
            canvas.Invalidate();
        }
        /// <summary>
        /// Creates a new universe grid (preserving existing live cells)
        /// </summary>
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
        /// <summary>
        /// Adjust zoom level on the grid (increase) event handler
        /// </summary>
        private void ZoomIn(object sender, RoutedEventArgs e)
        {
            vm.Zoom += 0.1;
            ResizeGrid();
            canvas.Invalidate();
        }
        /// <summary>
        /// Adjust zoom level on the grid (decrease) event handler
        /// </summary>
        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            vm.Zoom -= 0.1;
            ResizeGrid();
            canvas.Invalidate();
        }
        /// <summary>
        /// Randomize universe event handler
        /// </summary>
        private void Randomize(object sender, RoutedEventArgs e)
        {
            vm.universe.Randomize();
            canvas.Invalidate();
        }
        /// <summary>
        /// Clear history event handler
        /// </summary>
        private void ClearHistory(object sender, RoutedEventArgs e)
        {
            vm.universe.ClearDiffMap();
            canvas.Invalidate();
        }
        /// <summary>
        /// Save universe using Windows file picker
        /// </summary>
        private void FileSave(object sender, RoutedEventArgs e)
        {
            // Declare save picker object
            FileSavePicker SavePicker = new FileSavePicker();
            // Open picker in documents folder
            SavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Only allow saving as a txt or cells file
            SavePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt", ".cells" });
            // Set default file name
            SavePicker.SuggestedFileName = "Universe";
            // Pass picker into universe and let it handle serialization
            vm.universe.SaveToPlainText(SavePicker, vm.uName, vm.uDescription);
        }
        /// <summary>
        /// Async File loader that loads a universe in plaintext format
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> FileLoader()
        {
            var OpenPicker = new FileOpenPicker();
            OpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            OpenPicker.FileTypeFilter.Add(".txt");
            OpenPicker.FileTypeFilter.Add(".cells");
            // Container for lines
            List<string> lines = new List<string>();
            StorageFile file = await OpenPicker.PickSingleFileAsync();
            if (file == null) return new List<string>();
            // Tell file system to prevent file mutation until read is completed
            CachedFileManager.DeferUpdates(file);
            foreach (string s in await FileIO.ReadLinesAsync(file))
            {
                lines.Add(s);
            }
            // Allow file system to apply updates to file if necessary
            await CachedFileManager.CompleteUpdatesAsync(file);
            return lines;
        }
        /// <summary>
        /// Async file load event handler (replace existing universe)
        /// </summary>
        private async void FileLoad(object sender, RoutedEventArgs e)
        {
            // Get plaintext lines
            List<string> lines = await FileLoader();
            // If file is empty, just give up.
            if (lines.Count == 0) return;
            // res contains a tuple with the universe name and description
            var res = vm.universe.LoadFromFile(lines, false);
            if (res != null)
            {
                vm.uName = res.Item1;
                vm.uDescription = res.Item2;
            }
            canvas.Invalidate();
        }
        /// <summary>
        /// Async file import for loading universe as plaintext (keeps existing universe intact)
        /// </summary>
        private async void FileImport(object sender, RoutedEventArgs e)
        {
            List<string> lines = await FileLoader();
            if (lines.Count == 0) return;
            vm.universe.LoadFromFile(lines, true);
            canvas.Invalidate();
        }
        /// <summary>
        /// Event handler for toggling the current universe in the status bar
        /// </summary>
        /// <param name="sender">The toggle switch that was pressed</param>
        private void ToggleCurrent(object sender, RoutedEventArgs e)
        {
            vm.CurrentGenShown = !vm.CurrentGenShown;
            (sender as ToggleMenuFlyoutItem).IsChecked = vm.CurrentGenShown;
            StatusBar.Text = vm.StatusBarText;
        }
        /// <summary>
        /// Event handler for toggling the total generations in the status bar
        /// </summary>
        /// <param name="sender">The toggle switch that was pressed</param>
        private void ToggleTotal(object sender, RoutedEventArgs e)
        {
            vm.TotalGensShown = !vm.TotalGensShown;
            (sender as ToggleMenuFlyoutItem).IsChecked = vm.TotalGensShown;
            StatusBar.Text = vm.StatusBarText;
        }
        /// <summary>
        /// Event handler for toggling the total living cells in the status bar
        /// </summary>
        /// <param name="sender">The toggle switch that was pressed</param>
        private void ToggleLiving(object sender, RoutedEventArgs e)
        {
            vm.LivingCellsShown = !vm.LivingCellsShown;
            (sender as ToggleMenuFlyoutItem).IsChecked = vm.LivingCellsShown;
            StatusBar.Text = vm.StatusBarText;
        }
        /// <summary>
        /// Speed slider event handler which interactively changes speed even while the timer is running
        /// </summary>
        private void SpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            vm.Speed = (int)e.NewValue;
            CurrentSpeedItem.Text = "Current Speed: " + (int)e.NewValue + "ms";
            timer.Interval = new TimeSpan(0, 0, 0, 0, vm.Speed);
            // Restart dispatch timer with new interval
            if (timer.IsEnabled)
            {
                timer.Stop();
                timer.Start();
            }
        }
        /// <summary>
        /// Show neighbors event handler
        /// </summary>
        private void ShowNeighborsToggle_Click(object sender, RoutedEventArgs e)
        {
            vm.NeighborsShown = ShowNeighborsToggle.IsChecked;
            canvas.Invalidate();
        }
        /// <summary>
        /// Show grid event handler
        /// </summary>
        private void ShowGridToggle_Click(object sender, RoutedEventArgs e)
        {
            vm.GridShown = ShowGridToggle.IsChecked;
            canvas.Invalidate();
        }
        /// <summary>
        /// Show color modal event handler (asynchronous)
        /// </summary>
        private async void ShowColorModal(object sender, RoutedEventArgs e)
        {
            // Shows the modal without interrupting the main thread
            var res = await colorModal.ShowAsync();
            // If the commit button was pressed, change the colors
            if (res == ContentDialogResult.Primary)
            {
                vm.LiveCell = colorModal.Living;
                vm.DeadCell = colorModal.Dead;
                vm.GridColor = colorModal.Grid;
                canvas.Invalidate();
            }
            else // reset color picker values
            {
                colorModal.Living = vm.LiveCell;
                colorModal.Dead = vm.DeadCell;
                colorModal.Grid = vm.GridColor;
            }
        }
        /// <summary>
        /// Just call the Window Loaded event and let it reload the app state from file
        /// </summary>
        private void ReloadSaved_Click(object sender, RoutedEventArgs e)
        {
            Window_Loaded(sender, e);
        }
        /// <summary>
        /// Reset view model to constructor defaults and reload the window
        /// </summary>
        private async void RestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            vm = new ViewModel();
            await vm.SaveToFile();
            Window_Loaded(sender, e);
        }
        #endregion
    }
}