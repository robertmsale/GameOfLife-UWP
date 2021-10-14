using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GameOfLife_UWP
{
    public partial class MainPage
    {
        #region Universe Details Modal Events and Methods
        private async void OpenModal(object sender, RoutedEventArgs e)
        {
            var result = await editUniverseModal.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                vm.uName = editUniverseModal.Name;
                vm.uDescription = editUniverseModal.Description;
            }
            else
            {
                editUniverseModal.Name = vm.uName;
                editUniverseModal.Description = vm.uDescription;
            }
        }
        #endregion
    }
}