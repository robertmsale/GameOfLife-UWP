
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GameOfLife_UWP
{
    public struct CanvasDrag
    {
        public Point start;
        public Point end;
        public bool isDragging;
        public bool isStarted;
        public bool selectionValid;
        public DateTime startTime;
    }
    public struct Selected: IEquatable<Selected>
    {
        public int X;
        public int Y;
        public Selected(int x, int y) { X = x; Y = y; }
        public bool Equals(Selected i)
        {
            return X == i.X && Y == i.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
    public sealed partial class MainPage
    {
        #region Direct2D Canvas Events and Methods
        public double CanvasFontSize;
        public CanvasDrag cDrag;
        public HashSet<Selected> selectedCells = new();
        public HashSet<Selected> persistCells = new();
        public MenuFlyout CanvasFlyout = new();
        public bool[,] clipboard = new bool[0,0];
        public Point RightClickCell = new(0, 0);
        public bool wasRightClickPressed = false;
        /// <summary>
        /// Live cell color
        /// </summary>
        /// <param name="c">The Direct2D Canvas, which is passed by reference so the brush can be applied to it</param>
        /// <returns>The canvas brush with the color data embeded</returns>
        private ICanvasBrush LiveCell(CanvasControl c)
        {
            Color col = Colors.Chartreuse;
            return new CanvasSolidColorBrush(c, col);
        }
        /// <summary>
        /// Dead cell color
        /// </summary>
        /// <param name="c">The Direct2D Canvas, which is passed by reference so the brush can be applied to it</param>
        /// <returns>The canvas brush with the color data embeded</returns>
        private ICanvasBrush DeadCell(CanvasControl c)
        {
            Color col = Colors.Black;
            return new CanvasSolidColorBrush(c, col);
        }
        /// <summary>
        /// Canvas draw method. Uses sender and args so I don't have to get the canvas element from the view heirarchy
        /// </summary>
        /// <param name="sender">The canvas element used to configure color</param>
        /// <param name="args">The element's drawing session</param>
        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            Color transparentColor = new Color();
            transparentColor.A = 0;
            args.DrawingSession.FillRectangle(
                new Rect(0, 0, canvas.Width, canvas.Height),
                new CanvasSolidColorBrush(sender, Colors.Black));
            CanvasTextFormat ctf = new();
            ctf.FontSize = (float)(FontSize * vm.Zoom);
            ctf.VerticalAlignment = CanvasVerticalAlignment.Center;
            ctf.HorizontalAlignment = CanvasHorizontalAlignment.Center;
            byte[,] neighbors = vm.universe.neighborCount;
            CanvasSolidColorBrush LiveCell = new CanvasSolidColorBrush(sender, vm.LiveCell);
            CanvasSolidColorBrush DeadCell = new CanvasSolidColorBrush(sender, vm.DeadCell);
            CanvasSolidColorBrush GridColor = new CanvasSolidColorBrush(sender, vm.GridColor);
            for (int x = 0; x < vm.universe.XLen; x++)
            {
                for (int y = 0; y < vm.universe.YLen; y++)
                {
                    Color gridcolor = Colors.Azure;
                    CanvasSolidColorBrush cellColor;
                    if (vm.universe[x, y]) cellColor = LiveCell;
                    else cellColor = DeadCell;
                    Color TextColor = Color.FromArgb(255, (byte)(255 - cellColor.Color.R), (byte)(255 - cellColor.Color.G), (byte)(255 - cellColor.Color.B));
                    Point pos = new(x * vm.CellSize, y * vm.CellSize);
                    Size size = new(vm.CellSize, vm.CellSize);
                    Rect rect = new(pos, size);
                    args.DrawingSession.FillRectangle(rect, cellColor);
                    if (ShowGridToggle.IsChecked)
                        args.DrawingSession.DrawRectangle(rect, GridColor, 1.0f);
                    if (ShowNeighborsToggle.IsChecked)
                        args.DrawingSession.DrawText(
                            neighbors[x, y].ToString(),
                            rect,
                            TextColor,
                            ctf);
                }
            }
            foreach(Selected s in selectedCells)
            {
                Point spos = new(
                    (double)s.X * vm.CellSize,
                    (double)s.Y * vm.CellSize);
                Size size = new(
                    vm.CellSize,
                    vm.CellSize);
                args.DrawingSession.FillRectangle(
                    new Rect(spos, size),
                    new CanvasSolidColorBrush(sender, Color.FromArgb(30, 254, 254, 254)));
            }
            // Draw click drag rectangle on screen
            if (cDrag.isDragging)
            {
                Point pos = new Point(
                    Math.Min(cDrag.start.X, cDrag.end.X),
                    Math.Min(cDrag.start.Y, cDrag.end.Y));
                Size size = new Size(
                    Math.Abs(cDrag.start.X - cDrag.end.X),
                    Math.Abs(cDrag.start.Y - cDrag.end.Y));

                Rect r = new Rect(pos, size);
                args.DrawingSession.DrawRectangle(r, new CanvasSolidColorBrush(sender, Color.FromArgb(220, 0, 20, 166)), 1);
                args.DrawingSession.FillRectangle(r, new CanvasSolidColorBrush(sender, Color.FromArgb(60, 0, 20, 166)));
            }
            StatusBar.Text = vm.StatusBarText;
        }
        private void CanvasCopyClicked(object sender, RoutedEventArgs e)
        {
            int x = (int)(Math.Min(cDrag.start.X, cDrag.end.X) / vm.CellSize);
            int y = (int)(Math.Min(cDrag.start.Y, cDrag.end.Y) / vm.CellSize);
            int w = (int)Math.Ceiling(Math.Abs(cDrag.start.X - cDrag.end.X) / vm.CellSize);
            int h = (int)Math.Ceiling(Math.Abs(cDrag.start.Y - cDrag.end.Y) / vm.CellSize);
            clipboard = new bool[w, h];
            for (int i = x; i < x + w; i++)
            {
                for (int j = y; j < y + h; j++)
                {
                    clipboard[i - x, j - y] = vm.universe[i, j];
                }
            }
            selectedCells.Clear();
            canvas.Invalidate();
        }
        private void CanvasCutClicked(object sender, RoutedEventArgs e)
        {

            int x = (int)(Math.Min(cDrag.start.X, cDrag.end.X) / vm.CellSize);
            int y = (int)(Math.Min(cDrag.start.Y, cDrag.end.Y) / vm.CellSize);
            int w = (int)Math.Ceiling(Math.Abs(cDrag.start.X - cDrag.end.X) / vm.CellSize);
            int h = (int)Math.Ceiling(Math.Abs(cDrag.start.Y - cDrag.end.Y) / vm.CellSize);
            clipboard = new bool[w, h];
            for (int i = x; i < x + w; i++)
            {
                for (int j = y; j < y + h; j++)
                {
                    clipboard[i - x, j - y] = vm.universe[i, j];
                    vm.universe[i, j] = false;
                }
            }
            selectedCells.Clear();
            canvas.Invalidate();
        }
        private void CanvasPasteClicked(object sender, RoutedEventArgs e)
        {
            int x = (int)RightClickCell.X;
            int y = (int)RightClickCell.Y;
            for (int i = 0; i < clipboard.GetLength(0); i++)
            {
                for (int j = 0; j < clipboard.GetLength(1); j++)
                {
                    vm.universe[i + x, j + y] = clipboard[i, j];
                }
            }
            canvas.Invalidate();
        }
        /// <summary>
        /// Pointer pressed event for capturing the pointer location and changing a cell's value.
        /// </summary>
        /// <param name="e">The event which holds a snapshot of the pointer state and its current location at the moment this was triggered</param>
        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var ptr = e.GetCurrentPoint(sender as CanvasControl);
            Point pos = ptr.Position;
            if (ptr.Properties.IsRightButtonPressed)
            {
                wasRightClickPressed = true;
                RightClickCell.X = pos.X / vm.CellSize;
                RightClickCell.Y = pos.Y / vm.CellSize;
                CanvasFlyout.ShowAt(canvas, pos);
                return;
            }
            cDrag.isStarted = true;
            cDrag.startTime = DateTime.Now;
            cDrag.start = pos;
            if (selectedCells.Count > 0) selectedCells.Clear();
        }
        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (cDrag.startTime.AddMilliseconds(300) < DateTime.Now && cDrag.isStarted)
            {
                cDrag.selectionValid = true;
                cDrag.isDragging = true;
                cDrag.end = e.GetCurrentPoint(canvas).Position; 
                Point spos = new(
                     Math.Min(cDrag.start.X, cDrag.end.X),
                     Math.Min(cDrag.start.Y, cDrag.end.Y));
                Point epos = new(
                    Math.Max(cDrag.start.X, cDrag.end.X),
                    Math.Max(cDrag.start.Y, cDrag.end.Y));
                selectedCells.Clear();
                for (int x = (int)(spos.X / vm.CellSize); x < (int)Math.Ceiling(epos.X / vm.CellSize); x++)
                {
                    for (int y = (int)(spos.Y / vm.CellSize); y < (int)Math.Ceiling(epos.Y / vm.CellSize); y++)
                    {
                        Selected s = new Selected(x, y);
                        if (!selectedCells.Contains(s)) selectedCells.Add(s);
                    }
                }
                canvas.Invalidate();
            }
        }
        private void canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var ptr = e.GetCurrentPoint(sender as CanvasControl);
            if (wasRightClickPressed)
            {
                wasRightClickPressed = false;
                return;
            }
            cDrag.isStarted = false;
            if (!cDrag.isDragging)
            {
                vm.universe.ClickCell(
                    (int)(cDrag.start.X / vm.CellSize),
                    (int)(cDrag.start.Y / vm.CellSize));
                cDrag.selectionValid = false;
            } else
            {
                cDrag.isDragging = false;
            }
            canvas.Invalidate();
        }
        private void ResizeGrid()
        {
            vm.GridWidth = vm.CellSize * vm.universe.XLen;
            vm.GridHeight = vm.CellSize * vm.universe.YLen;
            canvas.Width = vm.GridWidth;
            canvas.Height = vm.GridHeight;
            canvas.Invalidate();
        }
        #endregion
    }
}