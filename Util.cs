using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utilities
{

    public static class Cacher
    {
        public static string GetPath(Type type, string Context)
        {
            return  "cach//"+Context+ " " + type.FullName;
        }
        public static void Cach(object ObjectToCach, string Context)
        {
            Util.SaveToFile(ObjectToCach, GetPath(ObjectToCach.GetType(), Context));
        }
        static public T TryLoadCach<T>(string Context)
        {
            string path=GetPath(typeof(T), Context);
            var erg = (T)Util.LoadFromFile(path);
            Console.WriteLine("Loaded from Cach:" + path);
            return erg;
        }
    }

    public class DuplicateKeyComparer<TKey>
                :
             IComparer<TKey> where TKey : IComparable
    {
        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1;   // Handle equality as beeing greater
            else
                return result;
        }

        #endregion
    }

    public static class RandomExtensions
    {
        public static int vary(this Random rnd, ref int Value, int MaxVariation)
        {
            Value = Value + rnd.Next((2 * MaxVariation) + 1) - MaxVariation;
            return Value;
        }

        public static double vary(this Random rnd, ref double Value, double MaxVariation)
        {
            Value = Value + (rnd.NextDouble() * 2 * MaxVariation) - MaxVariation;
            return Value;
        }

        public static int vary(this Random rnd, int Value, int MaxVariation)
        {
            return Value + rnd.Next((2 * MaxVariation) + 1) - MaxVariation;
        }

        public static double vary(this Random rnd, double Value, double MaxVariation)
        {
            return Value + (rnd.NextDouble() * 2 * MaxVariation) - MaxVariation;
        }

        public static T GetRandomElement<T>(this List<T> Liste, Random rnd)
        {
            return Liste[rnd.Next(Liste.Count)];
        }
    }
    public static class TimeSpanExtension
    {
        /// <summary>
        /// Multiplies a timespan by an integer value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, int multiplier)
        {
            return TimeSpan.FromTicks(multiplicand.Ticks * multiplier);
        }

        /// <summary>
        /// Multiplies a timespan by a double value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, double multiplier)
        {
            return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
        }

        /// <summary>
        /// divides a timespan by an integer value
        /// </summary>
        public static TimeSpan Divide(this TimeSpan multiplicand, int div)
        {
            return TimeSpan.FromTicks(multiplicand.Ticks / div);
        }


    }
    /// <span class="code-SummaryComment"><summary></span>
    /// Provides a method for performing a deep copy of an object.
    /// Binary Serialization is used to perform the copy.
    /// <span class="code-SummaryComment"></summary></span>
    public static class ObjectCopier
    {


        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
    public static class Util
    {
        public static void WriteInColor(string text, ConsoleColor CC)
        {
            var Color = Console.ForegroundColor;
            Console.ForegroundColor = CC;
            Console.Write(text);
            Console.ForegroundColor = Color;
        }
        public static void WriteRedN(string text)
        {
            WriteInColor(text, ConsoleColor.Red);
            Console.WriteLine();
        }
        public static void WriteGreenN(string text)
        {
            WriteInColor(text, ConsoleColor.Green);
            Console.WriteLine();
        }
        public static List<object> ToList(params object[] objects)
        {
            var list = new List<object>();
            list.AddRange(objects);
            return list;
        }

        public static IEnumerable<T> Shrink<T>(this IEnumerable<T> source, int left, int right)
        {
            int i = 0;
            var buffer = new Queue<T>(right + 1);

            foreach (T x in source)
            {
                if (i >= left) // Read past left many elements at the start
                {
                    buffer.Enqueue(x);
                    if (buffer.Count > right) // Build a buffer to drop right many elements at the end
                        yield return buffer.Dequeue();
                }
                else i++;
            }
        }
        public static IEnumerable<T> WithoutLast<T>(this IEnumerable<T> source, int n = 1)
        {
            return source.Shrink(0, n);
        }
        public static IEnumerable<T> WithoutFirst<T>(this IEnumerable<T> source, int n = 1)
        {
            return source.Shrink(n, 0);
        }



        public static IEnumerable<IEnumerable<T>>
  Section<T>(this IEnumerable<T> source, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length");

            var section = new List<T>(length);

            foreach (var item in source)
            {
                section.Add(item);

                if (section.Count == length)
                {
                    yield return section.AsReadOnly();
                    section = new List<T>(length);
                }
            }

            if (section.Count > 0)
                yield return section.AsReadOnly();
        }


        public static bool InteruptionOnConsole()
        {
            if (Console.KeyAvailable)
            {
                
                Console.WriteLine("Press Enter");
                Console.ReadLine();
                return true;
            }
            return false;
        }

        static private Random rnd = new Random();
        public static Random GetGoodRandomMT()
        {
            lock (rnd)
            {
                return new Random(rnd.Next());
            }
        }

        public static void SaveToFile(object Ding, string file)
        {

            Console.WriteLine(file);
            IFormatter binFmt = new BinaryFormatter();
            Stream s = File.Open(file, FileMode.Create);
            binFmt.Serialize(s, Ding);
            s.Close();
        }
        public static void SaveToFileWithDialog(object Ding)
        {
            SaveToFile(Ding, GetSaveFilePath());
        }
        public static object LoadFromFile(string file)
        {
            IFormatter binFmt = new BinaryFormatter();
            Stream s = File.Open(file, FileMode.Open);
            object result = null;
            try
            {
                result = binFmt.Deserialize(s);
            }
            catch (Exception e)
            {
                s.Close();
                throw e;
            }
            s.Close();
            return result;
        }
        public static object LoadFromFileWithDialog()
        {
            return LoadFromFile(GetLoadFilePath());
        }

        public static void SaveToFileAddWithDialog(object Ding)
        {
            SaveToFileAdd(GetSaveFilePath(), Ding);
        }

        public static void SaveToFileAdd(string p, object Ding)
        {
            FileStream fs = new FileStream(p, FileMode.Append);
            IFormatter binFmt = new BinaryFormatter();

            binFmt.Serialize(fs, Ding);
            fs.Close();
        }

        public static IEnumerable<object> OpenFromFileAddWithDialog()
        {
            return OpenFromFileAdd(GetLoadFilePath());
        }
        public static IEnumerable<object> OpenFromFileAdd(string p)
        {
            using (var fs = File.Open(p, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IFormatter binFmt = new BinaryFormatter();
                while (fs.Position < fs.Length)
                {
                    if (fs.Position > fs.Length)
                    {
                        fs.Close();
                    }
                    yield return binFmt.Deserialize(fs);
                }
            }
        }

        public static string GetLoadFilePath()
        {
            System.Windows.Forms.OpenFileDialog OFD = new System.Windows.Forms.OpenFileDialog();
            OFD.ShowDialog();
            return OFD.FileName;
        }
        public static string GetSaveFilePath()
        {
            System.Windows.Forms.SaveFileDialog OFD = new System.Windows.Forms.SaveFileDialog();
            OFD.ShowDialog();
            return OFD.FileName;
        }
    }
}
