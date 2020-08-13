using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AppsAndSubs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string file1 = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\AllDotnetAppsNoStoagePast10days.csv";
            string file2 = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\AllDotnetAppsNoStoagePast20-10days.csv";
            string file3 = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\AllDotnetAppsNoStoagePast30-20days.csv";
            var dotNetAppsNoStorage = CombineAndRemoveDuplicates(new string[] { file1, file2, file3 });

            string allDotNetAppsFile = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\AllDotnetAppsPast30days.csv";
            Dictionary<string, string> allDotNetApps = ToAppsAndSubsDict(File.ReadAllLines(allDotNetAppsFile));

            Dictionary<string, string> allDotNetAppsWithStorage = allDotNetApps.Where(a => !dotNetAppsNoStorage.Keys.Contains(a.Key)).ToDictionary(a => a.Key, a => a.Value);

            string donetBreakingAppsFile1 = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\DotNetAppsWithBreakingChangespast5days.csv";
            string donetBreakingAppsFile2 = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\DotNetAppsWithBreakingChangespast10To5days.csv";
            string donetBreakingAppsFile3 = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\DotNetAppsWithBreakingChangespast15To10days.csv";
            string donetBreakingAppsFile4 = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\DotNetAppsWithBreakingChangespast20To15days.csv";
            string donetBreakingAppsFile5 = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\DotNetAppsWithBreakingChangespast25To20days.csv";
            string donetBreakingAppsFile6 = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\DotNetAppsWithBreakingChangespast30To25days.csv";

            Dictionary<string, string> dotNetAppsBreaking = CombineAndRemoveDuplicates(new string[] { donetBreakingAppsFile1, donetBreakingAppsFile2, donetBreakingAppsFile3, donetBreakingAppsFile4, donetBreakingAppsFile5, donetBreakingAppsFile6 });

            Dictionary<string, string> allDotNetAppsNotBreaking = allDotNetAppsWithStorage.Where(a => !dotNetAppsBreaking.Keys.Contains(a.Key)).ToDictionary(a => a.Key, a => a.Value);

            File.WriteAllLines("AllDotNetNonBreakingApps.csv", allDotNetAppsNotBreaking.Select(s => $"{s.Value},{s.Key}"));

        }

        public static Dictionary<string, string> CombineAndRemoveDuplicates(string[] files)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            IEnumerable<string> allLines = new List<string>();
            foreach (string file in files)
            {
                allLines = allLines.Concat(File.ReadAllLines(file));
            }
            result = ToAppsAndSubsDict(allLines);
            return result;
        }

        private static Dictionary<string, string> ToAppsAndSubsDict(IEnumerable<string> allLines)
        {
            return allLines.Select(line => line.Split(','))
                     .GroupBy(arr => arr[1])
                     .ToDictionary(g => g.Key, g => g.First()[0]);
        }
    }
}
