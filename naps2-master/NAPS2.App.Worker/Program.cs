﻿using NAPS2.EntryPoints;

namespace NAPS2.Worker;

static class Program
{
    /// <summary>
    /// The NAPS2.Worker.exe main method.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Use reflection to avoid antivirus false positives (yes, really)
        typeof(WindowsWorkerEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
    }
}