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
            string matchMinorOrLowerAssembliesPath = @"matchMinorOrLowerAssemblies.txt";
            string runtimeVersionAssembliesPath = @"runtimeVersionAssemblies.txt";
            string removedAssembliesFromSharedFrameworkPath = @"removedAssembliesFromSharedFramework.txt";
            string removedFilesPath = @"removedFiles.txt";
            string errorsFromNotRemovedFilesPath = @"errorsFromNotRemovedFiles.txt";
            string directReferencesFilesPath = @"directReferencesFilesPath.txt";
            string noDirectRemovedReferencesFilesPathV2 = @"noDirectRemovedReferencesFilesPathV2.txt";


            // Get-ChildItem -Filter *.dll | Select-Object Name,@{n='AssemblyVersion';e={[Reflection.AssemblyName]::GetAssemblyName($_.FullName).Version}} | Format-Table -AutoSize > v3SharedFrameworkRefs.txt
            // Get-ChildItem -Filter Microsoft.Azure.SignalR | Select-Object Name,@{n='AssemblyVersion';e={[Reflection.AssemblyName]::GetAssemblyName($_.FullName).Version}} | Format-Table -AutoSize 
            string sharedFrameworkAspNet313AssembliesPath = @"v3SharedFrameworkRefs.txt";
            string sharedFrameworkAspNet228AssembliesPath = @"v2SharedFrameworkRefs.txt";
            string sharedFrameworkNetCore313AssembliesPath = @"v313NetCoreRefs.txt";
            string sharedFrameworkNetCore228AssembliesPath = @"v228NetCoreRefs.txt";
            string v3SiteExtensionAssembliesPath = @"v3Refs.txt";
            string v2SiteExtensionAssembliesPath = @"v2Refs.txt";
            string breakingChangesInRemovedReferencesFilesPath = "breakingChangesInRemovedReferencesFiles.txt";
            string breakingChangesInNonRemovedReferencesFilesPath = "breakingChangesInNonRemovedReferencesFiles.txt";
            string removedReferencesFromSharedFxNoReferencesInV3FilePath = "removedReferencesFromSharedFxNoReferencesInV3FilePath.txt";

            // This text is added only once to the file.
            string assembliesJson = File.ReadAllText(path);
            JObject assemblies = JObject.Parse(assembliesJson);

            var runtimeAssemblies = assemblies["runtimeAssemblies"]
                .ToObject<RuntimeAssembly[]>()
                .ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

            var matchMinorOrLowerAssemblies = runtimeAssemblies.Where(r => r.Value.ResolutionPolicy.Equals("minorMatchOrLower"));
            string matchMinorOrLowerassembliesString = string.Join($"{Environment.NewLine}", matchMinorOrLowerAssemblies.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(matchMinorOrLowerAssembliesPath, $"[{matchMinorOrLowerassembliesString}]");

            var runtimeVersionAssemblies = runtimeAssemblies.Where(r => r.Value.ResolutionPolicy.Equals("runtimeVersion"));
            string runtimeVersionAssembliesString = string.Join($"{Environment.NewLine}", runtimeVersionAssemblies.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(runtimeVersionAssembliesPath, $"[{runtimeVersionAssembliesString}]");

            // Removed assemblies from shared framework: https://github.com/dotnet/aspnetcore/issues/3755
            // Find assemblies in C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\2.2.8 not in C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\3.1.3
            // Find assemblies in C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.2.8 not in C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.3
            string sharedFramework2Path = @"C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\2.2.8";
            string sharedFramework3path = @"C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\3.1.3";
            string dotnetCore228AppPath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.2.8";
            string dotnetCore313AppPath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.3";
            var sharedFramework2PathFiles = Directory.GetFiles(sharedFramework2Path, "*.dll").Select(p => Path.GetFileName(p));
            var sharedFramework3PathFiles = Directory.GetFiles(sharedFramework3path, "*.dll").Select(p => Path.GetFileName(p));
            var dotnetCore228AppFiles = Directory.GetFiles(dotnetCore228AppPath, "*.dll").Select(p => Path.GetFileNameWithoutExtension(p).ToLower());
            var dotnetCore313AppFiles = Directory.GetFiles(dotnetCore313AppPath, "*.dll").Select(p => Path.GetFileNameWithoutExtension(p).ToLower());

            var missingFilesAspNet = sharedFramework2PathFiles.Where(f => !sharedFramework3PathFiles.Contains(Path.GetFileName(f))).Select(v => v.ToLower());
            var missingFilesNetCore = dotnetCore228AppFiles.Where(f => !dotnetCore313AppFiles.Contains(Path.GetFileName(f))).Select(v => v.ToLower());

            var missingFiles = missingFilesAspNet.Union(missingFilesNetCore);
            string missingFilesString = string.Join($"{Environment.NewLine}", missingFiles.Select(r => $"\"{r.ToLower()}\","));
            File.WriteAllText(removedAssembliesFromSharedFrameworkPath, $"[{missingFilesString}]");

            // Assemblies in unified list that are removed. These errors can be ignored from kusto if there is a direct reference in runtime
            // Get the list for assemblies in runtimeAssemblies.json that are removed from shared framework
            // Exclude assemblies from C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.2.8
            var runtimeAssembliesUnifiedRemoved = runtimeAssemblies.Where(r => missingFilesString.Contains(r.Value.Name.ToLower(), StringComparison.OrdinalIgnoreCase));
            runtimeAssembliesUnifiedRemoved = runtimeAssembliesUnifiedRemoved.Where(r => !dotnetCore228AppFiles.Contains(r.Value.Name.ToLower()));

            string removedFiles = string.Join($"{Environment.NewLine}", runtimeAssembliesUnifiedRemoved.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(removedFilesPath, $"[{removedFiles}]");

            // Extract assemblies we care errors about as these assemblies could have breaking changes betwee 2.2 and 3.1
            var runtimeAssembliesUnifiedNotRemoved = runtimeAssemblies.Where(r => !removedFiles.Contains(r.Value.Name.ToLower(), StringComparison.OrdinalIgnoreCase));
            string errorsFromNotRemovedFiles = string.Join($"{Environment.NewLine}", runtimeAssembliesUnifiedNotRemoved.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(errorsFromNotRemovedFilesPath, $"[{errorsFromNotRemovedFiles}]");


            //List assemblies with direct reference in runtime that are listed in run assemblies json
            string runtimeDirectReferencesSiteExtensionPath = @"C:\Users\pgopa\Downloads\2.0.12961\32bit";
            var runtimeDirectReferencesSiteExtensionFiles = Directory.GetFiles(runtimeDirectReferencesSiteExtensionPath, "*.dll").Select(p => Path.GetFileNameWithoutExtension(p).ToLower());

            var runtimeReferencesInAssemblyJson = runtimeAssemblies.Where(r => runtimeDirectReferencesSiteExtensionFiles.Contains(r.Value.Name.ToLower()));
            string runtimeReferencesInAssemblyJsonString = string.Join($"{Environment.NewLine}", runtimeReferencesInAssemblyJson.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(directReferencesFilesPath, $"[{runtimeReferencesInAssemblyJsonString}]");


            //List assemblies removed and not direct reference in runtime that are listed in run assemblies json
            var runtimeRemovedReferencesInAssemblyJson = runtimeAssembliesUnifiedRemoved.Where(r => !runtimeDirectReferencesSiteExtensionFiles.Contains(r.Value.Name.ToLower()));
            string runtimeRemovedReferencesInAssemblyJsonString = string.Join($"{Environment.NewLine}", runtimeRemovedReferencesInAssemblyJson.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(noDirectRemovedReferencesFilesPathV2, $"[{runtimeRemovedReferencesInAssemblyJsonString}]");


            Dictionary<string, RuntimeAssembly> v3RuntimeReferences = GetAssemblyDictionary(v3SiteExtensionAssembliesPath);
            Dictionary<string, RuntimeAssembly> v2RuntimeReferences = GetAssemblyDictionary(v2SiteExtensionAssembliesPath);
            Dictionary<string, RuntimeAssembly> v2SharedFrameworkReferences = GetAssemblyDictionary(sharedFrameworkAspNet228AssembliesPath);
            Dictionary<string, RuntimeAssembly> v3SharedFrameworkReferences = GetAssemblyDictionary(sharedFrameworkAspNet313AssembliesPath);
            Dictionary<string, RuntimeAssembly> v228SharedFrameworkNetCoreReferences = GetAssemblyDictionary(sharedFrameworkNetCore228AssembliesPath);
            Dictionary<string, RuntimeAssembly> v313SharedFrameworkNetCoreReferences = GetAssemblyDictionary(sharedFrameworkNetCore313AssembliesPath);

            // References available in V3 that are removed from shared framework 2.2.8
            var v3AddedRuntimeReferencesNotInShared313 = v3RuntimeReferences.Where(r => runtimeRemovedReferencesInAssemblyJsonString.Contains(r.Key, StringComparison.OrdinalIgnoreCase)).ToDictionary( g => g.Key, g => g.Value);


            // references listed in unification list, removed from shared fx 3.1.3 and references not added in V3 runtime
            var removedReferencesFromSharedFxNoReferencesInV3 = runtimeAssembliesUnifiedRemoved.Where(r => !v3RuntimeReferences.Keys.Contains(r.Key)  && !v3SharedFrameworkReferences.Keys.Contains(r.Key) && !v313SharedFrameworkNetCoreReferences.Keys.Contains(r.Key));
            string removedReferencesFromSharedFxNoReferencesInV3String = string.Join($"{Environment.NewLine}", removedReferencesFromSharedFxNoReferencesInV3.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(removedReferencesFromSharedFxNoReferencesInV3FilePath, $"[{removedReferencesFromSharedFxNoReferencesInV3String}]");


            // find references that differ in major version between aspnet core app 2.2.8 and v3
            var mismatchedMajorVersions = v3AddedRuntimeReferencesNotInShared313.Where(r =>
            {
                if (v2SharedFrameworkReferences.TryGetValue(r.Key, out RuntimeAssembly outValue))
                {
                    return r.Value.MajorVersion != outValue.MajorVersion;
                }
                return false;
            });

            // Likely breaking changes if shown in logs. Need to look at each api.
            var breakingChangesInRemovedReferences = runtimeRemovedReferencesInAssemblyJson.Where(r =>
            {
                if (v2SharedFrameworkReferences.TryGetValue(r.Key, out RuntimeAssembly outValue1) && v3AddedRuntimeReferencesNotInShared313.TryGetValue(r.Key, out RuntimeAssembly outValue2))
                {
                    if (outValue2.MajorVersion == outValue1.MajorVersion) return false;
                }
                return true;
            });

            string breakingChangesInRemovedReferencesString = string.Join($"{Environment.NewLine}", breakingChangesInRemovedReferences.Select(r => $"\"{r.Value.Name.ToLower()}\","));
            File.WriteAllText(breakingChangesInRemovedReferencesFilesPath, $"[{breakingChangesInRemovedReferencesString}]");

            // Likely breaking changes if shown in logs. Need to look at each api.
            var breakingChangesInNonRemovedReferences = runtimeAssembliesUnifiedNotRemoved.Where(r =>
            {
                if (v2SharedFrameworkReferences.TryGetValue(r.Key, out RuntimeAssembly outValue1) && v3SharedFrameworkReferences.TryGetValue(r.Key, out RuntimeAssembly outValue2))
                {
                    if (outValue2.MajorVersion == outValue1.MajorVersion)
                    {
                        return false;
                    }
                }
                if (v2SharedFrameworkReferences.TryGetValue(r.Key, out RuntimeAssembly shoutValue1) && v3RuntimeReferences.TryGetValue(r.Key, out RuntimeAssembly addedoutValue2))
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
