using System.Text;
using System.Threading;

namespace NAPS2.Tools;

public static class Cli
{
    public static void Run(string command, string args, Dictionary<string, string>? env = null,
        CancellationToken cancel = default, bool noVerbose = false, bool alwaysVerbose = false,
        string? ignoreErrorIfOutputContains = null, string? workingDir = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDir ?? Paths.SolutionRoot
        };
        if (env != null)
        {
            foreach (var kvp in env)
            {
                startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
        }
        Output.Verbose($"{command} {args}");
        var proc = Process.Start(startInfo);
        if (proc == null)
        {
            throw new Exception($"Could not start {command}");
        }

        void ConsoleCancel(object? sender, EventArgs e)
        {
            proc.Kill();
        }

        cancel.Register(proc.Kill);
        Console.CancelKeyPress += ConsoleCancel;
        try
        {
            var savedOutput = new StringBuilder();

            void Save(object sender, DataReceivedEventArgs e)
            {
                savedOutput.AppendLine(e.Data);
            }

            bool print = (Output.EnableVerbose && !noVerbose) || alwaysVerbose;
            proc.OutputDataReceived += print ? Print : Save;
            proc.ErrorDataReceived += print ? Print : Save;

            bool ignoreError = false;
            if (ignoreErrorIfOutputContains != null)
            {
                proc.OutputDataReceived += (_, e) =>
                    ignoreError |= e.Data?.Contains(ignoreErrorIfOutputContains) ?? false;
                proc.ErrorDataReceived += (_, e) =>
                    ignoreError |= e.Data?.Contains(ignoreErrorIfOutputContains) ?? false;
            }

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            while (!proc.WaitForExit(100))
            {
            }
            if (proc.ExitCode != 0 && !cancel.IsCancellationRequested && !ignoreError)
            {
                if (!print)
                {
                    Console.Write(savedOutput);
                }
                throw new Exception($"Command failed: {command} {args}");
            }
        }
        finally
        {
            Console.CancelKeyPress -= ConsoleCancel;
        }
    }

    private static void Print(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
            Console.WriteLine(e.Data);
        }
    }
}