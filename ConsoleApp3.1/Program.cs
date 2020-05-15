using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp3._1
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"runtimeassemblies.json";
            string breakingChangesRefsInRemainingMissingFilesFile = @"breakingChangesRefsInRemainingMissingFiles.txt";
            string removedFilesPath = @"removedFiles.txt";
            string nonRemovedFilesPath = @"nonRemovedFiles.txt";
            string missingFilesRefsInV3CleanUpFile = "missingFilesRefsInV3CleanUpFile.txt";

            // Get-ChildItem -Filter *.dll | Select-Object Name,@{n='AssemblyVersion';e={[Reflection.AssemblyName]::GetAssemblyName($_.FullName).Version}} | Format-Table -AutoSize > v3SharedFrameworkRefs.txt
            // Get-ChildItem -Filter Microsoft.Azure.SignalR | Select-Object Name,@{n='AssemblyVersion';e={[Reflection.AssemblyName]::GetAssemblyName($_.FullName).Version}} | Format-Table -AutoSize 
            string sharedFrameworkAspNet313AssembliesPath = @"v3SharedFrameworkRefs.txt";
            string sharedFrameworkAspNet228AssembliesPath = @"v2SharedFrameworkRefs.txt";
            string sharedFrameworkNetCore313AssembliesPath = @"v313NetCoreRefs.txt";
            string sharedFrameworkNetCore228AssembliesPath = @"v228NetCoreRefs.txt";
            string v3SiteExtensionAssembliesPath = @"v3Refs.txt";
            string v2SiteExtensionAssembliesPath = @"v2Refs.txt";
            string breakingChangesInNonRemovedReferencesFilesPath = "breakingChangesInNonRemovedReferencesFiles.txt";

            string assembliesJson = File.ReadAllText(path);
            JObject assemblies = JObject.Parse(assembliesJson);

            var runtimeAssemblies = assemblies["runtimeAssemblies"]
                .ToObject<RuntimeAssembly[]>()
                .ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

            Dictionary<string, RuntimeAssembly> v3RuntimeReferences = GetAssemblyDictionary(v3SiteExtensionAssembliesPath);
            Dictionary<string, RuntimeAssembly> v2RuntimeReferences = GetAssemblyDictionary(v2SiteExtensionAssembliesPath);
            Dictionary<string, RuntimeAssembly> v228SharedFrameworkAspNetReferences = GetAssemblyDictionary(sharedFrameworkAspNet228AssembliesPath);
            Dictionary<string, RuntimeAssembly> v313SharedFrameworkAspNetReferences = GetAssemblyDictionary(sharedFrameworkAspNet313AssembliesPath);
            Dictionary<string, RuntimeAssembly> v228SharedFrameworkNetCoreReferences = GetAssemblyDictionary(sharedFrameworkNetCore228AssembliesPath);
            Dictionary<string, RuntimeAssembly> v313SharedFrameworkNetCoreReferences = GetAssemblyDictionary(sharedFrameworkNetCore313AssembliesPath);

            // Removed assemblies from shared framework: https://github.com/dotnet/aspnetcore/issues/3755
            // Find assemblies in C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\2.2.8 not in C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\3.1.3
            // Find assemblies in C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.2.8 not in C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.3
            var missingFilesAspNet = v228SharedFrameworkAspNetReferences.Where(f => !v313SharedFrameworkAspNetReferences.Keys.Contains(f.Key) && runtimeAssemblies.Keys.Contains(f.Key)).ToDictionary(m => m.Key, m => m.Value);
            var missingFilesNetCore = v228SharedFrameworkNetCoreReferences.Where(f => !v313SharedFrameworkNetCoreReferences.Keys.Contains(f.Key) && runtimeAssemblies.Keys.Contains(f.Key)).ToDictionary(m => m.Key, m => m.Value);
            var uniqueMissingFiles = GetUnionDict(v313SharedFrameworkAspNetReferences, v313SharedFrameworkNetCoreReferences, missingFilesAspNet, missingFilesNetCore);

            string missingFilesString = string.Join($"{Environment.NewLine}", uniqueMissingFiles.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(removedFilesPath, $"[{missingFilesString}]");

            // If a missing file is not found any where, clean up runtimeAssemblies.Json
            var missingFilesRefsInV3CleanUp = uniqueMissingFiles.Where(r =>
            {
                if (v3RuntimeReferences.Keys.Contains(r.Key) || v313SharedFrameworkAspNetReferences.Keys.Contains(r.Key) || v313SharedFrameworkNetCoreReferences.Keys.Contains(r.Key))
                {
                    return false;
                }
                return true;
            }).ToDictionary(g => g.Key, g => g.Value);

            string missingFilesRefsInV3String = string.Join($"{Environment.NewLine}", missingFilesRefsInV3CleanUp.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(missingFilesRefsInV3CleanUpFile, $"[{missingFilesRefsInV3String}]");

            var remainingMissingFiles = uniqueMissingFiles.Where(r => !missingFilesRefsInV3CleanUp.Keys.Contains(r.Key));
            var breakingChangesRefsInRemainingMissingFiles = remainingMissingFiles.Where(r =>
            {
                if (v3RuntimeReferences.TryGetValue(r.Key, out RuntimeAssembly outValue))
                {
                    return r.Value.MajorVersion != outValue.MajorVersion;
                }
                if (v313SharedFrameworkAspNetReferences.TryGetValue(r.Key, out RuntimeAssembly outValue2))
                {
                    return r.Value.MajorVersion != outValue2.MajorVersion;
                }
                if (v313SharedFrameworkNetCoreReferences.TryGetValue(r.Key, out RuntimeAssembly outValue3))
                {
                    return r.Value.MajorVersion != outValue3.MajorVersion;
                }
                return true;
            });

            string breakingChangesRefsInRemainingMissingFilesString = string.Join($"{Environment.NewLine}", breakingChangesRefsInRemainingMissingFiles.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(breakingChangesRefsInRemainingMissingFilesFile, $"[{breakingChangesRefsInRemainingMissingFilesString}]");



            /// Handle non missing files:
            // Removed assemblies from shared framework: https://github.com/dotnet/aspnetcore/issues/3755
            // Find assemblies in C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\2.2.8 not in C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\3.1.3
            // Find assemblies in C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.2.8 not in C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.3
            var nonRemovedFilesAspNet = v228SharedFrameworkAspNetReferences.Where(f => v313SharedFrameworkAspNetReferences.Keys.Contains(f.Key) && runtimeAssemblies.Keys.Contains(f.Key)).ToDictionary(m => m.Key, m => m.Value);
            var nonRemovedFilesNetCore = v228SharedFrameworkNetCoreReferences.Where(f => v313SharedFrameworkNetCoreReferences.Keys.Contains(f.Key) && runtimeAssemblies.Keys.Contains(f.Key)).ToDictionary(m => m.Key, m => m.Value);

            IEnumerable<KeyValuePair<string, RuntimeAssembly>> uniqueNonRemovedFiles = GetUnionDict(v313SharedFrameworkAspNetReferences, v313SharedFrameworkNetCoreReferences, nonRemovedFilesAspNet, nonRemovedFilesNetCore);

            string nonRemovedFilesString = string.Join($"{Environment.NewLine}", uniqueNonRemovedFiles.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(nonRemovedFilesPath, $"[{nonRemovedFilesString}]");

            // Likely breaking changes if shown in logs. Need to look at each api.
            var breakingChangesInNonRemovedReferences = uniqueNonRemovedFiles.Where(r =>
            {
                if (v228SharedFrameworkAspNetReferences.TryGetValue(r.Key, out RuntimeAssembly outValue1) && v313SharedFrameworkAspNetReferences.TryGetValue(r.Key, out RuntimeAssembly outValue2))
                {
                    if (outValue2.MajorVersion == outValue1.MajorVersion)
                    {
                        return false;
                    }
                }
                if (v228SharedFrameworkAspNetReferences.TryGetValue(r.Key, out RuntimeAssembly shoutValue1) && v3RuntimeReferences.TryGetValue(r.Key, out RuntimeAssembly addedoutValue2))
                {
                    if (addedoutValue2.MajorVersion == shoutValue1.MajorVersion)
                    {
                        return false;
                    }
                }
                if (v2RuntimeReferences.TryGetValue(r.Key, out RuntimeAssembly v2RefOutValue) && v3RuntimeReferences.TryGetValue(r.Key, out RuntimeAssembly v3RefOutValue))
                {
                    if (v3RefOutValue.MajorVersion == v2RefOutValue.MajorVersion)
                    {
                        return false;
                    }
                }
                if (v228SharedFrameworkNetCoreReferences.TryGetValue(r.Key, out RuntimeAssembly v2netCoreRefOutValue) && v313SharedFrameworkNetCoreReferences.TryGetValue(r.Key, out RuntimeAssembly v3NetCoreRefOutValue))
                {
                    if (v2netCoreRefOutValue.MajorVersion == v2netCoreRefOutValue.MajorVersion)
                    {
                        return false;
                    }
                }
                return true;
            });

            string breakingChangesInNonRemovedReferencesString = string.Join($"{Environment.NewLine}", breakingChangesInNonRemovedReferences.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(breakingChangesInNonRemovedReferencesFilesPath, $"[{breakingChangesInNonRemovedReferencesString}]");
        }

        private static IEnumerable<KeyValuePair<string, RuntimeAssembly>> GetUnionDict(Dictionary<string, RuntimeAssembly> v313SharedFrameworkAspNetReferences, Dictionary<string, RuntimeAssembly> v313SharedFrameworkNetCoreReferences, Dictionary<string, RuntimeAssembly> inputDict1, Dictionary<string, RuntimeAssembly> inputDict2)
        {
            var unioinFiles = new Dictionary<string, RuntimeAssembly>();
            foreach (var inputFile in inputDict1)
            {
                unioinFiles[inputFile.Key] = inputFile.Value;
            }
            foreach (var inputFile in inputDict2)
            {
                unioinFiles[inputFile.Key] = inputFile.Value;
            }
            var uniqueFiles = unioinFiles.Where(r =>
            {
                if (v313SharedFrameworkAspNetReferences.TryGetValue(r.Key, out RuntimeAssembly outValue2))
                {
                    return r.Value.MajorVersion != outValue2.MajorVersion;
                }
                if (v313SharedFrameworkNetCoreReferences.TryGetValue(r.Key, out RuntimeAssembly outValue3))
                {
                    return r.Value.MajorVersion != outValue3.MajorVersion;
                }
                return true;
            });
            return uniqueFiles;
        }

        private static Dictionary<string, RuntimeAssembly> GetAssemblyDictionary(string fileName)
        {
            var v3ReferencesInSiteExt = File.ReadAllLines(fileName);
            Dictionary<string, RuntimeAssembly> runtimeReferences = new Dictionary<string, RuntimeAssembly>();
            foreach (string assemblyLine in v3ReferencesInSiteExt)
            {
                string[] parsedAssembly = System.Text.RegularExpressions.Regex.Split(assemblyLine, @"\s+");
                if (parsedAssembly.Length == 2)
                {
                    if (string.IsNullOrWhiteSpace(parsedAssembly[1]))
                    {
                        continue;
                    }
                    RuntimeAssembly runtimeAssembly = new RuntimeAssembly();
                    runtimeAssembly.Name = parsedAssembly[0];
                    runtimeAssembly.MajorVersion = Version.Parse(parsedAssembly[1]).Major;
                    runtimeAssembly.FullVersion = Version.Parse(parsedAssembly[1]);
                    runtimeReferences.Add(runtimeAssembly.Name.Replace(".dll", ""), runtimeAssembly);
                }
            }

            return runtimeReferences;
        }
    }
}
