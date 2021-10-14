using System;
using Windows.UI.Xaml;

namespace GameOfLife_UWP
{
    public partial class MainPage
    {
        #region About Modal Events and Methods
        /// <summary>
        /// Opens the about modal. Not much more to it.
        /// </summary>
        private async void OpenAboutModal(object sender, RoutedEventArgs e)
        {
            await aboutModal.ShowAsync();
        }
        #endregion
    }
}