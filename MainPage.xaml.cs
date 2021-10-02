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
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.ComponentModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GameOfLife_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public ViewModel ViewModel;
        public double GridWidth { get; set; }
        public double GridHeight{ get; set; }
        public bool timerRunning = false;
        public string PlayPause
        {
            get => timerRunning ? "Pause" : "Play";
        }
        DispatcherTimer timer = new DispatcherTimer();
        public MainPage()
        {
            this.ViewModel = new ViewModel();
            this.ViewModel.universe.Randomize();
            Rectangle r = new Rectangle();
            GridWidth = this.ViewModel.CellSize * this.ViewModel.universe.XLen;
            GridWidth = this.ViewModel.CellSize * this.ViewModel.universe.XLen;
            this.InitializeComponent();
            this.canvas.Width = GridWidth;
            this.canvas.Height = GridHeight;
            timer.Interval = new TimeSpan(10000); // milliseconds
            timer.Tick += Timer_Tick;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
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

        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Point p = e.GetCurrentPoint(this.canvas).Position;
            this.ViewModel.universe.ClickCell(
                (int)(p.X / this.ViewModel.CellSize),
                (int)(p.Y / this.ViewModel.CellSize)
                );
            this.canvas.Invalidate();
        }

        //private void NewUniverse(object sender, RoutedEventArgs e)
        //{
        //    this.ViewModel.universe = new Universe((int)this.NumberBoxWidth.Value, (int)this.NumberBoxHeight.Value, this.IsToroidalSwitch.IsOn);
        //    ResizeGrid();
        //    this.canvas.Invalidate();
        //}

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
            this.ViewModel.universe.GoTo(this.ViewModel.universe.Current - 1);
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
            this.ViewModel.universe = new Universe(50, true);
            ResizeGrid();
            this.canvas.Invalidate();
        }

        private void NewFinite(object sender, RoutedEventArgs e)
        {
            this.ViewModel.universe = new Universe(50, false);
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
