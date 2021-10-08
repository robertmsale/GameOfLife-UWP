using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GameOfLife_UWP
{
    
    public sealed partial class ColorDialog : ContentDialog
    {
        public Color Living
        {
            get => LivingCellColorPicker.Color;
            set => LivingCellColorPicker.Color = value;
        }
        public Color Dead
        {
            get => DeadCellColorPicker.Color;
            set => DeadCellColorPicker.Color = value;
        }
        public Color Grid
        {
            get => GridColorPicker.Color;
            set => GridColorPicker.Color = value;
        }
        public ColorDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
