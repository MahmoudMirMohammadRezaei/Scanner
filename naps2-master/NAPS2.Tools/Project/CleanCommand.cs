namespace NAPS2.Tools.Project;

public class CleanCommand : ICommand<CleanOptions>
{
    public int Run(CleanOptions opts)
    {
        Output.Info("Starting clean");
        bool hasError = false;
        foreach (var projectDir in new DirectoryInfo(Paths.SolutionRoot).EnumerateDirectories("NAPS2.*")
                     .Where(x => x.Name.ToLower() != "naps2.tools"))
        {
            foreach (var cleanDir in projectDir.EnumerateDirectories()
                         .Where(x => x.Name.ToLower() == "bin" || x.Name.ToLower() == "obj"))
            {
                foreach (var subDir in cleanDir.EnumerateDirectories())
                {
                    try
                    {
                        subDir.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        Output.Info($"Could not delete {projectDir.Name}/{cleanDir.Name}/{subDir.Name}: {ex.Message}");
                        hasError = true;
                    }
                }
            }
            Output.Verbose($"Cleaned {projectDir.Name}");
        }
        try
        {
            var docObj = new DirectoryInfo(Path.Combine(Paths.SolutionRoot, "NAPS2.Sdk", "_doc", "obj"));
            if (docObj.Exists)
            {
                docObj.Delete(true);
            }
        }
        catch (Exception ex)
        {
            Output.Info($"Could not delete NAPS2.Sdk/doc/obj: {ex.Message}");
            hasError = true;
        }
        if (hasError)
        {
            throw new Exception("Cleaned with failures.");
        }
        Output.Info("Cleaned.");
        return 0;
    }
}