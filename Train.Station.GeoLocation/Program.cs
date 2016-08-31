using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Train.Station.GeoLocation
{
    class Program
    {
        private static string DataPath = @"C:\Users\Lachlan\Documents\visual studio 2015\Projects\Solution1\ClassLibrary1";
        private static Dictionary<string, FileInfo> files; 
        static void Main(string[] args)
        {
            files = new Dictionary<string, FileInfo>();
            foreach (var directory in Directory.GetDirectories(DataPath))
            {
                var val = 0;
                if(!int.TryParse(new DirectoryInfo(directory).Name, out val )) continue;
                Console.WriteLine("Adding Data From Directory: " + directory);
                var tranistFiles = Directory.GetFiles(Path.Combine(directory, "google_transit"));
                foreach (var file in tranistFiles.Select(x=> new FileInfo(x)))
                {
                    if(file.Extension != ".txt") continue;
                    Console.WriteLine("     Adding Data From File: " + file.Name);
                    if (!files.ContainsKey(file.Name))
                    {
                        var path = Path.Combine(DataPath, file.Name);
                        File.WriteAllText(path, File.ReadAllLines(file.FullName)[0]);
                        files.Add(file.Name, new FileInfo(path));
                    }

                    var lines = File.ReadAllLines(file.FullName);
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(File.ReadAllText(files[file.Name].FullName));

                    for (int line = 1; line < lines.Length; line++) //skip first line
                    {
                        stringBuilder.AppendLine(lines[line]);
                    }
                    File.WriteAllText(files[file.Name].FullName, stringBuilder.ToString());
                }
            }
        }
    }
}
