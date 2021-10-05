
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Input;

namespace GameOfLife_UWP
{
    public sealed partial class MainPage
    {
        #region Direct2D Canvas Events and Methods
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
            transparentColor.A = 0; // Set alpha channel to zero for transparency
            args.DrawingSession.Clear(transparentColor); // Apply transparency to entire canvas just incase drawing over elements leaves unwanted artifacts
            byte[,] neighbors = vm.universe.neighborCount;
            for (int x = 0; x < vm.universe.XLen; x++)
            {
                for (int y = 0; y < vm.universe.YLen; y++)
                {
                    Color gridcolor = Colors.Azure;
                    ICanvasBrush cellColor;
                    if (vm.universe[x, y]) cellColor = LiveCell(sender);
                    else cellColor = DeadCell(sender);
                    args.DrawingSession.FillRectangle(
                        new Rect(
                            new Point(x * vm.CellSize, y * vm.CellSize),
                            new Size(vm.CellSize, vm.CellSize)
                            ),
                        cellColor);
                    if (ShowGridToggle.IsChecked)
                        args.DrawingSession.DrawRectangle(
                            new Rect(
                                new Point(x * vm.CellSize, y * vm.CellSize),
                                new Size(vm.CellSize, vm.CellSize)
                                ),
                            gridcolor,
                            1.0f);
                    if (ShowNeighborsToggle.IsChecked)
                        args.DrawingSession.DrawText(
                            neighbors[x, y].ToString(),
                            x * vm.CellSize, y * vm.CellSize,
                            Colors.White);
                }
            }
            StatusBar.Text = vm.StatusBarText;
        }
        /// <summary>
        /// Pointer pressed event for capturing the pointer location and changing a cell's value.
        /// </summary>
        /// <param name="e">The event which holds a snapshot of the pointer state and its current location at the moment this was triggered</param>
        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Point p = e.GetCurrentPoint(canvas).Position;
            vm.universe.ClickCell(
                (int)(p.X / vm.CellSize),
                (int)(p.Y / vm.CellSize)
                );
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