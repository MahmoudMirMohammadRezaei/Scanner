namespace NAPS2.App.Tests.Targets;

public class MacAppTestTarget : IAppTestTarget
{
    public AppTestExe Console => GetAppTestExe("console");
    public AppTestExe Gui => GetAppTestExe(null);
    public AppTestExe Worker => GetAppTestExe("worker");

    private AppTestExe GetAppTestExe(string argPrefix)
    {
        return new AppTestExe(
            Path.Combine(AppTestHelper.SolutionRoot, "NAPS2.App.Mac", "bin", "Debug", "net7-macos10.15"),
            Path.Combine("NAPS2.app", "Contents", "MacOS", "NAPS2"),
            argPrefix);
    }

    public override string ToString() => "Mac";
}