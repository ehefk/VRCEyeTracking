﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;

namespace VRCEyeTracking
{
    public static class DependencyManager
    {
        // Because SRanipal.dll needs to be loaded last.. Too lazy to automate moving it to back of load queue
        private static readonly List<string> RequiredToLoad = new List<string>
        {
            "VRCEyeTracking.SRanipal.libHTC_License.dll",
            "VRCEyeTracking.SRanipal.nanomsg.dll",
            "VRCEyeTracking.SRanipal.SRWorks_Log.dll",
            "VRCEyeTracking.SRanipal.ViveSR_Client.dll",
            "VRCEyeTracking.SRanipal.SRanipal.dll"
        };
        
        public static void Init()
        {
            var dllPaths = ExtractDLLs(RequiredToLoad.ToArray());
            foreach (var path in dllPaths)
                LoadDLL(path);
        }

        private static List<string> ExtractDLLs(string[] resourceNames)
        {
            var extractedPaths = new List<string>();

            var dirName = Path.Combine(Path.GetTempPath(), "VRCEyeTracking");
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            foreach (var dll in resourceNames)
            {
                var dllPath = Path.Combine(dirName, GetDLLNameFromPath(dll));

                using (var stm = Assembly.GetExecutingAssembly().GetManifestResourceStream(dll))
                {
                    try
                    {
                        using (Stream outFile = File.Create(dllPath))
                        {
                            const int sz = 4096;
                            var buf = new byte[sz];
                            while (true)
                            {
                                if (stm == null) continue;
                                var nRead = stm.Read(buf, 0, sz);
                                if (nRead < 1)
                                    break;
                                outFile.Write(buf, 0, nRead);
                            }
                        }

                        extractedPaths.Add(dllPath);
                    }
                    catch
                    {
                        MelonLogger.Error($"Failed to get DLL: ");
                    }
                }
            }
            return extractedPaths; 
        }

        private static string GetDLLNameFromPath(string path)
        {
            var splitPath = path.Split('.').ToList();
            splitPath.Reverse();
            return splitPath[1]+".dll";
        }
        
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        private static void LoadDLL(string path)
        {
            IntPtr h = LoadLibrary(path);
            if (h == IntPtr.Zero)
                MelonLogger.Msg("Unable to load library " + path);
        }
    }
}