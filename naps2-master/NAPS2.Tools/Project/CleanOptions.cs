using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("clean", HelpText = "Fully clean all projects (removing everything from bin/obj except nuget config)")]
public class CleanOptions : OptionsBase
{
}