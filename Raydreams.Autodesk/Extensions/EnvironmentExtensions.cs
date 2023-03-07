using System;
using System.Runtime.InteropServices;

namespace Raydreams.Autodesk.Extensions
{
    /// <summary>Simple OS test extensions</summary>
    public static class EnvironmentExtensions
    {
        public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}

