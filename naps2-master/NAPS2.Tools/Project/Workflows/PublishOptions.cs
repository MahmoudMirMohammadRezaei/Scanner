using CommandLine;

namespace NAPS2.Tools.Project.Workflows;

[Verb("publish", HelpText = "Build, test, package, and verify standard targets")]
public class PublishOptions : OptionsBase
{
    [Value(0, MetaName = "package type", Required = false, HelpText = "all|exe|msi|zip|flatpak|pkg|deb|rpm")]
    public string? PackageType { get; set; }
    
    [Option('p', "platform", Required = false, HelpText = "all|win|win32|win64|mac|macintel|macarm|linux")]
    public string? Platform { get; set; }

    [Option("noverify", Required = false, HelpText = "Don't run verification tests")]
    public bool NoVerify { get; set; }
}