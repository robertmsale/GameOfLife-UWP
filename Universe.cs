using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameOfLife_UWP
{
    /// <summary>
    /// Wrapper class for the universe to encapsulate functionality and implement safety mechanisms
    /// </summary>
    public class Universe
    {
        /// <summary>
        /// 2D Array of cells. I'd like to note that it took every fiber of my being to not make this an array of bitsets to save on horizontal memory.
        /// </summary>
        private bool[,] universe;
        /// <summary>
        /// Convenience method for acquiring the max X axis
        /// </summary>
        public int XLen { get { return universe.GetLength(0); } }
        /// <summary>
        /// Convenience method for acquiring the max Y axis
        /// </summary>
        public int YLen { get { return universe.GetLength(1); } }
        /// <summary>
        /// List of Diff maps. The list's Count also represents the total generations.
        /// </summary>
        private List<DiffMap> deltaT;
        /// <summary>
        /// public interface for acquiring the total number of generations
        /// </summary>
        public int TotalGenerations { get { return deltaT.Count; } }

        private int current;
        /// <summary>
        /// Current generation. The DiffMap at this index has been applied to the universe
        /// </summary>
        public int Current { get { return current; } private set { current = value; } }
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
                        if (universe[x, y]) acc++;
                    }
                }
                return acc;
            }
        }

        private bool isToroidal;
        /// <summary>
        /// Property which describes the neighbor counting algorithm for this universe (externally immutable)
        /// </summary>
        public bool IsToroidal
        {
            get { return isToroidal; }
            private set { isToroidal = value; }
        }

        /// <summary>
        /// Shorthand constructor for building a square grid
        /// </summary>
        /// <param name="size">Dimension of the grid</param>
        /// <param name="isToroidal">Does the grid wrap around?</param>
        public Universe(int size, bool isToroidal = false)
        {
            universe = new bool[size, size];
            deltaT = new List<DiffMap>();
            deltaT.Add(new DiffMap());
            this.IsToroidal = isToroidal;
        }
        /// <summary>
        /// Constructor for a rectangular universe
        /// </summary>
        /// <param name="x">X axis</param>
        /// <param name="y">Y axis</param>
        /// <param name="isToroidal"></param>
        public Universe(int x, int y, bool isToroidal = false)
        {
            universe = new bool[x, y];
            deltaT = new List<DiffMap>();
            deltaT.Add(new DiffMap());
            this.IsToroidal = isToroidal;
        }
        /// <summary>
        /// Indexer for safe access to array
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>State of cell at (x, y)</returns>
        public bool this[int x, int y] { 
            get { 
                return universe[x, y]; 
            } set { 
                if (isToroidal)
                {
                    universe[x % (XLen - 1), y % (YLen - 1)] = value;
                } else
                {
                    if (x >= XLen || y >= YLen || x < 0 || y < 0) return;
                    universe[x, y] = value;
                }
            } 
        }
        public void ClickCell(int x, int y)
        {
            deltaT.Last().ToggleInstruction(x, y);
            GoTo(TotalGenerations - 1);
        }
        /// <summary>
        /// Clears the diff map
        /// </summary>
        public void ClearDiffMap() { deltaT = new List<DiffMap>(); deltaT.Add(new DiffMap()); Current = 0; }
        /// <summary>
        /// Clears and generates a Random universe based on a seed
        /// </summary>
        /// <param name="seed">Optional: seed for the random generator (if null, uses time as seed)</param>
        public void Randomize(int? seed = null)
        {
            Random rng = new Random(seed.HasValue ? seed.Value : (int)DateTime.Now.Ticks);
            ClearDiffMap();
            universe = new bool[XLen, YLen];

            for (int y = 0; y < YLen; y++)
            {
                for (int x = 0; x < XLen; x++)
                {
                    if (rng.Next(3) == 0) deltaT[Current].Commit(x, y, true);
                }
            }
            GoTo(0);
        }
        /// <summary>
        /// Use diff map to traverse the universe over time
        /// </summary>
        /// <param name="generation">the specific point in time to travel to</param>
        public void GoTo(int generation)
        {
            int direction = Current <= generation ? 1 : -1;
            if (generation < 0) generation = 0;
            if (generation >= TotalGenerations) generation = TotalGenerations - 1;
            while (Current != generation)
            {
                foreach (DiffMapInstruction i in deltaT[Current].iterator)
                {
                    bool setTo = direction == -1 ? !i.WasBirth : i.WasBirth;
                    universe[i.X, i.Y] = setTo;
                }
                Current += direction;
            }
            if (direction == -1) return;
            foreach (DiffMapInstruction i in deltaT[current].iterator)
            {
                universe[i.X, i.Y] = i.WasBirth;
            }
        }
        /// <summary>
        /// Count neighbors, allowing cells to fall off the edge of the grid
        /// </summary>
        /// <param name="x">X coordinate on the grid</param>
        /// <param name="y">Y coordinate on the grid</param>
        /// <returns></returns>
        private int CountNeighborsFinite(int x, int y)
        {
            int count = 0;
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
                    count += universe[xCheck, yCheck] ? 1 : 0;
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
        private int CountNeighborsToroidal(int x, int y)
        {
            int count = 0;
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
                    count += universe[xCheck, yCheck] ? 1 : 0;
                }
            }
            return count;
        }
        /// <summary>
        /// Delegate matching the signature of the other Count neighbor methods, following the same format as a function pointer (no lambda expressions in C#)
        /// </summary>
        /// <param name="x">X coordinate on the grid</param>
        /// <param name="y">Y coordinate on the grid</param>
        /// <returns></returns>
        private delegate int CountNeighbors(int x, int y);
        /// <summary>
        /// Count neighbors, taking whether the universe is toroidal or not into account.
        /// </summary>
        public void CalculateNextGeneration()
        {
            CountNeighbors cn;
            if (Current != TotalGenerations - 1) GoTo(TotalGenerations - 1);
            if (IsToroidal) cn = CountNeighborsToroidal;
            else cn = CountNeighborsFinite;
            deltaT.Add(new DiffMap());
            for (int y = 0; y < YLen; y++)
            {
                for (int x = 0; x < XLen; x++)
                {
                    int neighbors = cn(x, y);
                    if (universe[x, y] && (neighbors < 2 || neighbors > 3)) deltaT.Last().Commit(x, y, false);
                    if (!universe[x, y] && neighbors == 3) deltaT.Last().Commit(x, y, true);
                }
            }
            GoTo(TotalGenerations - 1);
        }
        public async void SaveToPlainText(Windows.Storage.Pickers.FileSavePicker picker, string name, string desc)
        {
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
                    if (universe[x, y]) sb.Append('O');
                    else sb.Append('.');
                }
                sb.Append('\n');
            }
            Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                await Windows.Storage.FileIO.WriteTextAsync(file, sb.ToString());
                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
            }
        }
        public async Task<Tuple<string, string>> LoadFromFile(Windows.Storage.Pickers.FileOpenPicker picker, bool isImport)
        {
            List<string> lines = new List<string>();
            string name = "";
            string desc = "";
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return null;
            Windows.Storage.CachedFileManager.DeferUpdates(file);
            foreach(string s in await Windows.Storage.FileIO.ReadLinesAsync(file))
            {
                lines.Add(s);
            }
            await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
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
                universe = new bool[inx, iny];
            }

            for (int y = 0; y < iny; y++)
            {
                for (int x = 0; x < inx; x++)
                {
                    try
                    {
                        if (lines[y][x] == 'O') universe[x, y] = true;
                        else universe[x, y] = false;
                    } catch (Exception) { }
                }
            }
            return new Tuple<string, string>(name, desc);
        }
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
                        sb.Append(universe[x, y] ? 'O' : '.');
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
}
