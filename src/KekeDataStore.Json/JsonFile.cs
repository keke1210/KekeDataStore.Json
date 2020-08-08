using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace KekeDataStore.Json
{
    internal class JsonFile<T>
    {
        public JsonFile(string fileName)
        {
            FileName = fileName;

            Validate(FileName);

            FileDirectory = Path.GetTempPath();
            
            FilePath = Path.Combine(FileDirectory, FileName);
        }

        public JsonFile(string baseDirectory, string fileName)
        {
            FileName = fileName;

            Validate(FileName);

            FileDirectory = baseDirectory;

            Validate(FileDirectory);

            FilePath = Path.Combine(FileDirectory, FileName);
        }

        public string FileName { get; }
        public string FileDirectory { get; }
        public string FilePath { get; }
        

        // https://stackoverflow.com/questions/32943899/can-i-decompress-and-deserialize-a-file-using-streams/32944462#32944462

        // Buffer sized as recommended by Bradley Grainger, https://faithlife.codes/blog/2012/06/always-wrap-gzipstream-with-bufferedstream/
        // Do not use a buffer larger than 85,000 bytes since objects larger than that go on the large object heap.  See:
        // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap
        // Renting a larger array would also be an option, see https://docs.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1.rent?view=netcore-3.1
        private const int BufferSize = 8192;

        /// <summary>
        /// Writes to the File
        /// </summary>
        /// <param name="data"></param>
        public void WriteToFile(Dictionary<string, T> data)
        {
            Stopwatch sw = null;

            while (true)
            {
                try
                {
                    var settings = new JsonSerializerSettings();
                    SerializeToFileCompressed(data, FilePath, settings);
                    break;
                }
                catch (IOException e) when (e.Message.Contains("because it is being used by another process"))
                {
                    // If some other process is using this file, retry operation unless elapsed times is greater than 10 seconds
                    sw = sw ?? Stopwatch.StartNew();
                    if (sw.ElapsedMilliseconds > 10000)
                        throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Reads from File
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, T> LoadFromFile()
        {
            Stopwatch sw = null;

            while (true)
            {
                try
                {
                    return DeserializeFromFileCompressed(FilePath);
                }
                catch (FileNotFoundException)
                {
                    var data = new Dictionary<string, T>();
                    SerializeToFileCompressed(data, FilePath, null);
                    return data;
                }
                catch (IOException e) when (e.Message.Contains("because it is being used by another process"))
                {
                    // If some other process is using this file, retry operation unless elapsed times is greater than 10 seconds
                    sw = sw ?? Stopwatch.StartNew();
                    if (sw.ElapsedMilliseconds > 10000)
                        throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private static void SerializeToFileCompressed(Dictionary<string, T> value, string path, JsonSerializerSettings settings)
        {
            using (var fs = Stream.Synchronized(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read)))
                SerializeCompressed(value, fs, settings);
        }

        private static void SerializeCompressed(object value, Stream stream, JsonSerializerSettings settings)
        {
            using (var compressor = new GZipStream(stream, CompressionMode.Compress))
            using (var writer = new StreamWriter(compressor, Encoding.UTF8, BufferSize))
            {
                var serializer = JsonSerializer.CreateDefault(settings);
                serializer.Serialize(writer, value);
            }
        }

        private static Dictionary<string, T> DeserializeFromFileCompressed(string path, JsonSerializerSettings settings = null)
        {
            using (var fs = Stream.Synchronized(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
                return DeserializeCompressed(fs, settings);
        }

        private static Dictionary<string, T> DeserializeCompressed(Stream stream, JsonSerializerSettings settings = null)
        {
            using (var compressor = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(compressor))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var serializer = JsonSerializer.CreateDefault(settings);
                return serializer.Deserialize<Dictionary<string, T>>(jsonReader);
            }
        }

        /// <summary>
        /// Checks if filename is valid
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>void</returns>
        private static void Validate(string filename)
        {
            if (filename.IsEmpty())
                throw new ArgumentNullException(nameof(filename));

            if (filename.IndexOfAny(Path.GetInvalidPathChars()) > 0)
                throw new ArgumentException($"{filename} contains invalid characters.");
        }
    }
}
