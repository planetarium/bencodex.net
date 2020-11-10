using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Bencodex.Types;
using ByteSizeLib;

namespace Bencodex.Benchmarks
{
    public class CodecBenchmark
    {
        [ParamsSource(nameof(DataFiles))]
        public string DataFile { get; set; }

        public IValue Value { get; private set; } = new Null();

        public byte[] Bytes { get; private set; } = new byte[0];

        public Stream Stream { get; private set; } = Stream.Null;

        [GlobalSetup]
        public void LoadData()
        {
            using var f = File.OpenRead(DataFile);
            using var reader = new BinaryReader(f);
            Bytes = reader.ReadBytes((int)f.Length);
            Stream = new MemoryStream(Bytes);
            Value = Codec.Decode(Bytes);
        }

        [GlobalCleanup]
        public void CloseStream()
        {
            try
            {
                Stream?.Close();
            }
            catch (Exception)
            {
            }
        }

        [IterationSetup]
        public void ResetStream()
        {
            try
            {
                Stream?.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception)
            {
            }
        }

        [Benchmark]
        public void EncodeIntoBytes()
        {
            byte[] _ = Codec.Encode(Value);
        }

        [Benchmark]
        public void EncodeIntoStream()
        {
            Codec.Encode(Value, new MemoryStream());
        }

        [Benchmark]
        public void DecodeFromBytes()
        {
            IValue _ = Codec.Decode(Bytes);
        }

        [Benchmark]
        public void DecodeFromStream()
        {
            IValue _ = Codec.Decode(Stream);
        }

        public static Codec Codec = new Codec();

        public static string[] DataFiles { get; set; } = new string[0];

        public static void Main(string[] args)
        {
            const string dataDirVar = "BENCODEX_BENCHMARKS_DATA_DIR";
            const string simpleModeVar = "BENCODEX_BENCHMARKS_SIMPLE";
            const int simpleModeSample = 50;
            string dirPath =
                Environment.GetEnvironmentVariable(dataDirVar) ?? ".";
            Console.Error.WriteLine("Look up Bencodex benchmark data files in {0}...", dirPath);
            Console.Error.WriteLine("(You can configure the directory to look up by setting {0}.)",
                                    dataDirVar);

            bool simpleMode = new string[] { "1", "t", "true", "y", "yes", "on" }.Contains(
                (Environment.GetEnvironmentVariable(simpleModeVar) ?? "").ToLowerInvariant()
            );
            if (simpleMode)
            {
                Console.Error.WriteLine(
                    "Run benchmarks on the simple mode, which samples only the heaviest {0} files" +
                    "and also does not use BenchmarkDotNet...",
                    simpleModeSample
                );
                Console.Error.WriteLine("You can profile the benchmarks on the simple mode.");
            }
            else
            {
                Console.Error.WriteLine("Run benchmarks using BenchmarkDotNet...");
                Console.Error.WriteLine(
                    "There is the simple mode too, which can be turned on by setting {0}=true.",
                    simpleModeVar
                );
            }


            FileInfo[] files;
            if (Directory.Exists(dirPath))
            {
                files = Directory.EnumerateFiles(dirPath, "*.dat", SearchOption.AllDirectories)
                    .Select(p => new FileInfo(p))
                    .ToArray();
            }
            else
            {
                files = new[] { new FileInfo(dirPath) };
            }

            if (simpleMode)
            {
                files = files.OrderByDescending(f => f.Length).Take(simpleModeSample).ToArray();
            }

            int count = 0;
            long size = 0;
            foreach (FileInfo file in files)
            {
                size += file.Length;
                Console.Error.WriteLine("{0}", file);
                count++;
            }

            ByteSize byteSize = ByteSize.FromBytes(size);
            Console.Error.WriteLine(
                "Loading {0} files ({1} = {2} bytes)...", count, byteSize, size);

            if (simpleMode)
            {
                SimpleMode(files);
            }
            else
            {
                DataFiles = files.Select(f => f.FullName).ToArray();
                BenchmarkSwitcher.FromAssembly(typeof(CodecBenchmark).Assembly).Run(args);
            }
        }

        private static void SimpleMode(FileInfo[] files)
        {
            DateTimeOffset started = DateTimeOffset.UtcNow;
            (FileInfo, byte[])[] data = files.Select(f =>
            {
                byte[] bytes;
                using (var reader = new BinaryReader(f.OpenRead()))
                {
                    bytes = reader.ReadBytes((int)f.Length);
                }

                return (f, bytes);
            }).ToArray();
            DateTimeOffset loaded = DateTimeOffset.UtcNow;

            foreach ((FileInfo file, byte[] bytes) in data)
            {
                IValue decoded = Codec.Decode(bytes);
                byte[] _ = Codec.Encode(decoded);
            }

            DateTimeOffset ended = DateTimeOffset.UtcNow;
            Console.WriteLine("Loading\t{0}", loaded - started);
            Console.WriteLine("Codec\t{0}", ended - loaded);
            Console.WriteLine("Total\t{0}", ended - started);
        }
    }
}
