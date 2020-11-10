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
            const string envVarName = "BENCODEX_BENCHMARKS_DATA_DIR";
            string dirPath =
                Environment.GetEnvironmentVariable(envVarName) ?? ".";
            Console.Error.WriteLine("Look up Bencodex benchmark data files in {0}...", dirPath);
            Console.Error.WriteLine("(You can configure the directory to look up by setting {0}.)",
                                    envVarName);
            string[] files;
            if (Directory.Exists(dirPath))
            {
                files = Directory.EnumerateFiles(dirPath, "*.dat", SearchOption.AllDirectories)
                    .Select(Path.GetFullPath)
                    .ToArray();
            }
            else
            {
                files = new string[] { Path.GetFullPath(dirPath) };
            }

            int count = 0;
            long size = 0;
            foreach (string file in files)
            {
                var fileInfo = new FileInfo(file);
                size += fileInfo.Length;
                Console.Error.WriteLine("{0}", file);
                count++;
            }

            ByteSize byteSize = ByteSize.FromBytes(size);
            Console.Error.WriteLine(
                "Loading {0} files ({1} = {2} bytes)...", count, byteSize, size);
            DataFiles = files;

            BenchmarkSwitcher.FromAssembly(typeof(CodecBenchmark).Assembly).Run(args);
        }
    }
}
