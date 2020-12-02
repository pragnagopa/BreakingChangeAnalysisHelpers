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

            var percentileResult = 1191 % 14;


            string[] linuxAppsWithBreakingChangesAppService = Directory.GetFiles(@"C:\pgopa\BreakingChangeAnalysis\PinningV2Linuxconsumption");
            var resultlinuxApps = CombineAndRemoveDuplicates(linuxAppsWithBreakingChangesAppService);
            File.WriteAllLines("PublicLinuxAppsWithBreakingChangesConsumption.csv", resultlinuxApps.Select(s => $"SubId={s.Value},AppId={s.Key}---end---"));

            //var shuffledcards = appsAndSubsWithoutStorageAccount.OrderBy(a => Guid.NewGuid()).ToList();
            //SplitFiles();


            //string[] linuxAppsWithBreakingChanges = Directory.GetFiles(@"C:\pgopa\BreakingChangeAnalysis\LinuxAppsWithBreakingChanges");
            //var resultlinuxApps = CombineAndRemoveDuplicates(linuxAppsWithBreakingChanges);
            //File.WriteAllLines("PublicLinuxAppsWithBreakingChanges.csv", resultlinuxApps.Select(s => $"{s.Value},{s.Key}"));

            //WriteAppNames(@"C:\pgopa\BreakingChangeAnalysis\TotalAtRiskApps09-28\PublicCloudBreakingApps.csv");

            //string[] filesWithPinnedApps = Directory.GetFiles(@"C:\Users\pgopa\Downloads\PinnedToV2Apps09-28\PinnedToV2Apps09-28");
            //var resultPinnedDictionary = CombineAndRemoveDuplicates(filesWithPinnedApps);
            //File.WriteAllLines("PublicCloudPinnedApps.csv", resultPinnedDictionary.Select(s => $"{s.Value},{s.Key}"));

            //string[] filesToCombine = Directory.GetFiles(@"C:\pgopa\BreakingChangeAnalysis\TotalAtRiskApps09-28");
            //var resultDictionary = CombineAndRemoveDuplicates(filesToCombine);
            //var breakingAppsNotPinned = resultDictionary.Where(k => !resultPinnedDictionary.ContainsKey(k.Key));
            //File.WriteAllLines("PublicCloudBreakingApps.csv", breakingAppsNotPinned.Select(s => $"{s.Value},{s.Key}"));

            //string dotNetAppsNoStorageFile = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\BlackForestDotnetAppsNoStorage.csv";
            //var dotNetAppsNoStorage = ToAppsAndSubsDict(File.ReadAllLines(dotNetAppsNoStorageFile));

            //string allDotNetAppsFile = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\BlackForestAllDotnetApps.csv";
            //Dictionary<string, string> allDotNetApps = ToAppsAndSubsDict(File.ReadAllLines(allDotNetAppsFile));

            //Dictionary<string, string> allDotNetAppsWithStorage = allDotNetApps.Where(a => !dotNetAppsNoStorage.Keys.Contains(a.Key)).ToDictionary(a => a.Key, a => a.Value);

            //string donetBreakingAppsFile = @"C:\pgopa\BreakingChangeAnalysis\DotnetAppsNotAtRisk\BlackForestDotnetAppsBreaking.csv";
            //Dictionary<string, string> dotNetAppsBreaking = ToAppsAndSubsDict(File.ReadAllLines(donetBreakingAppsFile));

            //Dictionary<string, string> allDotNetAppsNotBreaking = allDotNetAppsWithStorage.Where(a => !dotNetAppsBreaking.Keys.Contains(a.Key)).ToDictionary(a => a.Key, a => a.Value);

            //File.WriteAllLines("BlackForestAllDotNetNonBreakingApps.csv", allDotNetAppsNotBreaking.Select(s => $"{s.Value},{s.Key}"));

        }

        private static void SplitFiles(List<string> runCmds, string filePrefix)
        {
            int index = 0;
            int fileCounter = 0;
            List<string> cmds = new List<string>();
            Dictionary<int, List<string>> cmdFilesDict = new Dictionary<int, List<string>>();
            int totalFiles = runCmds.Count / 90;
            foreach (var cmd in runCmds)
            {
                fileCounter = index % (totalFiles + 1);

                if (cmdFilesDict.TryGetValue(fileCounter, out List<string> outList))
                {
                    outList.Add(cmd);
                }
                else
                {
                    cmdFilesDict.Add(fileCounter, new List<string>());
                    cmdFilesDict[fileCounter].Add(cmd);
                }

                index++;
            }

            foreach (var dividedList in cmdFilesDict)
            {
                File.WriteAllLines($"filePrefix{dividedList.Key}.txt", dividedList.Value);
            }
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

        public static void WriteAppNames(string filePath)
        {
            IEnumerable<string> allLines = new List<string>();
            allLines = allLines.Concat(File.ReadAllLines(filePath));
            Dictionary<string, string> result = ToAppsAndSubsDict(allLines);
            Random rand = new Random();
            result = result.OrderBy(x => rand.Next())
              .ToDictionary(item => item.Key, item => item.Value);
            File.WriteAllLines("PublicCloudBreakingAppNames.txt", result.Select(s => $"\"{s.Key}\","));
        }

        private static Dictionary<string, string> ToAppsAndSubsDict(IEnumerable<string> allLines)
        {
            return allLines.Select(line => line.Split(','))
                     .GroupBy(arr => arr[1])
                     .ToDictionary(g => g.Key, g => g.First()[0]);
        }
    }
}
