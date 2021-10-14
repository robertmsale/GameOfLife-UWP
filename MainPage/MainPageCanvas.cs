
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
    /// <summary>
    /// Convenient singleton struct for handling the canvas drag event
    /// </summary>
    public struct CanvasDrag
    {
        public Point start;
        public Point end;
        public bool isDragging;
        public bool isStarted;
        public bool selectionValid;
        public DateTime startTime;
    }
    /// <summary>
    /// Class used as the values in a hash set value for very quick cell selection
    /// </summary>
    public struct Selected: IEquatable<Selected>
    {
        public int X;
        public int Y;
        public Selected(int x, int y) { X = x; Y = y; }
        /// <summary>
        /// Allows for equality testing for HashSet
        /// </summary>
        /// <returns>equality result</returns>
        public bool Equals(Selected i)
        {
            return X == i.X && Y == i.Y;
        }
        /// <summary>
        /// Makes this struct hashable and therefore O(1) accessible in a HashSet
        /// </summary>
        /// <returns>hash code to prevent key collisions</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
    public sealed partial class MainPage
    {
        #region Direct2D Canvas Events and Methods
        // Dedicated section for D2D specific fields in MainPage
        public double CanvasFontSize;
        public CanvasDrag cDrag;
        /// <summary>
        /// The hash set responsible for actively changing the canvas during drag event
        /// </summary>
        public HashSet<Selected> selectedCells = new();
        /// <summary>
        /// The hash set responsible for keeping the cells selected after drag event is complete
        /// </summary>
        public HashSet<Selected> persistCells = new();
        /// <summary>
        /// Contains Copy/Cut/Paste
        /// </summary>
        public MenuFlyout CanvasFlyout = new();
        /// <summary>
        /// Array of cells that were copied or cut
        /// </summary>
        public bool[,] clipboard = new bool[0,0];
        /// <summary>
        /// Point to draw the menu flyout
        /// </summary>
        public Point RightClickCell = new(0, 0);
        /// <summary>
        /// Persistent click state
        /// </summary>
        public bool wasRightClickPressed = false;
        /// <summary>
        /// Canvas draw method. Uses sender and args so I don't have to get the canvas element from the view heirarchy
        /// </summary>
        /// <param name="sender">The canvas element used to configure color</param>
        /// <param name="args">The element's drawing session</param>
        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            // Reset canvas to prevent weird overlapping draws
            Color transparentColor = new Color();
            transparentColor.A = 0;
            args.DrawingSession.FillRectangle(
                new Rect(0, 0, canvas.Width, canvas.Height),
                new CanvasSolidColorBrush(sender, Colors.Black));
            // Set canvas text format based on zoom level and center them in the cell
            CanvasTextFormat ctf = new();
            ctf.FontSize = (float)(FontSize * vm.Zoom);
            ctf.VerticalAlignment = CanvasVerticalAlignment.Center;
            ctf.HorizontalAlignment = CanvasHorizontalAlignment.Center;
            // Make a local array referencing the count
            byte[,] neighbors = vm.universe.neighborCount;
            // Get canvas colors
            CanvasSolidColorBrush LiveCell = new CanvasSolidColorBrush(sender, vm.LiveCell);
            CanvasSolidColorBrush DeadCell = new CanvasSolidColorBrush(sender, vm.DeadCell);
            CanvasSolidColorBrush GridColor = new CanvasSolidColorBrush(sender, vm.GridColor);
            for (int x = 0; x < vm.universe.XLen; x++)
            {
                for (int y = 0; y < vm.universe.YLen; y++)
                {
                    // If cell is live or dead, set that color here
                    CanvasSolidColorBrush cellColor;
                    if (vm.universe[x, y]) cellColor = LiveCell;
                    else cellColor = DeadCell;
                    // Text color is the inverted value of the cell color (pretty much guaranteed to be visible
                    Color TextColor = Color.FromArgb(255, (byte)(255 - cellColor.Color.R), (byte)(255 - cellColor.Color.G), (byte)(255 - cellColor.Color.B));
                    Point pos = new(x * vm.CellSize, y * vm.CellSize);
                    Size size = new(vm.CellSize, vm.CellSize);
                    Rect rect = new(pos, size);
                    // Fill cell
                    args.DrawingSession.FillRectangle(rect, cellColor);
                    // Draw grid
                    if (ShowGridToggle.IsChecked)
                        args.DrawingSession.DrawRectangle(rect, GridColor, 1.0f);
                    // Draw neighbors
                    if (ShowNeighborsToggle.IsChecked)
                        args.DrawingSession.DrawText(
                            neighbors[x, y].ToString(),
                            rect,
                            TextColor,
                            ctf);
                }
            }
            // Draw translucent gray rectangles to indicate cells are selected
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
        /// <summary>
        /// Copy cells selected event handler
        /// </summary>
        private void CanvasCopyClicked(object sender, RoutedEventArgs e)
        {
            // Derive selected cells by reversing cell size from the drag rectangle
            int x = (int)(Math.Min(cDrag.start.X, cDrag.end.X) / vm.CellSize);
            int y = (int)(Math.Min(cDrag.start.Y, cDrag.end.Y) / vm.CellSize);
            int w = (int)Math.Ceiling(Math.Abs(cDrag.start.X - cDrag.end.X) / vm.CellSize);
            int h = (int)Math.Ceiling(Math.Abs(cDrag.start.Y - cDrag.end.Y) / vm.CellSize);
            // Commit selection to clipboard
            clipboard = new bool[w, h];
            for (int i = x; i < x + w; i++)
            {
                for (int j = y; j < y + h; j++)
                {
                    clipboard[i - x, j - y] = vm.universe[i, j];
                }
            }
            // clear selected cells hash map to stop drawing gray cells
            selectedCells.Clear();
            canvas.Invalidate();
        }
        /// <summary>
        /// Canvas cut click event
        /// </summary>
        private void CanvasCutClicked(object sender, RoutedEventArgs e)
        {
            // Derive selected cells by reversing cell size from the drag rectangle
            int x = (int)(Math.Min(cDrag.start.X, cDrag.end.X) / vm.CellSize);
            int y = (int)(Math.Min(cDrag.start.Y, cDrag.end.Y) / vm.CellSize);
            int w = (int)Math.Ceiling(Math.Abs(cDrag.start.X - cDrag.end.X) / vm.CellSize);
            int h = (int)Math.Ceiling(Math.Abs(cDrag.start.Y - cDrag.end.Y) / vm.CellSize);
            // Commit selection to clipboard
            clipboard = new bool[w, h];
            for (int i = x; i < x + w; i++)
            {
                for (int j = y; j < y + h; j++)
                {
                    clipboard[i - x, j - y] = vm.universe[i, j];
                    // Set living cell to dead once copied
                    vm.universe[i, j] = false;
                }
            }
            // clear selected cells hash map to stop drawing gray cells
            selectedCells.Clear();
            canvas.Invalidate();
        }
        /// <summary>
        /// Canvas paste click event
        /// </summary>
        private void CanvasPasteClicked(object sender, RoutedEventArgs e)
        {
            // Get coordinate where the right click menu was spawned
            int x = (int)RightClickCell.X;
            int y = (int)RightClickCell.Y;
            // Perform pasting
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
                // Make sure pointer moved event knows we're in a right click event
                wasRightClickPressed = true;
                RightClickCell.X = pos.X / vm.CellSize;
                RightClickCell.Y = pos.Y / vm.CellSize;
                CanvasFlyout.ShowAt(canvas, pos);
                return;
            }
            // Pointer move needs to know it should do stuff now
            cDrag.isStarted = true;
            // This is how we keep track of whether this is actually a drag or just a click
            cDrag.startTime = DateTime.Now;
            cDrag.start = pos;
            // Clear selected cells no matter what click happened
            if (selectedCells.Count > 0) selectedCells.Clear();
        }
        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // After a certain amount of time, we know this is a real drag move
            if (cDrag.startTime.AddMilliseconds(300) < DateTime.Now && cDrag.isStarted)
            {
                // Tell canvas the selection is valid
                cDrag.selectionValid = true;
                // Tell move events we are definitely dragging now
                cDrag.isDragging = true;
                // Get drag end point
                cDrag.end = e.GetCurrentPoint(canvas).Position; 
                // Get min/max box dimensions
                Point spos = new(
                     Math.Min(cDrag.start.X, cDrag.end.X),
                     Math.Min(cDrag.start.Y, cDrag.end.Y));
                Point epos = new(
                    Math.Max(cDrag.start.X, cDrag.end.X),
                    Math.Max(cDrag.start.Y, cDrag.end.Y));
                // Clear selection so we can always have a fresh selection
                selectedCells.Clear();
                // For every cell inside the box, add that cell to the selection HashSet
                for (int x = (int)(spos.X / vm.CellSize); x < (int)Math.Ceiling(epos.X / vm.CellSize); x++)
                {
                    for (int y = (int)(spos.Y / vm.CellSize); y < (int)Math.Ceiling(epos.Y / vm.CellSize); y++)
                    {
                        // Do the magic
                        Selected s = new Selected(x, y);
                        // Don't add unless it doesn't already exist
                        if (!selectedCells.Contains(s)) selectedCells.Add(s);
                    }
                }
                canvas.Invalidate();
            }
        }
        /// <summary>
        /// Begin cleanup process after canvas is no longer being clicked
        /// </summary>
        private void canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var ptr = e.GetCurrentPoint(sender as CanvasControl);
            if (wasRightClickPressed)
            {
                // Right click shows the flyout, so go no further
                wasRightClickPressed = false;
                return;
            }
            // Tell pointer move that clicking is no longer happening
            cDrag.isStarted = false;
            // if click event was not a drag event, toggle the cell
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
        /// <summary>
        /// Resize the grid and render changes
        /// </summary>
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