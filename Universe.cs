using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace GameOfLife_UWP
{
    /// <summary>
    /// Wrapper class for the universe to encapsulate functionality and implement safety mechanisms
    /// </summary>
    [Serializable]
    public class Universe
    {

        /// <summary>
        /// Delegate matching the signature of the other Count neighbor methods, following the same format as a function pointer (no lambda expressions in C#)
        /// </summary>
        /// <param name="x">X coordinate on the grid</param>
        /// <param name="y">Y coordinate on the grid</param>
        /// <returns></returns>
        private delegate byte CountNeighbors(int x, int y);
        #region Properties and Fields
        /// <summary>
        /// Property which tracks the neighbor count for each cell.
        /// </summary>
        public byte[,] neighborCount 
        { 
            get
            {
                byte[,] cnt = new byte[XLen, YLen];
                for(int y = 0; y < YLen; y++)
                {
                    for (int x = 0; x < XLen; x++)
                    {
                        // safe cast because there is no chance there are more than 6 neighbors
                        cnt[x, y] = (byte)cn(x, y);
                    }
                }
                return cnt;
            }
        }
        /// <summary>
        /// Convenience method for acquiring the max X axis
        /// </summary>
        public int XLen { 
            get { 
                return deltaT[0].GetLength(0); 
            }
            set
            {
                if (value <= 0) return;
                bool[,] nu = new bool[value, YLen];
                for (int x = 0; x < Math.Min(value, XLen); x++)
                {
                    for (int y = 0; y < YLen; y++)
                    {
                        nu[x, y] = deltaT.Last()[x, y];
                    }
                }
                deltaT = new();
                deltaT.Add(nu);
            }
        }
        /// <summary>
        /// Convenience method for acquiring the max Y axis
        /// </summary>
        public int YLen { 
            get { 
                return deltaT[0].GetLength(1); 
            } 
            set
            {
                if (value <= 0) return;
                bool[,] nu = new bool[XLen, value];
                for (int x = 0; x < XLen; x++)
                {
                    for (int y = 0; y < Math.Min(YLen, value); y++)
                    {
                        nu[x, y] = deltaT.Last()[x, y];
                    }
                }
                deltaT = new();
                deltaT.Add(nu);
            }
        }
        /// <summary>
        /// List of Diff maps. The list's Count also represents the total generations.
        /// </summary>
        private List<bool[,]> deltaT;
        /// <summary>
        /// public interface for acquiring the total number of generations
        /// </summary>
        public int TotalGenerations { get { return deltaT.Count; } }

        /// <summary>
        /// Current generation. The DiffMap at this index has been applied to the universe
        /// </summary>
        public int Current { get; private set; }
        /// <summary>
        /// Computed property that returns the number of living cells at this generation
        /// </summary>
        public int TotalLiving
        {
            get
            {
                int acc = 0;
                for (int y = 0; y < YLen; ++y)
                {
                    for (int x = 0; x < XLen; ++x)
                    {
                        if (deltaT[Current][x, y]) acc++;
                    }
                }
                return acc;
            }
        }

        public byte[,] neighbors
        {
            get
            {
                byte[,] n = new byte[XLen, YLen];
                for (int x = 0; x < XLen; x++)
                {
                    for (int y = 0; y < YLen; y++)
                    {
                        n[x, y] = cn(x, y);
                    }
                }
                return n;
            }
        }

        private bool isToroidal;
        CountNeighbors cn { 
            get
            {
                if (IsToroidal) return CountNeighborsToroidal;
                else return CountNeighborsFinite;
            } 
        }
        /// <summary>
        /// Property which describes the neighbor counting algorithm for this universe (externally immutable)
        /// </summary>
        public bool IsToroidal
        {
            get { return isToroidal; }
            private set { isToroidal = value; }
        }
        #endregion
        #region Constructors
        public Universe()
        {
            deltaT = new List<bool[,]>();
            deltaT.Add(new bool[50, 50]);
            IsToroidal = true;
        }
        /// <summary>
        /// Shorthand constructor for building a square grid
        /// </summary>
        /// <param name="size">Dimension of the grid</param>
        /// <param name="isToroidal">Does the grid wrap around?</param>
        public Universe(int size, bool isToroidal = true)
        {
            deltaT = new List<bool[,]>();
            deltaT.Add(new bool[size, size]);
            this.IsToroidal = isToroidal;
        }
        /// <summary>
        /// Constructor for a rectangular universe
        /// </summary>
        /// <param name="x">X axis</param>
        /// <param name="y">Y axis</param>
        /// <param name="isToroidal"></param>
        public Universe(int x, int y, bool isToroidal = true)
        {
            deltaT = new List<bool[,]>();
            deltaT.Add(new bool[x, y]);
            this.IsToroidal = isToroidal;
        }
        /// <summary>
        /// Indexer for safe access to array
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>State of cell at (x, y)</returns>
        #endregion
        #region Utility Methods
        /// <summary>
        /// This indexer allows safe access to the underlying boolean array
        /// </summary>
        public bool this[int x, int y] { 
            get { 
                return deltaT[Current][x, y]; 
            } set { 
                if (isToroidal)
                {
                    // Allow imports to toroidal universe to wrap around grid
                    deltaT[Current][x % XLen, y % YLen] = value;
                } else
                {
                    // Otherwise Cull extraneous cells on import
                    if (x >= XLen || y >= YLen || x < 0 || y < 0) return;
                    deltaT[Current][x, y] = value;
                }
            }
        }

        /// <summary>
        /// Use diff map to traverse the universe over time
        /// </summary>
        /// <param name="generation">the specific point in time to travel to</param>
        public void GoTo(int generation)
        {
            if (generation >= TotalGenerations)
            {
                CalculateNextGeneration();
                Current = TotalGenerations - 1;
                return;
            }
            Current = generation;
        }
        /// <summary>
        /// Clears the diff map
        /// </summary>
        public void ClearDiffMap()
        {
            int x = XLen; int y = YLen;
            deltaT = new List<bool[,]>();
            deltaT.Add(new bool[x, y]);
            Current = 0;
        }
        #endregion
        #region Rubric Compliant Methods
        /// <summary>
        /// If clicking a cell, toggle that cell then fast forward to last generation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ClickCell(int x, int y)
        {
            GoTo(TotalGenerations - 1);
            this[x, y] = !this[x, y];
        }
        /// <summary>
        /// Clears and generates a Random universe based on a seed
        /// </summary>
        /// <param name="seed">Optional: seed for the random generator (if null, uses time as seed)</param>
        public void Randomize(int? seed = null)
        {
            Random rng = new Random(seed ?? (int)DateTime.Now.Ticks);
            ClearDiffMap();

            for (int y = 0; y < YLen; y++)
            {
                for (int x = 0; x < XLen; x++)
                {
                    if (rng.Next(3) == 0) this[x,y] = true;
                }
            }
        }
        /// <summary>
        /// Count neighbors, allowing cells to fall off the edge of the grid
        /// </summary>
        /// <param name="x">X coordinate on the grid</param>
        /// <param name="y">Y coordinate on the grid</param>
        /// <returns></returns>
        private byte CountNeighborsFinite(int x, int y)
        {
            byte count = 0;
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;
                    if (
                        xOffset == 0 && yOffset == 0 ||
                        xCheck < 0 ||
                        yCheck < 0 ||
                        xCheck >= XLen ||
                        yCheck >= YLen
                        ) continue;
                    count += (byte)(deltaT[Current][xCheck, yCheck] ? 1 : 0);
                }
            }
            return count;
        }
        /// <summary>
        /// Count neighbors, wrapping over the edge of the universe
        /// </summary>
        /// <param name="x">X coordinate on the grid</param>
        /// <param name="y">Y coordinate on the grid</param>
        /// <returns>Number of neighbors</returns>
        private byte CountNeighborsToroidal(int x, int y)
        {
            byte count = 0;
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;
                    if (xOffset == 0 && yOffset == 0) continue;
                    if (xCheck < 0) xCheck = XLen - 1;
                    if (yCheck < 0) yCheck = YLen - 1;
                    if (xCheck >= XLen) xCheck = 0;
                    if (yCheck >= YLen) yCheck = 0;
                    count += (byte)(deltaT[Current][xCheck, yCheck] ? 1 : 0);
                }
            }
            return count;
        }
        /// <summary>
        /// Count neighbors, taking whether the universe is toroidal or not into account.
        /// </summary>
        public void CalculateNextGeneration()
        {
            Current = TotalGenerations - 1;
            deltaT.Add(new bool[XLen, YLen]);
            deltaT[Current].CopyTo(deltaT.Last());
            for (int y = 0; y < YLen; y++)
            {
                for (int x = 0; x < XLen; x++)
                {
                    byte neighbors = cn(x, y);
                    if (deltaT[Current][x, y] && (neighbors < 2 || neighbors > 3)) deltaT.Last()[x, y] = false;
                    if (!deltaT[Current][x, y] && neighbors == 3) deltaT.Last()[x,y] = true;
                }
            }
            Current++;
        }

        public void ResizeUniverse(int? x, int? y)
        {
            if (!x.HasValue && !y.HasValue) return;
            int nx = x ?? XLen;
            int ny = y ?? YLen;
            bool[,] nu = new bool[nx, ny];
            for (int ix = 0; ix < Math.Min(nx, XLen); ix++)
            {
                for (int iy = 0; iy < Math.Min(ny, YLen); iy++)
                {
                    nu[ix, iy] = deltaT[Current][ix, iy];
                }
            }
            deltaT.Clear();
            deltaT.Add(nu);
        }
        public async Task<bool> SaveToPlainText(StorageFile file, string name, string desc)
        {
            if (file == null) return false;
            CachedFileManager.DeferUpdates(file);
            StringBuilder sb = new StringBuilder();
            sb.Append("!Name: " + name + "\n!");
            foreach (string str in desc.Split('\n'))
            {
                sb.Append(str + ' ');
            }
            sb.Append("\n");
            for (int y = 0; y < YLen; y++)
            {
                for (int x = 0; x < XLen; x++)
                {
                    if (this[x, y]) sb.Append('O');
                    else sb.Append('.');
                }
                sb.Append('\n');
            }
            await FileIO.WriteTextAsync(file, sb.ToString());
            await CachedFileManager.CompleteUpdatesAsync(file);
            return true;
        }
        public async Task<bool> SaveToPlainText(FileSavePicker picker, string name, string desc)
        {
            StorageFile file = await picker.PickSaveFileAsync();
            return await SaveToPlainText(file, name, desc);
        }
        public Tuple<string, string> LoadFromFile(List<string> lines, bool isImport)
        {
            string name = "";
            string desc = "";
            while (true)
            {
                if (lines.Count == 0) return null;
                if (lines[0].StartsWith("!Name: "))
                {
                    name = lines[0].Substring(7);
                    lines.RemoveAt(0);
                }
                else if (lines[0].StartsWith("!"))
                {
                    desc += lines[0].Substring(1) + " ";
                    lines.RemoveAt(0);
                }
                else break;
            }
            int inx = lines[0].Length;
            int iny = lines.Count;

            if (!isImport)
            {
                deltaT = new List<bool[,]>();
                deltaT.Add(new bool[inx, iny]);
                Current = 0;
            }

            for (int y = 0; y < iny; y++)
            {
                for (int x = 0; x < inx; x++)
                {
                    try
                    {
                        if (lines[y][x] == 'O') this[x, y] = true;
                        else this[x, y] = false;
                    } catch (Exception) { }
                }
            }
            return new Tuple<string, string>(name, desc);
        }
        #endregion
        #region Rubric Uncompliant Methods
        /// <summary>
        /// Serialize entire universe to file
        /// </summary>
        /// <param name="named">File path (automatically converts to .xml)</param>
        /// <param name="includeDiff">Whether or not to include diff map</param>
        public void SerializeToFile(string named, bool includeDiff)
        {
            BinaryFormatter s = new BinaryFormatter();
            if (!includeDiff)
            {
                StringBuilder sb = new StringBuilder();
                for (int y = 0; y < YLen; y++)
                {
                    for (int x = 0; x < XLen; x++)
                    {
                        //sb.Append(universe[x, y] ? 'O' : '.');
                    }
                    if (y != YLen - 1) sb.Append('\n');
                }
                // open, write, then immediately close file so it's not open for an unreasonable amount of time
                StreamWriter writer = new StreamWriter(Path.ChangeExtension(named, ".cell"));
                writer.Write(sb.ToString());
                writer.Close();
            }
            else
            {
                // open, write, close
                StreamWriter writer = new StreamWriter(Path.ChangeExtension(named, ".rgol"));
                s.Serialize(writer.BaseStream, this);
                writer.Close();
            }
        }

        /// <summary>
        /// Factory Method for Deserializing the universe
        /// </summary>
        /// <param name="fileName">name of the file (automatically transforms path into .xml)</param>
        /// <returns>The Universe that was on file</returns>
        public static Universe Deserialize(string fileName)
        {
            XmlSerializer s = new XmlSerializer(typeof(Universe));
            FileStream reader = new FileStream(Path.ChangeExtension(fileName, ".xml"), FileMode.Open);
            Universe u = (Universe)s.Deserialize(reader);
            reader.Close();
            return u;
        }
        #endregion
    }
    #region Utility Classes
    public static class Utilities
    {
        public static void CopyTo(this bool[,] from, bool[,] to)
        {
            for (int x = 0; x < from.GetLength(0); x++)
            {
                for (int y = 0; y < from.GetLength(1); y++)
                {
                    to[x, y] = from[x, y];
                }
            }
        }
    }
    /// <summary>
    /// Rather than storing each generation in their own array so you can "rewind" your game, create a diff map which stores changes to cells
    /// over time and group them by generation, thereby saving memory at the cost of slightly higher CPU cycles.
    /// </summary>
    public class DiffMapInstruction : IEquatable<DiffMapInstruction>
    {
        private int x;
        /// <summary>
        /// X coordinate of the cell at this particular diff
        /// </summary>
        public int X { get { return x; } private set { x = value; } }

        private int y;
        /// <summary>
        /// Y coordinate of the cell at this particular diff
        /// </summary>
        public int Y { get { return y; } private set { y = value; } }

        private bool wasBirth;
        /// <summary>
        /// Was the cell born at this moment or did it die?
        /// </summary>
        public bool WasBirth { get { return wasBirth; } private set { wasBirth = value; } }


        public DiffMapInstruction(int x, int y, bool wasBirth) { X = x; Y = y; WasBirth = wasBirth; }

        public bool Equals(DiffMapInstruction i)
        {
            return i != null && X == i.X && Y == i.Y && WasBirth == i.WasBirth;
        }
        public override bool Equals(object o)
        {
            return this.Equals(o as DiffMapInstruction);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ WasBirth.GetHashCode();
        }
    }

    /// <summary>
    /// This is the map of difference instructions. When rewinding or advancing this map executes every instruction ensuring no partial differences occur.
    /// </summary>
    public class DiffMap
    {
        /// <summary>
        /// List of instructions
        /// </summary>
        private HashSet<DiffMapInstruction> instructions = new HashSet<DiffMapInstruction>();

        public IEnumerable<DiffMapInstruction> iterator { get { return instructions.AsEnumerable(); } }

        public void ToggleInstruction(int x, int y)
        {
            DiffMapInstruction birth = new DiffMapInstruction(x, y, true);
            DiffMapInstruction death = new DiffMapInstruction(x, y, false);
            if (instructions.Contains(birth))
            {
                instructions.Remove(birth);
                instructions.Add(death);
            }
            else if (instructions.Contains(death))
            {
                instructions.Remove(death);
                instructions.Add(birth);
            }
            else
            {
                instructions.Add(birth);
            }
        }

        /// <summary>
        /// Add a new instruction to the diff map. As you can see, only a commit method is added so that no accidental removal of instructions can occur.
        /// </summary>
        /// <param name="x">X coordinate of the change</param>
        /// <param name="y">Y coordinate of the change</param>
        /// <param name="wasBirth">Did the cell become alive or did it die?</param>
        public void Commit(int x, int y, bool wasBirth) { instructions.Add(new DiffMapInstruction(x, y, wasBirth)); }
    }
    #endregion
}
