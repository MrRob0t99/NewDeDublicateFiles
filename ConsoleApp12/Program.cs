using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp12
{
    class Program
    {
        private static readonly HashSet<FileInfo> _files = new HashSet<FileInfo>();
        private static readonly StringBuilder _builder = new StringBuilder();
        private static File[] _list;
        private static TimeSpan time;

        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            Console.WriteLine("Enter the path to the directory");
            var directory = args != null && args.Length >= 1 ? args[0] : Console.ReadLine();
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Directory not found");
            }
            else
            {
                stopwatch.Start();
                var directoryInfo = new DirectoryInfo(directory);
                Console.WriteLine("Take all files " + stopwatch.Elapsed);
                try
                {
                    GetAllFileFromDirectory(directoryInfo);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Press any key");
                    Console.ReadKey();
                    Environment.Exit(0);
                }               
                Console.WriteLine("All files " + _files.Count);
                var sorted = _files
                    .AsParallel()
                    .Where(f => _files
                    .Where(fi => f.Length == fi.Length)
                    .Skip(1)
                    .Any())
                    .Select(x => new { x.FullName })
                    .ToList();
                
                Console.WriteLine("Filtered files " + stopwatch.Elapsed);
                Console.WriteLine("Filtered files " + sorted.Count);
                _list = new File[sorted.Count];
                Parallel.For(0, sorted.Count, i =>
                {
                    _list[i] = new File()
                    {
                        FileInfo = sorted[i].FullName,
                        Hash = GetCode(sorted[i].FullName)
                    };
                });

                time = stopwatch.Elapsed;
                var group = _list.AsParallel().GroupBy(x => x.Hash);
                foreach (var groupItem in group)
                {
                    if (groupItem.Select(p => p).AsParallel().Skip(1).Any())
                    {
                        _builder.AppendLine("Same files:");
                        foreach (var item in groupItem)
                        {
                            _builder.AppendLine(item.FileInfo);
                        }
                    }
                }
            }
            Console.WriteLine(_builder);
            stopwatch.Stop();
            Console.WriteLine("Time without output " + time);
            Console.WriteLine("Time all " + stopwatch.Elapsed);
            Console.ReadKey();
        }
        static string GetCode(string first)
        {
            return BitConverter.ToString(System.IO.File.ReadAllBytes(first));
        }

        static void GetAllFileFromDirectory(DirectoryInfo directoryInfo)
        {
            _files.UnionWith(directoryInfo.GetFiles().AsParallel());
            if (directoryInfo.GetDirectories().Any())
                foreach (var dir in directoryInfo.GetDirectories())
                {
                    GetAllFileFromDirectory(dir);
                }
        }

    }

    class File
    {
        public string Hash { get; set; }
        public string FileInfo { get; set; }
    }

}
