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
        public string DataFile { get; set; } = string.Empty;

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
            const string decodeOnlyVar = "BENCODEX_BENCHMARKS_DECODE_ONLY";
            const int simpleModeSample = 50;

            bool ToBool(string? v) =>
                new[] {"1", "t", "true", "y", "yes", "on"}.Contains(v?.ToLowerInvariant());

            string dirPath =
                Environment.GetEnvironmentVariable(dataDirVar) ?? ".";
            Console.Error.WriteLine("Look up Bencodex benchmark data files in {0}...", dirPath);
            Console.Error.WriteLine("(You can configure the directory to look up by setting {0}.)",
                                    dataDirVar);

            bool simpleMode = ToBool(Environment.GetEnvironmentVariable(simpleModeVar));
            bool decodeOnly = false;
            if (simpleMode)
            {
                Console.Error.WriteLine(
                    "Run benchmarks on the simple mode, which samples only the heaviest {0} files" +
                    "and also does not use BenchmarkDotNet...",
                    simpleModeSample
                );
                Console.Error.WriteLine("You can profile the benchmarks on the simple mode.");

                decodeOnly = ToBool(Environment.GetEnvironmentVariable(decodeOnlyVar));
                if (decodeOnly)
                {
                    Console.Error.WriteLine("Benchmark only decoding...");
                }
                else
                {
                    Console.Error.WriteLine(
                        "If you want to benchmark only decoding, configure {0}=true.",
                        decodeOnlyVar
                    );
                }
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
                SimpleMode(files, decodeOnly);
            }
            else
            {
                DataFiles = files.Select(f => f.FullName).ToArray();
                BenchmarkSwitcher.FromAssembly(typeof(CodecBenchmark).Assembly).Run(args);
            }
        }

        private static void SimpleMode(FileInfo[] files, bool decodeOnly)
        {
            DateTimeOffset started = DateTimeOffset.UtcNow;
            (FileInfo, MemoryStream)[] data = files.Select(f =>
            {
                byte[] bytes;
                using (var reader = new BinaryReader(f.OpenRead()))
                {
                    bytes = reader.ReadBytes((int)f.Length);
                }

                return (f, new MemoryStream(bytes));
            }).ToArray();
            DateTimeOffset loaded = DateTimeOffset.UtcNow;
            Console.Error.WriteLine("Loaded {0} files.", data.Length);
            DateTimeOffset ended;

            try
            {
                foreach ((FileInfo file, MemoryStream stream) in data)
                {
                    IValue decoded = Codec.Decode(stream);
                    if (!decodeOnly)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        Codec.Encode(decoded, stream);
                    }
                }

                ended = DateTimeOffset.UtcNow;
            }
            finally
            {
                foreach ((FileInfo _, MemoryStream stream) in data)
                {
                    stream.Close();
                }
            }

            Console.WriteLine("Loading\t{0}", loaded - started);
            Console.WriteLine(
                "{0}\t{1}",
                decodeOnly ? "Decoding" : "Codec",
                ended - loaded
            );
            Console.WriteLine("Total\t{0}", ended - started);
        }

        private enum SimpleModeFilter
        {
            Both,
            Encoding,
            Decoding,
        }
    }
}
