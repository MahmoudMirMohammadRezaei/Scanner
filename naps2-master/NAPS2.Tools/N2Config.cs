using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NAPS2.Tools;

// TODO: N2Config vs files in Naps2UserFolder?
public static class N2Config
{
    public static string ShareDir
    {
        get
        {
            var dir = EnsureConfigFile().Value<string>("share-dir");
            if (string.IsNullOrEmpty(dir))
            {
                throw new Exception("Expected share-dir to be specified in NAPS2.Tools/n2-config.json");
            }
            return dir;
        }
    }

    public static string? AutoUpdateCert => EnsureConfigFile().Value<string>("auto-update-cert") ?? "";

    public static string? MacApplicationIdentity => EnsureConfigFile().Value<string>("mac-application-identity") ?? "";

    public static string? MacInstallerIdentity => EnsureConfigFile().Value<string>("mac-installer-identity") ?? "";

    public static string? MacNotarizationArgs => EnsureConfigFile().Value<string>("mac-notarization-args") ?? "";

    public static string? FlatpakGpgKey => EnsureConfigFile().Value<string>("flatpak-gpg-key") ?? "";

    public static string? FlatpakRepo => EnsureConfigFile().Value<string>("flatpak-repo") ?? "";

    private static JToken EnsureConfigFile()
    {
        if (!File.Exists(Paths.ConfigFile))
        {
            File.WriteAllText(Paths.ConfigFile, "{\n    \"share-dir\": \"\",\n    \"mac-application-identity\": \"\",    \"mac-installer-identity\": \"\",\n    \"mac-notarization-args\": \"\",\n    \"flatpak-gpg-key\": \"\",\n    \"flatpak-repo\": \"\",\n    \"auto-update-cert\": \"\"\n}\n");
        }
        using var file = File.OpenText(Paths.ConfigFile);
        using var reader = new JsonTextReader(file);
        return JToken.ReadFrom(reader);
    }
}