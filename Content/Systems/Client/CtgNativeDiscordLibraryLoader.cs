using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria.ModLoader;

namespace CTG2.Content.Systems.Client;

public static class CtgNativeDiscordLibraryLoader
{
    private static bool _registered;

    public static void Register(Mod mod)
    {
        if (_registered)
        {
            mod.Logger.Info("[NativeLoader] Register() called but already registered, skipping.");
            return;
        }
        _registered = true;

        var asm = Assembly.GetExecutingAssembly();
        mod.Logger.Info($"[NativeLoader] Registering DllImportResolver for assembly: {asm.FullName}");

        NativeLibrary.SetDllImportResolver(asm, (libraryName, assembly, searchPath) =>
        {
            mod.Logger.Info($"[NativeLoader] Resolver invoked for libraryName='{libraryName}', assembly='{assembly.GetName().Name}'");

            var nativeLibraryPathInMod = $"lib/Native/{libraryName}";
            var exists = mod.FileExists(nativeLibraryPathInMod);
            mod.Logger.Info($"[NativeLoader] mod.FileExists('{nativeLibraryPathInMod}') = {exists}");

            if (!exists)
                return IntPtr.Zero;

            var nativeLibraryPathOnDisk = Path.GetTempFileName();
            using (var modStream = mod.GetFileStream(nativeLibraryPathInMod))
            using (var diskStream = File.Open(nativeLibraryPathOnDisk, FileMode.Create, FileAccess.Write))
                modStream.CopyTo(diskStream);

            mod.Logger.Info($"[NativeLoader] Extracted '{libraryName}' to '{nativeLibraryPathOnDisk}'");

            try
            {
                var handle = NativeLibrary.Load(nativeLibraryPathOnDisk);
                mod.Logger.Info($"[NativeLoader] NativeLibrary.Load succeeded, handle={handle}");
                return handle;
            }
            catch (Exception e)
            {
                mod.Logger.Error($"[NativeLoader] NativeLibrary.Load failed for '{nativeLibraryPathOnDisk}'", e);
                return IntPtr.Zero;
            }
        });

        mod.Logger.Info("[NativeLoader] DllImportResolver registered successfully.");
    }
}
