using System.Runtime.InteropServices;

namespace NAPS2.Platform.Linux;

internal static class LinuxInterop
{
    [DllImport("libdl.so.2")]
    public static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libdl.so.2")]
    public static extern string dlerror();

    [DllImport("libdl.so.2")]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);

    [DllImport("libc.so.6")]
    public static extern int setenv(string name, string value, int overwrite);

    [DllImport("libc.so.6")]
    public static extern int readlink(string path, byte[] buffer, int bufferSize);
    
    [DllImport("libc.so.6")]
    public static extern int symlink(string targetPath, string linkPath);
}