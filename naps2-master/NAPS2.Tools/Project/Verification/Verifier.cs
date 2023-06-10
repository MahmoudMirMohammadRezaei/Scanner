namespace NAPS2.Tools.Project.Verification;

public static class Verifier
{
    public static void RunVerificationTests(string testRoot)
    {
        Output.Info($"Running verification tests in: {testRoot}");
        Cli.Run("dotnet", "test -l \"console;verbosity=normal\" NAPS2.App.Tests", new()
        {
            { "NAPS2_TEST_DEPS", testRoot },
            { "NAPS2_TEST_ROOT", testRoot },
            { "NAPS2_TEST_VERIFY", "1" }
        });
        Output.Verbose($"Ran verification tests in: {testRoot}");
    }
}