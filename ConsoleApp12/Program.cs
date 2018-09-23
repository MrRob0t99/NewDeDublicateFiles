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
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            Console.WriteLine("Enter the path to the directory");
            var directory = Console.ReadLine();
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Directory not found");
            }
            else
            {
                stopwatch.Start();
                var directoryInfo = new DirectoryInfo(directory);
                GetAllFileFromDirectory(directoryInfo);
                Console.WriteLine(_files.Count);

                var sorted = _files
                    .AsParallel()
                    .Where(f => _files
                    .Where(fi => f.Length == fi.Length)
                    .Skip(1)
                    .Any())
                    .Select(x => new { x.FullName })
                    .ToList();

                Console.WriteLine(sorted.Count);
                Console.WriteLine("Take directory " + stopwatch.Elapsed);
                _list = new File[sorted.Count];
                var taskList = new List<Task>();

                Parallel.For(0, sorted.Count, i =>
                {
                    _list[i] = new File()
                    {
                        FileInfo = sorted[i].FullName,
                        Hash = IsEqualFiles(sorted[i].FullName)
                    };
                });

                Task.WaitAll(taskList.ToArray());
                var group = _list.AsParallel().GroupBy(x => x.Hash);
                foreach (var groupItem in group)
                {
                    if (groupItem.Select(p => p).Skip(1).Any())
                    {
                        _builder.AppendLine("Same files");
                        foreach (var item in groupItem)
                        {
                            _builder.AppendLine(item.FileInfo);
                        }
                    }
                }
            }
            Console.WriteLine(_builder);
            stopwatch.Stop();
            Console.WriteLine("Time " + stopwatch.Elapsed);
            Console.ReadKey();
        }
        static string IsEqualFiles(string first)
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
