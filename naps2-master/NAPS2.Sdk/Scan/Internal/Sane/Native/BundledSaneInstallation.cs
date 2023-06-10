using NAPS2.Unmanaged;

namespace NAPS2.Scan.Internal.Sane.Native;

internal class BundledSaneInstallation : ISaneInstallation
{
    private string? _libraryPath;
    private string[]? _libraryDeps;

    public bool CanStreamDevices => true;

    public void Initialize()
    {
        var testRoot = Environment.GetEnvironmentVariable("NAPS2_TEST_DEPS");
        _libraryPath = NativeLibrary.FindLibraryPath(PlatformCompat.System.SaneLibraryName, testRoot);
        _libraryDeps = PlatformCompat.System.SaneLibraryDeps
            ?.Select(path => NativeLibrary.FindLibraryPath(path, testRoot)).ToArray();
        if (_libraryDeps != null)
        {
            // If we're using a bundled SANE, we will need to manually set the environment
            // variables to the appropriate folders.
            var backendsFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(_libraryPath)!, "sane"));
            var configFolder =
                Path.GetFullPath(Path.Combine(Path.GetDirectoryName(_libraryPath)!, "..", "_config", "sane"));
            // We can't use Environment.SetEnvironmentVariable as that will just change the .NET
            // env and won't be visible to SANE. Instead we use setenv which is technically not
            // thread-safe but in practice should be fine here.
            PlatformCompat.System.SetEnv("LD_LIBRARY_PATH", backendsFolder);
            PlatformCompat.System.SetEnv("SANE_CONFIG_DIR", configFolder);
            // Note: We can add SANE debug variables here
            // PlatformCompat.System.SetEnv("SANE_DEBUG_DLL", "255");
        }
    }

    public string LibraryPath => _libraryPath ?? throw new InvalidOperationException();

    public string[]? LibraryDeps => _libraryDeps;
}