using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameOfLife_UWP
{
    /// <summary>
    /// This class is a wrapper that allows for certain data bindings to work well with UWP/XAML
    /// I included fields which I would like to load from file, and a factory method that ensures
    /// those settings are loaded from file if it exists.
    /// </summary>
    [Serializable]
    public class ViewModel
    {
        public Color LiveCell;
        public Color DeadCell;
        public Color GridColor;
        private static double _CellSize = 48.0;
        public bool CurrentGenShown;
        public bool TotalGensShown;
        public bool LivingCellsShown;
        public bool GridShown;
        public bool NeighborsShown;
        public double Zoom;
        public double GridWidth { get; set; }
        public double GridHeight { get; set; }
        /// <summary>
        /// Name of the universe
        /// </summary>
        public string uName;
        /// <summary>
        /// Description of the universe
        /// </summary>
        public string uDescription;
        /// <summary>
        /// Speed of ticker execution
        /// </summary>
        public int Speed;
        /// <summary>
        /// File path for saving without prompting
        /// </summary>
        public string FilePath;
        public int CellSize { get => (int)(_CellSize * Zoom); }
        public string StatusBarText { 
            get {
                StringBuilder sb = new StringBuilder();
                if (CurrentGenShown)
                {
                    sb.Append("Current: "); sb.Append(universe.Current + 1);
                }
                if (TotalGensShown)
                {
                    if (CurrentGenShown) sb.Append(" | ");
                    sb.Append("Total: "); sb.Append(universe.TotalGenerations);
                }
                if (LivingCellsShown)
                {
                    if (TotalGensShown || CurrentGenShown) sb.Append(" | ");
                    sb.Append("Living: "); sb.Append(universe.TotalLiving);
                }
                return sb.ToString();
            } 
        }
        public Universe universe;
        
        public ViewModel()
        {
            LiveCell = Colors.Chartreuse;
            DeadCell = Colors.Black;
            GridColor = Colors.Azure;
            Zoom = 1.0;
            universe = new Universe(20, 20, true);
            GridWidth = 960;
            GridHeight = 960;
            uName = "New Universe";
            uDescription = "New Universe";
            CurrentGenShown = true;
            TotalGensShown = true;
            LivingCellsShown = true;
            GridShown = false;
            Speed = 100000;
            FilePath = "";
        }
        private static void GetColorFromStream(JsonObject jo, ref Color c)
        {
            c.R = (byte)jo.GetNamedNumber("r");
            c.G = (byte)jo.GetNamedNumber("g");
            c.B = (byte)jo.GetNamedNumber("b");
        }
        public static async Task<ViewModel> Factory()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file;
            ViewModel v = new();
            try
            {
                file = await folder.GetFileAsync("settings.json");
            } catch (Exception)
            {
                return new ViewModel();
            }
            if (file == null) return new ViewModel();

            try
            {
                JsonObject jo = JsonObject.Parse(await FileIO.ReadTextAsync(file));
                GetColorFromStream(jo.GetNamedObject("liveCell"), ref v.LiveCell);
                GetColorFromStream(jo.GetNamedObject("deadCell"), ref v.DeadCell);
                GetColorFromStream(jo.GetNamedObject("gridColor"), ref v.GridColor);

                v.CurrentGenShown = jo.GetNamedBoolean("currentGenShown");
                v.TotalGensShown = jo.GetNamedBoolean("totalGensShown");
                v.LivingCellsShown = jo.GetNamedBoolean("livingCellsShown");
                v.GridShown = jo.GetNamedBoolean("gridShown");
                v.NeighborsShown = jo.GetNamedBoolean("neighborsShown");

                v.Zoom = jo.GetNamedNumber("zoom");
                v.GridWidth = jo.GetNamedNumber("gridWidth");
                v.GridHeight = jo.GetNamedNumber("gridHeight");

                v.uName = jo.GetNamedString("uName");
                v.uDescription = jo.GetNamedString("uDescription");

                v.Speed = (int)jo.GetNamedNumber("speed");

            } catch (Exception)
            {
                await file.DeleteAsync();
            }
            return v;
        }

        private JsonObject AddColorToBuffer(ref Color c)
        {
            JsonObject obj = new();
            obj.Add("r", JsonValue.CreateNumberValue(c.R));
            obj.Add("g", JsonValue.CreateNumberValue(c.G));
            obj.Add("b", JsonValue.CreateNumberValue(c.B));
            return obj;
        }
        public async Task<int> SaveToFile()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file;
            file = await folder.CreateFileAsync("settings.json", CreationCollisionOption.ReplaceExisting);
            if (file == null) return 1;
            CachedFileManager.DeferUpdates(file);

            JsonObject jo = new();
            jo.Add("liveCell", AddColorToBuffer(ref LiveCell));
            jo.Add("deadCell", AddColorToBuffer(ref DeadCell));
            jo.Add("gridColor", AddColorToBuffer(ref GridColor));

            jo.Add("currentGenShown", JsonValue.CreateBooleanValue(CurrentGenShown));
            jo.Add("totalGensShown", JsonValue.CreateBooleanValue(TotalGensShown));
            jo.Add("livingCellsShown", JsonValue.CreateBooleanValue(LivingCellsShown));
            jo.Add("gridShown", JsonValue.CreateBooleanValue(GridShown));
            jo.Add("neighborsShown", JsonValue.CreateBooleanValue(NeighborsShown));

            jo.Add("zoom", JsonValue.CreateNumberValue(Zoom));
            jo.Add("gridWidth", JsonValue.CreateNumberValue(GridWidth));
            jo.Add("gridHeight", JsonValue.CreateNumberValue(GridHeight));

            jo.Add("uName", JsonValue.CreateStringValue(uName));
            jo.Add("uDescription", JsonValue.CreateStringValue(uDescription));

            jo.Add("speed", JsonValue.CreateNumberValue(Speed));

            await FileIO.WriteTextAsync(file, jo.Stringify());

            if (FilePath == "")
            {
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync("tmp.cells", CreationCollisionOption.OpenIfExists);
            } else
            {
                file = await StorageFile.GetFileFromPathAsync(FilePath);
            }
            if (file == null) return 1;
            await universe.SaveToPlainText(file, uName, uDescription);
            return 0;
        }
    }
}
